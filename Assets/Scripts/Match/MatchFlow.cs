using UnityEngine;

public enum MatchState
{
    Menu,
    Aiming,
    Simulating,
    Resolving,
    GameOver
}

public sealed class MatchFlow : MonoBehaviour
{
    public MatchState State { get; private set; } = MatchState.Menu;
    public int CurrentPlayer { get; private set; }
    public IGameMode Mode { get; private set; }
    public bool VsAi;
    public int AiDifficulty = 1;
    public bool AiToMove => VsAi && CurrentPlayer == 1 && State == MatchState.Aiming;

    Ball[] _balls;
    Ball _cue;
    Table _table;
    CueController _cueCtl;
    ShotResolver _resolver;
    PhysicsSettledDetector _settled;
    PoolAi _ai;
    GameHud _hud;
    PoolAudio _audio;
    CameraRig _rig;
    PocketScanner _pockets;

    public void Wire(Ball[] balls, Table table, CueController cueCtl, ShotResolver resolver, PhysicsSettledDetector settled, PoolAi ai, GameHud hud, PoolAudio audio)
    {
        _balls = balls;
        _cue = balls[0];
        _table = table;
        _cueCtl = cueCtl;
        _resolver = resolver;
        _settled = settled;
        _ai = ai;
        _hud = hud;
        _audio = audio;
        _rig = GetComponent<CameraRig>();
        _pockets = GetComponent<PocketScanner>();
    }

    public void StartMatch(GameModeKind kind, bool vsAi, int difficulty)
    {
        VsAi = vsAi;
        AiDifficulty = difficulty;
        CurrentPlayer = 0;
        Mode = kind == GameModeKind.NineBall ? (IGameMode)new NineBallRules() : new EightBallRules();
        if (kind == GameModeKind.NineBall)
        {
            BallFactory.RackNineBall(_balls, _table.transform);
        }
        else
        {
            BallFactory.RackEightBall(_balls, _table.transform);
        }

        Mode.StartMatch(_balls);
        _settled.Bind(_balls);
        _cueCtl.AimYaw = 0f;
        _cueCtl.Power = 0.5f;
        _cueCtl.English = Vector2.zero;
        _cueCtl.PlacingCue = false;
        _cueCtl.InputLocked = false;
        SetIgnoreCuePockets(false);
        _cueCtl.SetVisible(true);
        _resolver.IgnorePocketsFor(0.45f);
        State = MatchState.Aiming;
        _rig?.SetAimView(true);
        _hud.ShowMatch();
        RefreshHud("Drag to aim  ·  Space or SHOOT");
        _audio.PlayClick();
    }

    public void Shoot()
    {
        if (_cueCtl.PlacingCue)
        {
            ConfirmPlacement();
            return;
        }

        if (State != MatchState.Aiming || _cue.Pocketed || !_cue.isActiveAndEnabled)
        {
            return;
        }

        _resolver.BeginShot();
        _cueCtl.Fire();
        _cueCtl.InputLocked = true;
        _cueCtl.SetVisible(false);
        _settled.ResetWatch();
        State = MatchState.Simulating;
        _audio.PlayShot();
        RefreshHud("Balls moving…");
    }

    public void ConfirmPlacement()
    {
        if (State != MatchState.Aiming || !_cueCtl.PlacingCue)
        {
            return;
        }

        _cueCtl.PlacingCue = false;
        SetIgnoreCuePockets(false);
        Mode.OnBreakPlaced();
        _cueCtl.SetVisible(true);
        RefreshHud(Mode.StatusLabel);
        if (AiToMove)
        {
            Invoke(nameof(RunAi), 0.35f);
        }
    }

    void Update()
    {
        if (State == MatchState.Aiming && _cueCtl.PlacingCue)
        {
            RescuePlacedCue();
        }

        if (State == MatchState.Aiming && !_cueCtl.InputLocked && !_cueCtl.PlacingCue &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            Shoot();
        }

        if (State == MatchState.Simulating)
        {
            _settled.Tick(Time.deltaTime);
            if (_settled.Settled)
            {
                ResolveShot();
            }
        }
    }

    void ResolveShot()
    {
        State = MatchState.Resolving;
        var snap = _resolver.EndShot();
        if (_cue.Pocketed)
        {
            var pos = _table.ClampOnCloth(PoolConstants.HeadSpot, Mode.KitchenOnlyPlacement);
            _cue.RespawnOnTable(pos);
        }

        var outcome = Mode.Resolve(snap, CurrentPlayer);
        if (outcome == TurnOutcome.Win || outcome == TurnOutcome.Loss)
        {
            State = MatchState.GameOver;
            _cueCtl.InputLocked = true;
            _cueCtl.SetVisible(false);
            _hud.ShowGameOver(Mode.WinnerMessage);
            _audio.PlayWin();
            return;
        }

        if (outcome == TurnOutcome.Foul)
        {
            CurrentPlayer = 1 - CurrentPlayer;
            _cueCtl.PlacingCue = Mode.BallInHand;
            _cueCtl.KitchenOnlyPlacement = Mode.KitchenOnlyPlacement;
            SetIgnoreCuePockets(_cueCtl.PlacingCue);
            _cueCtl.InputLocked = false;
            State = MatchState.Aiming;
            _cueCtl.SetVisible(!_cueCtl.PlacingCue);
            _rig?.SetAimView(true);
            RefreshHud(Mode.FoulMessage + (Mode.BallInHand ? " — place cue ball" : ""));
        }
        else if (outcome == TurnOutcome.Switch)
        {
            CurrentPlayer = 1 - CurrentPlayer;
            _cueCtl.PlacingCue = false;
            SetIgnoreCuePockets(false);
            _cueCtl.InputLocked = false;
            State = MatchState.Aiming;
            _cueCtl.SetVisible(true);
            RefreshHud("Player " + (CurrentPlayer + 1) + " — " + Mode.StatusLabel);
        }
        else
        {
            _cueCtl.PlacingCue = false;
            SetIgnoreCuePockets(false);
            _cueCtl.InputLocked = false;
            State = MatchState.Aiming;
            _cueCtl.SetVisible(true);
            RefreshHud("Continue — " + Mode.StatusLabel);
        }

        if (AiToMove)
        {
            if (_cueCtl.PlacingCue)
            {
                AutoPlaceCueForAi();
            }

            Invoke(nameof(RunAi), 0.55f);
        }
    }

    void AutoPlaceCueForAi()
    {
        var p = _table.ClampOnCloth(PoolConstants.HeadSpot, Mode.KitchenOnlyPlacement);
        _cue.PlaceAndFreeze(p);
        _cueCtl.PlacingCue = false;
        SetIgnoreCuePockets(false);
        Mode.OnBreakPlaced();
        _cueCtl.SetVisible(true);
    }

    void RunAi()
    {
        if (!AiToMove)
        {
            return;
        }

        _ai.ChooseShot(_balls, _cue, _table, Mode, AiDifficulty, _cueCtl);
        Shoot();
    }

    void RescuePlacedCue()
    {
        if (_cue == null || Mode == null)
        {
            return;
        }

        if (!_cue.Pocketed && _cue.isActiveAndEnabled)
        {
            return;
        }

        var p = _table.ClampOnCloth(PoolConstants.HeadSpot, Mode.KitchenOnlyPlacement);
        _cue.PlaceAndFreeze(p);
    }

    void SetIgnoreCuePockets(bool ignore)
    {
        if (_pockets != null)
        {
            _pockets.IgnoreCueBall = ignore;
        }

        _resolver.IgnoreCueBall = ignore;
    }

    void RefreshHud(string extra)
    {
        var who = VsAi
            ? (CurrentPlayer == 0 ? "You" : "CPU")
            : "Player " + (CurrentPlayer + 1);
        _hud.SetStatus(who, Mode.StatusLabel, extra, _cueCtl.PlacingCue);
    }

    public void ReturnToMenu()
    {
        State = MatchState.Menu;
        _cueCtl.SetVisible(false);
        _cueCtl.InputLocked = true;
        _rig?.SetAimView(false);
        CancelInvoke();
        _hud.ShowMenu();
    }
}

using UnityEngine;

public sealed class GameBootstrap : MonoBehaviour
{
    static bool _booted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBoot()
    {
        if (FindAnyObjectByType<GameBootstrap>() != null)
        {
            return;
        }

        var go = new GameObject("PocketShot");
        go.AddComponent<GameBootstrap>();
    }

    void Awake()
    {
        if (_booted)
        {
            Destroy(gameObject);
            return;
        }

        _booted = true;
        DontDestroyOnLoad(gameObject);
        ApplyMobile();
        BuildWorld();
    }

    static void ApplyMobile()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.antiAliasing = 2;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Physics.defaultSolverIterations = 16;
        Physics.defaultSolverVelocityIterations = 12;
        Physics.defaultContactOffset = 0.0008f;
        Physics.defaultMaxDepenetrationVelocity = 0.45f;
        Physics.bounceThreshold = 0.2f;
        Time.fixedDeltaTime = 0.01f;
    }

    void BuildWorld()
    {
        var look = LookLibrary.Create();

        var cloth = new PhysicsMaterial("Cloth")
        {
            dynamicFriction = 0.2f,
            staticFriction = 0.22f,
            bounciness = 0.03f,
            frictionCombine = PhysicsMaterialCombine.Average,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };
        var cushion = new PhysicsMaterial("Cushion")
        {
            dynamicFriction = 0.18f,
            staticFriction = 0.2f,
            bounciness = 0.72f,
            frictionCombine = PhysicsMaterialCombine.Average,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };
        var ballPhys = new PhysicsMaterial("Ball")
        {
            dynamicFriction = 0.06f,
            staticFriction = 0.06f,
            bounciness = 0.94f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.18f, 0.2f, 0.22f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.07f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.035f;

        var tableGo = new GameObject("Table");
        tableGo.transform.SetParent(transform, false);
        var table = tableGo.AddComponent<Table>();
        table.Build(cloth, cushion, look);

        var ballsRoot = new GameObject("Balls");
        ballsRoot.transform.SetParent(transform, false);
        var balls = new Ball[16];
        for (var i = 0; i < 16; i++)
        {
            balls[i] = BallFactory.Create(i, ballsRoot.transform, ballPhys, look);
            balls[i].gameObject.SetActive(false);
        }

        var lightGo = new GameObject("KeyLight");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.96f, 0.88f);
        light.intensity = 0.85f;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.55f;
        lightGo.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

        var lamp = new GameObject("OverheadLamp");
        var spot = lamp.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.color = new Color(1f, 0.92f, 0.72f);
        spot.intensity = 6.5f;
        spot.range = 7f;
        spot.spotAngle = 95f;
        spot.innerSpotAngle = 55f;
        spot.shadows = LightShadows.None;
        lamp.transform.position = new Vector3(0f, 2.15f, 0f);
        lamp.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        var fill = new GameObject("FillLight");
        var fl = fill.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.range = 10f;
        fl.intensity = 1.1f;
        fl.color = new Color(0.55f, 0.65f, 0.8f);
        fl.shadows = LightShadows.None;
        fill.transform.position = new Vector3(-1.4f, 1.6f, -1.2f);

        var rig = gameObject.AddComponent<CameraRig>();
        rig.Build();

        var cueCtl = gameObject.AddComponent<CueController>();
        cueCtl.Bind(balls[0], table, rig.Cam, look);
        rig.Bind(balls[0], cueCtl);
        cueCtl.SetVisible(false);
        cueCtl.InputLocked = true;

        var resolver = gameObject.AddComponent<ShotResolver>();
        resolver.Bind(balls[0], tableGo.GetComponentsInChildren<PocketTrigger>(true), balls);
        var pockets = gameObject.AddComponent<PocketScanner>();
        pockets.Bind(table, balls);

        var settled = gameObject.AddComponent<PhysicsSettledDetector>();
        settled.Bind(balls);

        var ai = gameObject.AddComponent<PoolAi>();
        var audio = gameObject.AddComponent<PoolAudio>();
        audio.Build();
        var flow = gameObject.AddComponent<MatchFlow>();
        var hud = gameObject.AddComponent<GameHud>();
        flow.Wire(balls, table, cueCtl, resolver, settled, ai, hud, audio);
        hud.Build(flow, cueCtl, look);
    }

    void OnDestroy()
    {
        if (_booted)
        {
            _booted = false;
        }
    }
}

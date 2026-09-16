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
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.antiAliasing = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Physics.defaultSolverIterations = 12;
        Physics.defaultSolverVelocityIterations = 12;
        Time.fixedDeltaTime = 0.01f;
    }

    void BuildWorld()
    {
        var lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
        var felt = MakeMat(lit, new Color(0.07f, 0.38f, 0.2f));
        var wood = MakeMat(lit, new Color(0.32f, 0.18f, 0.08f));
        var pocket = MakeMat(lit, new Color(0.02f, 0.02f, 0.02f));

        var cloth = new PhysicMaterial("Cloth")
        {
            dynamicFriction = 0.2f,
            staticFriction = 0.22f,
            bounciness = 0.03f,
            frictionCombine = PhysicMaterialCombine.Average,
            bounceCombine = PhysicMaterialCombine.Minimum
        };
        var cushion = new PhysicMaterial("Cushion")
        {
            dynamicFriction = 0.18f,
            staticFriction = 0.2f,
            bounciness = 0.72f,
            frictionCombine = PhysicMaterialCombine.Average,
            bounceCombine = PhysicMaterialCombine.Maximum
        };
        var ballPhys = new PhysicMaterial("Ball")
        {
            dynamicFriction = 0.06f,
            staticFriction = 0.06f,
            bounciness = 0.94f,
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Maximum
        };

        var tableGo = new GameObject("Table");
        tableGo.transform.SetParent(transform, false);
        var table = tableGo.AddComponent<Table>();
        table.Build(cloth, cushion, felt, wood, pocket);

        var ballsRoot = new GameObject("Balls");
        ballsRoot.transform.SetParent(transform, false);
        var balls = new Ball[16];
        for (var i = 0; i < 16; i++)
        {
            balls[i] = BallFactory.Create(i, ballsRoot.transform, ballPhys, lit);
            balls[i].gameObject.SetActive(false);
        }

        var lightGo = new GameObject("KeyLight");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.05f;
        light.shadows = LightShadows.None;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var fill = new GameObject("FillLight");
        var fl = fill.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.range = 8f;
        fl.intensity = 1.4f;
        fl.shadows = LightShadows.None;
        fill.transform.position = new Vector3(0f, 2.2f, 0f);

        var rig = gameObject.AddComponent<CameraRig>();
        rig.Build();

        var cueCtl = gameObject.AddComponent<CueController>();
        cueCtl.Bind(balls[0], table, rig.Cam);
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
        hud.Build(flow, cueCtl);
    }

    void OnDestroy()
    {
        if (_booted)
        {
            _booted = false;
        }
    }

    static Material MakeMat(Shader lit, Color c)
    {
        var m = new Material(lit);
        if (m.HasProperty("_BaseColor"))
        {
            m.SetColor("_BaseColor", c);
        }

        if (m.HasProperty("_Color"))
        {
            m.SetColor("_Color", c);
        }

        if (m.HasProperty("_Smoothness"))
        {
            m.SetFloat("_Smoothness", 0.35f);
        }

        return m;
    }
}

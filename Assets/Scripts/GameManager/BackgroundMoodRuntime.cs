using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// InGame 씬이 로드될 때마다 배경 연출(앰비언트/Fog/포스트프로세싱/액센트 라이트)을
/// 코드로 자동 적용한다.
///
/// 왜 런타임인가:
///   에디터에서 설정한 RenderSettings(앰비언트 등)는 플레이 진입 시 Unity의 GI 재계산에
///   덮여 다시 밝아지는 경우가 많다. 씬 로드 "직후"에 코드로 다시 적용하면 항상 이긴다.
///
/// 사용법: 그냥 두면 된다. 컴포넌트를 붙일 필요 없이 자동 실행된다.
///   (RuntimeInitializeOnLoadMethod + SceneManager.sceneLoaded)
///
/// 값 튜닝: 아래 상수만 바꾸면 된다.
/// </summary>
public static class BackgroundMoodRuntime
{
    // ───────── 적용 대상 ─────────
    const string TargetScene = "InGame";

    // ───────── 튜닝용 값 ─────────
    static readonly Color AmbientColor = new Color(0.12f, 0.13f, 0.18f, 1f);

    const bool   FogEnabled = true;
    static readonly Color FogColor = new Color(0.05f, 0.06f, 0.09f, 1f);
    const float  FogDensity = 0.012f;

    static readonly Color StartLightColor = new Color(0.55f, 0.75f, 1.0f); // 시작점: 청색
    static readonly Color GoalLightColor  = new Color(1.0f, 0.55f, 0.30f); // 끝점: 주황
    const float  AccentIntensity = 12f;
    const float  AccentRange     = 16f;
    const float  AccentHeight    = 5f;

    const string VolumeObjName  = "Global Volume (Runtime)";
    const string AccentRootName = "Scene Accent Lights (Runtime)";

    // ───────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Hook()
    {
        // 이후 씬 전환(메뉴 → InGame, 재시작 등)마다 적용
        SceneManager.sceneLoaded -= OnSceneLoaded; // 도메인 리로드 OFF 대비 중복 구독 방지
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 이미 첫 씬으로 InGame이 떠 있는 경우(InGame에서 바로 Play)도 적용
        Apply(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply(scene);

    static void Apply(Scene scene)
    {
        if (scene.name != TargetScene) return;

        ApplyAmbientAndFog();
        ApplyVolume();
        ApplyCameraPostProcessing();
        ApplyAccentLights();
    }

    static void ApplyAmbientAndFog()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = AmbientColor;

        RenderSettings.fog = FogEnabled;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = FogColor;
        RenderSettings.fogDensity = FogDensity;
    }

    static void ApplyVolume()
    {
        if (GameObject.Find(VolumeObjName) != null) return; // 중복 방지

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        var tone = profile.Add<Tonemapping>(true);
        tone.mode.overrideState = true;
        tone.mode.value = TonemappingMode.ACES;

        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.overrideState = true; bloom.threshold.value = 0.9f;
        bloom.intensity.overrideState = true; bloom.intensity.value = 0.7f;
        bloom.scatter.overrideState   = true; bloom.scatter.value   = 0.6f;
        bloom.tint.overrideState      = true; bloom.tint.value      = new Color(1f, 0.9f, 0.75f);

        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.overrideState  = true; vignette.intensity.value  = 0.34f;
        vignette.smoothness.overrideState = true; vignette.smoothness.value = 0.4f;

        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.overrideState = true; color.postExposure.value = 0.1f;
        color.contrast.overrideState     = true; color.contrast.value     = 12f;
        color.saturation.overrideState   = true; color.saturation.value   = 6f;

        var smh = profile.Add<ShadowsMidtonesHighlights>(true);
        smh.shadows.overrideState    = true; smh.shadows.value    = new Vector4(0.85f, 0.92f, 1.1f, 0f);
        smh.highlights.overrideState = true; smh.highlights.value = new Vector4(1.1f, 1.02f, 0.9f, 0f);

        var go = new GameObject(VolumeObjName);
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;
        volume.sharedProfile = profile;
    }

    static void ApplyCameraPostProcessing()
    {
        var cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        if (cam == null) return;
        var data = cam.GetUniversalAdditionalCameraData();
        if (data == null) return;
        data.renderPostProcessing = true;
        data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    }

    static void ApplyAccentLights()
    {
        if (GameObject.Find(AccentRootName) != null) return; // 중복 방지

        var tileMap = Object.FindFirstObjectByType<TileMap>();
        if (tileMap == null) return;

        // 시작점 = (z - x) 최대(좌상단), 끝점 = (x - z) 최대(우하단)
        Transform startTile = null, goalTile = null;
        float bestStart = float.NegativeInfinity, bestGoal = float.NegativeInfinity;
        foreach (Transform c in tileMap.transform)
        {
            float startScore = c.position.z - c.position.x;
            float goalScore  = c.position.x - c.position.z;
            if (startScore > bestStart) { bestStart = startScore; startTile = c; }
            if (goalScore  > bestGoal)  { bestGoal  = goalScore;  goalTile  = c; }
        }
        if (startTile == null || goalTile == null) return;

        var root = new GameObject(AccentRootName);
        MakePointLight("Start Light", startTile.position, StartLightColor, root.transform);
        MakePointLight("Goal Light",  goalTile.position,  GoalLightColor,  root.transform);
    }

    static void MakePointLight(string name, Vector3 tilePos, Color color, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = tilePos + Vector3.up * AccentHeight;
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = AccentIntensity;
        light.range = AccentRange;
        light.shadows = LightShadows.Soft;
    }
}

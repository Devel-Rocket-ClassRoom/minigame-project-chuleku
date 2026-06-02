#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 배경 연출용 조명 + 포스트프로세싱을 메뉴 클릭 한 번으로 세팅/제거한다.
/// - URP Global Volume (Tonemapping / Bloom / Vignette / Color Adjustments / SMH)
/// - 메인 카메라 포스트프로세싱 ON
/// - 무드 앰비언트 + 약한 Fog
/// - 시작점/끝점 위에 따뜻한 액센트 포인트 라이트
///
/// 모든 자동 생성물은 "(Auto)" 이름이 붙고, 재실행 시 깔끔히 교체된다.
/// 메뉴: Tools ▸ 배경 연출
///
/// ※ 값이 마음에 안 들면 아래 Settings 상수만 바꿔서 다시 실행하거나,
///   생성된 Global Volume / 라이트를 인스펙터에서 직접 만지면 된다.
/// </summary>
public static class BackgroundMoodSetup
{
    // ───────── 튜닝용 값 (여기만 바꿔도 됨) ─────────
    const string VolumeObjName  = "Global Volume (Auto)";
    const string AccentRootName = "Scene Accent Lights (Auto)";
    const string ProfilePath    = "Assets/Settings/InGame_PostProcess.asset";

    // 앰비언트(전체 환경광) — 살짝 어둡고 푸른 던전 톤
    static readonly Color AmbientColor = new Color(0.12f, 0.13f, 0.18f, 1f);

    // Fog — 너무 세면 탑다운이 뿌예지므로 아주 약하게
    const bool   FogEnabled = true;
    static readonly Color FogColor = new Color(0.05f, 0.06f, 0.09f, 1f);
    const float  FogDensity = 0.012f;

    // 액센트 라이트(시작/끝점)
    static readonly Color StartLightColor = new Color(0.55f, 0.75f, 1.0f); // 시작점: 차가운 청색
    static readonly Color GoalLightColor  = new Color(1.0f, 0.55f, 0.30f); // 끝점: 따뜻한 주황
    const float  AccentIntensity = 12f;
    const float  AccentRange     = 16f;
    const float  AccentHeight    = 5f;   // 타일 위 높이

    // ───────────────────────────────────────────────

    [MenuItem("Tools/배경 연출/조명 + 포스트프로세싱 세팅", priority = 0)]
    public static void Setup()
    {
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("배경 연출 세팅");

        SetupVolume();
        EnableCameraPostProcessing();
        SetupAmbientAndFog();
        SetupAccentLights();

        Undo.CollapseUndoOperations(undoGroup);
        MarkSceneDirty();
        Debug.Log("[배경 연출] 조명 + 포스트프로세싱 세팅 완료. 마음에 안 들면 'Tools ▸ 배경 연출 ▸ 제거(되돌리기)'.");
    }

    [MenuItem("Tools/배경 연출/제거 (되돌리기)", priority = 1)]
    public static void Remove()
    {
        DestroyByName(VolumeObjName);
        DestroyByName(AccentRootName);

        var cam = Camera.main;
        if (cam != null)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null) data.renderPostProcessing = false;
        }
        RenderSettings.fog = false;

        MarkSceneDirty();
        Debug.Log("[배경 연출] 자동 생성물 제거 완료. (앰비언트 색은 수동 복구 필요)");
    }

    // ───────── Global Volume + 프로파일 ─────────
    static void SetupVolume()
    {
        DestroyByName(VolumeObjName);

        // 프로파일 에셋 새로 생성(있으면 덮어씀)
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        var tone = profile.Add<Tonemapping>(true);
        tone.mode.overrideState = true;
        tone.mode.value = TonemappingMode.ACES; // 명암 대비를 영화적으로

        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.overrideState = true; bloom.threshold.value = 0.9f;
        bloom.intensity.overrideState = true; bloom.intensity.value = 0.7f;
        bloom.scatter.overrideState   = true; bloom.scatter.value   = 0.6f;
        bloom.tint.overrideState      = true; bloom.tint.value      = new Color(1f, 0.9f, 0.75f);

        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.overrideState = true; vignette.intensity.value = 0.34f;
        vignette.smoothness.overrideState = true; vignette.smoothness.value = 0.4f;

        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.overrideState = true; color.postExposure.value = 0.1f;
        color.contrast.overrideState     = true; color.contrast.value     = 12f;
        color.saturation.overrideState   = true; color.saturation.value   = 6f;

        var smh = profile.Add<ShadowsMidtonesHighlights>(true);
        smh.shadows.overrideState    = true; smh.shadows.value    = new Vector4(0.85f, 0.92f, 1.1f, 0f); // 그림자 약간 푸르게
        smh.highlights.overrideState = true; smh.highlights.value = new Vector4(1.1f, 1.02f, 0.9f, 0f);  // 하이라이트 약간 따뜻하게

        System.IO.Directory.CreateDirectory("Assets/Settings");
        AssetDatabase.CreateAsset(profile, ProfilePath);
        AssetDatabase.SaveAssets();

        var go = new GameObject(VolumeObjName);
        Undo.RegisterCreatedObjectUndo(go, "Create Global Volume");
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;
        volume.sharedProfile = profile;
    }

    static void EnableCameraPostProcessing()
    {
        var cam = Camera.main;
        if (cam == null) { Debug.LogWarning("[배경 연출] MainCamera 태그가 붙은 카메라를 못 찾음 — 포스트프로세싱 수동 ON 필요."); return; }
        var data = cam.GetUniversalAdditionalCameraData();
        Undo.RecordObject(data, "Enable Post Processing");
        data.renderPostProcessing = true;
        data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        EditorUtility.SetDirty(data);
    }

    static void SetupAmbientAndFog()
    {
        // ※ RenderSettings(앰비언트/Fog)는 Undo 스택에 직접 안 들어감.
        //   되돌리려면 'Tools ▸ 배경 연출 ▸ 제거' 사용 + 앰비언트는 수동 복구.
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = AmbientColor;

        RenderSettings.fog = FogEnabled;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = FogColor;
        RenderSettings.fogDensity = FogDensity;
    }

    // ───────── 시작/끝점 액센트 라이트 ─────────
    static void SetupAccentLights()
    {
        DestroyByName(AccentRootName);

        var tileMap = Object.FindFirstObjectByType<TileMap>();
        if (tileMap == null)
        {
            Debug.LogWarning("[배경 연출] TileMap을 못 찾아 액센트 라이트는 건너뜀.");
            return;
        }

        // 시작점 = X 최소 & Z 최대(좌상단) → (z - x) 최대
        // 끝점   = X 최대 & Z 최소(우하단) → (x - z) 최대
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
        Undo.RegisterCreatedObjectUndo(root, "Create Accent Lights");

        MakePointLight("Start Light (Auto)", startTile.position, StartLightColor, root.transform);
        MakePointLight("Goal Light (Auto)",  goalTile.position,  GoalLightColor,  root.transform);
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

    // ───────── 헬퍼 ─────────
    static void DestroyByName(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Undo.DestroyObjectImmediate(go);
    }

    static void MarkSceneDirty()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }
}
#endif

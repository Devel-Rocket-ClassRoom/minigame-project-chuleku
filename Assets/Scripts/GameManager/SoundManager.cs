using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private SoundDatabase db;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;
    
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string sfxParam = "SfxVolume";
    [SerializeField] private string bgmParam = "BgmVolume";
    [SerializeField] private string uiParam  = "UiVolume";

    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float uiVolume  = 1f;


    private const string PrefMaster = "vol_master";
    private const string PrefSfx = "vol_sfx";
    private const string PrefBgm = "vol_bgm";
    private const string PrefUi  = "vol_ui";

    void Awake()
    {
        Instance = this;
        if (sfxSource != null) sfxSource.playOnAwake = false;
        if (bgmSource != null) { bgmSource.playOnAwake = false; bgmSource.loop = true; }

        // 저장된 볼륨 복원 (없으면 인스펙터 기본값 사용)
        masterVolume = PlayerPrefs.GetFloat(PrefMaster, masterVolume);
        sfxVolume = PlayerPrefs.GetFloat(PrefSfx, sfxVolume);
        bgmVolume = PlayerPrefs.GetFloat(PrefBgm, bgmVolume);
        uiVolume  = PlayerPrefs.GetFloat(PrefUi,  uiVolume);
        ApplyMixer();
    }
    void Start()
    {
        masterSlider.value = masterVolume;
        sfxSlider.value = sfxVolume;
        bgmSlider.value = bgmVolume;
        uiSlider.value = uiVolume;
    }

    private void ApplyMixer()
    {
        if (mixer == null) return;
        mixer.SetFloat(masterParam, LinearToDb(masterVolume));
        mixer.SetFloat(sfxParam, LinearToDb(sfxVolume));
        mixer.SetFloat(bgmParam, LinearToDb(bgmVolume));
        mixer.SetFloat(uiParam,  LinearToDb(uiVolume));
    }

    // 슬라이더 초기값 표시용 (설정창 열 때 호출)
    public float GetMasterVolume() => masterVolume;
    public float GetSfxVolume() => sfxVolume;
    public float GetBgmVolume() => bgmVolume;
    public float GetUiVolume()  => uiVolume;

    // 선형 0~1 → 데시벨 변환 (믹서는 dB로 동작)
    private static float LinearToDb(float v)
    {
        if (v <= 0.0001f) return -80f;
        return Mathf.Log10(v) * 20f;
    }

    // 효과음 재생 (중복 재생 허용)
    public static void Play(string key)
    {
        if (Instance == null || Instance.db == null) return;
        var e = Instance.db.Get(key);
        if (e == null) { Debug.LogWarning($"SoundDatabase에 '{key}' 키 없음"); return; }
        if (Instance.sfxSource == null) { Debug.LogWarning("sfxSource 미할당"); return; }

        Instance.sfxSource.PlayOneShot(e.clip, e.volume * Instance.sfxVolume);
    }

    // BGM 재생 (같은 곡이면 무시, 다르면 교체)
    public static void PlayBgm(string key)
    {
        if (Instance == null || Instance.db == null) return;
        var e = Instance.db.Get(key);
        if (e == null) { Debug.LogWarning($"SoundDatabase에 '{key}' 키 없음"); return; }
        if (Instance.bgmSource == null) { Debug.LogWarning("bgmSource 미할당"); return; }

        var src = Instance.bgmSource;
        if (src.isPlaying && src.clip == e.clip) return;

        src.clip = e.clip;
        src.volume = e.volume * Instance.bgmVolume;
        src.loop = e.loop;
        src.Play();
    }

    public static void StopBgm()
    {
        if (Instance == null || Instance.bgmSource == null) return;
        Instance.bgmSource.Stop();
    }
    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        if (mixer != null) mixer.SetFloat(masterParam, LinearToDb(masterVolume));
        PlayerPrefs.SetFloat(PrefMaster, masterVolume);
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        if (mixer != null) mixer.SetFloat(sfxParam, LinearToDb(sfxVolume));
        PlayerPrefs.SetFloat(PrefSfx, sfxVolume);
    }

    public void SetBgmVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        if (mixer != null) mixer.SetFloat(bgmParam, LinearToDb(bgmVolume));
        PlayerPrefs.SetFloat(PrefBgm, bgmVolume);
    }

    public void SetUiVolume(float v)
    {
        uiVolume = Mathf.Clamp01(v);
        if (mixer != null) mixer.SetFloat(uiParam, LinearToDb(uiVolume));
        PlayerPrefs.SetFloat(PrefUi, uiVolume);
    }
}

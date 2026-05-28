using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Data/SoundDatabase")]
public class SoundDatabase : ScriptableObject
{
    public enum SoundType
    {
        Sfx,
        Bgm,
    }

    [System.Serializable]
    public class Entry
    {
        public string key;          // 코드에서 호출할 키 (예: "sfx_enemy_die")
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public SoundType type = SoundType.Sfx;
        public bool loop = false;   // BGM에서만 의미 있음
    }

    public List<Entry> entries;

    private Dictionary<string, Entry> _lookup;

    public Entry Get(string key)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<string, Entry>();
            foreach (var e in entries)
                if (e != null && e.clip != null && !string.IsNullOrEmpty(e.key))
                    _lookup[e.key] = e;
        }
        return _lookup.TryGetValue(key, out var entry) ? entry : null;
    }
}

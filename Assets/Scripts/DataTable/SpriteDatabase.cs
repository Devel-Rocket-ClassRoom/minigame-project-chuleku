using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "SpriteDatabase", menuName = "Data/SpriteDatabase")]
public class SpriteDatabase : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string key;     // CSV에 적힌 키 (예: "card_fireball")
        public Sprite sprite;  // Imported/ 안의 스프라이트를 드래그
    }
    public List<Entry> entries;

    private Dictionary<string, Sprite> _lookup;

    public Sprite Get(string key)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<string, Sprite>();
            foreach (var e in entries)
                if (e.sprite != null) _lookup[e.key] = e.sprite;
        }
        return _lookup.TryGetValue(key, out var s) ? s : null;
    }
}

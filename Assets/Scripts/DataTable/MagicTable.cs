using System.Collections.Generic;
using UnityEngine;

public class MagicTable : DataTable
{
    public class Data
    {
        public string Id { get; set; }
        public string Name { get; set; }    // StringTable 키
        public string Desc { get; set; }    // StringTable 키
        public string Image { get; set; }   // Resources/CardArt 하위 키
        public string Prefab { get; set; }  // Resources 하위 경로 키 (예: "MagicPrefab/Fireball")
    }

    private readonly Dictionary<string, Data> table = new();

    public override void Load(string filename)
    {
        table.Clear();

        var path = $"MagicData/{filename}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogWarning($"MagicTable: '{path}' 로드 실패");
            return;
        }

        var list = LoadCsv<Data>(textAsset.text);
        foreach (var data in list)
        {
            if (string.IsNullOrEmpty(data.Id)) continue;
            if (!table.ContainsKey(data.Id)) table.Add(data.Id, data);
            else Debug.LogWarning($"MagicTable 키 중복 '{data.Id}'");
        }
    }

    public Data Get(string key)
    {
        if (string.IsNullOrEmpty(key) || !table.ContainsKey(key)) return null;
        return table[key];
    }
}

using System.Collections.Generic;
using UnityEngine;

public class UnitTable : DataTable
{
    public class Data
    {
        public string Id { get; set; }
        public int Attack { get; set; }
        public float AttackSpeed { get; set; }
        public float Range { get; set; }
        public string Prefab { get; set; }   // Resources 하위 경로 키 (예: "UnitPrefab/Archer")
    }

    private readonly Dictionary<string, Data> table = new();

    public override void Load(string filename)
    {
        table.Clear();

        var path = $"UnitData/{filename}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogWarning($"UnitTable: '{path}' 로드 실패");
            return;
        }

        var list = LoadCsv<Data>(textAsset.text);
        foreach (var data in list)
        {
            if (string.IsNullOrEmpty(data.Id)) continue;
            if (!table.ContainsKey(data.Id)) table.Add(data.Id, data);
            else Debug.LogWarning($"UnitTable 키 중복 '{data.Id}'");
        }
    }

    public Data Get(string key)
    {
        if (string.IsNullOrEmpty(key) || !table.ContainsKey(key)) return null;
        return table[key];
    }
}

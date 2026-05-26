using System.Collections.Generic;
using UnityEngine;

public class MonsterTable : DataTable
{
    public class Data
    {
        public string Id { get; set; }
        public string Prefab { get; set; }      // Resources 하위 경로 (예: "MonsterPrefab/Goblin")
        public float Health { get; set; }       // 기본 체력
        public float HealthScale { get; set; }  // 스테이지당 체력 증가량
        public int Defence { get; set; }        // 기본 방어력
        public int DefenceScale { get; set; }   // 5스테이지마다 방어력 증가량
        public float MoveSpeed { get; set; }
        public EnemyType Type { get; set; }
    }

    private readonly Dictionary<string, Data> table = new();

    public override void Load(string filename)
    {
        table.Clear();

        // CSV가 UnitData/ 폴더에 있어서 경로도 동일하게 맞춤.
        // 나중에 MonsterData/ 로 분리하고 싶으면 이 경로만 바꾸면 됨.
        var path = $"UnitData/{filename}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogWarning($"MonsterTable: '{path}' 로드 실패");
            return;
        }

        var list = LoadCsv<Data>(textAsset.text);
        foreach (var data in list)
        {
            if (string.IsNullOrEmpty(data.Id)) continue;
            if (!table.ContainsKey(data.Id)) table.Add(data.Id, data);
            else Debug.LogWarning($"MonsterTable 키 중복 '{data.Id}'");
        }
    }

    public Data Get(string key)
    {
        if (string.IsNullOrEmpty(key) || !table.ContainsKey(key)) return null;
        return table[key];
    }
}

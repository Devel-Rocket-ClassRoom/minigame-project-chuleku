using System;
using System.Collections.Generic;
using UnityEngine;

public class StageTable : DataTable
{
    public class Data
    {
        public int ID { get; set; }
        public string MonsterName { get; set; }
        public string Prefab { get; set; }   // Resources 하위 경로 키 (예: "MonsterPrefab/Goblin")
        public int Count { get; set; }
        public float SpawnTime { get; set; } // 스테이지 시작 후 이 시간(초)부터 스폰 시작
        public float Delay { get; set; }     // 같은 그룹 내 몬스터 간 스폰 간격(초)
    }

    private readonly Dictionary<int, List<Data>> table = new();

    public override void Load(string filename)
    {
        table.Clear();

        var path = $"StageData/{filename}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogWarning($"StageTable: '{path}' 로드 실패");
            return;
        }

        var list = LoadCsv<Data>(textAsset.text);
        foreach (var data in list)
        {
            if (!table.ContainsKey(data.ID))
                table[data.ID] = new List<Data>();
            table[data.ID].Add(data);
        }
    }

    // 스테이지 단위 조회 — 해당 스테이지의 모든 몬스터 그룹 반환
    public IReadOnlyList<Data> Get(int stageId)
    {
        return table.TryGetValue(stageId, out var groups) ? groups : null;
    }

    public bool HasStage(int stageId) => table.ContainsKey(stageId);

    internal string Get(string name)
    {
        throw new NotImplementedException();
    }

    public int MaxStageId
    {
        get
        {
            int max = 0;
            foreach (var k in table.Keys) if (k > max) max = k;
            return max;
        }
    }
}

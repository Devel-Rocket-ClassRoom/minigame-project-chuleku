using System.Collections.Generic;
using UnityEngine;

// 팁 텍스트 테이블. StringTable과 동일하게 ID(키) → Desc(문자열)로 읽는다.
// 사용: DataTableManager.TipTable.Get("Tip01")
public class TipTable : DataTable
{
    public static readonly string UnKnown = "키 없음";

    public class Data
    {
        public string ID { get; set; }
        public string Desc { get; set; }
    }

    private readonly Dictionary<string, string> table = new Dictionary<string, string>();

    public override void Load(string filename)
    {
        table.Clear();

        var path = $"DataTable/{filename}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCsv<Data>(textAsset.text);
        foreach (var data in list)
        {
            if (!table.ContainsKey(data.ID))
            {
                // CSV의 "\n" 표기를 실제 줄바꿈으로 변환해서 저장 (TMP에서 줄바꿈 표시)
                table.Add(data.ID, data.Desc?.Replace("\\n", "\n"));
            }
            else
            {
                Debug.LogWarning($"키 중복'{data.ID} - {filename}'");
            }
        }
    }

    public string Get(string key)
    {
        if (!table.ContainsKey(key))
        {
            return UnKnown;
        }
        return table[key];
    }

    // 등록된 모든 팁 키 (랜덤 팁 출력 등에 사용)
    public IReadOnlyCollection<string> Keys => table.Keys;

    // 랜덤 팁 문자열 하나를 반환. 팁이 없으면 UnKnown.
    public string GetRandom()
    {
        if (table.Count == 0) return UnKnown;
        int idx = Random.Range(0, table.Count);
        int i = 0;
        foreach (var v in table.Values)
        {
            if (i == idx) return v;
            i++;
        }
        return UnKnown;
    }

    // (키도 함께 필요할 때) 랜덤 팁의 키-문자열 쌍을 반환.
    public KeyValuePair<string, string> GetRandomPair()
    {
        if (table.Count == 0) return new KeyValuePair<string, string>(null, UnKnown);
        int idx = Random.Range(0, table.Count);
        int i = 0;
        foreach (var kv in table)
        {
            if (i == idx) return kv;
            i++;
        }
        return new KeyValuePair<string, string>(null, UnKnown);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class CardTable : DataTable
{
    public class Data
    {
        public string Id { get; set; }
        public CardType Type { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public int Attack { get; set; }
        public int Mana { get; set; }
    }

    private readonly Dictionary<string, Data> table = new Dictionary<string, Data>();

    public override void Load(string filename)
    {
        table.Clear();

        var path = $"CardData/{filename}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCsv<Data>(textAsset.text);
        foreach (var data in list)
        {
            if (!table.ContainsKey(data.Id))
            {
                table.Add(data.Id, data);
            }
            else
            {
                Debug.LogWarning($"키 중복 '{data.Id} - {filename}'");
            }
        }
    }

    public Data Get(string key)
    {
        if (string.IsNullOrEmpty(key) || !table.ContainsKey(key))
        {
            return null;
        }
        return table[key];
    }
}

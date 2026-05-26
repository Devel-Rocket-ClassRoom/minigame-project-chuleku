using UnityEngine;
using System.Collections.Generic;

public static class DataTableManager
{
    private static readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

    public static StringTable StringTable => Get<StringTable>(DataTableIds.String);
    public static CardTable CardTable => Get<CardTable>(DataTableIds.Card);
    public static UnitTable UnitTable => Get<UnitTable>(DataTableIds.Unit);
    public static MagicTable MagicTable => Get<MagicTable>(DataTableIds.Magic);
    public static StageTable StageTable => Get<StageTable>(DataTableIds.Stage);
    public static MonsterTable MonsterTable => Get<MonsterTable>(DataTableIds.Monster);

    static DataTableManager()
    {
        Init();
    }

    private static void Init()
    {
        var stringTable = new StringTable();
        stringTable.Load(DataTableIds.String);
        tables.Add(DataTableIds.String, stringTable);

        var cardTable = new CardTable();
        cardTable.Load(DataTableIds.Card);
        tables.Add(DataTableIds.Card, cardTable);

        var unitTable = new UnitTable();
        unitTable.Load(DataTableIds.Unit);
        tables.Add(DataTableIds.Unit, unitTable);

        var magicTable = new MagicTable();
        magicTable.Load(DataTableIds.Magic);
        tables.Add(DataTableIds.Magic, magicTable);

        var stageTable = new StageTable();
        stageTable.Load(DataTableIds.Stage);
        tables.Add(DataTableIds.Stage, stageTable);

        var monsterTable = new MonsterTable();
        monsterTable.Load(DataTableIds.Monster);
        tables.Add(DataTableIds.Monster, monsterTable);
    }

    public static T Get<T>(string id) where T : DataTable
    {
        if (!tables.ContainsKey(id))
        {
            Debug.Log($"테이블 없음: {id}");
            return null;
        }
        return tables[id] as T;
    }
}

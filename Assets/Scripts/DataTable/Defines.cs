using UnityEngine;

public enum CardType
{
    Unit,
    Effect,
    Resource,
    Magic,
}
public static class DataTableIds
{
    public static readonly string String = "StringTable";
    public static readonly string Card = "CardTable";
    public static readonly string Unit = "UnitTable";
    public static readonly string Magic = "MagicTable";
    public static readonly string Stage = "StageTable";
}

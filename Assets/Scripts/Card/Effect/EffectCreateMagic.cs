using UnityEngine;

// "무작위 마법카드를 획득" 효과. MagicTable에서 ID를 뽑아 MagicManager에 넘긴다.
public class EffectCreateMagic : EffectCardBase
{
    public override void UseEffect()
    {
        if (MagicManager.Instance == null)
        {
            Debug.LogWarning("씬에 MagicManager가 없음");
            return;
        }

        string id = DataTableManager.MagicTable?.GetRandomId();
        if (string.IsNullOrEmpty(id)) return;

        MagicManager.Instance.AddMagic(id);
    }
}

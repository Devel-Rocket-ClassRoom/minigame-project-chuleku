using System.Collections.Generic;
using UnityEngine;

// 마법탭 전용 매니저. 마법은 손패/덱/묘지 사이클에 끼지 않고 이 매니저에서만 관리된다 (GDD 4.5).
// 상한 5장, 라운드 간 이월(EndRound에서 아무것도 안 함), 페이즈 필터를 모두 여기서 처리.
public class MagicManager : MonoBehaviour
{
    public const int MagicMax = 5;

    [SerializeField] private Transform magicPanelContent; // 씬의 마법탭 컨테이너

    public static MagicManager Instance { get; private set; }

    private readonly List<MagicInstance> magicDeck = new();
    private int nextInstanceId = 1;

    public IReadOnlyList<MagicInstance> MagicDeck => magicDeck;
    public int Count => magicDeck.Count;
    public bool IsFull => magicDeck.Count >= MagicMax;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 효과 카드 등 외부에서 호출. 상한 차면 폐기.
    public bool AddMagic(string magicId)
    {
        if (IsFull)
        {
            Debug.Log($"마법탭 상한({MagicMax}) 도달, '{magicId}' 폐기");
            return false;
        }

        var data = DataTableManager.MagicTable?.Get(magicId);
        if (data == null) { Debug.LogWarning($"MagicTable에 '{magicId}' 없음"); return false; }

        var prefab = Resources.Load<GameObject>(StripResourcesPrefix(data.Prefab));
        if (prefab == null) { Debug.LogWarning($"마법 프리팹 로드 실패: '{data.Prefab}'"); return false; }

        var parent = magicPanelContent != null ? magicPanelContent : transform;
        var go = Instantiate(prefab, parent);
        int id = nextInstanceId++;

        var magic = go.GetComponent<MagicBase>();
        if (magic != null) magic.SetInstanceId(id);

        magicDeck.Add(new MagicInstance(id, magicId, go));
        return true;
    }

    // 마법 GameObject가 발동 직후 호출. 리스트에서 빼고 View 정리.
    public void UseMagic(int instanceId)
    {
        int idx = magicDeck.FindIndex(m => m.InstanceId == instanceId);
        if (idx < 0) return;
        var inst = magicDeck[idx];
        magicDeck.RemoveAt(idx);
        if (inst.View != null) Destroy(inst.View);
    }

    private static string StripResourcesPrefix(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;
        const string prefix = "Resources/";
        return key.StartsWith(prefix) ? key.Substring(prefix.Length) : key;
    }
}

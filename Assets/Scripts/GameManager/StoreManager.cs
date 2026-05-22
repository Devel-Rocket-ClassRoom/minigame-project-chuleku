using System.Linq.Expressions;
using Unity.AppUI.UI;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance {get;private set;}
    public Button[] normalStoreSlot;
    public Button[] rotateStoreSlot;
    public GameObject unitInfo;
    public GameObject effectInfo;
    public GameObject resourceInfo;

    void Awake()
    {
        Instance = this;
    }


    public void StoreClickedInfo(string cardId)
    {
        var data = DataTableManager.CardTable.Get(cardId);
        AllClearInfo();
        switch(data.Type)
        {
            case CardType.Unit:
            unitInfo.SetActive(true);
            break;
        }   
    }
    public void AllClearInfo()
    {
        unitInfo.SetActive(false);
        effectInfo.SetActive(false);
        resourceInfo.SetActive(false);
    }
}

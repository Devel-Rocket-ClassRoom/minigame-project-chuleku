using GLTFast.Schema;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class InfoUi : MonoBehaviour
{
    public string saveCardId;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoDesc;
    public TextMeshProUGUI infoCost;
    public TextMeshProUGUI infoState;
    public Image infoImage;

    public void ViewInfo(string cardId)
    {
        saveCardId= cardId;
        var data = DataTableManager.CardTable?.Get(cardId);
        if(data==null) return;
        infoName.text = data.Name;
        infoCost.text = $"가격 : {data.Cost}";
        infoDesc.text = data.Desc;
        switch(data.Type)
        {
            case CardType.Unit:
            var unit = DataTableManager.UnitTable.Get(data.Id);
            if (data.UseAble)
            {
                infoState.text = $"공격력 : {unit.Attack}\n사거리 : {unit.Range}\n유닛 타입 : 유닛,효과 ";
            }
            else
            {
                infoState.text = $"공격력 : {unit.Attack}\n사거리 : {unit.Range}\n유닛 타입 : 유닛";
            }
            break;
            case CardType.Effect:
            break;
            case CardType.Resource:
            break;
        }
    }
}

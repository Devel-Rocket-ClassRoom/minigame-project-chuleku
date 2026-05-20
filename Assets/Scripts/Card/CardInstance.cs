// 카드 1장 = 인스턴스 1개의 1:1 대응을 위한 데이터 컨테이너.
// 같은 종류(CardId) 카드 여러 장도 InstanceId로 구별된다.
public class CardInstance
{
    public int InstanceId;
    public string CardId;

    public CardInstance(int instanceId, string cardId)
    {
        InstanceId = instanceId;
        CardId = cardId;
    }
}

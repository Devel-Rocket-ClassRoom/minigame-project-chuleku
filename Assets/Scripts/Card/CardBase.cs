using UnityEngine;
public enum CardType
{
    Unit,
    Effect,
    Resourse,
    Magic,
}
public abstract class CardBase : MonoBehaviour
{
    [SerializeField] protected int price;
    [SerializeField] protected int cost;
    [SerializeField] protected CardType cardType;
    public CardType GetCardType() => cardType;
    public int GetCost() => cost;

}

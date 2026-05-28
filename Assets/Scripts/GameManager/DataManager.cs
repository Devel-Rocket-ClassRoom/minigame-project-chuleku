using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    [SerializeField] private SpriteDatabase spriteDB;
    public SpriteDatabase SpriteDB => spriteDB;

    void Awake() { Instance = this; }
}

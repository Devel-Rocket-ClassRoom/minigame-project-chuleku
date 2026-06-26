using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;

    public static AuthManager Instance => instance;

    void Awake()
    {
        if (instance != null || instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    
}

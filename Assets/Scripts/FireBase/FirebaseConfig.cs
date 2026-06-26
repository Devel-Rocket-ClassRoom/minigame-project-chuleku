using UnityEngine;
using Firebase;
[CreateAssetMenu(fileName = "FirebaseConfig", menuName = "Firebase Study/Firebase Config")]
public class FirebaseConfig : ScriptableObject
{
    public string apiKey;
    public string appId;         
    public string projectId;   
    public string databaseUrl;  
    public string storageBucket; 

    public bool IsValid => !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(databaseUrl);
    
}

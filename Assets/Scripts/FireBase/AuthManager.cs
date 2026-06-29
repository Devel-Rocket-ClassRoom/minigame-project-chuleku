using UnityEngine;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using System;
using Unity.Collections;
using System.Diagnostics.CodeAnalysis;
public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;

    public static AuthManager Instance => instance;
	private FirebaseAuth m_Auth;
	private FirebaseUser m_CurrentUser;
    public string CurrentUserUid => m_CurrentUser?.UserId;
    public bool IsLoggedIn => m_CurrentUser != null;
    public void Initialize(FirebaseAuth auth)
    {
        m_Auth = auth;
        m_CurrentUser = m_Auth.CurrentUser;
        if(m_CurrentUser!=null)
        {
            Debug.Log($"[Auth] 이미 로그인된 상태 입니다.");
     
        }
        else
        {
            Debug.Log($"[Auth] 로그인이 필요합니다.");
        }
        NotifyLoginState();
    }

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
    public async UniTask<(bool success, string error)> SignUp(string email, string password) {
	try {
		Debug.Log($"[Auth] 이메일 회원가입 시도");
	
		AuthResult result = await m_Auth.CreateUserWithEmailAndPasswordAsync(email, password);
		m_CurrentUser = result.User;
		NotifyLoginState();
		Debug.Log($"[Auth] 이메일 회원가입 성공");
		return (true, null);	
	} catch (Exception ex) 
    {
		Debug.Log($"[Auth] 이메일 회원가입 실패 : {ex.Message}");
        string errorMessage = "이메일 또는 비밀번호가 맞지않습니다.";
		return (false,errorMessage);
	}
	}
    public async UniTask<(bool success,string error)> SignIn()
    {
        try
        {
            Debug.Log($"[Auth] 게스트 로그인 시도");
            AuthResult result = await m_Auth.SignInAnonymouslyAsync();
            m_CurrentUser = result.User;
            NotifyLoginState();
            Debug.Log("[Auth] 게스트 로그인 성공");
            return(true,null);
        }
        catch(Exception ex)
        {
            Debug.Log($"[Auth] 게스트 로그인 실패 : {ex.Message}");
            string errorMessage = "이메일 또는 비밀번호가 맞지않습니다.";
            return(false,errorMessage);
        }
    }
    public async UniTask<(bool success, string error)> SignIn(string email, string password) {
		try {
			Debug.Log($"[Auth] 이메일 로그인 시도");
			AuthResult result = await m_Auth.SignInWithEmailAndPasswordAsync(email, password);
		    m_CurrentUser = result.User;
			NotifyLoginState();
			Debug.Log($"[Auth] 이메일 로그인 성공");
			return (true, null);	
		} catch (Exception ex) 
        {
			Debug.Log($"[Auth] 이메일 로그인 실패 : {ex.Message}");
            string errorMessage = "이메일 또는 비밀번호가 맞지않습니다.";
			return (false, errorMessage);
		}
	}
    public async UniTask<(bool success,string error)> SignOut()
    {
        if(m_Auth==null)
        {
            m_CurrentUser =null;
            NotifyLoginState();
            return(false,"Firebase 인증이 준비되지 않았습니다");
        }
        try
        {
            Debug.Log("[Auth] 로그아웃 시도");
            m_Auth.SignOut();
            m_CurrentUser = m_Auth.CurrentUser;
            NotifyLoginState();
            if(m_CurrentUser==null)
            Debug.Log("[Auth] 로그아웃 성공");
            if(m_CurrentUser ==null)
            {
                return(true,null);
            }
            else
            {
                return(true,"로그아웃 후에도 로그인세션이 남아있습니다.");
            }
        }
        catch(Exception ex)
        {
            Debug.LogError($"[Auth] 로그아웃 실패 : {ex.Message}");
            m_CurrentUser = m_Auth.CurrentUser;
            NotifyLoginState();
            return(false,"로그아웃 처리 실패");
        }
        
    }
    public void NotifyLoginState()
    {
        if(m_CurrentUser==null)
        {
            Debug.Log($"[Auth] 로그아웃 상태");
            return;
        }
        Debug.Log($"[Auth] 로그인 상태 : {(m_CurrentUser.IsAnonymous ? "게스트 계정" : "이메일 계정")}");
    }
    
}

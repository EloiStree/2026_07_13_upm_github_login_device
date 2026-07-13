using UnityEngine;
using UnityEngine.Events;

public class LogDeviceMono_SaveLoadReadUserToken : MonoBehaviour
{
    
    public UnityEvent<string> m_onTokenSaved;
    public UnityEvent<string> m_onTokenLoaded;
    public UnityEvent m_onNotTokenLoaded;

    public string m_playerPrefsUniqueId = "github_read_user_token";

    public void SaveToken(string token)
    {
        PlayerPrefs.SetString(m_playerPrefsUniqueId, token);
        PlayerPrefs.Save();
        m_onTokenSaved?.Invoke(token);
    }
    public void LoadToken()
    {
        string token = PlayerPrefs.GetString(m_playerPrefsUniqueId, "");
        if (string.IsNullOrEmpty(token))
        {
            m_onTokenLoaded?.Invoke(token);
        }
        else 
        {
            m_onTokenLoaded?.Invoke(token);
        }
    }

    private void OnEnable()
    {
        LoadToken();
    }
}

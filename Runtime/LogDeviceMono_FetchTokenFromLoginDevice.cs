using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LogDeviceMono_FetchTokenFromLoginDevice : MonoBehaviour
{
    private const string DEVICE_CODE_URL = "https://github.com/login/device/code";
    private const string TOKEN_URL = "https://github.com/login/oauth/access_token";


    [SerializeField]string m_clientIdCode = "Iv23liTeQ7kcDWDgXOTO";
    public bool m_fetchTokenOnStart = true;

    [SerializeField]string m_deviceCodeToGiveToGitHub = "";
    public UnityEvent<string> m_onDeviceCodeGenerated;
    public UnityEvent<string> m_onTokenCodeGenerated;

    public UnityEvent<string> m_onErrorDuringProcess;

    private string accessToken = "";
    private string deviceCode = "";
    private int pollInterval = 5;
    private float expiresIn = 900f;

  


    public TokenResponseJson m_tokenResponse;
    public DeviceCodeResponseJson m_deviceCodeResponse;

    public bool m_openUrlWhenDeviceCodeIsReady = true;
    public void Start()
    {
        if (m_fetchTokenOnStart)
            LaunchCoroutineToFetchToken();
    }


    public void OpenUrlPage()
    {
        Application.OpenURL("https://github.com/login/device/code");
    }

    public void OpenUrlPageWithDeviceCode(string code) { 
    
        string url = "https://github.com/login/device";
        Application.OpenURL(url + "?q=" + code);
    }

    [ContextMenu("Launch Coroutine to fetch token")]
    public void LaunchCoroutineToFetchToken()
    {
        StartCoroutine(RequestDeviceCode());
    }

    private IEnumerator RequestDeviceCode()
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", m_clientIdCode);
        form.AddField("scope", "read:user");           // Minimal scope - only user info

        using (UnityWebRequest req = UnityWebRequest.Post(DEVICE_CODE_URL, form))
        {
            req.SetRequestHeader("Accept", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                m_onErrorDuringProcess?.Invoke("Failed to get device code: " + req.error);
                yield break;
            }

            DeviceCodeResponseJson resp = JsonUtility.FromJson<DeviceCodeResponseJson>(req.downloadHandler.text);
            m_deviceCodeToGiveToGitHub= resp.user_code;
            m_onDeviceCodeGenerated.Invoke(resp.user_code);
            if (m_openUrlWhenDeviceCodeIsReady)
            {
                OpenUrlPageWithDeviceCode(resp.user_code);
            }
            deviceCode = resp.device_code;
            pollInterval = resp.interval;
            expiresIn = resp.expires_in;
            m_deviceCodeResponse = resp;

            StartCoroutine(PollForToken());
        }
    }

    private IEnumerator PollForToken()
    {
        float timeElapsed = 0f;

        while (timeElapsed < expiresIn)
        {
            WWWForm form = new WWWForm();
            form.AddField("client_id", m_clientIdCode);
            form.AddField("device_code", deviceCode);
            form.AddField("grant_type", "urn:ietf:params:oauth:grant-type:device_code");

            using (UnityWebRequest req = UnityWebRequest.Post(TOKEN_URL, form))
            {
                req.SetRequestHeader("Accept", "application/json");
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    TokenResponseJson tokenResp = JsonUtility.FromJson<TokenResponseJson>(req.downloadHandler.text);
                    m_tokenResponse = tokenResp;

                    if (!string.IsNullOrEmpty(tokenResp.access_token))
                    {
                        accessToken = tokenResp.access_token;
                        m_onTokenCodeGenerated.Invoke(accessToken);
                        yield break;
                    }
                }
                else
                {
                    ErrorResponseJson err = JsonUtility.FromJson<ErrorResponseJson>(req.downloadHandler.text);

                    if (err.error == "slow_down")
                    {
                        pollInterval += 5;
                    }
                    else if (err.error == "expired_token" || err.error == "access_denied")
                    {
                        m_onErrorDuringProcess?.Invoke("Login failed: " + err.error);
                        yield break;
                    }
                    // authorization_pending = normal, keep polling
                }
            }

            yield return new WaitForSeconds(pollInterval);
            timeElapsed += pollInterval;
        }

        m_onErrorDuringProcess?.Invoke("Login timed out.");
    }


    // ====================== DATA CLASSES ======================
    [Serializable]
    public class DeviceCodeResponseJson
    {
        public string device_code;
        public string user_code;
        public string verification_uri;
        public int expires_in;
        public int interval;
    }

    [Serializable]
    public class TokenResponseJson
    {
        public string access_token;
        public string token_type;
        public string scope;
    }

    [Serializable]
    public class ErrorResponseJson
    {
        public string error;
        public string error_description;
    }

}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LogDeviceMono_DemoLoginRobot : MonoBehaviour
{
    private const string CLIENT_ID = "Iv23liTeQ7kcDWDgXOTO";
    private const string DEVICE_CODE_URL = "https://github.com/login/device/code";
    private const string TOKEN_URL = "https://github.com/login/oauth/access_token";

    private string accessToken = "";
    private string deviceCode = "";
    private int pollInterval = 5;
    private float expiresIn = 900f;

    [Header("Events")]
    public System.Action<string> OnUserCodeReady;     // user_code + verification_uri
    public System.Action<string> OnLoginSuccess;      // access token
    public System.Action<string> OnError;


    public UserInfo m_userInfo;
    public TokenResponse m_tokenResponse;
    public DeviceCodeResponse m_deviceCodeResponse;


    public void StartDeviceLogin()
    {
        StartCoroutine(RequestDeviceCode());
    }

    private IEnumerator RequestDeviceCode()
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", CLIENT_ID);
        form.AddField("scope", "read:user");           // Minimal scope - only user info

        using (UnityWebRequest req = UnityWebRequest.Post(DEVICE_CODE_URL, form))
        {
            req.SetRequestHeader("Accept", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                OnError?.Invoke("Failed to get device code: " + req.error);
                yield break;
            }

            DeviceCodeResponse resp = JsonUtility.FromJson<DeviceCodeResponse>(req.downloadHandler.text);

            deviceCode = resp.device_code;
            pollInterval = resp.interval;
            expiresIn = resp.expires_in;
            m_deviceCodeResponse= resp;
            OnUserCodeReady?.Invoke($"Go to:\n{resp.verification_uri}\n\nand enter code:\n{resp.user_code}");

            StartCoroutine(PollForToken());
        }
    }

    private IEnumerator PollForToken()
    {
        float timeElapsed = 0f;

        while (timeElapsed < expiresIn)
        {
            WWWForm form = new WWWForm();
            form.AddField("client_id", CLIENT_ID);
            form.AddField("device_code", deviceCode);
            form.AddField("grant_type", "urn:ietf:params:oauth:grant-type:device_code");

            using (UnityWebRequest req = UnityWebRequest.Post(TOKEN_URL, form))
            {
                req.SetRequestHeader("Accept", "application/json");
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    TokenResponse tokenResp = JsonUtility.FromJson<TokenResponse>(req.downloadHandler.text);
                    m_tokenResponse= tokenResp;

                    if (!string.IsNullOrEmpty(tokenResp.access_token))
                    {
                        accessToken = tokenResp.access_token;
                        OnLoginSuccess?.Invoke(accessToken);
                        yield break;
                    }
                }
                else
                {
                    ErrorResponse err = JsonUtility.FromJson<ErrorResponse>(req.downloadHandler.text);

                    if (err.error == "slow_down")
                    {
                        pollInterval += 5;
                    }
                    else if (err.error == "expired_token" || err.error == "access_denied")
                    {
                        OnError?.Invoke("Login failed: " + err.error);
                        yield break;
                    }
                    // authorization_pending = normal, keep polling
                }
            }

            yield return new WaitForSeconds(pollInterval);
            timeElapsed += pollInterval;
        }

        OnError?.Invoke("Login timed out.");
    }

    // ====================== GET USER INFO ======================
    public IEnumerator GetUserInfo(Action<UserInfo> onSuccess)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            OnError?.Invoke("No access token yet.");
            yield break;
        }

        using (UnityWebRequest req = UnityWebRequest.Get("https://api.github.com/user"))
        {
            req.SetRequestHeader("Authorization", "Bearer " + accessToken);
            req.SetRequestHeader("User-Agent", "UnityGitHubLogin/1.0");
            req.SetRequestHeader("Accept", "application/vnd.github.v3+json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                UserInfo user = JsonUtility.FromJson<UserInfo>(req.downloadHandler.text);
                m_userInfo= user;   
                onSuccess?.Invoke(user);
            }
            else
            {
                OnError?.Invoke("Failed to get user info: " + req.error);
            }
        }
    }

    // ====================== DATA CLASSES ======================
    [Serializable]
    public class DeviceCodeResponse
    {
        public string device_code;
        public string user_code;
        public string verification_uri;
        public int expires_in;
        public int interval;
    }

    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string token_type;
        public string scope;
    }

    [Serializable]
    public class ErrorResponse
    {
        public string error;
        public string error_description;
    }

    [Serializable]
    public class UserInfo
    {
        public string login;
        public string name;
        public string avatar_url;
        public string email;
        public string bio;
        public string company;
        public string location;
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LogDeviceMono_ValidateTokenByFetchingInfo : MonoBehaviour { 

    [SerializeField] string m_token= "ghu_wlnwIjsMczrZQI33KIYlwWpbZ4lETi2qxGEo";
    [Header("Events")]
    public UnityEvent<string> m_onStatusChanged;
    public UnityEvent<string> m_onUserInfoReceived;
    public UnityEvent<Texture2D> m_onAvatarReceived;


    public Texture2D m_avatarTexture;
    [TextArea(3, 10)]
    public string m_jsonReceived;
    public GitHubUserJson m_userInfoReceived;

    public bool m_checkInspectorTokenAtStart = false;


    //[ContextMenu("Check token in web page")]
    //public void OpenUrlWithTokenInformation() { 
    //    string url = "https://eloistree.github.io/SignText/GitHub/LoginDevice/CheckToken/index.html?q="+ m_token;
    //    Application.OpenURL(url);
    //}

    void Start()
    {
 
        if (m_checkInspectorTokenAtStart)
            StartCoroutine(CheckToken());
    }

    public void SetTokenAndLoad(string token) { 
    
        this.m_token = token;
        StartCoroutine(CheckToken());
    }


    IEnumerator CheckToken()
    {
        UnityWebRequest request =
            UnityWebRequest.Get("https://api.github.com/user");

        request.SetRequestHeader(
            "Authorization",
            "Bearer " + m_token
        );

        request.SetRequestHeader(
            "Accept",
            "application/vnd.github+json"
        );


        yield return request.SendWebRequest();


        if (request.result != UnityWebRequest.Result.Success)
        {
            m_onStatusChanged?.Invoke(
                "❌ Invalid token\n" +
                request.error
            );

            yield break;
        }

        string text = request.downloadHandler.text;
        m_jsonReceived = text;
        GitHubUserJson user =
            JsonUtility.FromJson<GitHubUserJson>(
                text
            );

        m_onStatusChanged?.Invoke(
            "✅ Valid token"
        );


        string info =
            $"Login: {user.login}\n" +
            $"Name: {user.name}\n" +
            $"ID: {user.id}\n" +
            $"Followers: {user.followers}\n" +
            $"Following: {user.following}\n" +
            $"Public repos: {user.public_repos}\n" +
            $"Profile: {user.html_url}";


        m_userInfoReceived = user;
        m_onUserInfoReceived?.Invoke(info);


        if (!string.IsNullOrEmpty(user.avatar_url))
        {
            StartCoroutine(
                DownloadAvatar(user.avatar_url)
            );
        }
    }


    IEnumerator DownloadAvatar(string url)
    {
        UnityWebRequest request =
            UnityWebRequestTexture.GetTexture(url);


        yield return request.SendWebRequest();


        if (request.result != UnityWebRequest.Result.Success)
        {
            m_onStatusChanged?.Invoke(
                "⚠ Avatar download failed: " +
                request.error
            );

            yield break;
        }


        Texture2D texture =
            DownloadHandlerTexture.GetContent(request);
        m_avatarTexture = texture;

        m_onAvatarReceived?.Invoke(texture);
    }

    string GetQueryParameter(string key)
    {
        string url = Application.absoluteURL;

        if (string.IsNullOrEmpty(url))
            return null;


        Uri uri = new Uri(url);

        foreach (string part in uri.Query.TrimStart('?').Split('&'))
        {
            string[] pair = part.Split('=');

            if (pair.Length == 2 &&
                pair[0] == key)
            {
                return Uri.UnescapeDataString(pair[1]);
            }
        }

        return null;
    }
}


[Serializable]
public class GitHubUserJson
{
    public string login;
    public int id;
    public string node_id;

    public string avatar_url;
    public string gravatar_id;

    public string url;
    public string html_url;

    public string followers_url;
    public string following_url;
    public string gists_url;
    public string starred_url;
    public string subscriptions_url;
    public string organizations_url;
    public string repos_url;
    public string events_url;
    public string received_events_url;

    public string type;
    public string user_view_type;

    public bool site_admin;

    public string name;
    public string company;
    public string blog;
    public string location;

    public string email;
    public bool hireable;

    public string bio;
    public string twitter_username;
    public string notification_email;

    public int public_repos;
    public int public_gists;

    public int followers;
    public int following;

    public string created_at;
    public string updated_at;
}


/*
 {
  "login": "EloiStree",
  "id": 20149493,
  "node_id": "MDQ6VXNlcjIwMTQ5NDkz",
  "avatar_url": "https://avatars.githubusercontent.com/u/20149493?v=4",
  "gravatar_id": "",
  "url": "https://api.github.com/users/EloiStree",
  "html_url": "https://github.com/EloiStree",
  "followers_url": "https://api.github.com/users/EloiStree/followers",
  "following_url": "https://api.github.com/users/EloiStree/following{/other_user}",
  "gists_url": "https://api.github.com/users/EloiStree/gists{/gist_id}",
  "starred_url": "https://api.github.com/users/EloiStree/starred{/owner}{/repo}",
  "subscriptions_url": "https://api.github.com/users/EloiStree/subscriptions",
  "organizations_url": "https://api.github.com/users/EloiStree/orgs",
  "repos_url": "https://api.github.com/users/EloiStree/repos",
  "events_url": "https://api.github.com/users/EloiStree/events{/privacy}",
  "received_events_url": "https://api.github.com/users/EloiStree/received_events",
  "type": "User",
  "user_view_type": "public",
  "site_admin": false,
  "name": "Éloi Strée",
  "company": "EloiStree",
  "blog": "https://github.com/eloistree",
  "location": "Belgium",
  "email": null,
  "hireable": null,
  "bio": "Fuck the Rules ! VR & AR, Unity 3D, New-tech, R&D. We want to know if it is possible. Grab some ☕ and 🍺 and let's try to code it.  ",
  "twitter_username": null,
  "notification_email": null,
  "public_repos": 1174,
  "public_gists": 29,
  "followers": 113,
  "following": 22,
  "created_at": "2016-06-26T07:48:16Z",
  "updated_at": "2026-07-13T09:49:42Z"
}

*/
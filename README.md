
```
git submodule add https://github.com/EloiStree/2026_07_13_upm_github_login_device.git Packages/be.elab.logindevice
```

# 2026_07_13_upm_github_login_device

> Use Login Device user:read in Unity


2026_07_13_upm_github_login_device  https://github.com/login/device/select_account


Unity: https://github.com/EloiStree/2026_07_13_upm_github_login_device   
Godot: https://github.com/EloiStree/2026_07_13_gdp_github_login_device  


Once the user has obtained a GitHub token, they must still prove their identity to the server.
To do this, the client sends the token to the server, which retrieves the user's information from GitHub and verifies that the token is valid.

To protect this exchange, the token should be secured using cryptographic techniques, such as hashing with a salt or another appropriate cryptographic mechanism, depending on your authentication design.

For more information, see:
https://chatgpt.com/share/6a54dbe2-e084-83eb-9c32-bd0990d85ab9

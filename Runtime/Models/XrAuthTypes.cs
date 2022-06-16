using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class LoginRequest
{
    [JsonProperty("email")]
    public string Email;
    [JsonProperty("password")]
    public string Password;
    //public bool RememberMe;
}

public class LoginResponse
{
    [JsonProperty("token")]
    public string Token;
}

public class GetUserResponse
{
    [JsonProperty("status")]
    public string Status;

    [JsonProperty("result")]
    public SturfeeUser Result;

    [JsonProperty("messages")]
    public List<string> Messages;
}

public class XrcsAccountModel
{
    public Guid Id;
    public Guid OwnerId;
    public string Name;
    public DateTime CreatedDate;
}

public class AddUserToAccountRequest
{
    public Guid AccountId;
    public Guid UserId;
    public Guid RequestByUserId;
    public string Role;
}

public class XrcsUserModel
{
    public Guid Id;

    public List<XrAccountUserModel> AccountUsers;

    public string Name;
    public string Email;
    public string AvatarUrl;
    public string Password;

    public bool IsDemoUser;

    public DateTime CreatedDate;
    public DateTime ModifiedDate;
}

public enum AccountRole
{
    Admin, // can do anything (add/remove users)
    Editor, // can create/edit Projects, Scenes, Assets
    Viewer // can on view Projects/Scenes
}

public class XrAccountUserModel
{
    public Guid Id;
    public Guid AccountId;
    public Guid UserId;
    public AccountRole Role;
}

public class SturfeeUser
{
    [JsonProperty("uuid")]
    public Guid Uuid;
    [JsonProperty("email")]
    public string Email;
    [JsonProperty("fullName")]
    public string FullName;
    [JsonProperty("organization")]
    public string Organization;
    [JsonProperty("username")]
    public string Username;
    [JsonProperty("roles")]
    public List<string> Roles;
}

public class AuthenticationResponse
{
    public XrcsUserModel User;
    public string Token;
}
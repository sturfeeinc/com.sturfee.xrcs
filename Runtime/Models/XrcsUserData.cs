using System;
using System.Collections.Generic;

public class XrcsUserData
{
    public Guid Id;
    public Guid? XrcsId;
    public Guid GlobalId;
    public Guid AccountId;
    public string RefId;

    public List<Guid> AccountIds;
    public List<Guid> ProjectIds;

    public List<XrAccountUserModel> AccountUsers;

    public string Name;
    public string Email;
    public string AvatarUrl;
    public string Password;

    public bool IsDemoUser;

    public string Token;
}

public class CachedXrcsUserData : XrcsUserData
{
    public DateTime LoginDate;
}

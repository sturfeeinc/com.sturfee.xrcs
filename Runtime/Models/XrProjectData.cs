using System;
using System.Collections.Generic;

[Serializable]
public enum ProjectPrivacy
{
    Private,
    Public,
    Unlisted
}

[Serializable]
public class XrProjectData
{
    public Guid Id;
    //public Guid ProjectId;
    public Guid AccountId;
    public Guid UserId;
    public XrcsUserData User;

    public DateTime CreatedDate;
    public DateTime ModifiedDate;
    public DateTime PublishedDate;
    public DateTime AssetsModifiedDate;

    public bool IsPublished;
    public bool IsPublic;
    public ProjectPrivacy Privacy;

    public XrGeoLocationData Location;

    // data
    public string Name;
    public List<Guid> SceneIds;
    public List<Guid> AssetIds;
    public List<Guid> ProjectAssetIds;
    public string DataUrl;

    public XrProjectData()
    {
        SceneIds = new List<Guid>();
        AssetIds = new List<Guid>();
        ProjectAssetIds = new List<Guid>();
    }
}

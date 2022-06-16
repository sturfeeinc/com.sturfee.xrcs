using System;

[Serializable]
public class XrSceneAssetData
{
    public Guid Id;
    public Guid SceneId;
    public Guid ProjectAssetId;
    public XrProjectAssetData ProjectAsset { get; set; }
    public Guid UserId;

    public string RefId;

    public DateTime CreatedDate;
    public DateTime ModifiedDate;
    public DateTime PublishedDate;

    public bool IsPublished;
    public bool IsPublic;

    // data
    public string Name;
    public XrGeoLocationData Location;
    public XrTransform Transform;

    public XrSceneAssetData()
    {
        Location = new XrGeoLocationData();
        Transform = new XrTransform();
    }
}

using System;

[Serializable]
public class DtEnhancementAssetData // instance of DTAsset
{
    public Guid Id;
    public Guid DtEnhancementId;    
    public Guid OwnerId;

    public Guid DtAssetId; // link to the DT Asset that this came from
    public DtAssetData DtAsset; // link to the DT Asset that this came from

    public DateTime CreatedDate;
    public DateTime ModifiedDate;
    public DateTime PublishedDate;

    public bool IsPublished;
    public bool IsPublic;

    // data
    public string Name;
    public XrGeoLocationData Location;
    public XrTransform Transform;
}

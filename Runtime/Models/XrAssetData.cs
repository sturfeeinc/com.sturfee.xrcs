using System;

[Serializable]
public class XrAssetData
{
    public Guid Id;
    public Guid AssetId;
    public Guid UserId;

    public string RefId;

    public DateTime CreatedDate;
    public DateTime ModifiedDate;
    public DateTime PublishedDate;

    public bool IsPublished;
    public bool IsPublic;

    public string Name;
    public string Description;
    public string Credits;
    public string Keywords;

    public string DataUrl; // for AssetBundle path

    public XrAssetDataType Type;

    public XrAssetStatus Status;

    public string OriginalFileName;

    public string AuthorName;

    public long FileSizeBytes;

    //public XrGeoLocationData Location;
    //public Transform Transform;

}

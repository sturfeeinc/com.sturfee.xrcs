using Newtonsoft.Json;
using System;

[Serializable]
public enum XrAssetDataType
{
    Zip,
    AssetBundle,
    AssetTemplate
}

[Serializable]
public enum XrAssetStatus
{
    None,
    Queued,
    Uploading,
    Uploaded,
    Error,
    Warning
}

[Serializable]
public enum TemplateAssetType
{
    None,
    Image,
    Billboard,
    SpawnPoint,
    WebView,

    Unknown = 999
}

[Serializable]
public class XrProjectAssetData
{
    public Guid Id;
    public Guid ProjectId;
    public Guid? AssetId; // just serves as a historical link to the Asset that this came from
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
    public string StatusMessage;

    //public TemplateAssetType TemplateType;
    public string TemplateData; // json string representing data for Template Assets

    [JsonProperty("TemplateType")]
    public string TemplateTypeId;

    [JsonIgnore]
    public TemplateAssetType TemplateType
    {
        get
        {
            TemplateAssetType outputValue = TemplateAssetType.Unknown;
            Enum.TryParse(TemplateTypeId, out outputValue);
            return outputValue;
        }
        //set
        //{
        //    TemplateTypeId = $"{value}";
        //}
    }

    //public XrGeoLocationData Location;
    //public Transform Transform;    
}

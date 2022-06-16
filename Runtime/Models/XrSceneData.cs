using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class XrSceneData
{
    public Guid Id;
    public Guid ProjectId;
    public XrProjectData Project;
    public Guid UserId;
    public XrcsUserData User;
    public DateTime CreatedDate;
    public DateTime ModifiedDate;
    public DateTime PublishedDate;
    public bool IsPublished;
    public bool IsPublic;

    public string Name;
    public XrGeoLocationData Location;

    // data
    public List<Guid> SceneAssetIds;
    public List<XrSceneAssetData> SceneAssets;

    public string DataUrl;

    public List<Guid> DtEnhancementIds;
    public List<DtEnhancementData> DtEnhancements;    
    

    public XrSceneData()
    {
        SceneAssetIds = new List<Guid>();
        SceneAssets = new List<XrSceneAssetData>();

        DtEnhancementIds = new List<Guid>();
        DtEnhancements = new List<DtEnhancementData>();
    }
}

//[Serializable]
//public class SharedSpaceData : XrSceneData
//{
//    public XrcsUserData User;
//}

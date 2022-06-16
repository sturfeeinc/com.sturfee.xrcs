using System;
using System.Collections.Generic;

[Serializable]
public class DtEnhancementData
{
    public Guid Id;  
    public Guid OwnerId;

    public DateTime CreatedDate;
    public DateTime ModifiedDate;

    public bool IsPublic;

    // data
    public string Name;
    public string Geohash;
    public XrGeoLocationData Location;

    public List<DtEnhancementAssetData> Assets;

    //public List<DtHiddenFeatureData> HiddenFeatures;
    public List<string> HiddenFeatures;
    //public List<XrSceneData> Scenes;


    public DtEnhancementData()
    {
        Assets = new List<DtEnhancementAssetData>();
        //HiddenFeatures = new List<DtHiddenFeatureData>();
        HiddenFeatures = new List<string>();
        //Scenes = new List<XrSceneData>();
    }
}

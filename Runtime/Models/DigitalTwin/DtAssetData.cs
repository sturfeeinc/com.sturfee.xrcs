using System;

[Serializable]
public enum FeatureLayer
{
    None,

    Tree,
    Road,
    Freeway,
    Lampost,
    TrainTrack,
    Sidewalk,
    Bridge,
    Greenspace,
    Concrete,
    Water,
    Building,

    LargeBridge,
    Terrain,

    Bushes,
    RoadMarking,
    Signage,
    Car,
    Misc
}

[Serializable]
public enum PropType
{
    None,
    Tree,
    Plant,
    Car,
    Streetlight,
    Bench,
    Sign,
    Statue,
    Other
}

[Serializable]
public enum CollisionType
{
    None,
    Player
}

[Serializable]
public class DtAssetData
{
    public Guid Id;
    public Guid OwnerId;
    public Guid AccountId;

    public FeatureLayer LayerType;
    public PropType PropType;
    public CollisionType CollisionType;

    public bool IsPublic;
    public XrAssetStatus Status;
    public string StatusMessage;

    public string Name;
    public string Tags;
    public string OriginalFileName;

    public DateTime CreatedDate;
    public DateTime ModifiedDate;
}

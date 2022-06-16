using System;
using System.Collections.Generic;

[Serializable]
public class XrLibraryData
{
    public Guid Id;
    public Guid UserId;
    public DateTime CreatedDate;
    public DateTime ModifiedDate;
    public DateTime PublishedDate;

    public bool IsPublished;
    public bool IsPublic;

    // data
    public string Name;
    public string Description;
    public string ThumbnailUrl; // png file
    public List<Guid> AssetIds;
}

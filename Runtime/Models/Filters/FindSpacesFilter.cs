using System;
using System.Collections.Generic;

public enum SortSpacesOption
{
    None,
    Date,
    Location
}

public enum SharedSpaceTag
{
    None,
    Featured,
    Favorite
}

[Serializable]
public class FindSpacesFilter
{
    // filter options

    public bool IsPublic { get; set; } = true;
    public ProjectPrivacy Privacy { get; set; } = ProjectPrivacy.Public;

    public List<Guid> Ids { get; set; }
    public List<Guid> UserIds { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public XrGeoLocationData Location { get; set; }
    public float Radius { get; set; }

    // sort options
    public SortSpacesOption SortOption { get; set; }

    public SharedSpaceTag Tag { get; set; }
}


using System;
using System.Collections.Generic;


[Serializable]
public class DtEnhancementFilter
{
    public bool IsPublic { get; set; } = true;
    public List<Guid> Ids { get; set; }
    public List<Guid> UserIds { get; set; }
    public List<string> Geohashes { get; set; }
}


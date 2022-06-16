using System;
using System.Collections.Generic;


[Serializable]
public class DtEnhancementAssetFilter
{
    public bool IsPublic { get; set; }
    public List<Guid> Ids { get; set; }
    public List<Guid> UserIds { get; set; }
    public List<Guid> EnhancementIds { get; set; }
}


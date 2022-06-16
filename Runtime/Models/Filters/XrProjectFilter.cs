using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Serializable]
public class XrProjectFilter
{
    public List<Guid> AccountIds { get; set; }
    public List<Guid> ProjectIds { get; set; }
    public XrGeoLocationData Location { get; set; }
    public float Radius { get; set; }
}


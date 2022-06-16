using System;
using UnityEngine;

[Serializable]
public class XrPointCloudData
{
    public string Id;
    public XrGeoLocationData Location;
    public XrVector3 Shift;
    public XrVector3 Size;
    public DateTime CreatedDate;
}

public class XrVector3
{
    public float X;
    public float Y;
    public float Z;
}

public class XrQuaternion
{
    public float W;
    public float X;
    public float Y;
    public float Z;
}

public class XrTransform
{
    public XrGeoLocationData Location;
    public XrVector3 Position;
    public XrVector3 Rotation;
    public XrQuaternion Orientation;
    public XrVector3 Scale;

    public XrTransform()
    {
        Location = new XrGeoLocationData();
        Position = new XrVector3();
        Rotation = new XrVector3();
        Orientation = new XrQuaternion();
        Scale = new XrVector3();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class XrSceneAsset : MonoBehaviour
{
    public string XrSceneAssetId;
    public string ProjectAssetId;

    public XrSceneAssetData XrSceneAssetData;

    //public XrSceneAsset Parent;
    //public List<XrSceneAsset> Children = new List<XrSceneAsset>();

    public static Action<XrSceneAsset> OnCreated;

    public GameObject Parent;
    public List<GameObject> Children = new List<GameObject>();

    private void Start()
    {
        if (XrSceneAssetData == null)
        {
            XrSceneAssetData = new XrSceneAssetData();
            if (!string.IsNullOrEmpty(XrSceneAssetId)) { XrSceneAssetData.Id = Guid.Parse(XrSceneAssetId); }
        }

        OnCreated?.Invoke(this);
    }

    [SerializeField]
    private XrTransform _xrTransformData;

    public XrTransform GetXrTransformData()
    {
        if (_xrTransformData == null) {
            _xrTransformData = new XrTransform();
            _xrTransformData.Location = new XrGeoLocationData();
            _xrTransformData.Position = new XrVector3();
            _xrTransformData.Rotation = new XrVector3();
            _xrTransformData.Orientation = new XrQuaternion();
            _xrTransformData.Scale = new XrVector3();
        }

        // location data:
        var gps = gameObject.GetComponent<XrGeoLocation>();
        if (gps == null) { Debug.LogError($"GPS IS NULL"); }

        _xrTransformData.Location.Latitude = gameObject.GetComponent<XrGeoLocation>().GPS.Latitude;
        _xrTransformData.Location.Longitude = gameObject.GetComponent<XrGeoLocation>().GPS.Longitude;
        _xrTransformData.Location.Altitude = gameObject.GetComponent<XrGeoLocation>().GPS.Altitude;

        // position data:
        _xrTransformData.Position.X = transform.position.x;
        _xrTransformData.Position.Y = transform.position.y;
        _xrTransformData.Position.Z = transform.position.z;

        // rotation data:
        _xrTransformData.Rotation.X = transform.rotation.eulerAngles.x;
        _xrTransformData.Rotation.Y = transform.rotation.eulerAngles.y;
        _xrTransformData.Rotation.Z = transform.rotation.eulerAngles.z;

        _xrTransformData.Orientation.X = transform.rotation.x;
        _xrTransformData.Orientation.Y = transform.rotation.y;
        _xrTransformData.Orientation.Z = transform.rotation.z;
        _xrTransformData.Orientation.W = transform.rotation.w;

        // scale data:
        MyLogger.Log($"XrSceneAsset :: GetXrTransformData => scale={transform.localScale}");
        _xrTransformData.Scale.X = transform.localScale.x;
        _xrTransformData.Scale.Y = transform.localScale.y;
        _xrTransformData.Scale.Z = transform.localScale.z;

        return _xrTransformData;
    }

    //void OnDestroy()
    //{
    //    var xrGeo = GetComponent<XrGeoLocation>();
    //    if (xrGeo != null)
    //    {
    //        DestroyImmediate(xrGeo);
    //    }
    //}

    public List<GameObject> GetChildren(bool includeDestroyed = false)
    {
        if (Children.Count == 0)
        {
            foreach (Transform childTransform in transform)
            {
                var child = childTransform.gameObject; //.GetComponent<XrSceneAsset>();
                if (child != null)// && (includeDestroyed || !child.MarkAsDestroyed))
                {
                    Children.Add(child);
                }
            }
        }
        return Children;
    }
}

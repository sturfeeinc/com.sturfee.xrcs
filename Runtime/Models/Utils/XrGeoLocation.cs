using SturfeeVPS.Core;
using System;
using UnityEngine;

//[RequireComponent(typeof(XrSceneAsset))] 
public class XrGeoLocation : MonoBehaviour
{
    //public Vector3 latLng { get; set; }

    public SturfeeVPS.Core.GeoLocation GPS { get { return _gps; } set { _gps = value; } }

    public UtmPosition UTM;

    public bool Enabled = false;

    [SerializeField]
    public SturfeeVPS.Core.GeoLocation _gps = new SturfeeVPS.Core.GeoLocation();
    private Vector3 _prevPosition = new Vector3();
    private SturfeeVPS.Core.GeoLocation _prevGps = new SturfeeVPS.Core.GeoLocation();

    // Start is called before the first frame update
    void Start()
    {
        Enabled = true;
    } 

    // Update is called once per frame
    void Update()
    {
        if (Enabled)
        {
            if (IsGpsSet() && !LocationsAreEqual(_gps, _prevGps)) //!_gps.Equals(_prevGps))
            {
                // GPS location input changed => change transform.position
                MyLogger.Log($"Current Position = {transform.position}");
                MyLogger.Log($"New Position = {LocManager.Instance.GetObjectPosition(_gps)}");
                transform.position = LocManager.Instance.GetObjectPosition(_gps);

                _prevGps.Latitude = _gps.Latitude;
                _prevGps.Longitude = _gps.Longitude;
                _prevGps.Altitude = _gps.Altitude;
                _prevPosition.x = transform.position.x;
                _prevPosition.y = transform.position.y;
                _prevPosition.z = transform.position.z;
            }
            else if (transform.position != _prevPosition)
            {
                // position changed => change GPS
                _gps = LocManager.Instance.GetObjectLocation(transform.position);

                _prevGps.Latitude = _gps.Latitude;
                _prevGps.Longitude = _gps.Longitude;
                _prevGps.Altitude = _gps.Altitude;
                _prevPosition.x = transform.position.x;
                _prevPosition.y = transform.position.y;
                _prevPosition.z = transform.position.z;
            }            
        }
    }

    public void ResetLocation()
    {
        _gps = new SturfeeVPS.Core.GeoLocation();
        _prevGps = new SturfeeVPS.Core.GeoLocation();
        _prevPosition = new Vector3();
    }

    bool IsGpsSet()
    {
        if (_gps.Latitude == 0 && _gps.Longitude == 0)
        {
            return false;
        }
        return true;
    }

    bool LocationsAreEqual(SturfeeVPS.Core.GeoLocation loc1, SturfeeVPS.Core.GeoLocation loc2)
    {
        if (Math.Abs(loc1.Latitude - loc2.Latitude) >= 0.0000001) { return false; }
        if (Math.Abs(loc1.Longitude - loc2.Longitude) >= 0.0000001) { return false; }
        if (Math.Abs(loc1.Altitude - loc2.Altitude) >= 0.0000001) { return false; }

        return true;
    }
}

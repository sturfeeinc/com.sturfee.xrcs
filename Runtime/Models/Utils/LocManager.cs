using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SturfeeVPS.Core;

public class LocManager : SimpleSingletonComponent<LocManager>
{
    // TODO: this should be dynamic
    public SturfeeVPS.Core.GeoLocation ReferenceGpsLocation = new SturfeeVPS.Core.GeoLocation
    {
        Latitude = 35.681563,
        Longitude = 139.765269
    };
    public UtmPosition ReferenceLocation = new UtmPosition();

    private void Awake()
    {
       
    }

    private void Start()
    {
        //GeoCoordinateConverter.GpsToUtm(ReferenceGpsLocation, ref ReferenceLocation);
        //MyLogger.Log($"Reference Location (N,E) = ({ReferenceLocation.Northing}, {ReferenceLocation.Easting})");
        //MyLogger.Log($"Reference Location (Lat,Lng) = ({ReferenceGpsLocation.Latitude}, {ReferenceGpsLocation.Longitude})");
    }

    private void Update()
    {

    }

    public void SetReferenceLocation(XrGeoLocationData locationData)
    {
        SetReferenceLocation(locationData.Latitude, locationData.Longitude, locationData.Altitude);
    }

    public void SetReferenceLocation(double latitude, double longitude, double altitude)
    {
        ReferenceGpsLocation = new SturfeeVPS.Core.GeoLocation
        {
            Latitude = latitude,
            Longitude = longitude,
            Altitude = altitude
        };

        CustomGeoCoordinateConverter.GpsToUtm(ReferenceGpsLocation, ref ReferenceLocation);
        MyLogger.Log($"Reference Location (N,E) = ({ReferenceLocation.Northing}, {ReferenceLocation.Easting})");
        MyLogger.Log($"Reference Location (Lat,Lng) = ({ReferenceGpsLocation.Latitude}, {ReferenceGpsLocation.Longitude})");
    }

    public SturfeeVPS.Core.GeoLocation GetObjectLocation(Transform obj)
    {
        return GetObjectLocation(obj.position);
    }

    public SturfeeVPS.Core.GeoLocation GetObjectLocation(Vector3 position)
    {
        var gps = new SturfeeVPS.Core.GeoLocation();

        //var utm = new UtmPosition
        //{
        //    X = position.x - ReferenceLocation.Easting,
        //    Y = position.y,
        //    Z = position.z - ReferenceLocation.Northing,
        //    Zone = ReferenceLocation.Zone,
        //    Hemisphere = ReferenceLocation.Hemisphere
        //};

        var utm = new UtmPosition
        {
            Easting = position.x + ReferenceLocation.Easting,
            Northing = position.z + ReferenceLocation.Northing,
            //X = position.x + ReferenceLocation.Easting,
            //Y = position.z + ReferenceLocation.Northing,
            //Z = position.y,
            Zone = ReferenceLocation.Zone,
            Hemisphere = ReferenceLocation.Hemisphere
        };

        CustomGeoCoordinateConverter.UtmToGps(utm, ref gps);

        gps.Altitude = position.y;

        return gps;
    }

    public Vector3 GetObjectPosition(SturfeeVPS.Core.GeoLocation location)
    {
        var utm = new UtmPosition();
        CustomGeoCoordinateConverter.GpsToUtm(location, ref utm);

        //MyLogger.Log($"location = {location}\n utm = {utm}\n ReferenceLocation = {ReferenceLocation}");

        // remove reference to get local position
        var posiiton = new Vector3();
        posiiton.x = (float)(utm.Easting - ReferenceLocation.Easting);
        posiiton.y = (float)(location.Altitude);
        posiiton.z = (float)(utm.Northing - ReferenceLocation.Northing);

        return posiiton;
    }
}

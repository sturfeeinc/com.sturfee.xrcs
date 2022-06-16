using UnityEngine;

namespace Sturfee.XRCS.Config
{
    public static class XrConstants
    {
        // for projects
        public static readonly string LOCAL_PROJECTS_PATH = "XRCS/Projects";
        public static readonly string LOCAL_POINTCLOUD_PATH = "XRCS/PointClouds";
        public static readonly string LOCAL_ASSETS_PATH = "XRCS/Assets";
        public static readonly string LOCAL_THUMBNAILS_PATH = "XRCS/Thumbnails";
        public static readonly string LOCAL_IMAGES_PATH = "SharedSpaces/Images";
        public static readonly string SCENES_FOLDER = "__Scenes";

        // for Digital Twin
        public static readonly string LOCAL_DT_ASSETS_PATH = "DT/Assets";

        // for users
        public static readonly string LOCAL_USER_PATH = "UserData";

        // https://sharedspaces-api.sturfee.com
        // for AWS content
        public static readonly string S3_REGION = "us-east-1";
        public static readonly string S3_PUBLIC_BUCKET = "sturfee-xrcs-prod-public";
        public static readonly string S3_PROJECTS_BUCKET = "sturfee-xrcs-prod-projects";        
        public static readonly string S3_ENHANCEMENTS_BUCKET = "sturfee-xrcs-prod-digitaltwin-enhancements";

        public static readonly string S3_APP_VERSION_INFO = "https://sturfee-xrcs-prod-public.s3.us-east-1.amazonaws.com/ss-update-info.json";
        public static readonly string S3_CITY_LIST = "https://sturfee-xrcs-prod-public.s3.us-east-1.amazonaws.com/ss-cities.json";

        // APIs
        public static readonly string PROXY_API = "https://sharedspaces-api.sturfee.com/api/v2.0";
        public static readonly string AUTH_API = "https://sharedspaces-api.sturfee.com/auth";

        public static readonly string STURFEE_API = "https://user.sturfee.com/api/v1";

        public static readonly string SSO_API = "https://sso.devsturfee.com/auth/realms/sturfee_dev/protocol/openid-connect";
        public static readonly string SSO_LOGIN_PAGE = "https://sso.devsturfee.com/auth/realms/sturfee_dev/protocol/openid-connect/auth?client_id=shared-spaces-auth-proxy&response_type=code&state=3e19f58e-b70d-4736-86a8-e776ff7ffab1&scope=openid&redirect_uri=https://sharedspaces.sturfee.com/auth-callback";


        public static string XRCS_API
        {
            get
            {
                //if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
                //{
                //    //return "https://localhost:44382/api/v2.0";
                //    return "https://zgae9fztj6.execute-api.us-east-2.amazonaws.com/Prod/api/v2.0";
                //}

                return "https://sharedspaces-api.sturfee.com/api/v2.0";
            }
        }

        public static string DTE_API
        {
            get
            {
                //if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
                //{
                //    //return "https://localhost:44382/api/v2.0";
                //    return "https://zgae9fztj6.execute-api.us-east-2.amazonaws.com/Prod/api/v2.0";
                //}

                return "https://sharedspaces-api.sturfee.com/api/v2.0";
            }
        }



    }

    public enum XrLayers
    {
        Buildings,
        Terrain,
        MapObject,
        PostProcessing,
        CoverageTile,
        Map,
        POIMarker,
        TransparentOcclusion,
        RTUI, // all runtime UI
        DigitalTwin, // TODO: remove RTObjects -- replaced by XrAssets
        EditorUI,
        Player,
        XrAssetPrefab,
        EditAsset,
        XrAssets, // all runtime XR content
        DtAssets // all runtime DT content (including enhancements)
    }

    public static class XrColors
    {
        public const string Blue = "#4C7BFD";
        public const string Red = "#E85D75";
        public const string Green = "#19BFC8";

        public const string Light = "#E3F0FF";
        public const string Dark = "#283558";

        public const string LightBlue = "#8DB0FF";
    }
}


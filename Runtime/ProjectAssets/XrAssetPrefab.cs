using Newtonsoft.Json;
using UnityEngine;

[RequireComponent(typeof(XrGeoLocation))]
public class XrAssetPrefab : MonoBehaviour
{
    public XrProjectAssetData XrProjectAssetData;

    public void SetData(XrProjectAssetData xrAssetData)
    {
        XrProjectAssetData = xrAssetData;
        //XrAssetData = JsonConvert.DeserializeObject<XrAssetData>(JsonConvert.SerializeObject(xrAssetData));
    }
}

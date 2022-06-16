using System;
using System.Collections.Generic;
using System.Web;
using UnityEngine;

public class BillboardButton : MonoBehaviour
{
    public bool Enabled = true;

    [SerializeField]
    private string _url;
    [SerializeField]
    private BillboardButtonType _type;

    public void SetUrl(string url, BillboardButtonType type)
    {
        _url = url;
        _type = type;
    }

    private void OnMouseDown()
    {
        if (!Enabled) {  return; }

        MyLogger.Log($"BillboardTemplateAsset :: Button Clicked!");
        if (!string.IsNullOrEmpty(_url))
        {
            var url = _url;
            if (_type == BillboardButtonType.PhoneNumber)
            {
                if (!url.Contains("tel:"))
                {
                    url = "tel:" + url;
                }
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (url.Contains($"https://sharedspaces.sturfee.com/spaces"))
                {
                    Uri myUri = new Uri(url);
                    string spaceId = HttpUtility.ParseQueryString(myUri.Query).Get("spaceId");

                    MyLogger.LogError(" Deeplinking is not setupp yet ");
                    //MyDeepLinkManager.Instance.ActivateLink(new Dictionary<string, string>
                    //{
                    //    { "spaceId", spaceId }
                    //});
                }
                else
                {
                    Application.OpenURL(url);
                }
            }
            else
            {
                Application.OpenURL(url);
            }            
        }
    }


}

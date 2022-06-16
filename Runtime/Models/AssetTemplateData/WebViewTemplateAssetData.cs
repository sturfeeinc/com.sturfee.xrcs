using System;

[Serializable]
public enum WebViewType
{
    WebPage,
    Video
}

[Serializable]
public class WebViewTemplateAssetData : TemplateAssetData
{
    public string Url;
    public WebViewType WebViewType;
    public bool AutoPlay;
    public bool AllowClicks = true;
    public bool AllowHover = true;
    public bool AllowScroll = true;
    public bool AllowOpenInBrowser = true;
}

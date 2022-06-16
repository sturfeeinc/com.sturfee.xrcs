using System;



[Serializable]
public enum BillboardButtonType
{
    None,
    WebLink,
    PhoneNumber
}


[Serializable]
public class BillboardTemplateAssetData : TemplateAssetData
{
    public string ImageId;
    public XrColor BackgroundColor;

    public string Text;
    public XrColor TextColor;
    public XrColor TextBackgroundColor;

    public XrColor ButtonColor;
    public string ButtonText;    
    public XrColor ButtonTextColor;

    public string Url;

    public BillboardButtonType ButtonType;

    public BillboardTemplateAssetData()
    {
        ImageId = string.Empty;
        Text = string.Empty;
        ButtonText = string.Empty;
        BackgroundColor = new XrColor { R = 1f, G = 1f, B = 1f, A = 1f };
        TextColor = new XrColor { R = 1f, G = 1f, B = 1f, A = 1f };
        ButtonColor = new XrColor { R = 1f, G = 1f, B = 1f, A = 1f };
        ButtonTextColor = new XrColor { R = 1f, G = 1f, B = 1f, A = 1f };
    }
}

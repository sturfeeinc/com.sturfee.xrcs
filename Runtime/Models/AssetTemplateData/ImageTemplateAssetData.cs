using System;

[Serializable]
public class ImageTemplateAssetData : TemplateAssetData
{
    public string ImageId;
    public string Caption;

    public ImageTemplateAssetData()
    {
        ImageId = string.Empty;
        Caption = string.Empty;
    }
}

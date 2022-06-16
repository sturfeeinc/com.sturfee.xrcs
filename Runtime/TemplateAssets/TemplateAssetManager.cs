using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void TemplateDataChanged(TemplateAssetType type, TemplateAssetData preVals, TemplateAssetData newVals);
public delegate void TemplateImageChanged(Guid projectAssetId, Guid imageId, string caption, Texture2D texture);
public delegate void TemplateImageCaptionChanged(Guid projectAssetId, string caption);

[Serializable]
public class TemplateAssetOption
{
    public TemplateAssetType Type;
    public GameObject Prefab;
}

public class TemplateAssetManager : SceneSingleton<TemplateAssetManager>
{
    public List<TemplateAssetOption> Prefabs;

    public event TemplateDataChanged OnTemplateDataChanged;

    public event TemplateImageChanged OnTemplateImageChanged;
    public event TemplateImageCaptionChanged OnTemplateImageCaptionChanged;

    public void EmitDataChange(TemplateAssetType type, TemplateAssetData preVals, TemplateAssetData newVals)
    {
        OnTemplateDataChanged?.Invoke(type, preVals, newVals);
    }

    public void EmitImageChange(Guid projectAssetId, Guid imageId, string caption, Texture2D texture)
    {
        OnTemplateImageChanged?.Invoke(projectAssetId, imageId, caption, texture);
    }

    public void EmitImageCaptionChange(Guid projectAssetId, string caption)
    {
        OnTemplateImageCaptionChanged?.Invoke(projectAssetId, caption);
    }
}

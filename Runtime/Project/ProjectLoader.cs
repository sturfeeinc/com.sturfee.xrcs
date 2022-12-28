using Newtonsoft.Json;
using SturfeeVPS.SDK;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public delegate void XrAssetLoadEvent(float progress, int _errorCount);
public delegate void XrAssetErrorEvent(string message);

public class ProjectLoader : SceneSingleton<ProjectLoader>
{
    public event XrAssetLoadEvent OnXrAssetLoadProgress;
    public event XrAssetLoadEvent OnXrSceneLoadProgress;
    public event XrAssetErrorEvent OnXrSceneLoadError;

    public async Task LoadSceneAssets(XrSceneData scene)
    {
        MyLogger.Log($"ProjectLoader :: Loading assets for scene: {JsonConvert.SerializeObject(scene)}");
    }
}

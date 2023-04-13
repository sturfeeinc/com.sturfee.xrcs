using SturfeeVPS.SDK;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoader : SceneSingleton<AssetBundleLoader>
{
    private List<AssetBundle> _loadedBundles = new List<AssetBundle>();

    private void OnDestroy()
    {
        UnloadAllBundles();
    }

    public void AddLoadedBundle(AssetBundle bundle)
    {
        if (bundle == null) { return; }

        _loadedBundles.Add(bundle);
    }

    public void RemoveUnloadedBundle(AssetBundle bundle)
    {
        if (bundle == null) { return; }

        if (_loadedBundles.Contains(bundle))
        {
            _loadedBundles.Remove(bundle);
        }        
    }

    private void UnloadAllBundles()
    {
        MyLogger.Log($"Unloading Asset Bundles...");

        foreach (var bundle in _loadedBundles)
        {
            bundle.Unload(true);
        }

        _loadedBundles.Clear();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AssetBundlePlatformHelper
{
    public static string GetBundleFileForCurrentPlatform(List<string> bundleFiles)
    {
        string bundleFileToLoad = null;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                bundleFileToLoad = bundleFiles.FirstOrDefault(file => file.Contains(SupportedAssetBundlePlatforms.WindowsExt));
                break;

            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                bundleFileToLoad = bundleFiles.FirstOrDefault(file => file.Contains(SupportedAssetBundlePlatforms.MacExt));
                break;

            case RuntimePlatform.Android:
                bundleFileToLoad = bundleFiles.FirstOrDefault(file => file.Contains(SupportedAssetBundlePlatforms.AndroidExt));
                break;

            case RuntimePlatform.IPhonePlayer:
                bundleFileToLoad = bundleFiles.FirstOrDefault(file => file.Contains(SupportedAssetBundlePlatforms.iOSExt));
                break;

            default:
                //MobileToastManager.Instance.ShowToast($"Error loading asset for this platform ({Application.platform})");
                MyLogger.LogError($"Error loading asset for this platform ({Application.platform})");
                break;
        }

        return bundleFileToLoad;
    }

    public static string GetBundleFileForCurrentPlatform(string bundleFilePrepend)
    {
        string bundleFileToLoad = null;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                bundleFileToLoad = $"{bundleFilePrepend}{SupportedAssetBundlePlatforms.WindowsExt}";
                break;

            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                bundleFileToLoad = $"{bundleFilePrepend}{SupportedAssetBundlePlatforms.MacExt}";
                break;

            case RuntimePlatform.Android:
                bundleFileToLoad = $"{bundleFilePrepend}{SupportedAssetBundlePlatforms.AndroidExt}";
                break;

            case RuntimePlatform.IPhonePlayer:
                bundleFileToLoad = $"{bundleFilePrepend}{SupportedAssetBundlePlatforms.iOSExt}";
                break;

            default:
                //MobileToastManager.Instance.ShowToast($"Error loading asset for this platform ({Application.platform})");
                MyLogger.LogError($"Error loading asset for this platform ({Application.platform})");
                break;
        }

        return bundleFileToLoad;
    }

    public void ApplyToAllPlatforms(string filename, Action<string> action)
    {
        var supportedPlatforms = new string[]
        {
            SupportedAssetBundlePlatforms.WindowsExt,
            SupportedAssetBundlePlatforms.MacExt,
            SupportedAssetBundlePlatforms.AndroidExt,
            SupportedAssetBundlePlatforms.iOSExt
        };

        string filePrepend = null;
        var nameParts = filename.Split('.');
        if (nameParts.Length > 2)
        {
            filePrepend = string.Join(".", nameParts.Take(nameParts.Length - 2));
            //MyLogger.Log($"   filePrepend = {filePrepend}");
        }

        var failList = new List<string>();
        for (var i = 0; i < supportedPlatforms.Length; i++)
        {
            var expectedFile = $"{filePrepend}{supportedPlatforms[i]}";
            //MyLogger.Log($"   expectedFile = {expectedFile}");
            var name = Path.GetFileNameWithoutExtension(expectedFile);

            if (File.Exists(expectedFile))
            {
                action?.Invoke(expectedFile);
            }
        }
    }

    public void ApplyToAllPlatforms(string filename, Func<string, Task> action)
    {

    }
}

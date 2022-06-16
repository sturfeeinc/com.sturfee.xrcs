using System.IO;
using UnityEngine;

public enum CacheItemType
{
    SturgTile,
    SvImage
}



public interface ICacheProvider<T>
{
    void SaveToCache(string key, T obj);
    T GetFromCache(string key);

    string CacheDir { get; }
}

public class StringCacheProvider : ICacheProvider<string>
{
    private string _localCachePath = $"{Application.persistentDataPath}/XRCS/Cache/";

    public string CacheDir => _cacheDir;
    private string _cacheDir { get { return Path.Combine(_localCachePath, $"SturG"); } }

    public void SaveToCache(string key, string text)
    {
        if (!Directory.Exists(_cacheDir)) { Directory.CreateDirectory(_cacheDir); }

        var filepath = Path.Combine(_cacheDir, key);
        File.WriteAllText(filepath, text);
    }

    public string GetFromCache(string key)
    {
        string text;

        if (!Directory.Exists(_cacheDir)) { return null; }

        var filepath = Path.Combine(_cacheDir, key);

        string fileData = null;
        if (File.Exists(filepath))
        {
            fileData = File.ReadAllText(filepath);
        }
        return fileData;
    }
}

public class TextureCacheProvider : ICacheProvider<Texture2D>
{
    private string _localCachePath = $"{Application.persistentDataPath}/XRCS/Cache/";

    public string CacheDir => _cacheDir;
    private string _cacheDir { get { return Path.Combine(_localCachePath, $"SVImages"); } }

    public void SaveToCache(string key, Texture2D texture)
    {
        byte[] bytes = texture.EncodeToJPG();

        if (!Directory.Exists(_cacheDir)) { Directory.CreateDirectory(_cacheDir); }

        var filepath = Path.Combine(_cacheDir, key);
        File.WriteAllBytes(filepath, bytes);
    }

    public Texture2D GetFromCache(string key)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (!Directory.Exists(_cacheDir)) { return null; }

        var filepath = Path.Combine(_cacheDir, key);

        if (File.Exists(filepath))
        {
            fileData = File.ReadAllBytes(filepath);
            tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(fileData);
        }
        return tex;
    }
}

using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class SimpleJsonIO
{
    public static void SaveJsonFile(string path, string filename, string json)
    {
        var filepath = Path.Combine($"{path}", $"{filename}");
        SaveJsonFile(filepath, json);
    }

    public static void SaveJsonFile(string filepath, string json)
    {
        MyLogger.Log($"Saving json to {filepath}");

        var path = Path.GetDirectoryName(filepath);
        if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

        using (StreamWriter r = new StreamWriter($"{filepath}"))
        {
            r.Write(json);
            r.Flush();
            r.Close();
            r.Dispose();
        }
    }

    public static string ReadJsonFile(string path, string filename)
    {
        MyLogger.Log($"Reading \"{filename}\" json from {path}");

        if (!File.Exists($"{path}/{filename}")) { return null; }

        using (StreamReader r = new StreamReader($"{path}/{filename}"))
        {
            string json = r.ReadToEnd();
            r.Close();
            r.Dispose();
            return json;
        }
    }

    public static async Task SaveJsonFileAsync(string path, string filename, string json)
    {
        var filepath = Path.Combine($"{path}", $"{filename}");
        await SaveJsonFileAsync(filepath, json);
    }

    public static async Task SaveJsonFileAsync(string filepath, string json)
    {
        var path = Path.GetDirectoryName(filepath);
        MyLogger.Log($"Saving json to {filepath}");
        if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

        using (StreamWriter r = new StreamWriter(filepath))
        {
            await r.WriteAsync(json);
            r.Flush();
            r.Close();
            r.Dispose();
        }
    }

    public static async Task<string> ReadJsonFileAsync(string path, string filename)
    {
        var filepath = Path.Combine($"{path}",$"{filename}");
        return await ReadJsonFileAsync(filepath);
    }

    public static async Task<string> ReadJsonFileAsync(string filepath)
    {
        MyLogger.Log($"Reading json from {filepath}...");

        if (!File.Exists(filepath)) { return null; }

        using (StreamReader r = new StreamReader(filepath))
        {
            string json = await r.ReadToEndAsync();
            r.Close();
            r.Dispose();
            return json;
        }
    }
}

using Sturfee.XRCS.Config;
using SturfeeVPS.Core;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public enum ImageFileType
{
    png, // the default
    jpg
}

public interface IThumbnailProvider
{
    Task<Texture> GetThumbnail(Guid id, ImageFileType ext = ImageFileType.png);
    Task SaveThumbnail(Guid id, ImageFileType ext = ImageFileType.png);
}

public class WebThumbnailProvider : IThumbnailProvider
{
    //private string XrConstants.XRCS_API = "https://localhost:44382/api/v1.0";
    //private string XrConstants.XRCS_API = "https://egv4146qah.execute-api.us-east-2.amazonaws.com/Prod/api/v1.0";
    //private string XrConstants.XRCS_API = "https://localhost:44382/api/v2.0";

    //private string _storageUrl = "https://xrcs-public.s3.us-east-2.amazonaws.com/thumbnails";
    private string _storageUrl = $"https://{XrConstants.S3_PUBLIC_BUCKET}.s3.{XrConstants.S3_REGION}.amazonaws.com/thumbnails";

    public async Task SaveThumbnail(Guid id, ImageFileType ext = ImageFileType.png)
    {
        MyLogger.Log($"Saving thumbnail to server ({id}.{ext})...");
        var baseDirectory = $"{Application.persistentDataPath}/{XrConstants.LOCAL_THUMBNAILS_PATH}";
        if (!Directory.Exists(baseDirectory)) { Directory.CreateDirectory(baseDirectory); }

        var url = string.Empty;

        // get upload URL
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/thumbnails/upload_url?id={id}&imageType={ext}");

        var response = await request.GetResponseAsync() as HttpWebResponse;
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Debug.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");
        }

        StreamReader reader = new StreamReader(response.GetResponseStream());
        string uploadUrl = reader.ReadToEnd();

        MyLogger.Log($"Upload Thumbnail Response from API:\n{uploadUrl}");

        await Task.Run(async () => {
            var thumbFile = $"{baseDirectory}/{id}.{ext}";
            if (File.Exists(thumbFile))
            {
                FileInfo fInfo = new FileInfo(thumbFile);
                long numBytes = fInfo.Length;
                FileStream fStream = new FileStream(thumbFile, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fStream);
                byte[] bdata = br.ReadBytes((int)numBytes);
                br.Close();
                fStream.Close();

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create($"{uploadUrl}");
                webRequest.Method = "PUT";
                webRequest.ContentType = $"image/{ext}"; // "multipart/form-data";// "application/x-www-form-urlencoded"; //"image/png";
                webRequest.ContentLength = bdata.Length;

                using (var streamWriter = webRequest.GetRequestStream())
                {
                    if (streamWriter == null)
                    {
                        Debug.LogError($"ERROR: NO REQUEST STREAM FOUND");
                    }

                    await streamWriter.WriteAsync(bdata, 0, bdata.Length);
                    streamWriter.Flush();

                    MyLogger.Log($"       Done writing data to stream {bdata.Length}");
                }

                MyLogger.Log($"  Uploading thumbnail to {uploadUrl}");
                using (var webResponse = await webRequest.GetResponseAsync() as HttpWebResponse)
                {
                    if (webResponse.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.LogError($"ERROR:: API => {webResponse.StatusCode} - {webResponse.StatusDescription}");
                    }

                    MyLogger.Log($"   RESPONSE => {webResponse.StatusCode} - {webResponse.StatusDescription}");
                    using (StreamReader responseReader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        string responseText = responseReader.ReadToEnd();
                        MyLogger.Log($"      Message = {responseText}");
                    }
                }                                     
            }
        });
    }


    public async Task<Texture> GetThumbnail(Guid id, ImageFileType ext = ImageFileType.png)
    {
        var baseDirectory = $"{Application.persistentDataPath}/{XrConstants.LOCAL_THUMBNAILS_PATH}";
        if (!Directory.Exists(baseDirectory)) { Directory.CreateDirectory(baseDirectory); }

        // try to get thumbnail locally
        var thumbFile = $"{baseDirectory}/{id}.{ext}";

#if !UNITY_EDITOR_WIN && !UNITY_STANDALONE_WIN
            MyLogger.Log($"Using 'file://' prepend...");
            thumbFile = $"file://{thumbFile}";
#endif

        if (File.Exists(thumbFile))
        {
            var fileData = File.ReadAllBytes(thumbFile);
            var image = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            image.LoadImage(fileData); // ..this will auto-resize the texture dimensions.
            return image;
        }
        else
        {
            // get from the server and store locally
            //var baseUrl = $"https://xrcs-public.s3.us-east-2.amazonaws.com/thumbnails";
            var url = $"{_storageUrl}/{id}.{ext}";

            //MyLogger.Log($"Loading thumbnail for {id}.{ext}");

            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
            await uwr.SendWebRequest();

            //MyLogger.Log($"   thumbnail fetched...");

            if (uwr.result == UnityWebRequest.Result.ConnectionError) //uwr.isNetworkError || uwr.isHttpError)
            {
                MyLogger.Log(uwr.error);
                return null;
            }
            else if (uwr.result == UnityWebRequest.Result.Success)
            {
                var image = ((DownloadHandlerTexture)uwr.downloadHandler).texture;

                // save to file system
                byte[] bytes = ext == ImageFileType.png ? image.EncodeToPNG() : image.EncodeToJPG();
                File.WriteAllBytes($"{baseDirectory}/{id}.{ext}", bytes);

                return image;
            }
        }

        return null;
    }

}

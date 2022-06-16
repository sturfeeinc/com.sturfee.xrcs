using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class ImageUtils
{
    // Usage from any other script:
    // MySprite = IMG2Sprite.LoadNewSprite(FilePath, [PixelsPerUnit (optional)], [spriteType(optional)])

    public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

        return NewSprite;
    }

    public static Sprite ConvertTextureToSprite(Texture2D texture, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.FullRect)
    {
        // Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

        Sprite NewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

        return NewSprite;
    }

    public static Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public static IEnumerator LoadTextureFromFileCo(string filePath, Action<Texture2D> onLoaded)
    {
        if (!File.Exists(filePath))
        {
            yield break;
        }
        var www = UnityWebRequestTexture.GetTexture("file://" + filePath);
        yield return www.SendWebRequest();

        var texture = DownloadHandlerTexture.GetContent(www);
        onLoaded?.Invoke(texture);
    }

    public static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight, bool mips = false, bool linear = true)
    {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        //Texture2D nTex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, true, true);
        //Texture2D nTex = new Texture2D(newWidth, newHeight, source.format, true, true);
        Texture2D nTex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, mips, linear);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return nTex;
    }

    public static Texture2D TryResizeImage(Texture2D texture, int maxWidth, int maxHeight, out bool resized)
    {
        resized = false;

        Texture2D resizedTexture = texture;

        // check if resize needed
        if (texture.width > maxWidth || texture.height > maxHeight)
        {
            // Get the image's original width and height
            int originalWidth = texture.width;
            int originalHeight = texture.height;

            // To preserve the aspect ratio
            float ratioX = (float)maxWidth / (float)originalWidth;
            float ratioY = (float)maxHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            // New width and height based on aspect ratio
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            MyLogger.Log($"Resizing Texture ({texture.width} x {texture.height}) => ({newWidth} x {newHeight})");

            resizedTexture = ResizeTexture(texture, newWidth, newHeight);
            resized = true;
        }

        return resizedTexture;
    }
}
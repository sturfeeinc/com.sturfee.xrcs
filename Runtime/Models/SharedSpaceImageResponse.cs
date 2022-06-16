using System;
using UnityEngine;

[Serializable]
public class SaveSharedSpaceImageResponse
{
    public string UploadUrl { get; set; }
    public SharedSpaceImageData SharedSpaceImage { get; set; }
}

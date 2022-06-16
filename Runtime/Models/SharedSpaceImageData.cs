using System;
using UnityEngine;

[Serializable]
public class SharedSpaceImageData
{
    public Guid Id { get; set; }
    public Guid SceneId { get; set; }

    public Guid UserId { get; set; }
    public XrcsUserData User { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public XrGeoLocationData Location { get; set; }
    public XrTransform Transform { get; set; }
}

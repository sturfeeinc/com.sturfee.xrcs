using Sturfee.XRCS;
using Sturfee.XRCS.Config;
using Sturfee.XRCS.Utils;
using SturfeeVPS.SDK;
using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;

public interface IThumbnailCreator
{
    Camera Camera
    {
        get;
    }

    bool CanCreatePreview(UnityObject obj);
    byte[] CreatePreviewData(UnityObject obj);
    Texture2D CreatePreview(UnityObject obj);
    Texture2D CreatePreview(GameObject obj);

    [Obsolete("Use CreatePreivew(GameObject) instead")]
    Texture2D TakeSnapshot(GameObject go);
}

public class ThumbnailCreator : SceneSingleton<ThumbnailCreator>, IThumbnailCreator
{
    [SerializeField]
    private ObjectToTexture m_objectToTextureCamera = null;

    [SerializeField]
    private GameObject m_fallbackPrefab = null;

    [SerializeField]
    private Vector3 m_scale = new Vector3(0.9f, 0.9f, 0.9f);

    private Shader m_unlitTexShader;

    public virtual Camera Camera
    {
        get
        {
            if (m_objectToTextureCamera != null)
            {
                return m_objectToTextureCamera.GetComponent<Camera>();
            }
            return null;
        }
    }

    protected virtual void Awake()
    {
        m_unlitTexShader = Shader.Find("Unlit/Texture");
        if (m_objectToTextureCamera == null)
        {
            GameObject objectToTextureGO = new GameObject("Object To Texture");
            objectToTextureGO.SetActive(false);
            objectToTextureGO.transform.SetParent(transform, false);

            Camera camera = objectToTextureGO.AddComponent<Camera>();
            camera.nearClipPlane = 0.01f;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.cullingMask = LayerMask.GetMask($"{XrLayers.XrAssetPrefab}");

            m_objectToTextureCamera = objectToTextureGO.AddComponent<ObjectToTexture>();
            m_objectToTextureCamera.objectImageLayer = LayerMask.NameToLayer($"{XrLayers.XrAssetPrefab}");

            Light[] lights = FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; ++i)
            {
                lights[i].cullingMask &= ~(LayerMask.GetMask($"{XrLayers.XrAssetPrefab}"));
            }

            GameObject lightGO = new GameObject("Directional light");
            lightGO.transform.SetParent(objectToTextureGO.transform, false);
            lightGO.layer = LayerMask.NameToLayer($"{XrLayers.XrAssetPrefab}");
            lightGO.transform.rotation = Quaternion.Euler(30, 0, 0);

            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.cullingMask = LayerMask.GetMask($"{XrLayers.XrAssetPrefab}");
        }
    }

    public virtual bool CanCreatePreview(UnityObject obj)
    {
        return obj is GameObject || obj is Material || obj is Texture2D || obj is Sprite;
    }

    public virtual Texture2D CreatePreview(UnityObject obj)
    {
        MyLogger.Log($"ThumbnailCreator :: CreatePreview(UnityObject obj)");
        Texture2D previewTexture = null;
        if (obj is GameObject)
        {
            GameObject go = (GameObject)obj;
            previewTexture = CreatePreview(go);
        }
        else if (obj is Material)
        {
            Material material = (Material)obj;
            Shader shader = material.shader;
            bool replaceParticlesShader = shader != null && shader.name.StartsWith("Particles/");
            if (replaceParticlesShader)
            {
                material.shader = m_unlitTexShader;
            }

            GameObject materialSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            materialSphere.transform.position = Vector3.zero;

            MeshRenderer renderer = materialSphere.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            previewTexture = CreatePreview(materialSphere);
            DestroyImmediate(materialSphere);

            if (replaceParticlesShader)
            {
                material.shader = shader;
            }
        }
        else if (obj is Texture2D)
        {
            Texture2D texture = (Texture2D)obj;
            bool isReadable = texture.isReadable;
            bool isSupportedFormat = texture.format == TextureFormat.ARGB32 ||
                                    texture.format == TextureFormat.RGBA32 ||
                                    texture.format == TextureFormat.RGB24 ||
                                    texture.format == TextureFormat.Alpha8;

            if (isReadable && isSupportedFormat)
            {
                texture = Instantiate(texture);
            }
            else
            {
                texture = texture.DeCompress();
            }

            float textureAspect = (texture.width * m_objectToTextureCamera.snapshotTextureHeight) / (float)Mathf.Max(1, texture.height * m_objectToTextureCamera.snapshotTextureWidth);
            TextureScale.Bilinear(texture, Mathf.RoundToInt(m_objectToTextureCamera.snapshotTextureWidth * textureAspect), m_objectToTextureCamera.snapshotTextureHeight);
            previewTexture = texture;
        }
        else if (obj is Sprite)
        {
            Sprite sprite = (Sprite)obj;
            previewTexture = FromSprite(sprite);
        }

        return previewTexture;
    }

    public byte[] CreatePreviewData(UnityObject obj)
    {
        Texture2D texture = CreatePreview(obj);

        byte[] result;
        if (texture != null)
        {
            result = texture.EncodeToPNG();
            Destroy(texture);
        }
        else
        {
            result = new byte[0];
        }

        return result;
    }

    public Texture2D CreatePreview(GameObject obj)
    {
        MyLogger.Log($"ThumbnailCreator :: CreatePreview(GameObject obj)");
        m_objectToTextureCamera.defaultScale = m_scale;
        m_objectToTextureCamera.gameObject.SetActive(true);
        Texture2D texture = m_objectToTextureCamera.TakeObjectSnapshot(obj, m_fallbackPrefab);
        m_objectToTextureCamera.gameObject.SetActive(false);
        return texture;
    }

    private Texture2D FromSprite(Sprite sprite)
    {
        if (sprite.texture != null && sprite.texture.isReadable)
        {
            Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                            (int)sprite.textureRect.y,
                                                            (int)sprite.textureRect.width,
                                                            (int)sprite.textureRect.height);
            texture.SetPixels(newColors);
            texture.Reinitialize(m_objectToTextureCamera.snapshotTextureWidth, m_objectToTextureCamera.snapshotTextureHeight);

            return texture;
        }

        return null;
    }

    [Obsolete]
    public Texture2D TakeSnapshot(GameObject go)
    {
        return CreatePreview(go);
    }
}


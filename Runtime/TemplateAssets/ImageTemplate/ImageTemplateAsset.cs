using Newtonsoft.Json;
using Sturfee.XRCS.Config;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sturfee.XRCS
{
    public enum XrAssetType
    {
        ProjectAsset,
        SceneAsset
    }



    public class ImageTemplateAsset : MonoBehaviour
    {
        public XrAssetType AssetType = XrAssetType.ProjectAsset;

        public ImageTemplateAssetData Data = new ImageTemplateAssetData();

        public Canvas Canvas;
        public Image Image;
        public TextMeshProUGUI Caption;

        public Image LoadingIcon;

        public GameObject MeshHelper;

        private void Start()
        {
            var collider = gameObject.GetComponent<BoxCollider>();
            if (collider == null) { collider = gameObject.AddComponent<BoxCollider>(); }
            collider.size = new Vector3(10, 10, 2);

            MeshHelper.transform.localScale = new Vector3(10, 10, 2);

            TemplateAssetManager.CurrentInstance.OnTemplateImageChanged += HandleImageChange;
            TemplateAssetManager.CurrentInstance.OnTemplateImageCaptionChanged += HandleImageCaptionChange;

            Canvas.worldCamera = Camera.main;
        }

        private void OnDestroy()
        {
            if (TemplateAssetManager.CurrentInstance != null)
            {
                TemplateAssetManager.CurrentInstance.OnTemplateImageChanged -= HandleImageChange;
                TemplateAssetManager.CurrentInstance.OnTemplateImageCaptionChanged -= HandleImageCaptionChange;
            }
        }

        public void SetData(Texture2D image, string imageId, string caption = "")
        {
            Data.ImageId = $"{imageId}";
            Data.Caption = caption;

            LoadingIcon.gameObject.SetActive(true);

            SetupImage(image);
            Caption.SetText(caption);
        }

        public void UpdateSceneAssetLinks(Texture2D texture)
        {
            // update all of the instances of this prefab

            var assetPrefab = GetComponent<XrAssetPrefab>();
            if (assetPrefab == null) { return; }

            var imageAssets = FindObjectsOfType<ImageTemplateAsset>();

            foreach (var imageAsset in imageAssets)
            {
                var sceneAsset = imageAsset.gameObject.GetComponent<XrSceneAsset>();
                if (sceneAsset != null)
                {
                    if (sceneAsset.XrSceneAssetData.ProjectAssetId == assetPrefab.XrProjectAssetData.Id)
                    {
                        imageAsset.SetData(texture, Data.ImageId, Data.Caption);
                    }
                }
            }
        }

        public async void SetCaption(string caption)
        {
            Data.Caption = caption;
            Caption.SetText(caption);

            // save to project asset
            var xrAssetPrefab = GetComponent<XrAssetPrefab>();
            if (xrAssetPrefab == null) // this is attached to the Scene Asset
            {
                var xrSceneAsset = GetComponent<XrSceneAsset>();
                if (xrSceneAsset != null)
                {
                    MyLogger.Log($"ImageTemplateAsset :: Apply changes to local Project Asset (Image Template) using Scene Asset with ID = {xrSceneAsset.XrSceneAssetData.Id} and ProjectAssetId = {xrSceneAsset.XrSceneAssetData.ProjectAssetId}");

                    var projectAssets = FindObjectsOfType<XrAssetPrefab>(true);
                    var theProjectAsset = projectAssets.FirstOrDefault(x => x.XrProjectAssetData.Id == xrSceneAsset.XrSceneAssetData.ProjectAssetId);
                    if (theProjectAsset != null)
                    {
                        var xrAssetData = theProjectAsset.GetComponent<XrAssetPrefab>().XrProjectAssetData;
                        xrAssetData.TemplateData = JsonConvert.SerializeObject(Data);

                        // use scene asset data to overwrite project asset
                        var imageTemplate = theProjectAsset.gameObject.GetComponent<ImageTemplateAsset>();
                        imageTemplate.SetCaption($"{Data.Caption}");
                    }
                    else
                    {
                        Debug.LogError($"Cannot find Project Asset with ID = {xrSceneAsset.XrSceneAssetData.ProjectAssetId}");
                    }
                }
                else
                {
                    Debug.LogError($"ImageTemplateAsset :: Scene Asset NOT FOUND");
                }
            }
            else // this is attached to the Project Asset
            {
                // save update to the server
                MyLogger.Log($"ImageTemplateAsset :: Save Project Asset (Image Template) with ID = {xrAssetPrefab.XrProjectAssetData.Id}");
                TemplateAssetManager.CurrentInstance.EmitImageCaptionChange(xrAssetPrefab.XrProjectAssetData.Id, Data.Caption);

                try
                {
                    var projectProvider = IOC.Resolve<IProjectProvider>();
                    await projectProvider.SaveProjectAsset(xrAssetPrefab.XrProjectAssetData);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    //MobileToastManager.Instance.ShowToastWithButton("Error Saving Asset Changes. Please try again.");
                    MyLogger.LogError("Error Saving Asset Changes. Please try again.");
                    throw;
                }
            }

        }

        public async void SaveNewImage(Guid imageId, Texture2D image)
        {
            LoadingIcon.gameObject.SetActive(true);

            Data.ImageId = $"{imageId}";
            SetupImage(image);

            // save to project asset
            var xrAssetPrefab = GetComponent<XrAssetPrefab>();
            if (xrAssetPrefab == null)
            {
                var xrSceneAsset = GetComponent<XrSceneAsset>();
                if (xrSceneAsset != null)
                {
                    MyLogger.Log($"ImageTemplateAsset :: Apply changes to local Project Asset (Image Template) using Scene Asset with ID = {xrSceneAsset.XrSceneAssetData.Id} and ProjectAssetId = {xrSceneAsset.XrSceneAssetData.ProjectAssetId}");

                    var projectAssets = FindObjectsOfType<XrAssetPrefab>(true);
                    var theProjectAsset = projectAssets.FirstOrDefault(x => x.XrProjectAssetData.Id == xrSceneAsset.XrSceneAssetData.ProjectAssetId);
                    if (theProjectAsset != null)
                    {
                        var xrAssetData = theProjectAsset.GetComponent<XrAssetPrefab>().XrProjectAssetData;
                        xrAssetData.TemplateData = JsonConvert.SerializeObject(Data);

                        // use scene asset data to overwrite project asset
                        var imageTemplate = theProjectAsset.gameObject.GetComponent<ImageTemplateAsset>();
                        imageTemplate.Data.ImageId = $"{Data.ImageId}";
                        imageTemplate.Data.Caption = $"{Data.Caption}";
                        imageTemplate.SaveNewImage(imageId, image);
                    }
                    else
                    {
                        Debug.LogError($"Cannot find Project Asset with ID = {xrSceneAsset.XrSceneAssetData.ProjectAssetId}");
                    }
                }
                else
                {
                    Debug.LogError($"ImageTemplateAsset :: Scene Asset NOT FOUND");
                }
            }
            else
            {
                // save update to the server
                MyLogger.Log($"ImageTemplateAsset :: Save Project Asset (Image Template) with ID = {xrAssetPrefab.XrProjectAssetData.Id}");
                TemplateAssetManager.CurrentInstance.EmitImageChange(xrAssetPrefab.XrProjectAssetData.Id, imageId, Data.Caption, image);

                try
                {
                    var projectProvider = IOC.Resolve<IProjectProvider>();
                    await projectProvider.SaveProjectAsset(xrAssetPrefab.XrProjectAssetData);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    //MobileToastManager.Instance.ShowToastWithButton("Error Saving Asset Changes. Please try again.");
                    MyLogger.LogError("Error Saving Asset Changes. Please try again.");
                    throw;
                }

                // create and save thumbnail
                if (ThumbnailCreator.CurrentInstance.CanCreatePreview(this.gameObject))
                {
                    LoadingIcon.gameObject.SetActive(false);

                    MyLogger.Log($"ImageTemplateAsset ::   Creating Thumbnail Image...");
                    var bytes = ThumbnailCreator.CurrentInstance.CreatePreviewData(this.gameObject);

                    if (bytes != null && bytes.Length > 0)
                    {
                        //File.WriteAllBytes($"{Application.persistentDataPath}/{XrConstants.LOCAL_ASSETS_PATH}/{xrAssetData.Id}/thumb.png", bytes);
                        File.WriteAllBytes($"{Application.persistentDataPath}/{XrConstants.LOCAL_THUMBNAILS_PATH}/{xrAssetPrefab.XrProjectAssetData.Id}.png", bytes);
                        MyLogger.Log($"ImageTemplateAsset ::     DONE! Thumbnail Created.");

                        MyLogger.Log($"ImageTemplateAsset ::   Uploading Thumbnail to server...");
                        var thumbnailProvider = IOC.Resolve<IThumbnailProvider>();
                        await thumbnailProvider.SaveThumbnail(xrAssetPrefab.XrProjectAssetData.Id);
                        MyLogger.Log($"ImageTemplateAsset ::     DONE! Thumbnail Updated.");

                        //// refresh project assets UI
                        //if (ProjectAssetsView.CurrentInstance != null)
                        //{
                        //    ProjectAssetsView.CurrentInstance.RefreshAssets();
                        //}
                    }
                    else
                    {
                        Debug.LogError($"ImageTemplateAsset :: ERROR Creating Thumbnail (no bytes)");
                    }

                }
            }
        }

        private void HandleImageChange(Guid projectAssetId, Guid imageId, string caption, Texture2D texture)
        {
            var xrSceneAsset = GetComponent<XrSceneAsset>();
            if (xrSceneAsset != null)
            {
                if (xrSceneAsset.XrSceneAssetData.ProjectAssetId == projectAssetId)
                {
                    SetData(texture as Texture2D, $"{imageId}", caption);
                }
            }
        }

        private void HandleImageCaptionChange(Guid projectAssetId, string caption)
        {
            var xrSceneAsset = GetComponent<XrSceneAsset>();
            if (xrSceneAsset != null)
            {
                if (xrSceneAsset.XrSceneAssetData.ProjectAssetId == projectAssetId)
                {
                    Data.Caption = caption;
                    Caption.SetText(Data.Caption);
                }
            }
        }

        private void SetupImage(Texture2D image)
        {
            if (image != null)
            {
                MyLogger.Log($"ImageTemplateAsset :: SetupImage => {image.width} x {image.height} ({(float)image.width / (float)image.height})");

                Image.sprite = ImageUtils.ConvertTextureToSprite(image);

                var aspect = (float)image.width / (float)image.height;
                if (aspect == 0) { aspect = 1; }

                Canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 1000f / aspect);

                LoadingIcon.gameObject.SetActive(false);

                var collider = gameObject.GetComponent<BoxCollider>();
                if (collider == null) { collider = gameObject.AddComponent<BoxCollider>(); }
                collider.size = new Vector3(10, 10 / aspect, 2);

                MeshHelper.transform.localScale = new Vector3(10, 10 / aspect, 2);

                // update project asset

            }
        }
    }

}
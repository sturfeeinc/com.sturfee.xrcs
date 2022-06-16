using Newtonsoft.Json;
using Sturfee.XRCS.Config;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Sturfee.XRCS
{
    public class BillboardTemplateAsset : MonoBehaviour
    {
        public XrAssetType AssetType = XrAssetType.ProjectAsset;

        public BillboardTemplateAssetData Data = new BillboardTemplateAssetData();

        public Canvas Canvas;
        //public Image Background;
        public RawImage Background;
        public Image TextBackground;
        public TextMeshProUGUI Text;
        public Button Button;
        public BillboardButton BillboardButton;
        public TextMeshProUGUI ButtonText;

        public GameObject ImageLoader;

        public Color LoadingTextColor = Color.black;

        public GameObject MeshHelper;

        private void Awake()
        {
            MyLogger.Log($"BillboardTemplateAsset :: Awake");
            var collider = gameObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(10, 10, 2);
            }

            //MeshHelper.transform.localScale = new Vector3(10, 10, 1);

            LoadingTextColor = XrColorUtils.GetColor(XrColors.Dark);

            Button.onClick.AddListener(HandleButtonClick);

            //if (BillboardVisuals.CurrentInstance != null)
            //{
            //    BillboardVisuals.CurrentInstance.OnBillboardChange += HandleBillboardChange;
            //}

            Canvas.worldCamera = Camera.main;
        }

        private void OnDestroy()
        {
            //if (BillboardVisuals.CurrentInstance != null)
            //{
            //    BillboardVisuals.CurrentInstance.OnBillboardChange -= HandleBillboardChange;
            //}

            if (Button != null)
            {
                Button.onClick.RemoveListener(HandleButtonClick);
            }
        }

        public void SetData(Texture2D image, BillboardTemplateAssetData data)
        {
            ImageLoader.SetActive(true);

            var oldImageId = "" + Data.ImageId;

            Data = JsonConvert.DeserializeObject<BillboardTemplateAssetData>(JsonConvert.SerializeObject(data));

            Background.color = ColorUtils.GetColor(Data.BackgroundColor);

            TextBackground.color = ColorUtils.GetColor(Data.TextBackgroundColor);

            Button.gameObject.SetActive(Data.ButtonType != BillboardButtonType.None);
            Button.image.color = ColorUtils.GetColor(Data.ButtonColor);
            ButtonText.SetText(Data.ButtonText);
            ButtonText.color = ColorUtils.GetColor(Data.ButtonTextColor);

            if (Data.ImageId != oldImageId || Background.texture == null) // .sprite == null)
            {
                Text.color = LoadingTextColor;
                Text.SetText($"Loading...");
                SetupImage(image);
            }
            else
            {
                Text.SetText(Data.Text);
                Text.color = ColorUtils.GetColor(Data.TextColor);
            }

            BillboardButton.SetUrl(Data.Url, Data.ButtonType);
        }

        public void UpdateSceneAssetLinks()
        {
            // update all of the instances of this prefab

            var assetPrefab = GetComponent<XrAssetPrefab>();
            if (assetPrefab == null) { return; }

            var billboards = FindObjectsOfType<BillboardTemplateAsset>();

            foreach (var billboard in billboards)
            {
                var sceneAsset = billboard.gameObject.GetComponent<XrSceneAsset>();
                if (sceneAsset != null)
                {
                    if (sceneAsset.XrSceneAssetData.ProjectAssetId == assetPrefab.XrProjectAssetData.Id)
                    {
                        billboard.SetData(Background.texture as Texture2D, this.Data);
                    }
                }
            }
        }

        public async void SetText(string text)
        {
            Data.Text = text;
            Text.SetText(text);

            // save to project asset
            var xrAssetPrefab = GetComponent<XrAssetPrefab>();
            if (xrAssetPrefab == null) // this is attached to the Scene Asset
            {
                var xrSceneAsset = GetComponent<XrSceneAsset>();
                if (xrSceneAsset != null)
                {
                    MyLogger.Log($"BillboardTemplateAsset :: Apply changes to local Project Asset (Image Template) using Scene Asset with ID = {xrSceneAsset.XrSceneAssetData.Id} and ProjectAssetId = {xrSceneAsset.XrSceneAssetData.ProjectAssetId}");

                    var projectAssets = FindObjectsOfType<XrAssetPrefab>(true);
                    var theProjectAsset = projectAssets.FirstOrDefault(x => x.XrProjectAssetData.Id == xrSceneAsset.XrSceneAssetData.ProjectAssetId);
                    if (theProjectAsset != null)
                    {
                        var xrAssetData = theProjectAsset.GetComponent<XrAssetPrefab>().XrProjectAssetData;
                        xrAssetData.TemplateData = JsonConvert.SerializeObject(Data);

                        // use scene asset data to overwrite project asset
                        var imageTemplate = theProjectAsset.gameObject.GetComponent<BillboardTemplateAsset>();
                        imageTemplate.SetText($"{Data.Text}");
                    }
                    else
                    {
                        Debug.LogError($"Cannot find Project Asset with ID = {xrSceneAsset.XrSceneAssetData.ProjectAssetId}");
                    }
                }
                else
                {
                    Debug.LogError($"BillboardTemplateAsset :: Scene Asset NOT FOUND");
                }
            }
            else // this is attached to the Project Asset
            {
                // save update to the server
                MyLogger.Log($"BillboardTemplateAsset :: Save Project Asset (Image Template) with ID = {xrAssetPrefab.XrProjectAssetData.Id}");
                TemplateAssetManager.CurrentInstance.EmitImageCaptionChange(xrAssetPrefab.XrProjectAssetData.Id, Data.Text);

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
            Text.SetText($"Loading...");
            Text.color = LoadingTextColor;

            Data.ImageId = $"{imageId}";
            SetupImage(image);

            // save to project asset
            var xrAssetPrefab = GetComponent<XrAssetPrefab>();
            if (xrAssetPrefab == null)
            {
                var xrSceneAsset = GetComponent<XrSceneAsset>();
                if (xrSceneAsset != null)
                {
                    MyLogger.Log($"BillboardTemplateAsset :: Apply changes to local Project Asset (Image Template) using Scene Asset with ID = {xrSceneAsset.XrSceneAssetData.Id} and ProjectAssetId = {xrSceneAsset.XrSceneAssetData.ProjectAssetId}");

                    var projectAssets = FindObjectsOfType<XrAssetPrefab>(true);
                    var theProjectAsset = projectAssets.FirstOrDefault(x => x.XrProjectAssetData.Id == xrSceneAsset.XrSceneAssetData.ProjectAssetId);
                    if (theProjectAsset != null)
                    {
                        var xrAssetData = theProjectAsset.GetComponent<XrAssetPrefab>().XrProjectAssetData;
                        xrAssetData.TemplateData = JsonConvert.SerializeObject(Data);

                        // use scene asset data to overwrite project asset
                        var imageTemplate = theProjectAsset.gameObject.GetComponent<BillboardTemplateAsset>();
                        imageTemplate.Data.ImageId = $"{Data.ImageId}";
                        imageTemplate.Data.Text = $"{Data.Text}";
                        imageTemplate.SaveNewImage(imageId, image);
                    }
                    else
                    {
                        Debug.LogError($"Cannot find Project Asset with ID = {xrSceneAsset.XrSceneAssetData.ProjectAssetId}");
                    }
                }
                else
                {
                    Debug.LogError($"BillboardTemplateAsset :: Scene Asset NOT FOUND");
                }
            }
            else
            {
                // save update to the server
                MyLogger.Log($"BillboardTemplateAsset :: Save Project Asset (Image Template) with ID = {xrAssetPrefab.XrProjectAssetData.Id}");
                TemplateAssetManager.CurrentInstance.EmitImageChange(xrAssetPrefab.XrProjectAssetData.Id, imageId, Data.Text, image);

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
                    Text.SetText(Data.Text);
                    Text.color = Color.white;

                    MyLogger.Log($"BillboardTemplateAsset ::   Creating Thumbnail Image...");
                    var bytes = ThumbnailCreator.CurrentInstance.CreatePreviewData(this.gameObject);

                    if (bytes != null && bytes.Length > 0)
                    {
                        //File.WriteAllBytes($"{Application.persistentDataPath}/{XrConstants.LOCAL_ASSETS_PATH}/{xrAssetData.Id}/thumb.png", bytes);
                        File.WriteAllBytes($"{Application.persistentDataPath}/{XrConstants.LOCAL_THUMBNAILS_PATH}/{xrAssetPrefab.XrProjectAssetData.Id}.png", bytes);
                        MyLogger.Log($"BillboardTemplateAsset ::     DONE! Thumbnail Created.");

                        MyLogger.Log($"BillboardTemplateAsset ::   Uploading Thumbnail to server...");
                        var thumbnailProvider = IOC.Resolve<IThumbnailProvider>();
                        await thumbnailProvider.SaveThumbnail(xrAssetPrefab.XrProjectAssetData.Id, ImageFileType.png);
                        MyLogger.Log($"BillboardTemplateAsset ::     DONE! Thumbnail Updated.");

                        //// refresh project assets UI
                        //if (ProjectAssetsView.CurrentInstance != null)
                        //{
                        //    ProjectAssetsView.CurrentInstance.RefreshAssets();
                        //}
                    }
                    else
                    {
                        Debug.LogError($"BillboardTemplateAsset :: ERROR Creating Thumbnail (no bytes)");
                    }

                }
            }
        }

        private void HandleButtonClick()
        {
            //MyLogger.Log($"BillboardTemplateAsset :: Button Clicked!");
            //if (!string.IsNullOrEmpty(Data.Url))
            //{
            //    var url = Data.Url;
            //    if (Data.ButtonType == BillboardButtonType.PhoneNumber)
            //    {
            //        if (!url.Contains("tel:"))
            //        {
            //            url = "tel:" + url;
            //        }
            //    }

            //    Application.OpenURL(url);
            //}
        }

        private async void HandleBillboardChange(Guid projectAssetId, BillboardTemplateAssetData data, Texture2D texture)
        {
            MyLogger.Log($"BillboardTemplateAsset :: HandleBillboardChange");
            // update the scene assets AND asset prefabs
            var sceneAsset = GetComponent<XrSceneAsset>();
            var assetPrefab = GetComponent<XrAssetPrefab>();
            if (sceneAsset != null || assetPrefab != null)
            {
                var id = sceneAsset == null ? assetPrefab.XrProjectAssetData.Id : sceneAsset.XrSceneAssetData.ProjectAssetId;

                if (id == projectAssetId)
                {
                    var currentImageId = "" + Data.ImageId;
                    //Data = JsonConvert.DeserializeObject<BillboardTemplateAssetData>(JsonConvert.SerializeObject(data));                

                    MyLogger.Log($"BillboardTemplateAsset :: UPDATE ME!!!\n{JsonConvert.SerializeObject(data)}");
                    SetData(null, data);

                    if (texture != null)
                    {
                        SetData(texture as Texture2D, data);
                    }
                    else if (data.ImageId != currentImageId && texture == null)
                    {
                        Guid imageId;
                        if (Guid.TryParse(data.ImageId, out imageId))
                        {
                            var thumbnailProvider = IOC.Resolve<IThumbnailProvider>();
                            var thumb = await thumbnailProvider.GetThumbnail(imageId, ImageFileType.png);

                            if (thumb != null)
                            {
                                SetData(thumb as Texture2D, data);
                            }
                        }
                    }
                    else
                    {
                        SetData(null, data);
                    }
                }

                if (assetPrefab != null)
                {
                    assetPrefab.XrProjectAssetData.TemplateData = JsonConvert.SerializeObject(Data);
                }
            }

            // TODO: regenerate the thumbnail for this asset
        }

        private void SetupImage(Texture2D image)
        {
            if (image != null)
            {
                MyLogger.Log($"BillboardTemplateAsset :: SetupImage => {image.width} x {image.height} ({(float)image.width / (float)image.height})");

                //Background.sprite = ImageUtils.ConvertTextureToSprite(image);
                Background.texture = image;

                var aspect = (float)image.width / (float)image.height;
                if (aspect == 0) { aspect = 1; }

                Canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 1000f / aspect);

                Text.SetText(Data.Text);
                Text.color = ColorUtils.GetColor(Data.TextColor);
                //Text.color = Color.white;

                var collider = gameObject.GetComponent<BoxCollider>();
                if (collider == null) { collider = gameObject.AddComponent<BoxCollider>(); }
                collider.size = new Vector3(10, 10 / aspect, 2);
                StartCoroutine(SetupColliderCo(aspect));

                MeshHelper.transform.localScale = new Vector3(10, 10 / aspect, 1);

                // update project asset

                ImageLoader.SetActive(false);
            }
            else
            {
                //MyLogger.Log($"No Image Loaded");
                //Background.sprite = null;
                Background.texture = null;

                Text.color = ColorUtils.GetColor(Data.TextColor);
                Text.SetText(Data.Text);
            }
        }

        private IEnumerator SetupColliderCo(float aspect)
        {
            yield return new WaitForSeconds(1);

            var collider = gameObject.GetComponent<BoxCollider>();
            if (collider == null) { collider = gameObject.AddComponent<BoxCollider>(); }
            collider.size = new Vector3(10, 10 / aspect, 2);
        }
    } 
}

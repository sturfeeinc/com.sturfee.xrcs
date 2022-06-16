using UnityEngine;

namespace Sturfee.XRCS
{
    public class SpawnPointTemplateAsset : MonoBehaviour
    {
        public XrAssetType AssetType = XrAssetType.ProjectAsset;

        //[Header("Refs")]
        public Canvas Canvas;
        public GameObject Helper;
        public Collider Collider;

        //public bool ShowUi
        //{
        //    get => _showUi;
        //    set
        //    {
        //        _showUi = value;
        //        Canvas.gameObject.SetActive(_showUi);
        //        Helper.gameObject.SetActive(_showUi);
        //        Collider.enabled = _showUi;
        //    }
        //}
        private bool _showUi = false;

        [Header("Config")]
        public float ScaleFactor = 1f;
        public float ScaleMin = 0.5f;
        public float ScaleMax = 10f;

        public Camera Camera;

        private Vector3 _lookPos = Vector3.one;
        private Vector3 _scale = Vector3.one;
        private Quaternion _rotation = Quaternion.identity;

        private void Start()
        {
            if (Camera == null) { Camera = Camera.main; }

            //ShowUi = false;

            //var editor = FindObjectOfType<MobileEditorManager>();
            //_showUi = editor != null;

            Canvas.gameObject.SetActive(_showUi);
            Helper.gameObject.SetActive(_showUi);
            Collider.enabled = _showUi;

        }

        private void Update()
        {
            if (Camera == null) { Camera = Camera.main; }

            if (_showUi)
            {
                //transform.LookAt(Camera.transform.position);
                _lookPos = Camera.transform.position - transform.position;
                _lookPos.y = 0;
                _rotation = Quaternion.LookRotation(_lookPos);
                Canvas.transform.rotation = _rotation; // Quaternion.Slerp(Canvas.transform.rotation, _rotation, Time.deltaTime);

                _scale = Vector3.one * Mathf.Clamp(ScaleFactor * Vector3.Distance(transform.position, Camera.transform.position), ScaleMin, ScaleMax);
                transform.localScale = _scale;
            }
        }
    } 
}

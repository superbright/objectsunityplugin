using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using SB.Objects.Model;



namespace SB.Objects
{
    static class AWS
    {
        [SerializeField]
        public const string AWS_ADDRESS = @"https://s3.amazonaws.com/";

        [SerializeField]
        public const string S3Bucket = "s3deploy.test";

        [SerializeField]
        public const string ThumbnailDir = "img";

        public static string ProjectPath
        {
            get { return Path.Combine(AWS.AWS_ADDRESS, AWS.S3Bucket); }
        }

        public static string ThumbnailRoot
        {
            get { return Path.Combine(ProjectPath, AWS.ThumbnailDir); }
        }
    }
}

namespace SB.Objects
{
    [System.Serializable]
    public class ObjectsBundle
    {
        public Object Asset;
        public Object[] AllObjects;
        public AssetBundle Bundle;
        public string Name;
    }

    public class ObjectsManager : MonoBehaviour
    {

        [SerializeField]
        string _dbAddress = @"http://167.99.82.243/";

        [SerializeField]
        int _port = 2222;


        public string APIRoot
        {
            get { return @"http://" + _dbAddress + ":" + _port + @"/api/"; }
        }

        public string LatestAssetEndpoint
        {
            get { return APIRoot + @"assets/latest"; }
        }

        public string AssetsEndpoint
        {
            get { return APIRoot + @"assets"; }
        }

        public bool PollForAssetChanges
        {
            get { return _pollForAssetChanges; }
            set
            {
                if (value != _pollForAssetChanges)
                {
                    _pollForAssetChanges = value;

                    if (_pollForAssetChanges)
                    {
                        StartCoroutine(PollLatestAssets());
                    }
                }
            }
        }

        [SerializeField] bool _pollForAssetChanges = true;
        public int PollDelay = 10;

        BundleData _latestBundle;

        public int latestId = 0;

        public List<BundleData> BundleModels { get { return _bundleModels; } }

        [SerializeField]
        List<BundleData> _bundleModels = new List<BundleData>();

        [SerializeField]
        List<ObjectsBundle> _bundles = new List<ObjectsBundle>();

        Dictionary<int, ObjectsBundle> ObjectsBundleDictionary = new Dictionary<int, ObjectsBundle>();

        public ObjectsBundle LatestBundle
        {
            get { return _bundles.Last(); }
        }

        public int ValidBundleCount
        {
            get { return _bundleModels.Select(itm => itm.bundlepath != null).Count(); }
        }

        public System.Action AssetBundleUpdated;
        public System.Action BundleModelsUpdated;

        private void Start()
        {
            if (PollForAssetChanges)
                StartCoroutine(PollLatestAssets());
        }

        IEnumerator PollLatestAssets()
        {
            Debug.Log("Start Polling for asset changes");

            while (_pollForAssetChanges)
            {
                UnityWebRequest req = UnityWebRequest.Get(AssetsEndpoint);

                yield return req.SendWebRequest();
                if (req.isNetworkError || req.isHttpError)
                    Debug.LogError("Err: " + req.error);
                else
                {
                    var resp = req.downloadHandler.text;

                    //Debug.Log("Resp: " + resp);
                    var bundleModels = resp.ToAssetBundleModel().data.Where(itm => itm.bundlepath.Length > 0).ToList();

                    var newValidBundles = bundleModels.Where(itm => itm.bundlepath.Length > 0).Count();

                    if (newValidBundles != ValidBundleCount)
                    {
                        _bundleModels = bundleModels;

                        if (BundleModelsUpdated != null)
                        {
                            BundleModelsUpdated();
                        }
                    }

                }

                yield return new WaitForSeconds(10);
            }

            Debug.Log("No longer polling for asset changes");
        }

        public BundleData GetBundleData(int id)
        {
            return _bundleModels.FirstOrDefault(i => i.id.Equals(id));
        }

        private List<string> _currentRequests = new List<string>();

        public IEnumerator GetObjectsBundleAsync(BundleData data, System.Action<ObjectsBundle> callback)
        {
            if (ObjectsBundleDictionary.ContainsKey(data.id))
                callback(ObjectsBundleDictionary[data.id]);
            else
                yield return StartCoroutine(GetAssetBundleAsync(data, callback));
        }

        IEnumerator GetAssetBundleAsync(BundleData data, System.Action<ObjectsBundle> callback)
        {
            _currentRequests.Add(name);

            //Path.Split is to negate breaking change to bundlepath for multiple platforms
            var path = Path.Combine(AWS.ProjectPath, data.bundlepath.Split('.')[0]);
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path += ".android";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path += ".ios";
                    break;
                default:
                    path += ".android";
                    break;
            }

            Debug.Log("Attempting to get AssetBundle from: " + path);

            var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(path);
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
                Debug.LogError(webRequest.error);
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);

                Debug.Log("Got Asset Bundle: " + bundle.name);

                bundle.GetAllAssetNames().ToList().ForEach(t =>
                {
                    Debug.Log(t);
                });

                var objects = bundle.LoadAllAssetsAsync();

                yield return objects;

                ObjectsBundle b = new ObjectsBundle()
                {
                    Name = name,
                    Bundle = bundle,
                    AllObjects = objects.allAssets,
                    Asset = objects.asset
                };

                _bundles.Add(b);
                ObjectsBundleDictionary.Add(data.id, b);

                Debug.Log("Loaded AllAssets");

                _currentRequests.Remove(name);

                if (callback != null)
                    callback(b);
            }
        }
    }
}
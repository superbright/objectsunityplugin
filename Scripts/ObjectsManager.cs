using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using SB.Objects.Model;



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
        const string AWS_ADDRESS = @"https://s3.amazonaws.com/";

        [SerializeField]
        string _dbAddress = @"http://167.99.82.243/";

        [SerializeField]
        int _port = 2222;

        [SerializeField]
        string _s3Bucket = "s3deploy.test";

        public string ProjectPath
        {
            get { return Path.Combine(AWS_ADDRESS, _s3Bucket); }
        }

        public string APIRoot
        {
            get { return @"http://" + _dbAddress + ":" + _port + @"/api/"; }
        }

        public string LatestAssetEndpoint
        {
            get { return APIRoot + @"assets/latest"; }
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

        AssetBundleModel _latestBundle;

        public int latestId = 0;

        [SerializeField]
        List<ObjectsBundle> _bundles = new List<ObjectsBundle>();

        public ObjectsBundle LatestBundle
        {
            get { return _bundles.Last(); }
        }

        public System.Action AssetBundleUpdated;

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
                UnityWebRequest req = UnityWebRequest.Get(LatestAssetEndpoint);

                yield return req.SendWebRequest();
                if (req.isNetworkError || req.isHttpError)
                    Debug.LogError("Err: " + req.error);
                else
                {
                    var resp = req.downloadHandler.text;

                    Debug.Log("Resp: " + resp);

                    _latestBundle = resp.ToAssetBundleModel();

                    if (_latestBundle.data.bundlepath != null)
                    {
                        Debug.Log(_latestBundle.data.id);

                        if (_latestBundle.data.id > latestId)
                        {
                            latestId = _latestBundle.data.id;
                            GetObjectsBundle(_latestBundle.data.bundlepath);
                        }
                    }

                }

                yield return new WaitForSeconds(10);
            }

            Debug.Log("No longer polling for asset changes");
        }


        private List<string> _currentRequests = new List<string>();

        public void GetObjectsBundle(string name)
        {
            StartCoroutine(GetAssetBundleAsync(name));
        }

        IEnumerator GetAssetBundleAsync(string name)
        {
            _currentRequests.Add(name);

            var path = Path.Combine(ProjectPath, name);
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

                Debug.Log("Loaded AllAssets");

                _currentRequests.Remove(name);

                if (AssetBundleUpdated != null)
                    AssetBundleUpdated();
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SB.Objects
{
    [System.Serializable]
    public class Data
    {
        public int id;
        public string key;
        public string bundlepath;
    }

    [System.Serializable]
    public class ServerAssetBundle
    {
        public Data data;
    }


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
        string _dbAddress = @"http://10.0.1.32:2222/";

        public string ProjectPath
        {
            get { return Path.Combine(AWS_ADDRESS, _bucket); }
        }


        [SerializeField] string _bucket;
        //[SerializeField] string _projectName;

        public ServerAssetBundle _latestBundle;
        public int latestId = 0;

        public ObjectsBundle LatestBundle = null;

        private void Start()
        {
            StartCoroutine(PollLatestAssets());
        }

        IEnumerator PollLatestAssets()
        {
            while (true)
            {
                UnityWebRequest req = UnityWebRequest.Get(_dbAddress + @"api/assets/latest");
                Debug.Log(req.uri);

                yield return req.SendWebRequest();
                if (req.isNetworkError || req.isHttpError)
                    Debug.LogError(req.error);
                else
                {
                    var resp = req.downloadHandler.text;

                    Debug.Log(resp);

                    _latestBundle = JsonUtility.FromJson<ServerAssetBundle>(resp);
                    //var bundle = _bundles.Where(t=>t.bundlepath!=null).FirstOrDefault();

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

        }


        private Dictionary<string, ObjectsBundle> _bundleDictionary = new Dictionary<string, ObjectsBundle>();
        private Dictionary<string, System.Action> _loadCompleteCallbacks = new Dictionary<string, System.Action>();
        private Dictionary<string, System.Action> _bundleUpdatedCallbacks = new Dictionary<string, System.Action>();


        public void RegisterOnUpdatedCallback(string name, System.Action callback)
        {
            if (!_bundleUpdatedCallbacks.ContainsKey(name))
                _bundleUpdatedCallbacks.Add(name, callback);
            else
                _bundleUpdatedCallbacks[name] += callback;
        }

        public void RegisterCompleteCallback(string name, System.Action callback)
        {
            if (!_loadCompleteCallbacks.ContainsKey(name))
                _loadCompleteCallbacks.Add(name, callback);
            else
                _loadCompleteCallbacks[name] += callback;
        }

        private List<string> _currentRequests = new List<string>();


        public ObjectsBundle GetObjectsBundle(string name)
        {
            if (_bundleDictionary.ContainsKey(name))
            {
                Debug.Log("ObjectsManager already loaded Bundle: " + name);
                return _bundleDictionary[name];
            }
            else if (_currentRequests.Contains(name))
                return null;
            else
                StartCoroutine(GetAssetBundleAsync(name));

            return null;
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

                LatestBundle = b;

                Debug.Log("Loaded AllAssets");

                if (_bundleDictionary.ContainsKey(name))
                    _bundleDictionary[name] = b;
                else
                    _bundleDictionary.Add(name, b);

                _currentRequests.Remove(name);

                OnLoadComplete(name);
            }
        }

        private void OnLoadComplete(string name)
        {
            if (_loadCompleteCallbacks.ContainsKey(name))
                _loadCompleteCallbacks[name]();
        }
    }
}
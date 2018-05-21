using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SB.Objects
{

    public class ObjectsBundle
    {
        public Object[] AllObjects;
        public AssetBundle Bundle;
        public string Name;
    }

    public class ObjectsManager : MonoBehaviour
    {

        public string ProjectPath
        {
            get { return Path.Combine(_serverURL, _projectName); }
        }

        [SerializeField] string _serverURL;
        [SerializeField] string _projectName;


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
                    AllObjects = objects.allAssets
                };

                Debug.Log("Loaded AllAssets");

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
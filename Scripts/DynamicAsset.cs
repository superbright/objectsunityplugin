using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.IO;

namespace SB.Objects
{
    public class DynamicAsset : MonoBehaviour
    {
        ObjectsManager _manager;

        public string _key;
        //public string _assetName;
        public bool _instantiateOnStart = false;

        private bool _loadComplete = false;
        private Object[] _allObjects;

        ObjectsBundle _bundle;
        GameObject _child;

        public int latestId;

        public void Instantiate(Transform t = null)
        {
            if (!_loadComplete) return;
            if (_bundle == null || _bundle.AllObjects.Length.Equals(0))
            {
                Debug.LogError("Asset Bundle is null or empty");
                //return;
            }

            //var obj = _assetName.Length < 1 ? _bundle.AllObjects[0]
            //    : _bundle.AllObjects.Where(itm => itm.name.Equals(_assetName)).FirstOrDefault();

            //if (obj == null)
            //{
            //    Debug.LogError("Couldn't Find Asset " + _assetName + " In Bundle: " + _bundle.Name);
            //    return;
            //}
            _bundle = _manager.LatestBundle;
            latestId = _manager.latestId;

            var obj = _bundle.Asset;

            if (t == null)
                _child = Instantiate(obj) as GameObject;
            else
                _child = Instantiate(obj, t) as GameObject;


        }

        IEnumerator CheckForNewAsset()
        {
            while (true)
            {
                yield return new WaitForSeconds(5);

                if (latestId != _manager.latestId || _child == null)
                {
                    _bundle = _manager.LatestBundle;
                    latestId = _manager.latestId;

                    if (_child != null)
                        GameObject.Destroy(_child);
                    _child = null;

                    Instantiate(transform);
                }

            }

            yield return 0;
        }

        void Start()
        {
            if (_manager == null)
                _manager = FindObjectOfType<ObjectsManager>();

            _bundle = _manager.GetObjectsBundle(_key);
            StartCoroutine(CheckForNewAsset());
            //_manager.RegisterCompleteCallback(_bundleName, () =>
            //{
            //    _bundle = _manager.GetObjectsBundle(_bundleName);
            //    _loadComplete = true;

            //    Debug.Log("DynamicAssetOnLoadComplete");
            //    if (_instantiateOnStart)
            //        Instantiate(transform);
            //});

        }

        private void Update()
        {
            if (_bundle == null)
            {
                _bundle = _manager.GetObjectsBundle(_key);
            }
            else
            {
                _loadComplete = true;
                if (_instantiateOnStart && _child == null)
                    Instantiate(transform);
            }

        }



#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_manager == null)
                _manager = FindObjectOfType<ObjectsManager>();
        }
#endif

    }
}

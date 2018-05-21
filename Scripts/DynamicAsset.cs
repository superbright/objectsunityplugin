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

        public string _bundleName;
        public string _assetName;
        public bool _instantiateOnStart = false;

        private bool _loadComplete = false;
        private Object[] _allObjects;

        ObjectsBundle _bundle;

        public void Instantiate(Transform t = null)
        {
            if (!_loadComplete) return;

            foreach (var n in _bundle.AllObjects)
            {
                Debug.Log(n.name);
            }

            var obj = _bundle.AllObjects.Where(itm => itm.name.Equals(_assetName)).FirstOrDefault();
            if (obj == null)
            {
                Debug.LogError("Couldn't Find Asset " + _assetName + " In Bundle: " + _bundle.Name);
                return;
            }

            GameObject gobj;
            if (t == null)
                gobj = Instantiate(obj) as GameObject;
            else
                gobj = Instantiate(obj, t) as GameObject;
        }

        void Start()
        {
            if (_manager == null)
                _manager = FindObjectOfType<ObjectsManager>();

            _bundle = _manager.GetObjectsBundle(_bundleName);

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
                _bundle = _manager.GetObjectsBundle(_bundleName);
            }
            else
            {
                _loadComplete = true;
                if (_instantiateOnStart)
                    Instantiate(transform);
                enabled = false;
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

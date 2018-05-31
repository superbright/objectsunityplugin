using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.IO;
using SB.Objects.Model;

namespace SB.Objects
{
    public class DynamicAsset : MonoBehaviour
    {
        public ObjectsManager _manager;

        public string _key;
        //public string _assetName;
        public bool _instantiateOnStart = false;

        private bool _loadComplete = false;
        private Object[] _allObjects;

        public ObjectsBundle _bundle;
        public BundleData DataModel { get; set; }

        GameObject _child;

        public void Instantiate(Transform t = null)
        {
            StartCoroutine(
            _manager.GetObjectsBundleAsync(DataModel, (ab) =>
            {
                _bundle = ab;
                if (_bundle == null || _bundle.AllObjects.Length.Equals(0))
                {
                    Debug.LogError("Asset Bundle is null or empty");
                }
                else
                {
                    if (_child != null)
                        GameObject.Destroy(_child);
                    _child = null;


                    var obj = _bundle.Asset;

                    if (t == null)
                        _child = Instantiate(obj, transform) as GameObject;
                    else
                        _child = Instantiate(obj, t) as GameObject;

                    _child.GetComponentsInChildren<Camera>().ToList().ForEach(c => c.enabled = false);
                }
            }));

        }


        void Awake()
        {
            if (_manager == null)
                _manager = FindObjectOfType<ObjectsManager>();

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

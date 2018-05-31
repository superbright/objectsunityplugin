using SB.Objects;
using SB.Objects.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SB.Objects.UI
{
    public class ObjectsListController : MonoBehaviour
    {
        [SerializeField]
        ObjectsManager _manager;

        public Button _toggleButton;

        public Sprite _toggleButtonActive;
        public Sprite _toggleButtonInactive;

        [SerializeField] bool _isListShowing = false;

        public GameObject ListRoot;
        public GameObject _listItemPrefab;

        [SerializeField]
        public List<ObjectsListItemController> _children;

        

        public Action<int> ListItemSelected;
        public Action ClearSceneButtonPressed;

        private bool _firstRun = true;

        // Use this for initialization
        void Start()
        {
            _manager = FindObjectOfType<ObjectsManager>();

            if (_manager == null)
            {
                Debug.LogError("You need a ObjectsManager on the scene");
                enabled = false;
                return;
            }

            _manager.BundleModelsUpdated += () =>
                {
                    BuildListview();
                };

            ClearListView();

            SetChildrenActive(_isListShowing);

        }

        public void HandleClearSceneButton()
        {
            if (ClearSceneButtonPressed != null)
                ClearSceneButtonPressed();
        }

        private void ClearListView()
        {
            foreach (Transform child in ListRoot.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void BuildListview()
        {
            Debug.Log("Building Listview");

            var assetBundles = _manager.BundleModels;

            foreach (var b in assetBundles)
            {
                if (b.bundlepath != "")
                {
                    if (_children.Select(itm => itm.name).Contains(b.id.ToString()))
                        continue;

                    var go = GameObject.Instantiate(_listItemPrefab);
                    go.transform.parent = ListRoot.transform;

                    //Put them newest first
                    if (!_firstRun)
                        go.transform.SetAsFirstSibling();

                    go.name = b.id.ToString();

                    var ctrl = go.GetComponent<ObjectsListItemController>();
                    ctrl.ThumbnailUrl = b.thumbnail;
                    ctrl.Name = b.id.ToString();
                    ctrl.BundlePath = b.bundlepath;

                    ctrl.OnSelected += (ObjectsListItemController ctr) =>
                        {
                            Debug.Log("Selected " + ctr.name);
                            ListItemSelected(ctr.ID);
                        };

                    _children.Add(ctrl);
                }
            }
            _firstRun = false;
        }

        public void SetChildrenActive(bool active)
        {
            foreach (Transform c in transform)
            {
                c.gameObject.SetActive(active);
            }
        }

        public void ToggleListView()
        {
            _isListShowing = !_isListShowing;

            _toggleButton.GetComponent<Image>().sprite = _isListShowing ? _toggleButtonActive : _toggleButtonInactive;

            SetChildrenActive(_isListShowing);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

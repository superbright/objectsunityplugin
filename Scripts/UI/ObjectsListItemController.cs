using SB.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ObjectsListItemController : MonoBehaviour
{
    public int ID
    {
        get { return int.Parse(name); }
    }

    public string ThumbnailUrl
    {
        set
        {
            if (_thumbnail != value)
            {
                _thumbnail = value;
                _needsDownloadThumbnail = true;
            }
        }
    }
    [SerializeField] string _thumbnail;

    public string Name
    {
        set
        {
            if (_name != value)
            {
                _name = value;
                idTxt.text = _name;
            }
        }
    }

    [SerializeField] string _bundlePath;
    public string BundlePath
    {
        get
        {
            return _bundlePath;
        }
        set
        {
            if (_bundlePath != value)
                _bundlePath = value;
        }
    }

    [SerializeField] string imgUrl;

    public IEnumerator DownloadThumbnail()
    {
        _isDownloadingThumbnail = true;

        var url = Path.Combine(AWS.ThumbnailRoot, _thumbnail + ".png");
        imgUrl = url;

        using (WWW www = new WWW(url))
        {
            yield return www;

            var tex = www.texture;

            _isDownloadingThumbnail = false;
            _needsDownloadThumbnail = false;

            BtnBg.sprite = Sprite.Create(www.texture, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }


    [SerializeField] string _name;

    [SerializeField]
    public Image BtnBg;
    public Text idTxt;

    public bool _needsDownloadThumbnail;
    public bool _isDownloadingThumbnail = false;

    public Action<ObjectsListItemController> OnSelected;

    public void OnEnable()
    {
        if (_needsDownloadThumbnail && !_isDownloadingThumbnail)
        {
            StartCoroutine(DownloadThumbnail());
        }
    }

    public void OnDisable()
    {
        _isDownloadingThumbnail = false;
    }

    // Use this for initialization
    void Start()
    {
        BtnBg = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleButtonPress()
    {
        if (OnSelected != null)
            OnSelected(this);
    }
}

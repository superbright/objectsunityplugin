using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.Objects.Model
{
    [System.Serializable]
    public class AssetBundleModel
    {
        public BundleData data;
    }

    [System.Serializable]
    public class BundleData
    {
        public int id;
        public string key;
        public string bundlepath;
    }

    public static class AssetBundleModelExt
    {
        public static AssetBundleModel ToAssetBundleModel(this string json)
        {
            return JsonUtility.FromJson<AssetBundleModel>(json);
        }

        public static string ToJson(this AssetBundleModel m)
        {
            return JsonUtility.ToJson(m);
        }
    }
}

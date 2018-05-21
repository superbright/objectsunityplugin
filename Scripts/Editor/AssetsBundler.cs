using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetsBundler : MonoBehaviour {

	const string output_path = @"C:\Users\turle\workspace\Objects\AssetBundles";

	static void Bundle () {
		string[] a3dfiles = Directory.GetFiles(Application.dataPath, "*.fbx", SearchOption.AllDirectories);
		string[] daefiles = Directory.GetFiles(Application.dataPath, "*.dae", SearchOption.AllDirectories);


		List<string> allfiles = new List<string>();
		allfiles.AddRange (a3dfiles);
		allfiles.AddRange (daefiles);

		foreach(string objpath in allfiles)
		{
			Debug.Log ("obj path " + objpath);
			string assetPath = "Assets" + objpath.Replace(Application.dataPath, "").Replace("\\", "/");
			GameObject gobj = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
			// .. do whatever you like
			Debug.Log ("gobj " + objpath);
		}

         var args = System.Environment.GetCommandLineArgs ();
        var timestamp = args[8];
        Debug.Log("-----------Name------------");
        Debug.Log(timestamp);
        
		AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

		string[] assetnames = new string[allfiles.Count];

		for (int i = 0; i < allfiles.Count; i++)
		{
			string assetPath = "Assets" + allfiles[i].Replace(Application.dataPath, "").Replace("\\", "/");
			Debug.Log ("path " + assetPath);
			assetnames[i] = assetPath;
		}

		buildMap[0].assetNames = assetnames;
		buildMap[0].assetBundleName = timestamp + ".android";
		Debug.Log ("buildmap " + buildMap);

		var output_dir = output_path + timestamp;

		
		if (!Directory.Exists (output_dir))
			Directory.CreateDirectory (output_dir);
		
		//BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.None,BuildTarget.StandaloneOSXUniversal);
		//Old version
		// BuildPipeline.BuildAssetBundles(output_path, buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
		
        BuildPipeline.BuildAssetBundles(output_dir, buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
	}
}

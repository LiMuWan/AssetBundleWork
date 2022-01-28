using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.Linq;


public class BuildAssetBundle 
{

    private static string GetMD5(string fileName)
    {
        string result = string.Empty;
        using(FileStream fs = File.OpenRead(fileName))
        {
            result = BitConverter.ToString(new MD5CryptoServiceProvider().
                ComputeHash(fs));
            fs.Close();
        }
        return result;
    }

    [MenuItem("AssetBundle/Build")]
    public static void Build()
    {
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/../AssetBundles",BuildAssetBundleOptions.CompleteAssets,BuildTarget.StandaloneWindows);

        List<string> files = Directory.GetFiles(Application.dataPath + "/../AssetBundles","*.ab").ToList();
        files.Add(Application.dataPath + "/../AssetBundles/AssetBundles");

        FileList fileList = new FileList();

        foreach (string file in files)
        {
            fileList.nameList.Add(new FileInfo(file).Name);
            fileList.md5List.Add(GetMD5(file));
        }

        FileList.Save(Application.dataPath + "/../AssetBundles/filelist.json", fileList);

        if (Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.Delete(Application.streamingAssetsPath);
        }
        if(!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        
        AssetDatabase.Refresh();
    }
}

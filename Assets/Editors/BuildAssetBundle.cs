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
        // 在我们加载（资源已经在本地了）资源有两种方式：
        //1. 异步加载AssetBundle,会造成时序上的困扰（时序不容易控制）
        //2. 同步加载AssetBundle.LoadFromFile:资源打包时必须使用BuildAssetBundleOptions.
        //UncompressedAssetBundle选项
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/../AssetBundles",BuildAssetBundleOptions.UncompressedAssetBundle,BuildTarget.StandaloneWindows);

        //在资源打包时，资源的路径有一个规则，必须是以Application.dataPath为根目录的相对路径
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

    public static string outputPath = Application.dataPath + "/StreamingAssets";
    private static string inputPath = Application.dataPath + "/MyScripts";

    public static void BuildLuaResource()
    {
        AssetBundleBuild abLua = new AssetBundleBuild();
        abLua.assetBundleName = "Lua";
        string[] files = Directory.GetFiles(inputPath, "*.bytes");
        //在资源打包时，资源的路径有一个规则：必须是以Appliacation.dataPath为根目录
        //的相对路径
        for (int i = 0; i < files.Length; i++)
        {
            //E:\king\students\download\Assets\MyScripts\Player.bytes
            //=>Assets/MyScripts.bytes
            files[i] = "Assets" + files[i].Replace(Application.dataPath, "").Replace("\\", "/");
        }
        abLua.assetNames = files;
        AssetBundleBuild[] buildMap = new AssetBundleBuild[] { abLua };

        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
}

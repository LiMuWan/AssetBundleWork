using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadAssets : MonoBehaviour
{
    public Text text_loading;
    private WWW webRequest;
    //private WWW www; //WWW不能共用
    private string server_url = "http://192.168.20.168:6688/AssetBundles/";
    // Start is called before the first frame update
    IEnumerator Start()
    {
        //热更新流程解决方案
        //1.一般首次加载的时候，会把StreamingAssets的目录下的资源
        // 移动到PersistentDataPath目录
        if(PlayerPrefs.GetInt("IsFirstLoad",1) == 1)
        {
            //CopyDirectory(Application.streamingAssetsPath, Application.persistentDataPath);
            PlayerPrefs.SetInt("IsFirstLoad", 0);
        }
        //2.与服务器版本比对，检查是否需要更新，如果需要更新，就下载资源

        //2.1 读取本地的fileList.json 
        FileList localFileList = FileList.Load(Application.persistentDataPath + "fileList.json");
        //2.2 下载服务器上的fileList，读取出来，不需要保存到本地，所以可以用WWW
        WWW downLoadwww = new WWW("http://192.168.20.18:6688/AssetBundles/fileList.json");
        yield return downLoadwww;
        //2.3 比较服务器和本地的MD5编码，以服务器为准
        FileList serverFileList = JsonUtility.FromJson<FileList>(downLoadwww.text);
        for (int indexServer = 0; indexServer < serverFileList.md5List.Count; indexServer++)
        {
            string nameServer = serverFileList.nameList[indexServer];
            int indexLocal = localFileList.nameList.IndexOf(nameServer);
            if (indexLocal == -1)
            {
                //Debug.Log(string.Format("服务器增加了{0}文件", nameServer));
                string md5Server = serverFileList.md5List[indexServer];
                StartCoroutine(AppendAB(nameServer, localFileList, md5Server));
            }
            else
            {
                //对比md5码
                string serverMd5 = serverFileList.md5List[indexServer];
                string localMd5 = localFileList.md5List[indexLocal];
                if(!string.Equals(serverMd5,localMd5))
                {
                    //Debug.Log(string.Format("服务器此{0}文件已更新", nameServer));
                    StartCoroutine(UpdateAB(nameServer, localFileList, serverMd5, indexLocal));
                }
            }
        }

        //以客户端为基准进行对比，从而检测服务器删除了哪些文件
        for (int indexLocal = 0; indexLocal < serverFileList.md5List.Count; indexLocal++)
        {
            string nameLocal = localFileList.nameList[indexLocal];
            int indexServer = localFileList.nameList.IndexOf(nameLocal);
            if (indexServer == -1)
            {
                //Debug.Log(string.Format("服务器减少了{0}文件", nameLocal));
                string md5Local = localFileList.md5List[indexLocal];
                DeleteAB(nameLocal, md5Local, localFileList);
            }
        }
        //2.4 如果有差异，从服务器重新下载
        //2.5 下载差异部分后，本地fileList需要更新成与服务器完全一致
        //3.学习情况下，就学下载资源就可以了
        //为什么使用协同，因为协同时和主线程并行的，主线程就相当于一条流水线
        //它会一圈一圈的跑，每次执行到协同的时候，都会调用一下我们的协同程序
        //而yield 的会返回当前帧，下一次执行过来的时候会从当前协同上次yield的
        //下一句执行
        StartCoroutine(DownloadAndLoad());
    }

    /// <summary>
    /// 更新资源：传入资源名称，从服务器下载下来的资源来覆盖本地的同名资源，
    /// 并用服务器资源的MD5编码刷新本地FileList中对应的资源的MD5码
    /// </summary>
    /// <param name="nameServer">服务器上的ab名</param>
    /// <param name="fileListLocal">本地FileList</param>
    /// <param name="serverMd5">服务器上的AB的MD5</param>
    /// <param name="indexLocal">本地AB在FileList中存储的位置序号</param>
    private IEnumerator UpdateAB(string nameServer, FileList localFileList, string serverMd5, int indexLocal)
    {
        string path = Application.persistentDataPath + "/" + nameServer;
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        string manifestPath = Application.persistentDataPath + "/" + nameServer + ".manifest";
        if (File.Exists(manifestPath))
        {
            File.Delete(manifestPath);
        }

        WWW www = new WWW(server_url + nameServer);
        yield return www;
        using(FileStream fs = File.OpenWrite(Application.persistentDataPath + "/" + nameServer))
        {
            fs.Write(www.bytes, 0, www.bytes.Length);
            fs.Close();
        }
        www = new WWW(server_url + nameServer +".manifest");
        yield return www;
        using (FileStream fs = File.OpenWrite(Application.persistentDataPath + "/" + nameServer + ".manifest"))
        {
            fs.Write(www.bytes, 0, www.bytes.Length);
            fs.Close();
        }
        localFileList.nameList[indexLocal] = nameServer;
        localFileList.md5List[indexLocal] = serverMd5;
        FileList.Save(Application.persistentDataPath + "/" + "fileList.json", localFileList);
    }

    /// <summary>
    /// 删除本地的AB资源
    /// </summary>
    /// <param name="nameLocal">本地的ab名</param>
    /// <param name="localFileList">本地FileList</param>
    private void DeleteAB(string nameLocal,string md5Local, FileList localFileList)
    {
        string path = Application.persistentDataPath + "/" + nameLocal;
        if(File.Exists(path))
        {
            File.Delete(path);
        }
        string manifestPath = Application.persistentDataPath + "/" + nameLocal + ".manifest";
        if (File.Exists(manifestPath))
        {
            File.Delete(manifestPath);
        }
        localFileList.nameList.Remove(nameLocal);
        localFileList.md5List.Remove(md5Local);
        FileList.Save(Application.persistentDataPath + "/" + "fileList.json", localFileList);
    }

    /// <summary>
    /// 追加服务器上新增的ab资源,传入资源名称，从服务器下载该ab资源到本地
    /// 并且将ab信息（名字，MD5）追加到本地FileList
    /// </summary>
    /// <param name="nameServer">服务器ab名</param>
    /// <param name="localFileList">本地FileList</param>
    /// <param name="md5Server">服务器ab的MD5</param>
    private IEnumerator AppendAB(string nameServer, FileList localFileList, string md5Server)
    {
        WWW www = new WWW(server_url + nameServer);
        yield return www;
        using(FileStream fs = File.OpenWrite(Application.persistentDataPath + "/" + nameServer))
        {
            fs.Write(www.bytes, 0, www.bytes.Length);
            fs.Close();
        }

        www = new WWW(server_url + nameServer + ".manifest");
        yield return www;
        using (FileStream fs = File.OpenWrite(Application.persistentDataPath + "/" + nameServer + ".manifest"))
        {
            fs.Write(www.bytes, 0, www.bytes.Length);
            fs.Close();
        }

        localFileList.nameList.Add(nameServer);
        localFileList.md5List.Add(md5Server);
        FileList.Save(Application.persistentDataPath + "/" + "fileList.json", localFileList);
    }

    // Update is called once per frame
    void Update()
    {
        if (webRequest == null) return;
        text_loading.text = webRequest.progress.ToString();
    }

    private void Load(string abName, string assetName)
    {
        //yield break;//中断当前协同
        ////先下载总包
        //webRequest = new WWW("http://192.168.20.168:6688/AssetBundles/AssetBundles");
        //yield return webRequest;//等待下载结束
        //AssetBundleManifest assetBundleManifest = webRequest.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        //获取所有的依赖项
        AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.persistentDataPath + "/AssetBundles");
        AssetBundleManifest assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] deps = assetBundleManifest.GetAllDependencies(abName);
        foreach (string dep in deps)
        {
            //webRequest = new WWW("http://192.168.20.168:6688/AssetBundles/"+ dep);
            //yield return webRequest;//等待下载结束
            AssetBundle abDep = AssetBundle.LoadFromFile(Application.persistentDataPath + "/" + dep);
        }
        //webRequest = new WWW("http://192.168.20.168:6688/AssetBundles/material-bundle");
        //yield return webRequest;//等待下载结束
        //webRequest.assetBundle.LoadAllAssets();//加载所有的ab，没必要，耗性能

        AssetBundle cubeAb = AssetBundle.LoadFromFile(Application.persistentDataPath + "/" + abName);
        //webRequest = new WWW("http://192.168.20.168:6688/AssetBundles/cube-bundle");
        //yield return webRequest;//等待下载结束

        //载入预制体
        //GameObject cubePrefab = webRequest.assetBundle.LoadAsset<GameObject>("MyCube.prefab");
        GameObject cubePrefab = cubeAb.LoadAsset<GameObject>(assetName);
        Instantiate(cubePrefab);
        cubeAb.Unload(false);
        //webRequest.assetBundle.Unload(false);
    }
}

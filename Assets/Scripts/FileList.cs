using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class FileList
{
    public List<string> nameList = new List<string>();
    public List<string> md5List = new List<string>();

    public static FileList Load(string file)
    {
        string jsonString = File.ReadAllText(file);
        return JsonUtility.FromJson<FileList>(jsonString);
    }

    public static void Save(string fileName,FileList fileList)
    {
        string fileJson = JsonUtility.ToJson(fileList);
        File.WriteAllText(fileName, fileJson);
    }
}
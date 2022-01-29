using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ABInfo 
{
    public string abName;
    public string md5;
    public int version;
}

[Serializable]
public class ABInfoList
{
    public List<ABInfo> list = new List<ABInfo>();
}


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class LuaGameController : MonoBehaviour
//{
//    public static LuaGameController instance;

//    public LuaState lua;

//    private void Awake()
//    {
//        instance = this;
//    }

//    public string LoadScriptFromAB(string moduleName)
//    {
//        string s = string.Empty;//要返回的脚本内容
//        //如果加载本地的不用协同
//        AssetBundle ab = AssetBundle.LoadFromFile(Application.persistentDataPath + "lua.ab");
//        //判断加载完
//        if(ab)
//        {
//            //lua脚本是text资源
//            s = ab.LoadAsset<TextAsset>(moduleName).text;
//            Debug.Log("Script content");
//            //如果不卸载，下次加载unity会抛异常
//            ab.Unload(false);
//        }
//        else
//        {
//            Debug.LogWarning(string.Format("Load{0}failed!", moduleName));
//        }
//        return s;
//    }

//    public void Reload()
//    {
//        string s = LoadScriptFromAB(moduleName);
//        table = (LuaTable)luaState.DoFile("moduleName"+".lua")[0];
//        table = luaState.DoString(s, "LuaBridge.cs")[0];

//    }
//}

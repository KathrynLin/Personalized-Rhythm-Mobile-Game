using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using UnityEditor.Rendering.Universal;

public class SheetLoader : MonoBehaviour
{
    static SheetLoader instance;
    public static SheetLoader Instance
    {
        get
        {
            return instance;
        }
    }

    public string pathExternal;
    string streamingAssetsPath = $"{Application.streamingAssetsPath}/Sheet";
    public int sheetCount = 0;
    public bool bLoadFinish = false;
    private static Mutex sheetLock = new Mutex();
    int remain = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        // hardcoded internal sheet count
        sheetCount += 1;

        pathExternal = $"{Application.persistentDataPath}/Sheet";

        if (Directory.Exists(pathExternal))
        {
            DirectoryInfo d = new DirectoryInfo(pathExternal);
            sheetCount += d.GetDirectories().Length;
            remain += d.GetDirectories().Length;
            //StartCoroutine(IELoadExternal());
        }
        else
        {
            Directory.CreateDirectory(pathExternal);
        }
        StartCoroutine(IELoadInternal());
    }

    IEnumerator IELoadInternal()
    {
        //sheetLock.WaitOne();
        /**
         * FIXME: We can actually use an AssetBundle to make this more elegant
         */
        yield return Parser.Instance.IEParseInternal("Distorted Fate"); // load hardcoded internal sheet
        GameManager.Instance.sheets.Add("Distorted Fate", Parser.Instance.sheet);
        yield return Parser.Instance.IEParseInternal("Zattou Bokuranomachi"); // load hardcoded internal sheet
        GameManager.Instance.sheets.Add("Zattou Bokuranomachi", Parser.Instance.sheet);
        yield return Parser.Instance.IEParseInternal("Life is PIANO Junk"); // load hardcoded internal sheet
        GameManager.Instance.sheets.Add("Life is PIANO Junk", Parser.Instance.sheet);
        yield return Parser.Instance.IEParseInternal("Fallen Symphony"); // load hardcoded internal sheet
        GameManager.Instance.sheets.Add("Fallen Symphony", Parser.Instance.sheet);
        //sheetLock.ReleaseMutex();


        DirectoryInfo di = new DirectoryInfo(pathExternal);
        foreach (DirectoryInfo d in di.GetDirectories())
        {
            sheetLock.WaitOne();
            {
                yield return Parser.Instance.IEParseExternal(d.Name);
                GameManager.Instance.sheets.Add(d.Name, Parser.Instance.sheet);
            }
            sheetLock.ReleaseMutex();
        }
        bLoadFinish = true;

        yield return null;
    }

    IEnumerator IELoadExternal()
    {


        // TODO: remove this
        yield return null;
    }
}

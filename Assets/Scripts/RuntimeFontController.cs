using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.IO;

using TMPro;

using UnityEngine;

// Singleton
public class RuntimeFontController : MonoBehaviour
{
    private static RuntimeFontController instance;
    public static RuntimeFontController Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("Initialized RuntimeFontController (Singleton) on static property");
                instance = (RuntimeFontController)FindObjectOfType(typeof(RuntimeFontController));

                assetizeUserFonts();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            Debug.Log("Initialized RuntimeFontController (Singleton) on Awake()");
            instance = this;

            assetizeUserFonts();
        }
    }

    static string userFontsDirPath = "";
    static List<string> userFontPaths  = new List<string>();
    static List<TMP_FontAsset> userFontAssets = new List<TMP_FontAsset>();

    public static void assetizeUserFonts()
    {
        userFontsDirPath = Application.dataPath + "/UserConfig/UserFonts";
        userFontPaths = new List<string>();

        IEnumerable<string> ttf_filepaths = Directory.GetFiles(userFontsDirPath, "*.ttf").Select(x => userFontsDirPath + "/" + Path.GetFileName(x));
        foreach (var ttf_filepath in ttf_filepaths) userFontPaths.Add(ttf_filepath);
        IEnumerable<string> otf_filepaths = Directory.GetFiles(userFontsDirPath, "*.otf").Select(x => userFontsDirPath + Path.GetFileName(x));
        foreach (var otf_filepath in otf_filepaths) userFontPaths.Add(otf_filepath);

        foreach (var fontPath in userFontPaths)
        {
            Font font = new Font(fontPath);
            userFontAssets.Add(TMP_FontAsset.CreateFontAsset(font));
        }
    }

    // AddUserFontsToFallback() が既に呼ばれたかどうか
    bool alreadyCalled = false;
    // add user fonts to the prefab's fallback
    public void AddUserFontsToFallback(TMP_FontAsset prefabFont)
    {
        if (!alreadyCalled)
        {
            alreadyCalled = true;

            foreach (var font in userFontAssets)
            {
                prefabFont.fallbackFontAssetTable.Add(font);
                Debug.Log($"Added {font.name} to fallback.");
            }
        }
    }
}

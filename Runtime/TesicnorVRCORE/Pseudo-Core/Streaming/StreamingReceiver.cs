using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StreamingReceiver : MonoBehaviour
{
    #region PARAMETERS
    public string path;
    public Material screen;
    #endregion

    #region FUNCTIONS
    private void Update()
    {
        screen.mainTexture = GetTextureFromFile();
    }
    Texture2D GetTextureFromFile()
    {
        Texture2D result = new Texture2D(256,256, TextureFormat.ARGB32, false);
        if (isThereAFile())
        {
            result.LoadImage(File.ReadAllBytes(path));
            result.Apply();
        }

        return result;
    }
    bool isThereAFile()
    {
        return File.Exists(path);
    }
    #endregion
}

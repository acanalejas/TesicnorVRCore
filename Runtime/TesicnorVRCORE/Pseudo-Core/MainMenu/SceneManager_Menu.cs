using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManager_Menu 
{
    #region PARAMETERS
    public static int currentLoadedSceneCount = 0;
    #endregion

    #region FUNCTIONS
    /// <summary>
    /// Va a la escena del nombre que le digamos
    /// </summary>
    /// <param name="sceneName"></param>
    private static void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    /// <summary>
    /// Va a la escena del indice que le digamos
    /// </summary>
    /// <param name="index"></param>
    private static void ChangeScene(int index)
    {
        SceneManager.LoadScene(index);
    }
    /// <summary>
    /// Va a la escena que sea combinacion del nombre de seccion y de la propia escena
    /// Tener en cuenta que hay un espacio entre ambos textos por defecto
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="section"></param>
    private static void ChangeScene(string sceneName, string section)
    {
        SceneManager.LoadScene(section + " " + sceneName);
    }

    /// <summary>
    /// Cambia de escena teniendo de referencia el SO SceneData que le pasemos
    /// </summary>
    /// <param name="data"></param>
    public static void ChangeScene(SceneData data)
    {
        SceneManager.LoadScene(data.SceneID);
    }

    /// <summary>
    /// Vuelve a lanzar la escena activa
    /// </summary>
    public static void ResetScene()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(0);
        SceneManager.LoadScene(index);
    }

    /// <summary>
    /// Vuelve al menu principal
    /// </summary>
    public static void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }
    #endregion
}

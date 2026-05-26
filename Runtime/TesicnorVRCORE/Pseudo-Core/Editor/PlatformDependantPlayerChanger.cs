#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlatformDependantPlayerChanger
{
    private static string scenesPath = "Assets/Scenes";
    
    static List<string> scenesList = new List<string>();

    static PlatformDependantPlayerChanger()
    {
        //BuildProfileChangeDetector.onBuildProfileChanged += (s, type) => { UpdatePlayers(); };
    }
    [MenuItem("Tesicnor/UpdatePlayers")]
    public static void UpdatePlayers()
    {
        scenesList.Clear();

        if (!Directory.Exists(scenesPath))
        {
            Debug.LogError("El directorio de escenas : " + scenesPath + " no existe, introduzca uno válido");
            return;
        }
        
        var subdirectorios = Directory.EnumerateDirectories(scenesPath);
        
        scenesList.AddRange(ScenesInDirectory(scenesPath));

        foreach (var sd in subdirectorios)
        {
            scenesList.AddRange(ScenesInDirectory(sd));
        }

        var initialScene = EditorSceneManager.GetActiveScene();
        foreach (var scene in scenesList)
        {
            OpenScene(scene);
            SearchForPlayers();
            SaveChangesOnScene();
        }

        EditorSceneManager.OpenScene(initialScene.path, OpenSceneMode.Single);
    }

    static List<string> ScenesInDirectory(string _directory)
    {
        var escenas = Directory.EnumerateFiles(_directory, "*.unity");
        List<string> validScenes = new List<string>();
        foreach (var escena in escenas)
        {
            validScenes.Add(escena.Replace('\\', '/'));
        }
        
        return validScenes;
    }

    static void DebugScenesList(List<string> list)
    {
        foreach (var s in list)
        {
            Debug.Log(s);
        }
    }

    static void OpenScene(string _scene)
    {
        EditorSceneManager.CloseScene(EditorSceneManager.GetActiveScene(), true);
        EditorSceneManager.OpenScene(_scene, OpenSceneMode.Single);
    }

    static void SearchForPlayers()
    {
        PlatformPlayer[] players = GameObject.FindObjectsByType<PlatformPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var player in players)
        {
            player.gameObject.SetActive(false);
            if(player.Platform == BuildProfileChangeDetector.currentPlatform) player.gameObject.SetActive(true);
        }
    }

    static void SaveChangesOnScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
#endif
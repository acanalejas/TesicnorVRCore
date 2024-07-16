using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif
using System.IO;

public class ConfigManager : MonoBehaviour
{
    #region PARAMETERS

    #endregion

    #region METHODS

    #endregion
}

/// <summary>
/// Atributo que se usa para exclusivamente bools static, que nos añade los eventos necesarios de configuracion a la clase deseada
/// </summary>
public class EventHandlerAttribute : Attribute
{

    public EventHandlerAttribute(string boolName, string className, bool isStatic, string FileName = "ConfigEventsManager.cs")
    {
        string path = Application.dataPath + "/" + FileName;

        FileStream stream = OverrideCode.BothStream(path);

        bool classExists = OverrideCode.bFileContainsClass(stream, path, "ConfigEventsManager");

        if (!classExists) { FileStream stream_2 = OverrideCode.BothStream(path); OverrideCode.AddClassToFile(stream_2, path, "ConfigEventsManager", true); }

        FileStream stream_3 = OverrideCode.BothStream(path);
        OverrideCode.AddField(stream_3, "on" + boolName + "Event", "", "ConfigEventsManager", "UnityEvent<bool>", true);
        stream_3.Close();
        FileStream stream_4 = OverrideCode.BothStream(path);
        OverrideCode.AddCodeToMethod(stream_4, "Start", "on" + boolName + "Event" + ".Invoke(" + (isStatic ? (className + "." + boolName) : ("FindObjectByType<" + className + ">()." + boolName)) + ");", "ConfigEventsManager");
        stream_4.Close();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ConfigManager), true)]
public class ConfigManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TypeCache.FieldInfoCollection fic = TypeCache.GetFieldsWithAttribute(typeof(EventHandlerAttribute));
        TypeCache.MethodCollection mc = TypeCache.GetMethodsWithAttribute(typeof(EventHandlerAttribute));

        foreach (var f in fic)
        {
            f.GetCustomAttribute(typeof(EventHandlerAttribute));
        }

        foreach (var m in mc)
        {
            m.GetCustomAttribute(typeof(EventHandlerAttribute));
        }
    }

    private void OnEnable()
    {
        
    }
}
#endif

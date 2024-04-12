using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BackendConstants
{
    /// <summary>
    /// Url usada para obtener los datos generales de usuario y experiencias adquiridas
    /// </summary>
    public static string urlNoParams { get { return "https://app.e-xtinguisher.com/api/vr-users/public?"; } }

    /// <summary>
    /// Url base usada para a�adir el campo de par�metro para recibir datos especificos de un usuario
    /// </summary>
    public static string urlForParams { get { return "https://app.e-xtinguisher.com/api/public/"; } }

    /// <summary>
    /// Par�metro de tiempo de uso del usuario
    /// </summary>
    public static string TimeParam { get { return "client-time-uses?"; } }

    /// <summary>
    /// Par�metro para el post del tiempo de uso
    /// </summary>
    public static string TimePostParam { get { return "usage-records?"; } }

    /// <summary>
    /// Url completa usada para obtener el tiempo de uso
    /// </summary>
    public static string urlForTime { get { return urlForParams + TimeParam; } }

    /// <summary>
    /// Url para enviar los datos de tiempo de uso
    /// </summary>
    public static string urlForPostTime { get { return urlForParams + TimePostParam; } }

    /// <summary>
    /// Key del player prefs para los datos generales de usuario
    /// </summary>
    public static string BackendDataKey { get { return "BackendKey"; } }

    /// <summary>
    /// Key del player prefs para los datos de tiempo de uso del usuario
    /// </summary>
    public static string BackendTimeDataKey { get { return "BackendTimeKey"; } }

    /// <summary>
    /// Key para cuando se intenta acceder con una key incorrecta al guardado de datos
    /// </summary>
    public static string IncorrectKey { get { return "IncorrectKey"; } }

    /// <summary>
    /// Tipo de usuario que funciona con tiempo de uso
    /// </summary>
    public static string TimeType { get { return "TIME_TYPE"; } }

    /// <summary>
    /// Tipo de usuario que NO funciona con tiempo de uso
    /// </summary>
    public static string NoLimitType { get { return "NO_LIMIT_TYPE"; }}

    /// <summary>
    /// La aplicaci�n tiene conexion a internet?
    /// </summary>
    public static bool bHasInternetConnection { get { return Application.internetReachability != NetworkReachability.NotReachable; } }

    public static string TimeQueueKey { get { return "jsonDatos"; } }

    public static string DataOnDisableKey { get { return "DataOnDisable"; } }
}

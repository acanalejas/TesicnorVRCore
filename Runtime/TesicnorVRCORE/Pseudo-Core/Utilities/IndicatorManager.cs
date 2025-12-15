using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    #region PARAMETERS
    [Header("La lista en orden de los indicadores de la escena")]
    [SerializeField] List<string> IndicatorsNames = new List<string>();
    [Header("La lista en el mismo orden de los objetos instanciados")]
    [SerializeField] List<GameObject> IndicatorsGOs = new List<GameObject>();

    protected Dictionary<string, GameObject> Indicators = new Dictionary<string, GameObject>();
    #endregion
    #region METHODS

    //Metodos propios de Unity
    protected virtual void Awake()
    {
        CheckSingleton();
        FillDictionary();
    }

    //Metodos de la clase

    protected virtual void FillDictionary()
    {
        if (IndicatorsNames.Count <= 0) { Debug.LogError("La lista de nombres no puede estar vacía"); return; }
        if (IndicatorsGOs.Count <= 0) { Debug.LogError("La lista de los objetos no puede estar vacía"); return; }
        if (IndicatorsNames.Count != IndicatorsGOs.Count) { Debug.LogError("Las listas de nombres y objetos deben tener la misma longitud"); return; }

        for (int i = 0; i < IndicatorsNames.Count; i++)
        {
            Indicators.Add(IndicatorsNames[i], IndicatorsGOs[i]);
        }
    }

    public virtual void EnableIndicator(string name)
    {
        GameObject ind = null;
        bool isValid = Indicators.TryGetValue(name, out ind);

        if (!isValid) { Debug.LogError("El valor de indicador " + name + " no existe, introduzca uno valido añadido a la lista"); return; }

        ind.SetActive(true);
    }

    public virtual void DisableIndicator(string name)
    {
        GameObject ind = null;
        bool isValid = Indicators.TryGetValue(name, out ind);

        if (!isValid) { Debug.LogError("El valor de indicador " + name + " no existe, introduzca uno valido añadido a la lista"); return; }

        ind.SetActive(false);
    }

    public virtual void ToggleIndicator(string name)
    {
        GameObject ind = null;
        bool isValid = Indicators.TryGetValue(name, out ind);

        if (!isValid) { Debug.LogError("El valor de indicador " + name + " no existe, introduzca uno valido añadido a la lista"); return; }

        ind.SetActive(!ind.activeSelf);
    }

    public void DisableAllIndicators()
    {
        foreach(var pair in Indicators)
        {
            if(pair.Value != null)
                pair.Value.SetActive(false);
        }
    }

    public virtual void EnableAllIndicators()
    {
        foreach(var pair in Indicators)
        {
            if (pair.Value != null)
                pair.Value.SetActive(true);
        }
    }

    public virtual void DisableAllAndEnableOne(string name)
    {
        DisableAllIndicators();
        EnableIndicator(name);
    }

    public virtual void EnableAllIndicatorsDisableOne(string name)
    {
        EnableAllIndicators();
        DisableIndicator(name);
    }

    #endregion
    #region SINGLETON
    protected static IndicatorManager instance;
    public static IndicatorManager Instance { get { return instance; } }

    protected void CheckSingleton()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    #endregion
}

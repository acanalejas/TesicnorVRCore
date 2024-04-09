using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveData_PP 
{
    #region PARAMETERS

    #endregion

    #region FUNCTIONS
    #region Setting Data
    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public static void SetBool(string key, bool value)
    {
        int _value = 0;
        if (value) _value = 1;

        PlayerPrefs.SetInt(key,_value);
    }

    public static void SetVector3(string key, Vector3 value)
    {
        float x = value.x;
        float y = value.y;
        float z = value.z;

        string _value = x.ToString() + "|" + y.ToString() + "|" + z.ToString();
        PlayerPrefs.SetString(key, _value);
    }

    public static void SetVector2(string key, Vector2 value)
    {
        float x = value.x;
        float y = value.y;

        string _value = x.ToString() + "|" + y.ToString();
        PlayerPrefs.SetString(key,_value);
    }

    public static void SetStringList(string key, List<string> value)
    {
        string result = "";

        foreach(string s in value)
        {
            result += s + "|";
        }

        PlayerPrefs.SetString(key, result);
    }

    #endregion

    #region Getting Data
    public static int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public static float GetFloat(string key)
    {
        return PlayerPrefs.GetFloat(key);
    }

    public static string GetString(string key)
    {
        return PlayerPrefs.GetString(key);
    }

    public static bool GetBool(string key)
    {
        bool result = false;
        int _result = PlayerPrefs.GetInt(key);

        if (_result == 1) result = true;

        return result;
    }

    public static Vector3 GetVector3(string key)
    {
        Vector3 result = Vector3.zero;

        string _value = PlayerPrefs.GetString(key);
        string[] _values = _value.Split("|");

        if(_values.Length == 3)
        {
            float x; float.TryParse(_values[0], out x);
            float y; float.TryParse(_values[1], out y);
            float z; float.TryParse(_values[2], out z);

            result = new Vector3(x, y, z);
        }

        return result;
    }

    public static Vector2 GetVector2(string key)
    {
        Vector2 result = Vector2.zero;

        string _value = PlayerPrefs.GetString(key);
        string[] _values = _value.Split("|");
        
        if(_values.Length == 2)
        {
            float x; float.TryParse(_values[0], out x);
            float y; float.TryParse(_values[1], out y);

            result = new Vector2(x, y);
        }

        return result;
    }

    public static List<string> GetStringList(string key)
    {
        List<string> result = new List<string>();

        string storedValue = PlayerPrefs.GetString(key);

        string[] split = storedValue.Split('|');

        foreach(string s in split)
        {
            if(!string.IsNullOrEmpty(s)) result.Add(s);
        }

        return result;
    }

    #endregion
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keyboard_Local : MonoBehaviour
{
    #region PARAMETERS
    [Header("El campo donde se va a escribir")]
    public VRInteractable_InputField inputField;

    [Header("El numpad al que se cambia")]
    public GameObject NumPad;

    [Header("El objeto que contiene el teclado")]
    public GameObject Kyeboard;

    private bool shift = false;

    #endregion

    #region METHODS

    public void Shift()
    {
        shift = true;
    }

    public void SetInputField(VRInteractable_InputField _inputField)
    {
        inputField = _inputField;
    }

    public void SwitchKeyboard()
    {
        if (NumPad.activeSelf) { NumPad.SetActive(false); Kyeboard.SetActive(true); }
        else { NumPad.SetActive(true); Kyeboard.SetActive(false); }
    }

    public void Delete()
    {
        if (!inputField) return;
        string str = inputField.writeText.text;

        string result = "";

        for (int i = 0; i < str.Length - 1; i++) result += str[i];

        inputField.writeText.text = result;
        inputField.onTextWritten.Invoke();
    }

    public void ResetText()
    {
        if (!inputField) return;
        inputField.writeText.text = "";
        inputField.onTextWritten.Invoke();
    }

    public void WriteChar(string s)
    {
        if (inputField == null || string.IsNullOrEmpty(s)) return;
        char c = (char)s[0];
        char input = shift ? char.ToUpper(c) : char.ToLower(c);
        inputField.writeText.text += input;
        inputField.onTextWritten.Invoke();
    }

    #endregion
}

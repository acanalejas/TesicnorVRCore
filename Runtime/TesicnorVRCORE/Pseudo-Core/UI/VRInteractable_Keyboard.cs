using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;

public class VRInteractable_Keyboard : MonoBehaviour
{
    #region SINGLETON
    private static VRInteractable_Keyboard instance;
    public static VRInteractable_Keyboard Instance { get { return instance; } }

    void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
    #endregion

    #region PARAMETERS
    public static TextMeshProUGUI inputfieldText;

    public static bool shifted;

    public Dictionary<VRInteractable_Button, KeyboardButton> buttons = new Dictionary<VRInteractable_Button, KeyboardButton>();
    #endregion

    #region FUNCTIONS
    public static void setInputField(VRInteractable_InputField inputField)
    {
        inputfieldText = inputField.writeText;
        Instance.PrepareButtons();
    }

    private void Awake()
    {
        CheckSingleton();
    }


    private void PrepareButtons()
    {
        if (buttons.Count <= 0) return;
        foreach(var button in buttons)
        {
            button.Key.onClick.AddListener(button.Value.WriteChar);
        }
    }

    private void PressKeyboardSwitch(KeyboardButton keyboardButton)
    {
        if (inputfieldText != null)
            inputfieldText.text += keyboardButton.character;
    }

    public void EnableKeyboard(bool enabled)
    {
        this.gameObject.SetActive(enabled);
    }
    #endregion
}
#if UNITY_EDITOR
[CustomEditor(typeof(VRInteractable_Keyboard), true)]
[DisallowMultipleComponent]
[CanEditMultipleObjects]
public class VRInteractable_Keyboard_Editor: Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var keyboard = target as VRInteractable_Keyboard;

        var buttons = keyboard.GetComponentsInChildren<VRInteractable_Button>();
        var keyboardButtons = new List<KeyboardButton>();

        foreach(var button in buttons)
        {
            if (button.gameObject.GetComponent<KeyboardButton>() != null) continue;
            KeyboardButton kb = button.gameObject.AddComponent<KeyboardButton>();
            keyboardButtons.Add(kb);
            keyboard.buttons.Add(button, kb);
        }

    }
}
#endif

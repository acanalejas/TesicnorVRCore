using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardButton : MonoBehaviour
{
    [Header("El caracter que marca la tecla")]
    public char character;


    private void Start()
    {
        GetComponent<VR_Interactable>().onRelease.AddListener(WriteChar);
    }

    public void WriteChar()
    {
        if (VRInteractable_Keyboard.inputfieldText == null) return;
        if (character != 'ç' && character != 'º')
        {
            if (!VRInteractable_Keyboard.shifted)
                VRInteractable_Keyboard.inputfieldText.text += character;
            else { VRInteractable_Keyboard.inputfieldText.text += char.ToUpper(character); VRInteractable_Keyboard.shifted = false; }
        }
            
        if (character == 'ç')
        {
            char[] buffer = VRInteractable_Keyboard.inputfieldText.text.ToCharArray();

            VRInteractable_Keyboard.inputfieldText.text = "";
            for (int i = 0; i < buffer.Length - 1; i++)
            {
                VRInteractable_Keyboard.inputfieldText.text += buffer[i];
            }


        }
        if(character == 'º')
        {
            VRInteractable_Keyboard.shifted = true;
        }
    }
}

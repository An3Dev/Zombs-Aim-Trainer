using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Keybinds : MonoBehaviour
{
    public Dictionary<string, KeyCode> keybindsDictionary = new Dictionary<string, KeyCode>();

    [SerializeField] GameObject keybindButtonsContainer;

    private void Start()
    {
        if (keybindsDictionary.Count <= 0)
        {
            ResetKeybinds();
        }
        RefreshTextValues();
        //// loops through all of the buttons that are parents of the keybind text
        //for(int i = 0; i < keybindButtonsContainer.transform.childCount; i++)
        //{
        //    // gets the text of the button that says what keybind is assigned
        //    Transform text = keybindButtonsContainer.transform.GetChild(0);
        //    keybindsDictionary.Add(text.name, KeyCode.)
        //}


    }

    void RefreshTextValues()
    {
        // loops through all of the buttons that are parents of the keybind text
        for (int i = 0; i < keybindButtonsContainer.transform.childCount; i++)
        {
            // gets the text of the button that says what keybind is assigned
            Transform text = keybindButtonsContainer.transform.GetChild(i).GetChild(0);

            Debug.Log(text.name);
            // gets the key code value that is assigned to this keybind. The text name must be the same as the dictionary key
            text.GetComponent<TextMeshProUGUI>().text = keybindsDictionary[text.name].ToString();
        }
    }

    void ResetKeybinds()
    {
        keybindsDictionary.Add("MoveForward", KeyCode.W);
        keybindsDictionary.Add("MoveBackward", KeyCode.S);
        keybindsDictionary.Add("MoveLeft", KeyCode.A);
        keybindsDictionary.Add("MoveRight", KeyCode.D);
        keybindsDictionary.Add("Reload", KeyCode.R);
        keybindsDictionary.Add("Melee", KeyCode.Tab);
        keybindsDictionary.Add("SlotOne", KeyCode.Alpha1);
        keybindsDictionary.Add("SlotTwo", KeyCode.Alpha2);
        keybindsDictionary.Add("SlotThree", KeyCode.Alpha3);
        keybindsDictionary.Add("SlotFour", KeyCode.Alpha4);
        keybindsDictionary.Add("SlotFive", KeyCode.Alpha5);

    }
}

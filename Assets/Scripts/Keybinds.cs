using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Keybinds : MonoBehaviour
{
    public Dictionary<string, KeyCode> keybindsDictionary = new Dictionary<string, KeyCode>();

    [SerializeField] GameObject keybindButtonsContainer;

    TextMeshProUGUI moveForward, moveBackward, moveLeft, moveRight, reload, melee, slotOne, slotTwo, slotThree, slotFour, slotFive;

    GameObject currentKey;

    public Color32 normalColor, selectedTextColor;
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
        keybindsDictionary.Add("MoveForward", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveForward", "W")));
        keybindsDictionary.Add("MoveBackward", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveBackward", "S")));
        keybindsDictionary.Add("MoveLeft", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveLeft", "A")));
        keybindsDictionary.Add("MoveRight", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MoveRight", "D")));
        keybindsDictionary.Add("Reload", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Reload", "R")));
        keybindsDictionary.Add("Melee", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Melee", "Tab")));
        keybindsDictionary.Add("SlotOne", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SlotOne", "Alpha1")));
        keybindsDictionary.Add("SlotTwo", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SlotTwo", "Alpha2")));
        keybindsDictionary.Add("SlotThree", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SlotThree", "Alpha3")));
        keybindsDictionary.Add("SlotFour", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SlotFour", "Alpha4")));
        keybindsDictionary.Add("SlotFive", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SlotFive", "Alpha5")));
    }

    public void OnGUI()
    {
        if (currentKey != null)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    // Cancel
                    currentKey.transform.GetComponent<TextMeshProUGUI>().color = normalColor;

                    currentKey = null;
                    return;
                }
                //for(int i = 0; i < keybindsDictionary.)
                keybindsDictionary[currentKey.name] = e.keyCode;
                currentKey.GetComponent<TextMeshProUGUI>().text = e.keyCode.ToString();

                currentKey.transform.GetComponent<TextMeshProUGUI>().color = normalColor;
                currentKey = null;
            }
        }
    }

    public void ChangeKey(GameObject clicked)
    {
        currentKey = clicked;
        clicked.transform.GetComponent<TextMeshProUGUI>().color = selectedTextColor;
    }

    public void SaveKeys()
    {
        foreach(var key in keybindsDictionary)
        {
            PlayerPrefs.SetString(key.Key, key.Value.ToString());
        }
        PlayerPrefs.Save();
    }


    public void LoadKeys()
    {

    }
}

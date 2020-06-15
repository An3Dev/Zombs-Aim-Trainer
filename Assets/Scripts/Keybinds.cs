using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
public class Keybinds : MonoBehaviour
{
    public Dictionary<string, KeyCode> keybindsDictionary = new Dictionary<string, KeyCode>();

    [SerializeField] GameObject keybindButtonsContainer;

    TextMeshProUGUI moveForward, moveBackward, moveLeft, moveRight, reload, melee, slotOne, slotTwo, slotThree, slotFour, slotFive;

    public GameObject currentKey;

    public Color32 normalColor, selectedTextColor;


    Movement thisMovementScript;
    private void Awake()
    {
        if (keybindsDictionary.Count <= 0)
        {
            LoadKeys();
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

    public void Start()
    {
        GetMovementScript();
    }

    void GetMovementScript()
    {
        foreach (PhotonView photonView in PhotonNetwork.PhotonViews)
        {
            if (photonView.IsMine)
            {
                thisMovementScript = photonView.transform.GetComponent<Movement>();
            }
        }
    }

    public KeyCode GetKeybind(string keybindName)
    {
        if (keybindsDictionary[keybindName] != KeyCode.None)
        {
            return keybindsDictionary[keybindName];
        } else
        {
            return KeyCode.None;
        }
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
        keybindsDictionary.Clear();
        LoadKeys();
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
        Debug.Log(clicked);
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
        if (!thisMovementScript)
        {
            thisMovementScript = FindObjectOfType<Movement>();
        }
        thisMovementScript.AssignKeybinds();
    }

    public void LoadKeys()
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
}

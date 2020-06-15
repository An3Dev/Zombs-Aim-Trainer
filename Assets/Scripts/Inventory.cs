using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class Inventory : MonoBehaviour
{

    public List<Item> itemsInInventory = new List<Item>();

    public Item selectedItem;

    public Item P90, Sniper, Scar, Shotgun;

    Transform inventoryLayoutGroup;
    Movement movementScript;
    PhotonView photonView;
    private void Awake()
    {
        inventoryLayoutGroup = GameObject.Find("InventoryLayoutGroup").transform;

        movementScript = transform.root.GetComponent<Movement>();

        photonView = GetComponent <PhotonView>();
        //if (!photonView.IsMine)
        //{
        //    return;
        //}

        AddItem(P90);
        AddItem(Scar);
        AddItem(Shotgun);
        AddItem(Sniper);       

        SelectItem(0);
        movementScript.SetAmmo(true, 0, 0);

        AssignItemUI();
    }
    private void Start()
    {
    }

    public int GetItemIndex(Item item)
    {
        for(int i = 0; i < itemsInInventory.Count; i++)
        {
            if (item == itemsInInventory[i])
            {
                return i;
            }
        }
        return 0;
    }

    public void AddItem(Item item)
    {
        itemsInInventory.Add(item);
    }

    void AssignItemUI()
    {
        for(int i = 0; i < inventoryLayoutGroup.childCount; i++)
        {
            Transform slot = inventoryLayoutGroup.GetChild(i);
            Transform itemNameText = slot.Find("ItemNameText");
            Transform image = slot.Find("ItemImage");
            Transform ammoText = slot.Find("AmmoText");

            // if there is an extra slot, make everything blank
            if (i == itemsInInventory.Count)
            {
                itemNameText.GetComponent<TextMeshProUGUI>().text = "";
                image.GetComponent<Image>().sprite = null;
                ammoText.GetComponent<TextMeshProUGUI>().text = "";
                return;
            }

            itemNameText.GetComponent<TextMeshProUGUI>().text = itemsInInventory[i].itemName;            
            image.GetComponent<Image>().sprite = itemsInInventory[i].topViewSprite;
            // Gets the index of the current item.
            ammoText.GetComponent<TextMeshProUGUI>().text = movementScript.totalAmmoList[GetItemIndex(itemsInInventory[i])].ToString();
        }
    }

    public void UpdateAmmo(int index, int totalAmmo)
    {
        inventoryLayoutGroup.GetChild(index).Find("AmmoText").GetComponent<TextMeshProUGUI>().text = totalAmmo.ToString();
    }

    public Item SelectItem(int index)
    { 
        // if the index is not accessible
        if (index >= itemsInInventory.Count)
        {
            if (selectedItem != null)
            {
                return selectedItem;
            }
            else
            {
                return itemsInInventory[0];
            }
        }

        if (selectedItem != null) {
            inventoryLayoutGroup.GetChild(GetItemIndex(selectedItem)).GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            //inventoryLayoutGroup.GetChild(GetItemIndex(selectedItem)).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
        }

        if (index < itemsInInventory.Count)
        {
            selectedItem = itemsInInventory[index];
        }
        else
        {
            selectedItem = itemsInInventory[0];
        }

        inventoryLayoutGroup.GetChild(index).GetComponent<RectTransform>().sizeDelta = new Vector2(115, 115);
        //inventoryLayoutGroup.GetChild(GetItemIndex(selectedItem)).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 125);


        return selectedItem;
    }

}

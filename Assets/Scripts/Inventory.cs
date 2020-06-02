using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public List<Item> itemsInInventory = new List<Item>();

    public Item selectedItem;

    public Item P90, Sniper, Scar, Shotgun;

    // Start is called before the first frame update
    void Awake()
    {
        AddItem(P90);
        AddItem(Sniper);
        AddItem(Scar);
        AddItem(Shotgun);

        SelectItem(0);
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

    public Item SelectItem(int index)
    {
        if (index < itemsInInventory.Count)
        {
            selectedItem = itemsInInventory[index];
        }
        else
        {
            selectedItem = itemsInInventory[0];
        }
        return selectedItem;
    }

}

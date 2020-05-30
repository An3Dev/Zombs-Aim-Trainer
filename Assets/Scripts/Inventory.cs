using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public List<Item> itemsInInventory = new List<Item>();

    public Item selectedItem;

    public Item P90, Sniper, Scar, Shotgun;

    public void AddItem(Item item)
    {
        itemsInInventory.Add(item);

        Debug.Log("Added Item");
        selectedItem = item;
        Debug.Log(item.name);
    }

    // Start is called before the first frame update
    void Start()
    {
        AddItem(P90);
        AddItem(Sniper);
        AddItem(Scar);
        AddItem(Shotgun);

        SelectItem(0);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}

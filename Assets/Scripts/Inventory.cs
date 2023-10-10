using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : Singleton<Inventory>
{
    [SerializeField] private List<Item> items = new();
    [SerializeField] private ItemSlot[] itemSlots;
    [SerializeField] private Item selectedItem = null;
    [SerializeField] private Transform inventoryBar;
    [SerializeField] private RopeSystem RopeSystem;
    private const int maxItemCount = 4;

    private void Start()
    {
        if (inventoryBar != null)
        itemSlots = inventoryBar.GetComponentsInChildren<ItemSlot>();
    }
    public bool addItem(Item item)
    {
        bool itemAdded;
        if (items.Count >= maxItemCount)
        {
            Debug.Log("inventory is full");
            itemAdded = false;
        }
        else
        {
            items.Add(item);
            updateInventory();
            itemAdded = true;
        }
        return itemAdded;
    }
    private void updateInventory()
    {
        for (int i = 0; i < items.Count; i++)
        {
            itemSlots[i].item = items[i];
        }

    }

    public void switchHeldItem(int itemSlot)
    {
        resetSelectionFrames();
        itemSlots[itemSlot].selectionFrame.enabled = true;
        selectedItem = itemSlots[itemSlot].item;
        if (selectedItem && selectedItem.enableRope == true)
            RopeSystem.enabled = true;
        else 
            RopeSystem.enabled = false;

    }

    private void resetSelectionFrames()
    {
        foreach(ItemSlot itemSlot in itemSlots)
        {
            itemSlot.selectionFrame.enabled = false;
        }
    }

    public Item getSelectedItem()
    {
        return selectedItem;
    }

    




}

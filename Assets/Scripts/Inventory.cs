using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    [SerializeField] private List<Item> items;
    [SerializeField] private ItemSlot[] itemSlots;
    [SerializeField] private Item selectedItem = null;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private RopeSystem RopeSystem;
    private const int maxItemCount = 4;

    private void OnValidate()
    {
        if (itemsParent != null)
            itemSlots = itemsParent.GetComponentsInChildren<ItemSlot>();

        updateInventory();
    }
    public bool addItem(Item item)
    {
        if (items.Count >= maxItemCount)
        {
            Debug.Log("inventory is full");
            return false;
        }
        else
        {
            items.Add(item);
            updateInventory();
            return true;
        }
    }
    private void updateInventory()
    {
        int i = 0;

        for (; i < items.Count && i < itemSlots.Length; i++)
        {
            itemSlots[i].item = items[i];
        }

        for (; i < itemSlots.Length; ++i)
        {
            itemSlots[i].item = null;
        }

    }

    public void switchHeldItem(int itemSlot)
    {
        resetSelectionFrames();
        itemSlots[itemSlot].selectionFrame.enabled = true;
        selectedItem = itemSlots[itemSlot].item;
        onDeselectRope();
        if (selectedItem != null)
        switch (selectedItem.itemType)
        {
            case Item.itemTypes.KEY:
                break;
            case Item.itemTypes.ROPE:
                onSelectRope();
                break;
            default: break;
        }
        

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

    private void onSelectRope()
    {
        RopeSystem.enabled = true;
    }

    private void onDeselectRope()
    {
        RopeSystem.enabled = false;
    }

    




}

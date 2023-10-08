using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : Singleton<Inventory>
{
    private List<Item> items = new(4);

    private List<GameObject> itemFrames = new(4);

    private GameObject selectedFrame;
    private Item selectedItem = null;

    private int currentlySelected = 0;

    private void Start()
    {

        itemFrames.Add(transform.Find("ItemFrame01").gameObject);
        itemFrames.Add(transform.Find("ItemFrame02").gameObject);
        itemFrames.Add(transform.Find("ItemFrame03").gameObject);
        itemFrames.Add(transform.Find("ItemFrame04").gameObject);


    }
    public void addItem(Item item)
    {
        items.Add(item);
        updateInventory();
    }
    private void updateInventory()
    {
        for (int i = 0; i < items.Count; i++)
        {
            itemFrames[i].GetComponent<Image>().sprite = items[i].itemSprite;

        }

        if (items.Count >= currentlySelected && currentlySelected != 0)
            selectedItem = items[currentlySelected - 1];
        else
            selectedItem = null;


    }

    public void switchHeldItem(int itemSlot)
    {
        resetSelectionFrames();
        selectedFrame = itemFrames[itemSlot - 1].transform.Find("selectedFrame").gameObject;
        selectedFrame.GetComponent<Image>().enabled = true;
        currentlySelected = itemSlot;

        if (items.Count >= itemSlot)
            selectedItem = items[itemSlot - 1];
        else 
            selectedItem = null;
    }

    private void resetSelectionFrames()
    {
        itemFrames[0].transform.Find("selectedFrame").gameObject.GetComponent<Image>().enabled = false;
        itemFrames[1].transform.Find("selectedFrame").gameObject.GetComponent<Image>().enabled = false;
        itemFrames[2].transform.Find("selectedFrame").gameObject.GetComponent<Image>().enabled = false;
        itemFrames[3].transform.Find("selectedFrame").gameObject.GetComponent<Image>().enabled = false;
    }

    public Item getSelectedItem()
    {
        return selectedItem;
    }

    




}

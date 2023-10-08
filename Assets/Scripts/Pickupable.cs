using System;
using UnityEngine;


public class Pickupable : Interactable
{
    [SerializeField] private string itemName;
    Item item;
    SpriteRenderer sr;
    private void Start()
    {
        item = Resources.Load<Item>(String.Concat("Items/", itemName));
        sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sprite = item.itemSprite;
    }

    public override void Interact()
    {
        Inventory.instance.addItem(item);
        GameObject.Destroy(gameObject);
    }
}
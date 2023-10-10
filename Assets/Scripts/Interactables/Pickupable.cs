using System;
using UnityEngine;


public class Pickupable : Interactable
{
    [SerializeField] Item item;
    SpriteRenderer sr;
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = item.itemSprite;
    }

    public override void Interact()
    {
        if (Inventory.instance.addItem(item))
        GameObject.Destroy(gameObject);
    }
}
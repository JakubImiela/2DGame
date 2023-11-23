using UnityEngine;

public abstract class Item : ScriptableObject
{
    public enum itemTypes
    {
        KEY,
        ROPE
    }
    public itemTypes itemType;
    public string itemName;
    [TextArea]
    public string itemDescription;
    public int itemCost;
    public Sprite itemSprite;

    
}

using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Assets/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    [TextArea]
    public string itemDescription;
    public int itemCost;
    public bool openDoor;
    public Sprite itemSprite;
    public bool enableRope;
    
}

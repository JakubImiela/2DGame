using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    private Image image;
    public Image selectionFrame;

    [SerializeField] private Item _item;
    public Item item
    {
        get 
        { 
            return _item;
        }
        set
        {
            _item = value;

            if (image == null)
                image = GetComponent<Image>();

            if (_item == null)
            {
                image.enabled = false;

            }
            else 
            {
                
                image.sprite = _item.itemSprite;
                image.enabled = true;
 
            }
        }
    }

    private void OnValidate()
    {
        if (image == null)
            image = GetComponent<Image>();

    }


}

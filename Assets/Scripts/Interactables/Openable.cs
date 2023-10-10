using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Openable : Interactable
{
    public Sprite open;
    public Sprite closed;

    private SpriteRenderer sr;
    private bool isOpen;

    private GameObject collisionObject;
    private BoxCollider2D collision;


    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = closed;
        collisionObject = transform.Find("Collision").gameObject;
        collision = collisionObject.GetComponent<BoxCollider2D>();

    }
    public override void Interact()
    {
        if (Inventory.instance.getSelectedItem() == null)
            return;
        if (Inventory.instance.getSelectedItem().openDoor == false)
            return;

        if (isOpen)
        {
            sr.sprite = closed;
            collision.enabled = true;
        }
        else
        {
            sr.sprite = open;
            collision.enabled = false;
        }
            isOpen = !isOpen;
    }

   
}

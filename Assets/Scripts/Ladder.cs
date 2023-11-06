using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

public class Ladder : MonoBehaviour
{
    [HideInInspector] public Vector2 lowPosition;
    [HideInInspector] public Vector2 topPosition;
    [SerializeField] private Transform topTransform;
    [SerializeField] private Transform midTransform;
    [SerializeField] private Transform botTransform;
    [SerializeField] private Transform topCollisionTransform;
    public BoxCollider2D topCollision = null;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnValidate()
    {
        topTransform.localPosition = new Vector2(0f, +(midTransform.GetComponent<SpriteRenderer>().size.y/2 + 0.32f));
        midTransform.localPosition = new Vector2(0f, 0f);
        botTransform.localPosition = new Vector2(0f, -(midTransform.GetComponent<SpriteRenderer>().size.y/2 + 0.32f));
        topCollisionTransform.localPosition = new Vector2(0f, midTransform.GetComponent<SpriteRenderer>().size.y / 2 + 0.48f);
    }

    private void Start()
    {
        topPosition = GetComponent<Collider2D>().bounds.max;
        lowPosition = GetComponent<Collider2D>().bounds.min;
    }

    public void disableTopCollision()
    {
        topCollision.enabled = false;
    }

    public void enableTopCollision()
    {
        topCollision.enabled = true;
    }


}

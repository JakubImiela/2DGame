using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

public class Ladder : MonoBehaviour
{
    public Vector2 lowPosition; 
    public Vector2 topPosition;
    [SerializeField] private BoxCollider2D topCollision = null;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
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

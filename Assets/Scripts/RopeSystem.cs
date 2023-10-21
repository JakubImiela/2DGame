using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RopeSystem : MonoBehaviour
{
    //references
    [SerializeField] private Player playerMovement;

    //local components
    public GameObject ropeHingeAnchor;
    [SerializeField] private Transform crosshair;
    [SerializeField] private Transform ropeAttachmentPoint;
    [SerializeField] private LayerMask ropeLayerMask;
    private SpriteRenderer crosshairSprite;
    private DistanceJoint2D ropeJoint;
    private LineRenderer ropeRenderer;
    private bool ropeAttached;
    private bool distanceSet;
    private SpriteRenderer ropeHingeAnchorSprite;
    private Vector3 aimDirection;
    private Dictionary<Vector3, int> wrapPointsLookup = new Dictionary<Vector3, int>();
    private List<Vector3> ropePositions = new List<Vector3>();


    //config
    private float ropeMaxCastDistance = 10f;

    void Awake()
    {
        ropeHingeAnchorSprite = ropeHingeAnchor.GetComponent<SpriteRenderer>();
        crosshairSprite = crosshair.GetComponent<SpriteRenderer>();
        ropeJoint = transform.GetComponent<DistanceJoint2D>();
        ropeRenderer = transform.GetComponent<LineRenderer>();
    }

    private void Start()
    {
        ropeJoint.enabled = false;
        crosshairSprite.enabled = true;
    }
    void Update()
    {
        if (!ropeAttached)
        {
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldMousePosition.z = 0;
            Vector3 facingDirection = worldMousePosition - transform.position;
            var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
            if (aimAngle < 0f)
            {
                aimAngle = Mathf.PI * 2 + aimAngle;
            }
            aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) *
            Vector2.right;
            SetCrosshairPosition(aimAngle);
        }
        else
        {
            if (ropePositions.Count < 1)
            {
                ResetRope();
                return;
            }

            if (ropePositions.Count > 1)
            {
                RaycastHit2D previousWrapPointHit = Physics2D.Raycast(transform.position,
                    ropePositions[ropePositions.Count - 2] - transform.position,
                    Vector2.Distance(transform.position, ropePositions[ropePositions.Count - 2]) - 0.1f,
                    ropeLayerMask);

                if (!previousWrapPointHit) unwrapLast();
            }

            if (maxDistanceReached())
            {
                
                if (!playerMovement.isGrounded())
                {
                    ropeJoint.enabled = true;
                    playerMovement.isSwinging = true;
                }
                else
                {
                    transform.GetComponent<Rigidbody2D>().AddForce(
                        (ropeHingeAnchor.transform.position - transform.position) * 5f,
                        ForceMode2D.Force);
                    ropeJoint.enabled = false;
                    playerMovement.isSwinging = false;
                }
            }

            RaycastHit2D playerToCurrentNextHit = Physics2D.Raycast(transform.position,
                (ropePositions.Last() - transform.position).normalized,
                Vector2.Distance(transform.position, ropePositions.Last()) - 0.1f, ropeLayerMask);

            if (playerToCurrentNextHit)
            {
                var colliderWithVertices = playerToCurrentNextHit.collider as BoxCollider2D;
                if (colliderWithVertices != null)
                {
                    var closestPointToHit =
                    GetClosestColliderPointFromRaycastHit(playerToCurrentNextHit, colliderWithVertices);

                    if (wrapPointsLookup.ContainsKey(closestPointToHit))
                    {
                        return;
                    }

                        ropePositions.Add(closestPointToHit);
                        wrapPointsLookup.Add(closestPointToHit, 0);
                        distanceSet = false;
                }
            }
            

        }

        HandleInput(aimDirection);
        UpdateRopePositions();

    }

    private void SetCrosshairPosition(float aimAngle)
    {
        var x = transform.position.x + 2f * Mathf.Cos(aimAngle);
        var y = transform.position.y + 2f * Mathf.Sin(aimAngle);

        crosshair.transform.position = new Vector3(x, y, 0);
    }

    private void HandleInput(Vector2 aimDirection)
    {
        if (Input.GetMouseButton(0) && !ropeAttached)
        {
            var hit = Physics2D.Raycast(transform.position, aimDirection, ropeMaxCastDistance, ropeLayerMask);

            if (hit.collider != null)
            {
                ropeRenderer.enabled = true;
                ropeAttached = true;
                ropePositions.Add(hit.point);
                ropeJoint.distance = calculateRopeRemainingDistance();
                ropeHingeAnchorSprite.enabled = true;
                crosshairSprite.enabled = false;
            }
        }
        if (Input.GetMouseButton(1))
        {
            ResetRope();
        }
    }

    private void ResetRope()
    {
        ropeJoint.enabled = false;
        ropeAttached = false;
        playerMovement.isSwinging = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        ropeHingeAnchorSprite.enabled = false;
        wrapPointsLookup.Clear();
        crosshairSprite.enabled = true;
    }

    private void UpdateRopePositions()
    {
        if (!ropeAttached) return;

        ropeRenderer.positionCount = ropePositions.Count;

        for (var i = ropeRenderer.positionCount - 1; i >= 0; i--)
        {
            ropeRenderer.SetPosition(i, ropePositions[i]);
        }

        ropeRenderer.positionCount++;

        if (playerMovement.isSwinging)
            ropeRenderer.SetPosition(ropeRenderer.positionCount - 1, ropeAttachmentPoint.position);
        else
            ropeRenderer.SetPosition(ropeRenderer.positionCount - 1, transform.position);

        ropeHingeAnchor.transform.position = ropePositions.Last();

        if (!distanceSet)
        {
            ropeJoint.distance = calculateRopeRemainingDistance();
            distanceSet = true;
        }

    }

    private Vector2 GetClosestColliderPointFromRaycastHit(RaycastHit2D hit, BoxCollider2D boxCollider)
    {


        var distanceDictionary = points(boxCollider).ToDictionary<Vector2, float>
            (position => Vector2.Distance(hit.point, position));

        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);

        return orderedDictionary.Any() ? orderedDictionary.First().Value :
            Vector2.zero;
    }

    private List<Vector2> points(BoxCollider2D boxCollider)
    {
        List<Vector2> points = new List<Vector2>();

        points.Add(new Vector2(
            boxCollider.bounds.center.x - boxCollider.bounds.extents.x,
            boxCollider.bounds.center.y - boxCollider.bounds.extents.y));
        points.Add(new Vector2(
            boxCollider.bounds.center.x + boxCollider.bounds.extents.x,
            boxCollider.bounds.center.y - boxCollider.bounds.extents.y));
        points.Add(new Vector2(
            boxCollider.bounds.center.x + boxCollider.bounds.extents.x,
            boxCollider.bounds.center.y + boxCollider.bounds.extents.y));
        points.Add(new Vector2(
            boxCollider.bounds.center.x - boxCollider.bounds.extents.x,
            boxCollider.bounds.center.y + boxCollider.bounds.extents.y));

        return points;


    }

    private float calculateRopeRemainingDistance()
    {
        if (ropePositions.Count <= 0) return 0;
        if (ropePositions.Count == 1) return ropeMaxCastDistance;
        float x = ropeMaxCastDistance;
        for (int i = 0; i < ropePositions.Count - 1; i++)
        {
            x -= Vector2.Distance(ropePositions[i], ropePositions[i + 1]);
        }

        return x;

    }

    private bool maxDistanceReached()
    {
        return Vector2.Distance(transform.position, ropePositions.Last()) > ropeJoint.distance;
    }
    
    private void unwrapLast()
    {
        ropePositions.RemoveAt(ropePositions.Count - 1);
        wrapPointsLookup.Remove(wrapPointsLookup.Keys.Last());
        ropeJoint.distance = calculateRopeRemainingDistance();
    }

    private void OnDisable()
    {
        ResetRope();
        crosshairSprite.enabled = false;
    }

    private void OnEnable()
    {
        crosshairSprite.enabled = true;
    }

}

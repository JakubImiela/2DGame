using JetBrains.Annotations;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{

    //debug

    //references
    private Ladder ladder;
    [SerializeField] private RopeSystem ropeSystem;


    //local components
    Rigidbody2D rigidbodyComponent;
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float angleToPivot;
    [SerializeField] private Transform frontCheckTransform;
    [SerializeField] private Transform ropeAttachmentPointTransform;
    [SerializeField] private Transform heldItemTransform;
    private Animator animator;

    //player state
    private bool onLadder;
    public bool isSwinging;

    //key presses
    private bool jumpKeyWasPressed;

    private float horizontalInput;
    private float verticalInput;

    private bool inventoryOneKeyWasPressed;
    private bool inventoryTwoKeyWasPressed;
    private bool inventoryThreeKeyWasPressed;
    private bool inventoryFourKeyWasPressed;

    //config

    private const float defaultGravityScale = 1.0f;
    private const float playerSpeed = 4f;
    private const float playerSwingForce = 2f;

    // Start is called before the first frame update
    void Start()
    {
        rigidbodyComponent = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (verticalInput != 0)
        {
            checkLadder(verticalInput);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpKeyWasPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckInteraction();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            inventoryOneKeyWasPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            inventoryTwoKeyWasPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            inventoryThreeKeyWasPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            inventoryFourKeyWasPressed = true;
        }


        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

    }

    private void FixedUpdate()
    {
        animator.SetFloat("horizontalInputAbs", Mathf.Abs(horizontalInput));
        animator.SetBool("isGrounded", isGrounded());
        animator.SetBool("isSwinging", isSwinging);
        animator.SetBool("isClimbing", onLadder);

        if (onLadder)
        {
            if (Mathf.Abs(verticalInput) > 0.1f)
                animator.speed = 1;
            else
                animator.speed = 0;
        }


        if (horizontalInput > 0)
        {
            turnRight();
            
        }
        if (horizontalInput < 0)
        {
            turnLeft();
            
        }

        if (inventoryOneKeyWasPressed)
        {
            Inventory.instance.switchHeldItem(0);
            inventoryOneKeyWasPressed = false;
        }
        if (inventoryTwoKeyWasPressed)
        {
            Inventory.instance.switchHeldItem(1);
            inventoryTwoKeyWasPressed = false;
        }
        if (inventoryThreeKeyWasPressed)
        {
            Inventory.instance.switchHeldItem(2);
            inventoryThreeKeyWasPressed = false;
        }
        if (inventoryFourKeyWasPressed)
        {
            Inventory.instance.switchHeldItem(3);
            inventoryFourKeyWasPressed = false;
        }

        if (isSwinging)
        {

            angleToPivot = Vector2.SignedAngle(Vector2.down,
                transform.position - ropeSystem.ropeHingeAnchor.transform.position);
            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angleToPivot));
            rigidbodyComponent.AddForce(new Vector2(horizontalInput * playerSwingForce, 0), ForceMode2D.Force);

            return;

        }
        else transform.rotation = Quaternion.identity;

        if (onLadder)
        {
            rigidbodyComponent.velocity = new Vector2(rigidbodyComponent.velocity.x, verticalInput * 3);
            if (GetComponent<Collider2D>().bounds.min.y > ladder.topPosition.y && verticalInput > 0.0f)
                getOffLadder();
            if (GetComponent<Collider2D>().bounds.max.y < ladder.lowPosition.y && verticalInput < 0.0f)
                getOffLadder();
            if (isGrounded() && verticalInput < 0.0f)
                getOffLadder();
            if (jumpKeyWasPressed)
            {
                getOffLadder();
                jumpKeyWasPressed = false;
            }

            return;
        }

        rigidbodyComponent.velocity = new Vector2(horizontalInput * playerSpeed, rigidbodyComponent.velocity.y);

   
        if (!isGrounded())
        {
            jumpKeyWasPressed = false;
            return;
        }

        if (jumpKeyWasPressed)
        {
            rigidbodyComponent.velocity = new Vector2(rigidbodyComponent.velocity.x, 6f);
            jumpKeyWasPressed = false;
        }

    }

    public void OpenInteractableIcon()
    {
        ;
    }

    public void CloseInteractableIcon()
    {
        ;
    }

    private void CheckInteraction()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(0.1f, 1f), 0, Vector2.zero);

        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform.GetComponent<Interactable>())
                {
                    hit.transform.GetComponent<Interactable>().Interact();
                    return;
                }
            }
        }
    }

    private void checkLadder(float verticalInput)
    {
        Vector2 playerLowPosition = new Vector2(transform.position.x, GetComponent<Collider2D>().bounds.min.y);
        Vector2 playerTopPosition = new Vector2(transform.position.x, GetComponent<Collider2D>().bounds.max.y);
        Vector2 raycastStartPosition;

        if (verticalInput > 0)
            raycastStartPosition = playerTopPosition;
        else
            raycastStartPosition = playerLowPosition;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(raycastStartPosition, 0.1f, new Vector2(0f, 0f));

        if (hits.Length > 0)
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform.GetComponent<Ladder>() && onLadder == false)
                {
                    rigidbodyComponent.gravityScale = 0;
                    rigidbodyComponent.velocity = Vector2.zero;
                    rigidbodyComponent.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                    transform.position = new Vector2(hit.transform.position.x, transform.position.y);
                    onLadder = true;
                    ladder = hit.transform.GetComponent<Ladder>();
                    ladder.disableTopCollision();
                    return;
                }
            }
    }

    private void getOffLadder()
    {
        rigidbodyComponent.gravityScale = defaultGravityScale;
        rigidbodyComponent.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidbodyComponent.velocity = Vector2.zero;
        ladder.enableTopCollision();
        onLadder = false;
        animator.speed = 1;
    }

    public bool isGrounded()
    {
        return Physics2D.OverlapCircleAll(groundCheckTransform.position, 0.1f, collisionMask).Length > 0;
    }

    public void turnRight()
    {
        transform.GetComponent<SpriteRenderer>().flipX = false;
        frontCheckTransform.localPosition = new Vector2(0.5f, 0f);
        ropeAttachmentPointTransform.localPosition = new Vector2(0.17f, 0.25f);
        heldItemTransform.localPosition = new Vector2(0.065f, -0.005f);
    }

    public void turnLeft()
    {
        transform.GetComponent<SpriteRenderer>().flipX = true;
        frontCheckTransform.transform.localPosition = new Vector2(-0.5f, 0f);
        ropeAttachmentPointTransform.localPosition = new Vector2(-0.17f, 0.25f);
        heldItemTransform.localPosition = new Vector2(-0.065f, -0.005f);
    }



    






}

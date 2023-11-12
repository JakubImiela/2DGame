using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Swat : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private Rigidbody2D rigidbodyComponent;
    private Vector2 startingPosition;
    public Vector2 facingDirection = Vector2.right;

    public GPS.area currentArea;
    public Ladder ladder;
    
    
    public List<AreaIdentifier> transitionAreas;
    public AreaIdentifier lastArea;
    public GPS.area destinationArea;
    private float[,] dijkstraGraph;

    private bool canSeePlayer;
    private bool onLadder = false;
    private bool isChasingPlayer = false;
    private bool onTheMove = false;

    private float visibilityTimer;
    private const float maxVisibilityTime = 5f;
    private const float runningSpeed = 4f;
    private const float walkingSpeed = 2f;
    private const float climingSpeed = 1f;
    private const float defaultGravityScale = 1f;
    private float horizontalDirection = 1f;
    private float verticalDirection = 1f;

    void Start()
    {
        rigidbodyComponent = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreCollision(playerTransform.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());
        startingPosition = transform.position;
        fillTransitionAreas();
        dijkstraGraph = new float[transitionAreas.Capacity, transitionAreas.Capacity];
        currentArea = transitionAreas[0].areaName;
        destinationArea = currentArea;
    }

    void Update()
     {
        if(destinationArea != currentArea && onTheMove == false)
        {
            Debug.Log("Destination Area - " + destinationArea);
            Debug.Log("Current Area - " + currentArea);
            Debug.Log("On the move - " + onTheMove);
            Debug.Log("moving to new destination - " + destinationArea);
            StartCoroutine(getToArea(destinationArea));
            onTheMove = true;
        }
            //StartCoroutine(getToArea(Dijkstra(0,4, this)));
            //StartCoroutine(getToArea(transitionAreas[4]));
          
     }

    private void OnEnable()
    {
        Actions.playerChangedLocation += setGoalArea;
    }

    private void OnDisable()
    {
        Actions.playerChangedLocation -= setGoalArea;
    }

    private void setGoalArea()
    {
        ;
    }
    private IEnumerator getToArea(GPS.area target)
    {
        Debug.Log("Started heading to area " + target);
        int targetAreaIndex = 0;
        int startingAreaIndex = 0;

        while (currentArea != target)
        {
            Debug.Log("getToArea iteration start");
            for (int i = 0; i < transitionAreas.Count; i++)
            {
                if (transitionAreas[i].areaName == target)
                    targetAreaIndex = i;
            }
            AreaIdentifier nextArea = Dijkstra(startingAreaIndex, targetAreaIndex);
            Debug.Log("nextArea set to " + nextArea);

            if (nextArea.areaName == currentArea)
            {
                yield return runTo(nextArea.transform.position); 
            }
            else //if (nextArea.areaName != currentArea)
            {
                if (nextArea.transitionType == GPS.transitionType.Door)
                {
                    yield return runTo(nextArea.transform.position);
                }
                else //if (nextArea.transitionType == GPS.transitionType.Ladder)
                {
                    yield return climbLadder(nextArea.transportObject, lastArea.transform.position, nextArea.transform.position);
                }
            }
            for (int i = 0; i < transitionAreas.Count; i++)
            {
                if (transitionAreas[i] == lastArea)
                    startingAreaIndex = i;
            }
            Debug.Log("set startingArea to " + transitionAreas[startingAreaIndex]);
        }
        Debug.Log("Finished heading to area " + target);
        onTheMove = false;
    }

    private IEnumerator climbLadder(Transform ladderTransform, Vector2 ladderStart, Vector2 ladderEnd)
    {
        Debug.Log("Climb ladder called");
        rigidbodyComponent.gravityScale = 0;
        rigidbodyComponent.velocity = Vector2.zero;
        rigidbodyComponent.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        transform.position = new Vector2(ladderStart.x, transform.position.y);
        onLadder = true;
        ladder = ladderTransform.GetComponent<Ladder>();
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ladder.topCollision);
        if (ladderStart.y < ladderEnd.y)
            verticalDirection = 1f;
        else verticalDirection = -1f;

        Debug.Log("vertical direction = " + verticalDirection);
       
        while ((GetComponent<Collider2D>().bounds.min.y < ladder.topPosition.y && verticalDirection > 0f)
            || (GetComponent<Collider2D>().bounds.min.y > ladder.lowPosition.y && verticalDirection < 0f))
        {
            yield return new WaitForSeconds(0.1f);
            rigidbodyComponent.velocity = new Vector2(rigidbodyComponent.velocity.x, verticalDirection * climingSpeed);
        }
        getOffLadder();
    }
    private IEnumerator runTo(Vector2 target)
    {
        Debug.Log("Run to called");
        
        while (Mathf.Abs(transform.position.x - target.x) > 0.1f)
        {
            yield return new WaitForSeconds(0.1f);

            if (target.x > transform.position.x && facingDirection != Vector2.right)
            {
                Debug.Log("Turned Right");
                turnRight();
            }
            if (target.x < transform.position.x && facingDirection != Vector2.left)
            {
                Debug.Log("Turned Left");
                turnLeft();
            }

            rigidbodyComponent.velocity = new Vector2(walkingSpeed * horizontalDirection, rigidbodyComponent.velocity.y);
        }
        rigidbodyComponent.velocity = Vector2.zero;
        Debug.Log("Finished runTo (" + target + ")");
    }
    private IEnumerator idleMovement()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (transform.position.x < startingPosition.x - 2 && runningSpeed < 0f)
                turnRight();
                
            if (transform.position.x > startingPosition.x + 2 && runningSpeed > 0f)
                turnLeft();
           
            
            rigidbodyComponent.velocity = new Vector2(walkingSpeed, rigidbodyComponent.velocity.y);
        }
    }

    private IEnumerator chasingPlayer()
    {
        visibilityTimer = maxVisibilityTime;
        isChasingPlayer = true;
        if (currentArea != GPS.instance.playerCurrentArea)
        {
            
        }
        while (visibilityTimer > 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
            if (canSeePlayer == true) visibilityTimer = maxVisibilityTime;
            visibilityTimer -= 0.1f;
        }
        isChasingPlayer = false;
        StopAllCoroutines();
        StartCoroutine(idleMovement());
    }

    
    public AreaIdentifier Dijkstra(int source, int destination)
    {
        int verticesCount = transitionAreas.Capacity;
        fillDijkstraGraph();
        float[] distance = new float[verticesCount];
        bool[] shortestPathTreeSet = new bool[verticesCount];
        AreaIdentifier[,] paths = new AreaIdentifier[verticesCount, verticesCount];

        for (int i = 0; i < verticesCount; ++i)
        {
            distance[i] = float.MaxValue;
            shortestPathTreeSet[i] = false;
            paths[i, 0] = transitionAreas[source];
        }
        distance[source] = 0;
        for (int count = 0; count < verticesCount; ++count)
        {
            int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
            shortestPathTreeSet[u] = true;

            for (int v = 0; v < verticesCount; ++v)
            {
                if (!shortestPathTreeSet[v] && Convert.ToBoolean(dijkstraGraph[u, v]) && distance[u] != int.MaxValue && distance[u] + dijkstraGraph[u, v] < distance[v])
                {
                    distance[v] = distance[u] + dijkstraGraph[u, v];
                    for (int i = 1; i < verticesCount; ++i)
                    {
                        if (paths[u, i] == null)
                        {
                            paths[v, i] = transitionAreas[v];
                            break;
                        }

                        paths[v, i] = paths[u, i];
                        
                    }
                }
            }
        }
        return paths[destination, 1];
    }

    

    private void fillDijkstraGraph()
    {
        
        for (int u = 0; u < transitionAreas.Capacity; u++)
            for (int v = u; v < transitionAreas.Capacity; v++)
            {
                if (transitionAreas[u].oppositeExit == transitionAreas[v] || transitionAreas[u].areaName == transitionAreas[v].areaName)
                {
                    dijkstraGraph[u, v] = Vector2.Distance(transitionAreas[u].transform.position, transitionAreas[v].transform.position);
                    dijkstraGraph[v, u] = dijkstraGraph[u, v];
                }
                else
                {
                    dijkstraGraph[u, v] = 0f;
                    dijkstraGraph[v, u] = 0f;
                }
            }
        
    }

    private void fillTransitionAreas()
    {
        AreaIdentifier[] temp = GPS.instance.GetComponentsInChildren<AreaIdentifier>();
        transitionAreas = new List<AreaIdentifier>(){GetComponent<AreaIdentifier>()};
        transitionAreas.AddRange(temp);
    }

    public int MinimumDistance(float[] distance, bool[] shortestPathTreeSet, int verticesCount)
    {
        float min = float.MaxValue;
        int minIndex = 0;

        for (int v = 0; v < verticesCount; ++v)
        {
            if (shortestPathTreeSet[v] == false && distance[v] <= min)
            {
                min = distance[v];
                minIndex = v;
            }
        }

        return minIndex;
    }

    private void getOffLadder()
    {
        rigidbodyComponent.gravityScale = defaultGravityScale;
        rigidbodyComponent.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidbodyComponent.velocity = Vector2.zero;
        Physics2D.IgnoreCollision(ladder.topCollision, GetComponent<BoxCollider2D>(), false);
        onLadder = false;
    }

    private void turnRight()
    {
        transform.GetComponent<SpriteRenderer>().flipX = false;
        horizontalDirection = 1f;
        facingDirection = Vector2.right;
    }

    private void turnLeft()
    {
        transform.GetComponent<SpriteRenderer>().flipX = true;
        horizontalDirection = -1f;
        facingDirection = Vector2.left;
    }

}





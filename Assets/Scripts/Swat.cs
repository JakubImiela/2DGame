using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Swat : MonoBehaviour
{
    private enum activity {idle, chasingPlayer}
    private activity currentActivity = activity.idle;
    [SerializeField] private Transform playerTransform;
    private Rigidbody2D rigidbodyComponent;
    private Vector2 startingPosition;
    public Vector2 facingDirection = Vector2.right;

    public GPS.area currentArea;
    private Ladder ladder;
    
    
    private List<AreaIdentifier> transitionAreas;
    public AreaIdentifier lastArea;
    [SerializeField] private GPS.area destinationArea;
    private float[,] dijkstraGraph;
    FieldOfView fovRef;


    private const float runningSpeed = 4f;
    private const float walkingSpeed = 2f;
    private const float climingSpeed = 2f;
    private const float defaultGravityScale = 1f;
    private float horizontalDirection = 1f;
    private float verticalDirection = 1f;

    void Start()
    {
        fovRef = GetComponent<FieldOfView>();
        rigidbodyComponent = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreCollision(playerTransform.GetComponent<CapsuleCollider2D>(), GetComponent<BoxCollider2D>());
        startingPosition = transform.position;
        fillTransitionAreas();
        dijkstraGraph = new float[transitionAreas.Capacity, transitionAreas.Capacity];
        currentArea = transitionAreas[0].areaName;
        destinationArea = currentArea;
        StartCoroutine(idleMovement());
    }

    void Update()
     {
        if (fovRef.canSeePlayer)
        {
            if (currentActivity != activity.chasingPlayer)
            {
                currentActivity = activity.chasingPlayer;
                StopAllCoroutines();
                StartCoroutine(chasingPlayer());
            }
        }
        

     }

    private IEnumerator chasingPlayer()
    {
        StartCoroutine(chasingTimer());
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            while (currentArea != GPS.instance.playerCurrentArea)
            {
                yield return getToArea(GPS.instance.playerCurrentArea);
            }
            if (playerTransform.position.x > transform.position.x)
            {
                if (facingDirection != Vector2.right) turnRight();
            }
            else
            {
                if (facingDirection != Vector2.left) turnLeft();
            }
            rigidbodyComponent.velocity = new Vector2(horizontalDirection * runningSpeed, rigidbodyComponent.velocity.y);
        }
    }

    private IEnumerator chasingTimer()
    {
        float timer = 10f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (fovRef.canSeePlayer) timer = 10f;
            yield return null;
        }
        StopAllCoroutines();
        StartCoroutine(idleMovement());
    }

    private void setGoalArea()
    {
        destinationArea = GPS.instance.playerCurrentArea;
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
    }

    private IEnumerator climbLadder(Transform ladderTransform, Vector2 ladderStart, Vector2 ladderEnd)
    {
        Debug.Log("Climb ladder called");
        rigidbodyComponent.gravityScale = 0;
        rigidbodyComponent.velocity = Vector2.zero;
        rigidbodyComponent.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        transform.position = new Vector2(ladderStart.x, transform.position.y);
        ladder = ladderTransform.GetComponent<Ladder>();
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ladder.topCollision);
        if (ladderStart.y < ladderEnd.y)
            verticalDirection = 1f;
        else verticalDirection = -1f;

       
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
        
        while (Mathf.Abs(transform.position.x - target.x) > 0.5f)
        {
            yield return new WaitForSeconds(0.1f);

            if (target.x > transform.position.x && facingDirection != Vector2.right)
            {
                turnRight();
            }
            if (target.x < transform.position.x && facingDirection != Vector2.left)
            {
                turnLeft();
            }

            rigidbodyComponent.velocity = new Vector2(runningSpeed * horizontalDirection, rigidbodyComponent.velocity.y);
        }
        rigidbodyComponent.velocity = Vector2.zero;
    }
    private IEnumerator idleMovement()
    {
        currentActivity = activity.idle;
        yield return getToArea(GPS.area.B);
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (transform.position.x < startingPosition.x - 2 && horizontalDirection < 0f)
            {
                rigidbodyComponent.velocity = Vector2.zero;
                yield return new WaitForSeconds(2f);
                turnRight();
            }

            if (transform.position.x > startingPosition.x + 2 && horizontalDirection > 0f)
            {
                rigidbodyComponent.velocity = Vector2.zero;
                yield return new WaitForSeconds(2f);
                turnLeft();
            }
            
            rigidbodyComponent.velocity = new Vector2(walkingSpeed * horizontalDirection, rigidbodyComponent.velocity.y);
        }
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

    private void OnEnable()
    {
        Actions.playerChangedLocation += setGoalArea;
    }

    private void OnDisable()
    {
        Actions.playerChangedLocation -= setGoalArea;
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





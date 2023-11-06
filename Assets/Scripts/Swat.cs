using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Swat : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private Rigidbody2D rigidbodyComponent;
    private Vector2 startingPosition;
    public Vector2 facingDirection = Vector2.right;

    public GPS.area currentArea;
    public Ladder ladder;
    
    
    public List<AreaIdentifier> transitionAreas;
    public AreaIdentifier currentGoal;
    public AreaIdentifier lastArea;
    private GPS.area startingArea;
    private float[,] dijkstraGraph;

    private bool canSeePlayer;
    private bool onLadder = false;
    private bool isChasingPlayer = false;

    private float visibilityTimer;
    private const float maxVisibilityTime = 5f;
    private float runningSpeed = 4f;
    private float walkingSpeed = 2f;
    private float climingSpeed = 1f;
    private const float defaultGravityScale = 1f;

    void Start()
    {
        rigidbodyComponent = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreCollision(playerTransform.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());
        startingPosition = transform.position;
        fillTransitionAreas();
        dijkstraGraph = new float[transitionAreas.Capacity, transitionAreas.Capacity];
        //testing stuff below - erase later
        startingArea = transitionAreas[0].areaName;
        StartCoroutine(getToArea(Dijkstra(0, 4)));
    }

    void Update()
     {

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
    private IEnumerator getToArea(AreaIdentifier target)
    {
        
        
        if (target.transitionType == GPS.transitionType.Ladder && target.oppositeExit == lastArea)
        {
            Debug.Log("going on ladder");
            ladder = target.transportObject.transform.GetComponent<Ladder>();
            rigidbodyComponent.gravityScale = 0;
            rigidbodyComponent.velocity = Vector2.zero;
            rigidbodyComponent.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            transform.position = new Vector2(target.transform.position.x, transform.position.y);
            ladder = target.transportObject.GetComponent<Ladder>();
            Physics2D.IgnoreCollision(ladder.topCollision, GetComponent<BoxCollider2D>());
            onLadder = true;
            if (target.transform.position.y > target.oppositeExit.transform.position.y)
                climingSpeed = 2f;
            else climingSpeed = -2f;
        }

        Debug.Log("just running");

        while (lastArea != target)
        {
            yield return new WaitForSeconds(0.2f);
            if (onLadder)
            {
                rigidbodyComponent.velocity = new Vector2(0, 1f * climingSpeed);
            }
            else 
            {
                if (transform.position.x < target.transform.position.x && runningSpeed < 0f)
                {
                    turnRight();
                }
                if (transform.position.x > target.transform.position.x && runningSpeed > 0f)
                {
                    turnLeft();
                }
               
                rigidbodyComponent.velocity = new Vector2(runningSpeed, rigidbodyComponent.velocity.y);
            }
        }

        Debug.Log("stopped moving");

        if (onLadder) getOffLadder();
        StopAllCoroutines();
        if (lastArea != currentGoal)
        {
            Debug.Log("ide dalej");
            int nextSourceIndex = 0;
            int goalIndex = 0;
            for (int i = 0; i < transitionAreas.Capacity; i++)
            {
                if (transitionAreas[i] == target)
                    nextSourceIndex = i;
                if (transitionAreas[i] == currentGoal)
                    goalIndex = i;
            }
            Debug.Log("going from " + transitionAreas[nextSourceIndex] + " to " + transitionAreas[goalIndex]);
            StartCoroutine(getToArea(Dijkstra(nextSourceIndex, goalIndex)));
        }
        else
        {
            Debug.Log("wracam na pozycje");
            if (transitionAreas[0].areaName == startingArea)
            {
                Debug.Log("dotarlem do startowej strefy, idle movement");
                StartCoroutine(idleMovement());
            }
            else
            {
                Debug.Log("ide dalej");
                int nextSourceIndex = 0;
                int goalIndex = 0;
                for (int i = 0; i < transitionAreas.Capacity; i++)
                {
                    if (transitionAreas[i] == target)
                        nextSourceIndex = i;
                    if (transitionAreas[i].areaName == startingArea)
                    {
                        goalIndex = i;
                        currentGoal = transitionAreas[i];
                    }
                }
                Debug.Log("going from " + transitionAreas[nextSourceIndex] + " to " + transitionAreas[goalIndex]);
                StartCoroutine(getToArea(Dijkstra(nextSourceIndex, goalIndex)));
            }
            
        }
    }


    private IEnumerator idleMovement()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
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
        foreach(AreaIdentifier area in transitionAreas)
        {
            Debug.Log("transition area - " + area);
        }
        
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
        runningSpeed = -runningSpeed;
        facingDirection = Vector2.right;
    }

    private void turnLeft()
    {
        transform.GetComponent<SpriteRenderer>().flipX = true;
        runningSpeed = -runningSpeed;
        facingDirection = Vector2.left;
    }

}





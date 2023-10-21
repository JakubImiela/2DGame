using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Swat : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private bool canSeePlayer;
    private Rigidbody2D rigidbodyComponent;
    private Vector2 startingPosition;
    private float characterSpeed = 2f;
    private float climingSpeed = 1f;
    public Vector2 facingDirection = Vector2.right;
    private float visibilityTimer;
    private const float maxVisibilityTime = 5f;
    private bool isChasingPlayer = false;
    public GPS.area currentArea = GPS.area.A;
    public TransitionArea lastArea;
    public Ladder ladder;
    private bool onLadder;
    private const float defaultGravityScale = 1f;
    private static float[,] dijkstraGraph;
    public static AreaIdentifier[] transitionAreas;

    void Start()
    {
        rigidbodyComponent = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreCollision(playerTransform.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());
        startingPosition = transform.position;
        StartCoroutine(playAfterSecond());
        dijkstraGraph = fillDjkistraGraph();
        transitionAreas = GPS.instance.GetComponentsInChildren<AreaIdentifier>();
    }

    void Update()
     {
        
        if (GetComponent<FieldOfView>().canSeePlayer)
        {
            
        }
     }

    private IEnumerator playAfterSecond()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(getToArea(Dijkstra(0, 1, transform.GetComponent<Swat>())));
    }

    private IEnumerator getToArea(AreaIdentifier target)
    {
        yield return null;

        if (target.transitionType == GPS.transitionType.Ladder && target.oppositeExit == lastArea)
        {
            rigidbodyComponent.gravityScale = 0;
            rigidbodyComponent.velocity = Vector2.zero;
            rigidbodyComponent.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            transform.position = new Vector2(target.transform.position.x, transform.position.y);
            ladder = target.transportObject.GetComponent<Ladder>();
            Physics2D.IgnoreCollision(ladder.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());
            onLadder = true;
        }

        while (lastArea != target.transform.GetComponent<TransitionArea>())
        {
            if (onLadder)
            {
                rigidbodyComponent.velocity = new Vector2(0, (transform.position - target.transform.position).normalized.y * climingSpeed);
            }
            else 
            {
                if (transform.position.x < target.transform.position.x && characterSpeed < 0f)
                {
                    turnRight();
                }
                if (transform.position.x > target.transform.position.x && characterSpeed > 0f)
                {
                    turnLeft();
                }

                rigidbodyComponent.velocity = new Vector2(characterSpeed, rigidbodyComponent.velocity.y);
            }
        }

        if (onLadder) getOffLadder();
        StopAllCoroutines();
        StartCoroutine(idleMovement());
    }

    private void getOffLadder()
    {
        rigidbodyComponent.gravityScale = defaultGravityScale;
        rigidbodyComponent.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigidbodyComponent.velocity = Vector2.zero;
        Physics2D.IgnoreCollision(ladder.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>(), false);
        onLadder = false;
    }



    private IEnumerator idleMovement()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if (transform.position.x < startingPosition.x - 2 && characterSpeed < 0f)
                turnRight();
                
            if (transform.position.x > startingPosition.x + 2 && characterSpeed > 0f)
                turnLeft();
           
            
            rigidbodyComponent.velocity = new Vector2(characterSpeed, rigidbodyComponent.velocity.y);
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

    

    

    private void turnRight()
    {
        transform.GetComponent<SpriteRenderer>().flipX = false;
        characterSpeed = -characterSpeed;
        facingDirection = Vector2.right;
    }

    private void turnLeft()
    {
        transform.GetComponent<SpriteRenderer>().flipX = true;
        characterSpeed = -characterSpeed;
        facingDirection = Vector2.left;
    }


    private float[,] fillDjkistraGraph()
    {
        float[,] graph = new float[transitionAreas.Length + 1, transitionAreas.Length + 1];
        for (int u = 0; u < transitionAreas.Length; u++)
            for (int v = u; v < transitionAreas.Length; v++)
            {
                if (transitionAreas[v].oppositeExit == transitionAreas[u] || transitionAreas[u].areaName == transitionAreas[v].areaName)
                {
                    graph[u, v] = Vector2.Distance(transitionAreas[u].transform.position, transitionAreas[v].transform.position);
                    graph[v, u] = graph[u, v];
                }
                else
                {
                    graph[u, v] = 0f;
                    graph[v, u] = 0f;
                }
            }
        return graph;
    }
    private void addEnemyToGraph()
    {
        for (int u = 0; u < transitionAreas.Length; u++)
            if (currentArea == transitionAreas[u].areaName)
            {
                dijkstraGraph[u, transitionAreas.Length] = Vector2.Distance(transform.position, transitionAreas[u].transform.position);
                dijkstraGraph[transitionAreas.Length, u] = dijkstraGraph[u, transitionAreas.Length];
            }
        dijkstraGraph[transitionAreas.Length, transitionAreas.Length] = 0f;

    }

    //Dijkstra start
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

    public AreaIdentifier Dijkstra(int source, int destination, Swat enemy)
    {
        int verticesCount = transitionAreas.Length + 1;
        float[,] graph = dijkstraGraph;
        
        //add enemy to the graph
        for (int u = 0; u < verticesCount; u++)
        {
            Debug.Log(u + "adding enemy u");
            if (u > 3)
            {
                graph[4, 4] = 0f;
                break;
            }
            if (transitionAreas[u].areaName == enemy.currentArea && u < 4)
            {
                graph[u, 4] = Vector2.Distance(enemy.transform.position, transitionAreas[u].transform.position);
                graph[4, u] = graph[u, 4];
            }
            else
            {
                graph[u, 4] = 0f;
                graph[4, u] = 0f;
            }
        }


        float[] distance = new float[verticesCount];
        bool[] shortestPathTreeSet = new bool[verticesCount];
        AreaIdentifier[,] paths = new AreaIdentifier[verticesCount, verticesCount];



        for (int i = 0; i < verticesCount; ++i)
        {
            distance[i] = float.MaxValue;
            shortestPathTreeSet[i] = false;
        }

        distance[source] = 0;
        paths[source, 0] = transitionAreas[source];


        for (int count = 0; count < verticesCount - 1; ++count)
        {

            int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
            shortestPathTreeSet[u] = true;

            for (int v = 0; v < verticesCount; ++v)
            {
                if (!shortestPathTreeSet[v] && Convert.ToBoolean(graph[u, v]) && distance[u] != int.MaxValue && distance[u] + graph[u, v] < distance[v])
                {
                    distance[v] = distance[u] + graph[u, v];
                    int i = 0;
                    while (paths[u, i] != null && i < verticesCount)
                    {
                        paths[v, i] = paths[u, i];
                        i++;
                    }
                    paths[v, i] = transitionAreas[v];


                }

            }


        }

        return paths[destination, 0].transform.GetComponent<AreaIdentifier>();
    }
    //Dijkstra end

}





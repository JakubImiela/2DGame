using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius = 5f;
    [Range(0, 360)] public float angle = 45f;

    public GameObject playerRef;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstructionMask;

    public bool canSeePlayer {  get; private set; }

    private void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());
    }


    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            canSeePlayer = FieldOfViewCheck();
        }
    }

    private bool FieldOfViewCheck()
    {
        Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);

        if (rangeChecks.Length == 0)
            return false;

        Transform target = rangeChecks[0].transform;
        Vector2 directionToTarget = (target.position - transform.position).normalized;


        if (Vector2.Angle(GetComponent<Swat>().facingDirection, directionToTarget) >= angle / 2)
            return false;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
            return false;

        return true;
    }


}

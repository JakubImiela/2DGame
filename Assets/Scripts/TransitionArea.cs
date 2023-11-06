using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(BoxCollider2D))]
[RequireComponent (typeof(AreaIdentifier))]


public class TransitionArea : MonoBehaviour
{
    AreaIdentifier identifier;
    private void Start()
    {
        identifier = GetComponent<AreaIdentifier>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GPS.instance.setCurrentArea(identifier.areaName);
            UI.instance.setAreaName(identifier.areaName.ToString());
        }
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Swat>().lastArea = identifier;
            collision.GetComponent<AreaIdentifier>().areaName = identifier.areaName;
        }
    }

 
}

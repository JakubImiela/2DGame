using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaIdentifier : MonoBehaviour
{
    public Transform transportObject;
    public AreaIdentifier oppositeExit;
    public GPS.transitionType transitionType;
    public GPS.area areaName;
}

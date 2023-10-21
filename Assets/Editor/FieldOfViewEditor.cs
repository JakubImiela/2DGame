using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireDisc(fov.transform.position, Vector3.forward, fov.radius);

        Vector3 viewAngle01 = DirectionFromAngle(-fov.transform.eulerAngles.z, -fov.angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(-fov.transform.eulerAngles.z, fov.angle / 2);

        Handles.color = Color.yellow;

        if (fov.GetComponent<Swat>().facingDirection.x > 0f)
        {
            Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle01 * fov.radius);
            Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle02 * fov.radius);
        }
        else
        {
            Handles.DrawLine(fov.transform.position, fov.transform.position - viewAngle01 * fov.radius);
            Handles.DrawLine(fov.transform.position, fov.transform.position - viewAngle02 * fov.radius);
        }



        if (fov.canSeePlayer)
        {
            Handles.color = Color.green;
            Handles.DrawLine(fov.transform.position, fov.playerRef.transform.position);
        }


    }

    Vector2 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        //return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        return new Vector2(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
    }
}

/*

private void UpdateRopePositions()
{
    if (!ropeAttached) return;

    ropeRenderer.positionCount = ropePositions.Count + 1;

    int ropeRendererLastPosition = ropeRenderer.positionCount - 1;

    ropeRenderer.SetPosition(ropeRendererLastPosition, transform.position);

    for (var i = ropeRendererLastPosition - 1; i >= 0; i--)
    {
        ropeRenderer.SetPosition(i, ropePositions[i]);

        if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
        {
            Vector2 ropePosition = ropePositions[ropePositions.Count - 1];

            ropeHingeAnchorRb.transform.position = ropePosition;
            if (!distanceSet)
            {
                ropeJoint.distance = calculateRopeRemainingDistance();
                distanceSet = true;
            }

        }
        else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
        {
            Vector2 ropePosition = ropePositions.Last();
            ropeHingeAnchorRb.transform.position = ropePosition;
            if (!distanceSet)
            {
                ropeJoint.distance =
                    Vector2.Distance(transform.position, ropePosition);
            }
        }


    }

}


*/
using UnityEngine;
using System.Collections;

public class TargetController : MonoBehaviour {

    public float target = 1.0f;
    public GridController.Direction direction = GridController.Direction.Left;

    public void SetTarget(TargetController copyFrom)
    {
        target = copyFrom.target;
        direction = copyFrom.direction;
    }
    public void SetTarget(float newTarget, GridController.Direction newDirection)
    {
        target = newTarget;
        direction = newDirection;
    }
}

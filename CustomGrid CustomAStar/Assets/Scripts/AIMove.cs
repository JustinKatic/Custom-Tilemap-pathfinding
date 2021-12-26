using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMove : MonoBehaviour
{
    [SerializeField] PathFinding pathFinding;
    [SerializeField] Map map;
    [SerializeField] float speed = 5f;

    private List<Vector3> path;
    private int currentPathIndex = 0;

    private void Update()
    {
        //Sets path for AI to move towards mouse click Pos
        if (Input.GetMouseButtonDown(0))
        {
            if (path == null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 playerPos = transform.position;

                currentPathIndex = 0;

                path = pathFinding.FindPath(playerPos, mousePos);
            }
        }

        //moves AI towards each index in path till path complete
        if (path != null)
        {
            Vector3 targetDest = path[currentPathIndex];

            if (Vector3.Distance(transform.position, targetDest) > 0.01f)
            {
                Move(targetDest);
            }
            else
            {
                currentPathIndex++;
                if (currentPathIndex >= path.Count)
                    path = null;
            }
        }
    }

    //Moves towards given destination
    private void Move(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
    }

    //Draws AI Path
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }
}

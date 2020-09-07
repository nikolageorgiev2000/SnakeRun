using System;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public bool canMove;
    private bool moving;
    public float moveSpeed;
    private Vector3 moveDir;

    public bool isRotatableXY;
    public bool isRotatableZ;

    void Awake()
    {
        //Awake is always called just after the prefab is instantiated!!!!!
        randomizeOrientation();
        randomizeMovement();
    }

    void Update()
    {

    }

    public void updateTransform(float snakeSpeed, float heightUnits, Bounds bounds)
    {

        transform.position += Vector3.down * snakeSpeed * Time.fixedDeltaTime;

        //start moving
        if (canMove && moving && transform.position.y < heightUnits / 2 + 1)
        {
            transform.position += Time.fixedDeltaTime * moveDir;
            transform.right = moveDir;
        }

        RaycastHit hit;

        //stop moving if snake crosses path (like a robot)
        //spherecast of radius 1, since robot is about 1 unit wide
        if (Physics.BoxCast(transform.position, bounds.extents, moveDir, out hit, transform.rotation))
        {
            List<string> tags = new List<string>(new string[] { "Player","Obstacle","LethalObstacle","PickUp","Boundary"});
            if (tags.Contains(hit.collider.gameObject.tag)
                && hit.distance < 0.5f)
            {
                moving = false;
            }
        } else
        {
            if (canMove)
            {
                moving = true;
            }
        }

        if (transform.position.y < -10)
        {
            Destroy(this.gameObject);
        }
    }

    public void randomizeMovement()
    {
        //if the position.x sign is negative, direction should be to the right, and vice versa
        if (canMove)
        {
            moving = true;
            Vector3 randomDir = UnityEngine.Random.insideUnitSphere;
            moveDir = new Vector3(-Mathf.Sign(transform.position.x) * Mathf.Abs(randomDir.x), randomDir.y, 0);
        }
    }

    public void randomizeOrientation()
    {
        //add random rotations to add obstacle variety
        if (isRotatableXY && isRotatableZ)
        {
            transform.rotation = Quaternion.Euler(360 * UnityEngine.Random.insideUnitSphere);
        } else if (isRotatableXY)
        {
            transform.rotation = Quaternion.Euler(Vector3.Scale(360 * UnityEngine.Random.insideUnitSphere,Vector3.one - Vector3.forward));
        } else if (isRotatableZ)
        {
            transform.rotation = Quaternion.Euler(Vector3.Scale(360 * UnityEngine.Random.insideUnitSphere, Vector3.forward));
        }
    }

}

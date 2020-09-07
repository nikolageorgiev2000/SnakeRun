using UnityEngine;
using System.Collections.Generic;

public class SnakeBodyPhysics : MonoBehaviour
{
    public float length;
    public int segCount;
    private float segLen;

    //point position updates per second
    [Range(0.1f, 5)]
    public float updateFreq;
    private float segUpdatePeriod;

    private Vector3[] trailPoints;
    private Vector3[] prevPoints;
    private Vector3[] goalPoints;

    private LineRenderer line;
    private float timeTracker;

    private float speed;
    public float maxVertSpeed;
    public float accelScaler;
    public float thrustDragRatio;

    void Start()
    {
        segLen = length / segCount;

        trailPoints = new Vector3[segCount + 1];

        //instantiate trail's line renderer vertices (necessary although looks redundant)
        line = GetComponent<LineRenderer>();
        line.positionCount = segCount + 1;
        for (int i = 0; i <= segCount; i++)
        {
            trailPoints[i] = transform.position + i * Vector3.down * segLen;
            line.SetPosition(i, trailPoints[i]);
        }

        //instantiation and proper initial values inserted
        prevPoints = new Vector3[segCount + 1];
        goalPoints = new Vector3[segCount + 1];
        refreshTempPoints();

        timeTracker = 0;

        speed = 0;
        updateFreq = Mathf.Min(1, speed / (trailPoints[0].y - trailPoints[segCount].y));
        segUpdatePeriod = 1 / updateFreq / segCount;
    }

    void Update()
    {
        //draw body
        line.SetPositions(trailPoints);

    }

    void FixedUpdate()
    {
        calcSpeed();

        //increment time
        //need to delay transition from current position to position of previous trail point
        //so count up to 1/updateRate and use fractional translations in between
        timeTracker += Time.fixedDeltaTime;

        trailPoints[0] = transform.position;

        segUpdatePeriod = 1 / updateFreq / segCount;

        //move points to position expected at current time
        for (int i = segCount; i > 0; i--)
        {
            Vector3 horTrans = Mathf.Min(timeTracker / segUpdatePeriod, 1) * Vector3.Scale((goalPoints[i] - prevPoints[i]), new Vector3(1, 0, 0));
            trailPoints[i] = prevPoints[i] + horTrans;
        }

        adjustSnakeLength();

        //if update time is reached (1/updateRate) refresh goal and prev trail point positions
        //mod time tracker with update time to not lose the movements expected 
        //during the time spent past reaching the goal positions
        if (timeTracker > segUpdatePeriod)
        {
            timeTracker = timeTracker % (segUpdatePeriod);

            //goal points reached
            refreshTempPoints();
        }
    }

    private void adjustSnakeLength()
    {
        float actualLength = 0;
        for (int i = 1; i <= segCount; i++)
        {
            actualLength += (trailPoints[i] - trailPoints[i - 1]).magnitude;
        }
        float heightAdjustmentRatio = 1 - length / actualLength;
        for (int i = 1; i <= segCount; i++)
        {
            trailPoints[i].y += heightAdjustmentRatio * (trailPoints[0].y - trailPoints[i].y);
        }
    }

    //update PrevPoints and GoalPoints
    void refreshTempPoints()
    {
        for (int i = 1; i <= segCount; i++)
        {
            prevPoints[i] = trailPoints[i];
            goalPoints[i] = trailPoints[i - 1];
        }
    }


    private void calcSpeed()
    {
        //adjust to right responsiveness to match up with objects moving by
        //frequency = speed / distance = 1 / period time
        updateFreq = Mathf.Max(0.8f, speed / (trailPoints[0].y - trailPoints[segCount].y));

        //the horizontal components sum of the snake provide thrust
        float thrust = 0;
        int thrustedSegmentsCount = 0;

        bool afterFirstCurve = false;
        int frictionAngle = 10;

        for (int i = 1; i <= segCount; i++)
        {
            if (Vector3.Angle(trailPoints[i] - trailPoints[i - 1], Vector3.down) <= 10)
            {

            }

            if (afterFirstCurve)
            {
                //angles less than 15 degrees from the vertical don't have grip (lifted off ground)
                if (Vector3.Angle(trailPoints[i] - trailPoints[i - 1], Vector3.down) > frictionAngle)
                {
                    thrustedSegmentsCount++;
                    thrust += Vector3.Project(trailPoints[i] - trailPoints[i - 1], Vector3.right).magnitude;
                }
            } else
            {
                //angles less than 15 degrees from the vertical don't have grip (lifted off ground)
                if (Vector3.Angle(trailPoints[i] - trailPoints[i - 1], Vector3.down) <= frictionAngle)
                {
                    afterFirstCurve = true;
                }
            }



        }

        // thrustScalar + dragScalar = accelScalar = dragScalar * (1 + thrustDragRatio) 
        float thrustScaler = accelScaler / (1 + 1 / thrustDragRatio);
        float dragScaler = accelScaler / (1 + thrustDragRatio);

        //add thrust
        speed += thrustScaler * thrust * Time.fixedDeltaTime;

        //add drag
        speed -= dragScaler * speed / maxVertSpeed * Time.fixedDeltaTime;

        //clamp the speed
        speed = Mathf.Max(0, speed);
        speed = Mathf.Min(maxVertSpeed, speed);
    }


    public float getSnakeSpeed()
    {
        return speed;
    }

    public Vector3 getMoveDirection()
    {
        if(trailPoints == null)
        {
            return Vector3.up;
        } else
        {
            return (trailPoints[0] - trailPoints[1]).normalized;
        }
    }

}

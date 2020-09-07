using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public float maxHorV;
    public float minHorV;

    [Range(0, 25)]
    public float turnSharpness;

    private bool moveLeft;
    private bool moveRight;

    //public Material[] blobHues;
    //public Material[] trailHues;
    //private int currentBlobHue;
    //private int currentTrailHue;

    public enum PlayerMoveDir { Left, Forward, Right };

    private float health;
    public float maxHealth;
    public float healthLossRate;
    private float score;
    public float maxObstacleDamage;
    public float pickupHealthBoost;
    public float pickupPoints;

    private Vector3 transAmount;

    private GameObject lastObstacle;

    // Start is called before the first frame update
    void Start()
    {
        moveLeft = false;
        moveRight = false;

        score = 0;

        transAmount = Vector3.zero;

        health = maxHealth;

    }

    void OnCollisionEnter(Collision c)
    {
        Vector3 playerDir = GetComponentInChildren<SnakeBodyPhysics>().getMoveDirection();
        Vector3 collisionNorm = Vector3.zero;
        for (int i = 0; i < c.contactCount; i++)
        {
            collisionNorm += c.GetContact(0).normal;
        }
        collisionNorm /= c.contactCount;

        float collisionAngle = Mathf.Abs(Vector3.Angle(-c.contacts[0].normal, playerDir));

        switch (c.gameObject.tag)
        {
            case "Obstacle":
                //deal only half damage at 30 degree angle, full at 0 degree (total head-on collision)
                health -= Mathf.Cos(Mathf.Deg2Rad * collisionAngle) * maxObstacleDamage;
                break;
            case "LethalObstacle":
                health -= Mathf.Cos(Mathf.Deg2Rad * collisionAngle) * maxObstacleDamage;
                //lethal when head-on collision only
                if (collisionAngle < 30)
                {
                    health = 0;
                }
                break;
            case "PickUp":
                health += pickupHealthBoost;
                score += pickupPoints;
                //disable collider and destroy after 0.1s
                c.collider.enabled = false;
                Destroy(c.gameObject, 0.1f);
                break;
        }

        if(health <= 0)
        {
            //if the collider GO actually has a parent (like the rocks), destroy the parent
            Debug.Log(c.gameObject.transform.parent != null);
            lastObstacle = (c.gameObject.transform.parent != null) ? (c.gameObject.transform.parent.gameObject) : (c.gameObject);
        }

        health = Mathf.Max(0, Mathf.Min(100, health));
    }

    // Update is called once per frame
    void Update()
    {
        //lose healthLossRate amount of health every second
        health -= healthLossRate * Time.deltaTime;

        if ((Application.platform == RuntimePlatform.WindowsEditor) || (Application.platform == RuntimePlatform.WindowsPlayer))
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                moveLeft = true;
            }
            else
            {
                moveLeft = false;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                moveRight = true;
            }
            else
            {
                moveRight = false;
            }

            //if (Input.GetKeyDown(KeyCode.X))
            //{
            //    colorSwitch();
            //}
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.touchCount > 0)
            {
                moveRight = true;
                moveLeft = false;
            }
            else
            {
                moveLeft = true;
                moveRight = false;
            }
        }

    }

    void FixedUpdate()
    {
        //smooths turns using Lerp, prevents get sharp trail corners
        float turnTime = turnSharpness * Time.fixedDeltaTime;

        float horV = Mathf.Min(maxHorV, minHorV + (maxHorV - minHorV) * GetComponentInChildren<SnakeBodyPhysics>().getSnakeSpeed() / GetComponentInChildren<SnakeBodyPhysics>().maxVertSpeed);

        if (moveLeft)
        {
            transAmount = Vector3.Lerp(transAmount, new Vector3(-horV * Time.fixedDeltaTime, 0, 0), turnTime);
            //transform.Translate(new Vector3(-horV * Time.fixedDeltaTime, 0, 0));
        }
        else if (moveRight)
        {
            transAmount = Vector3.Lerp(transAmount, new Vector3(horV * Time.fixedDeltaTime, 0, 0), turnTime);
            //transform.Translate(new Vector3(horV * Time.fixedDeltaTime, 0, 0));
        }
        else
        {
            transAmount = Vector3.Lerp(transAmount, Vector3.zero, turnTime);
        }

        //NEEDS TO BE IN WORLD SPACE!!! Player is rotated locally, so without specifying world space local space leads to weirdness
        transform.Translate(transAmount, Space.World);

        transform.up = GetComponentInChildren<SnakeBodyPhysics>().getMoveDirection();
    }

    //void colorSwitch()
    //{
    //    currentBlobHue = (currentBlobHue + 1) % blobHues.Length;
    //    currentTrailHue = (currentTrailHue + 1) % trailHues.Length;
    //    GetComponent<MeshRenderer>().material = blobHues[currentBlobHue];
    //    GetComponentInChildren<LineRenderer>().material = trailHues[currentTrailHue];
    //}

    public PlayerMoveDir getPlayerDir()
    {
        if (moveLeft)
            return PlayerMoveDir.Left;
        if (moveRight)
            return PlayerMoveDir.Right;
        else
            return PlayerMoveDir.Forward;
    }

    public Vector3 getPlayerMoveVel()
    {
        return transAmount;
    }

    public float getScore()
    {
        return score;
    }

    public float getHealth()
    {
        return health / maxHealth;
    }

    public void resetHealth()
    {
        health = maxHealth;
    }

    public GameObject getLastObstacle()
    {
        return lastObstacle;
    }

}

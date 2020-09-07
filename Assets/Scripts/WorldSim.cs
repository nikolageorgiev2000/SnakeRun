using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class WorldSim : MonoBehaviour
{

    public enum HandSide { Right, Left };
    public HandSide playerHandSide;
    public GameObject mainCam;

    public GameObject snakePrefab;
    private GameObject playerSnake;

    public int obsCount;
    private int obsTypeCount;
    public GameObject[] obsPrefabs;
    private GameObject[] obsInstances;
    public float[] obsMaxWeights;
    private float[] obsCurrentWeights;

    public GameObject ground;
    private GameObject[] boundaries;
    public Texture boundaryTex;

    private float aspectRatio;
    private float widthUnits;
    private float heightUnits;
    public GameObject scoreDisplay;
    public GameObject healthUI;
    public GameObject playerUI;
    public GameObject endGameMenu;

    private float startTime;


    void Awake()
    {

        bool works = Physics.BoxCast(new Vector3(20, 20, 10), new Vector3(.2f, .2f, .2f), Vector3.forward);
        Debug.Log(works);


        playerSnake = Instantiate(snakePrefab, new Vector3(0, -2.0f, 0), transform.rotation);

        aspectRatio = (float)Screen.height / Screen.width;
        heightUnits = mainCam.GetComponent<Camera>().orthographicSize * 2;
        widthUnits = mainCam.GetComponent<Camera>().orthographicSize * 2 / aspectRatio;

        boundaries = new GameObject[2];
        for (int i = 0; i < 2; i++)
        {
            boundaries[i] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            boundaries[i].transform.position = new Vector3(Mathf.Pow(-1, i) * widthUnits / 2, 0, 0);
            boundaries[i].transform.localScale = new Vector3(0.5f, heightUnits * 3, 0.5f);
            boundaries[i].gameObject.tag = "Boundary";
            boundaries[i].GetComponent<MeshRenderer>().materials[0].shader = Shader.Find("Mobile/Diffuse");
            boundaries[i].GetComponent<MeshRenderer>().materials[0].mainTexture = boundaryTex;
            boundaries[i].GetComponent<MeshRenderer>().materials[0].mainTextureScale = new Vector2(1, boundaries[i].transform.localScale.y);
        }

        obsInstances = new GameObject[obsCount];
        for (int i = 0; i < obsCount; i++)
        {
            obsInstances[i] = null;
        }


        obsTypeCount = obsPrefabs.Length;
        obsCurrentWeights = new float[obsTypeCount];
        for (int i = 0; i < obsTypeCount; i++)
        {
            obsCurrentWeights[i] = 0;
        }

        //for (int i = 0; i < obsTypeCount; i++)
        //{
        //    Debug.Log(getColliderBounds(Instantiate(obsPrefabs[i])));
        //    GameObject.CreatePrimitive(PrimitiveType.Cube).transform.localScale = getColliderBounds(Instantiate(obsPrefabs[i])).extents;
        //}

        startTime = Time.time;

        healthUI.transform.position = mainCam.GetComponent<Camera>().WorldToScreenPoint(playerSnake.transform.position + Vector3.down);

        //make sure the game was not paused!
        Time.timeScale = 1;
    }


    void Update()
    {
        //start with 5 obstacles at a time, add one every seven seconds for a max of 25
        obsCount = Mathf.Min(8 + (int)(Time.time - startTime) / 8, 24);

        updateUI();

        if(playerSnake.GetComponent<SnakeController>().getHealth() <= 0.0f)
        {
            EndGame();
        }
    }

    void FixedUpdate()
    {
        float snakeSpeed = playerSnake.GetComponentInChildren<SnakeBodyPhysics>().getSnakeSpeed();
        instantiateObstacles();
        updateObstacles(snakeSpeed);
        moveGroundBoundaryTextures(snakeSpeed);
    }

    void updateObstacles(float snakeSpeed)
    {
        for (int i = 0; i < obsCount; i++)
        {
            if (obsInstances[i] != null)
            {
                obsInstances[i].GetComponent<Obstacle>().updateTransform(snakeSpeed, heightUnits, getColliderBounds(obsInstances[i]));
            }
        }
    }

    //place incoming obstacles appropriately
    void instantiateObstacles()
    {
        //allow for dynamically increasing the number of obstacles
        if (obsCount != obsInstances.Length)
        {
            GameObject[] temp = new GameObject[obsCount];
            for (int i = 0; i < obsCount; i++)
            {
                temp[i] = (i < obsInstances.Length) ? (obsInstances[i]) : (null);
            }
            obsInstances = temp;
        }

        //use weights to determine which obstacles are more frequent
        float obsTotalWeight;

        for (int i = 0; i < obsCount; i++)
        {
            //if there is an empty spot for an obstacle instance
            if (obsInstances[i] == null)
            {

                //adjust and sum obstacle weights for picking one
                for (int j = 0; j < obsTypeCount; j++)
                {
                    obsCurrentWeights[j] = Mathf.Min(obsMaxWeights[j], obsCurrentWeights[j] + obsMaxWeights[j] / obsTypeCount);
                }

                obsTotalWeight = 0;
                for (int j = 0; j < obsTypeCount; j++)
                {
                    obsTotalWeight += obsCurrentWeights[j];
                }

                string typesCount = "";
                for (int l = 0; l < obsTypeCount; l++)
                {
                    typesCount += ", " + obsCurrentWeights[l];
                }
                //Debug.Log(obsTotalWeight);
                //Debug.Log(typesCount);

                //select the obstacle prefab and instatiate it
                float randNum = Random.Range(0, obsTotalWeight);
                float weightTracker = 0;
                for (int j = 0; j < obsTypeCount; j++)
                {
                    weightTracker += obsCurrentWeights[j];
                    if (weightTracker > randNum)
                    {

                        int tries = 0;

                        //find a location to place the obstacle
                        float x, y;
                        do
                        {
                            if (obsInstances[i] != null)
                            {
                                Destroy(obsInstances[i].gameObject);
                            }

                            x = 0.8f * widthUnits / 2 * Random.insideUnitCircle.x;
                            y = 1.5f * heightUnits + heightUnits * Random.insideUnitCircle.y;
                            obsInstances[i] = Instantiate(obsPrefabs[j]);

                            //GO needs to be set active for bounds.extents to be nonzero
                            //however, GO cannot be in the position currently being tested, as it would always detect itself as an obstacle
                            //that's why you instatiate all obstacles at a far location
                            obsInstances[i].transform.position = Vector3.one * 100;

                            //if more than 10 random positions don't work, move to next timestep (don't stall)
                            tries++;
                            if (tries >= 10)
                            {
                                Destroy(obsInstances[i].gameObject);
                                Debug.Log("FAILED TO FIND SPOT");
                                obsInstances[i] = null;
                                break;
                            }
                            else
                            {
                                obsInstances[i].transform.position = new Vector3(x, y, 0);
                            }
                        } while (obstacleAt(x, y, obsInstances[i]));

                        obsCurrentWeights[j] = obsMaxWeights[j] / obsTypeCount;
                        //can only place one obstacle at a time
                        //since any further added ones wouldnt be able to check its collider until next timestep
                        break;
                    }
                }
                break;
            }
        }
    }

    void moveGroundBoundaryTextures(float speed)
    {
        Vector2 newBGoffset = ground.GetComponent<MeshRenderer>().materials[0].mainTextureOffset;
        newBGoffset += new Vector2(speed * Time.fixedDeltaTime / heightUnits, 0);
        newBGoffset.x = newBGoffset.x % 1;
        ground.GetComponent<MeshRenderer>().materials[0].mainTextureOffset = newBGoffset;
        for (int i = 0; i < 2; i++)
        {
            //multiply by heightUnits since the plane has one tile while boundaries have heightUnits number of tiles
            boundaries[i].GetComponent<MeshRenderer>().materials[0].mainTextureOffset = new Vector2(Mathf.Pow(-1, i) * newBGoffset.x, newBGoffset.x * heightUnits / 2);
        }
    }

    bool obstacleAt(float x, float y, GameObject obstacle)
    {
        Bounds bounds = getColliderBounds(obstacle);

        //previous position detection attempts
        //RaycastHit hit;
        //bool temp = Physics.SphereCast(Vector3.back * 15, bounds.extents.magnitude / Mathf.Sqrt(3), new Vector3(x, y, 15), out hit, 1000);
        //bool temp = Physics.BoxCast(Vector3.back * 15, bounds.extents, new Vector3(x, y, 15), out hit, obstacle.transform.rotation, 1000);

        //1.5 times extents so obstacles are spread out in a way the snake can actually move around
        Collider[] temp = Physics.OverlapBox(new Vector3(x, y, 0), 1.5f * bounds.extents, obstacle.transform.rotation);

        //Visualization of obstacle placements
        //Debug.Log(obstacle.transform.rotation + ", " + bounds.extents + ", " + (temp.Length > 0));

        //Debug.DrawLine(Vector3.back * 15, Vector3.back * 15 + new Vector3(x, y, 15), Color.red, 10);

        //Vector3[] points = CubePoints(Vector3.back * 15 + new Vector3(x, y, 15), bounds.extents, obstacle.transform.rotation);
        //DrawCubePoints(points);

        //if no colliders in overlap box, spot is free
        return temp.Length > 0;
    }

    Bounds getColliderBounds(GameObject x)
    {
        Bounds bounds;
        if (x.GetComponent<Collider>() != null)
        {
            bounds = x.GetComponent<Collider>().bounds;
        }
        else
        {
            bounds = x.GetComponentInChildren<Collider>().bounds;
        }
        return bounds;
    }

    Vector3[] CubePoints(Vector3 center, Vector3 extents, Quaternion rotation)
    {
        Vector3[] points = new Vector3[8];
        points[0] = rotation * Vector3.Scale(extents, new Vector3(1, 1, 1)) + center;
        points[1] = rotation * Vector3.Scale(extents, new Vector3(1, 1, -1)) + center;
        points[2] = rotation * Vector3.Scale(extents, new Vector3(1, -1, 1)) + center;
        points[3] = rotation * Vector3.Scale(extents, new Vector3(1, -1, -1)) + center;
        points[4] = rotation * Vector3.Scale(extents, new Vector3(-1, 1, 1)) + center;
        points[5] = rotation * Vector3.Scale(extents, new Vector3(-1, 1, -1)) + center;
        points[6] = rotation * Vector3.Scale(extents, new Vector3(-1, -1, 1)) + center;
        points[7] = rotation * Vector3.Scale(extents, new Vector3(-1, -1, -1)) + center;

        return points;
    }

    void DrawCubePoints(Vector3[] points)
    {
        Debug.DrawLine(points[0], points[1], Color.red, 10);
        Debug.DrawLine(points[0], points[2], Color.red, 10);
        Debug.DrawLine(points[0], points[4], Color.red, 10);

        Debug.DrawLine(points[7], points[6], Color.red, 10);
        Debug.DrawLine(points[7], points[5], Color.red, 10);
        Debug.DrawLine(points[7], points[3], Color.red, 10);

        Debug.DrawLine(points[1], points[3], Color.red, 10);
        Debug.DrawLine(points[1], points[5], Color.red, 10);

        Debug.DrawLine(points[2], points[3], Color.red, 10);
        Debug.DrawLine(points[2], points[6], Color.red, 10);

        Debug.DrawLine(points[4], points[5], Color.red, 10);
        Debug.DrawLine(points[4], points[6], Color.red, 10);
    }

    private void updateUI()
    {
        scoreDisplay.GetComponent<TMPro.TextMeshProUGUI>().text = ((int) playerSnake.GetComponent<SnakeController>().getScore()).ToString();
        healthUI.GetComponentsInChildren<Image>()[1].fillAmount = playerSnake.GetComponent<SnakeController>().getHealth();
        Vector3 targetHealthPos = playerSnake.transform.position + Vector3.down;
        healthUI.transform.position = Vector3.Slerp(healthUI.transform.position, mainCam.GetComponent<Camera>().WorldToScreenPoint(targetHealthPos), 5 * Time.deltaTime);
    }

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Resume()
    {
        Time.timeScale = 1;
    }

    public void Continue()
    {
        Time.timeScale = 1;
        GameObject lastObs = playerSnake.GetComponent<SnakeController>().getLastObstacle();
        Destroy(lastObs);
        playerSnake.GetComponent<SnakeController>().resetHealth();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
        updateSaveData();
    }

    public void EndGame()
    {
        Time.timeScale = 0;
        playerUI.SetActive(false);
        endGameMenu.SetActive(true);
    }

    public void Restart()
    {
        updateSaveData();
        SceneManager.LoadScene(1);
    }

    public void updateSaveData()
    {
        Data previousState = SaveData.LoadPlayer();
        previousState.update(playerSnake.GetComponent<SnakeController>().getScore());
        SaveData.SavePlayer(previousState);
    }


    [System.Serializable]
    public class Data
    {
        [SerializeField]
        private float points;
        [SerializeField]
        private float highestScore;
        [SerializeField]
        private float lastScore;

        public Data(float points, float highestScore, float lastScore)
        {
            this.points = points;
            this.highestScore = highestScore;
            this.lastScore = lastScore;
        }

        public void update(float gameScore)
        {
            points += gameScore;
            highestScore = Mathf.Max(gameScore, highestScore);
            lastScore = gameScore;
        }

        public override bool Equals(object obj)
        {
            return obj is Data data &&
                   points == data.points &&
                   highestScore == data.highestScore &&
                   lastScore == data.lastScore;
        }

        public float getTotalPoints()
        {
            return points;
        }

        public float getHighestScore()
        {
            return highestScore;
        }

        public float getLastScore()
        {
            return lastScore;
        }

        public override int GetHashCode()
        {
            var hashCode = -985458238;
            hashCode = hashCode * -1521134295 + points.GetHashCode();
            hashCode = hashCode * -1521134295 + highestScore.GetHashCode();
            hashCode = hashCode * -1521134295 + lastScore.GetHashCode();
            return hashCode;
        }
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MainMenu : MonoBehaviour
{

    public GameObject bestScoreUI;
    public GameObject lastScoreUI;
    public GameObject totalScoreUI;

    void Start()
    {
        WorldSim.Data data = SaveData.LoadPlayer();
        Debug.Log(data);
        bestScoreUI.GetComponent<TMPro.TextMeshProUGUI>().text = "BEST \n " + (int)data.getHighestScore();
        lastScoreUI.GetComponent<TMPro.TextMeshProUGUI>().text = "LAST \n " + (int)data.getLastScore();
        totalScoreUI.GetComponent<TMPro.TextMeshProUGUI>().text = "Total Points \n " + (int)data.getTotalPoints();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

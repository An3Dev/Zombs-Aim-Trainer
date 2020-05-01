using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class SpawnTargets : MonoBehaviour
{

    public GameObject targetPrefab;


    public static SpawnTargets Instance;

    public bool startGame = false;

    public int maxNumOfTargets = 30;

    int targetsLeft;

    public TextMeshProUGUI targetsLeftText, timerText, accuracyText, targetsDestroyedText;

    public GameObject clickToStart, duringGameUI, gameOverUI;


    float timer = 0;

    public bool gameDone = false;

    float accuracy;

    bool showedGameUI = false;
    private void Awake()
    {
        Instance = this;
        targetsLeft = maxNumOfTargets;
    }

    // Start is called before the first frame update
    void Start()
    {
        targetsLeftText.text = targetsLeft + " targets left";
    }

    // Update is called once per frame
    void Update()
    {
        if (targetsLeft <= 0)
        {
            gameDone = true;
        }

        if (gameDone && !showedGameUI)
        {
            // Bring up stats
            accuracy = (float)maxNumOfTargets / (float)(Shoot.Instance.timesShot - 1);

            gameOverUI.SetActive(true);
            duringGameUI.SetActive(false);

            accuracyText.text = "You hit " + (accuracy * 100).ToString("00.0") + "% of your shots";

            targetsDestroyedText.text = "You destroyed " + maxNumOfTargets + " targets in " + SecondsToFormattedTime(timer);

            showedGameUI = true;
        }

        if (startGame && !gameDone)
        {
            timer += Time.deltaTime;
            timerText.text = SecondsToFormattedTime(timer);
        }

        if (Input.GetMouseButtonDown(0) && !startGame && targetsLeft > 0)
        {
            startGame = true;
            SpawnTarget();

            duringGameUI.SetActive(true);
            clickToStart.SetActive(false);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    string SecondsToFormattedTime(float seconds)
    {
        int intTime = (int)seconds;
        int minutes = intTime / 60;
        int s = intTime % 60;
        float fraction = seconds * 1000;
        fraction = (fraction % 1000);
        string timeText = (minutes != 0) ? string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction) : string.Format("{0:00}:{1:00}", seconds, fraction);
        return timeText;
    }

    void SpawnTarget()
    {
        if (gameDone || targetsLeft <= 0) return;

        Vector2 randomPos = new Vector2(Random.Range(-9, 9), Random.Range(-4.5f, 4.5f));
        Instantiate(targetPrefab, randomPos, Quaternion.identity);
    }

    public void DestroyedTarget()
    {
        targetsLeft--;
        targetsLeftText.text = targetsLeft + " targets left";

        SpawnTarget();
        
    }
}

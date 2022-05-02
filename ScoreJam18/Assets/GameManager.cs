using System.Collections;
using LootLocker.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    
    [SerializeField]
    float seaLevel = 10;
    [SerializeField]
    ShopButton[] shopButtons = null;

    int score = 0;
    int money = 0;

    [SerializeField]
    TextMeshProUGUI scoreText = null;
    [SerializeField]
    Button castButton, repairButton = null;
    [SerializeField]
    int repairCost = 10;
    [SerializeField]
    GameObject gameoverScreen = null;
    [SerializeField]
    GameObject camera = null;
    [SerializeField]
    GameObject canvas = null;
    [SerializeField]
    Transform drone = null;
    [SerializeField]
    Transform ballon = null, waterSurface = null;


    [Header("Fish Spawn Properties")]
    [SerializeField]
    int maxPop = 10;
    [SerializeField]
    GameObject[] fishToSpawn = null;
    [SerializeField]
    Vector2 timeVariance = Vector2.zero;
    [SerializeField]
    float commonUpperBound, uncommonUpperBound, rareUpperBound;
    [SerializeField]
    Vector2 depthMinMax = Vector2.zero;
    [SerializeField]
    Vector2 widthMinMax = Vector2.zero;

    [Header("UI Properties")]
    [SerializeField]
    GameObject scoreBoard = null;
    [SerializeField]
    GameObject scoreboardHolder =null, scoreElement = null, resetButton = null;
    [SerializeField]
    GameObject scoreSubmit = null;


    private void Awake()
    {
        if (GM == null)
        {
            GM = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < shopButtons.Length; i++)
        {
            shopButtons[i].SetButtonActive(money);
        }

        for (int i = 0; i < maxPop; i++)
        {
            CreateFish();
        }

        repairButton.gameObject.SetActive(false);
        castButton.gameObject.SetActive(true);
        
        gameoverScreen.SetActive(false);

        
    }

    private void InitializeLootLocker(string _name)
    {
        // Roy: Use these methods to start a session, set the name, set the score, and get the list
        Debug.Log("It Worky: " + _name);
        //return;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.Log("error starting LootLocker session");
                return;
            }

            Debug.Log("successfully started LootLocker session");
            LootLockerSDKManager.SetPlayerName(/*"Steve Sux"*/ _name,
                r =>
                {
                    LootLockerSDKManager.SubmitScore(response.player_id.ToString(), score, 2771,
                        scoreResponse =>
                        {
                            if (!scoreResponse.success)
                            {
                                Debug.Log("Failed to submit score");
                                return;
                            }
                    
                            Debug.Log("Submitted score!");
                            LootLockerSDKManager.GetScoreList( 2771, 15, 
                                scoreResponse2 =>
                                {
                                    for (int i = 0; i < 15; i++)
                                    {
                                        Transform _scoreElement = Instantiate(scoreElement, scoreboardHolder.transform).transform;

                                        if (i < scoreResponse2.items.Length)
                                        {
                                            _scoreElement.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                                            scoreResponse2.items[i].player.name;

                                            _scoreElement.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                                            GetDisplayScore(scoreResponse2.items[i].score);
                                        }
                                        else
                                        {
                                            _scoreElement.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                                            "N/A";

                                            _scoreElement.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                                            GetDisplayScore(0);
                                        }
                                    }

                                    Instantiate(resetButton, scoreboardHolder.transform).GetComponent<Button>().onClick.AddListener(ResetGame);
                                });
                        });
                });
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float SeaLevel
    { 
        get { return seaLevel; }
    }

    public void AddScore(int _value)
    {
        StartCoroutine(UpdateScoreText(score, money, _value));

        score += _value;
        money += _value;

        UpdateButtons();
    }

    public void SubtractMoney(int _value)
    {
        StartCoroutine(UpdateMoneyText(money, _value));

        money -= _value;

        UpdateButtons();
    }

    public IEnumerator UpdateScoreText(int curScore, int curMoney, int pointsToAdd)
    {
        int displayScore = curScore;
        int displayMoney = curMoney;

        for (int i = 0; i <= pointsToAdd; i++)
        {
            scoreText.text = "Score: " + GetDisplayScore(displayScore) + "\nMoney: $" + GetDisplayScore(displayMoney);
            
            yield return new WaitForSeconds(0.01f);

            displayScore++;
            displayMoney++;
        }
    }

    public IEnumerator UpdateMoneyText(int curMoney, int pointsToAdd)
    {
        int displayScore = curMoney;

        for (int i = 0; i <= pointsToAdd; i++)
        {
            scoreText.text = "Score: " + GetDisplayScore(score) + "\nMoney: $" + GetDisplayScore(displayScore);

            yield return new WaitForSeconds(0.01f);

            displayScore--;
        }
    }

    public void UpdateButtons()
    {
        for (int i = 0; i < shopButtons.Length; i++)
        {
            shopButtons[i].SetButtonActive(money);
        }
    }

    public string GetDisplayScore(int score)
    {
        string scoreText = score.ToString();
        string returnString = "";
        
        for (int i = 0; i < 10 - scoreText.Length; i++)
        {
            returnString += "0";
        }
        returnString += scoreText;

        return returnString;
    }

    public void CastDrone() 
    {
        LeanTween.move(drone.gameObject, waterSurface.position, 2);
        LeanTween.scale(drone.GetChild(1).GetChild(1).gameObject, Vector3.one, 2);
        LeanTween.delayedCall(2f, () => drone.GetComponent<LureBehavior>().enabled = true);
        LeanTween.delayedCall(2f, () => drone.GetChild(1).gameObject.SetActive(false));
        
        canvas.SetActive(false);
    }

    public void TransitionTopSide(bool _caughtFish, bool _didPlayerDie)
    {
        drone.GetComponent<LureBehavior>().enabled = false;

        LeanTween.move(drone.gameObject, ballon.position, 2);
        LeanTween.rotate(drone.gameObject, Vector3.zero, 2);
        LeanTween.move(camera, new Vector3(0, 13.5f, -90), 2);
        
        canvas.SetActive(true);

        if (_caughtFish)
        {
            LeanTween.delayedCall(Random.Range(timeVariance.x, timeVariance.y), CreateFish);
        }

        if (_didPlayerDie)
        {
            if (money >= repairCost)
            {
                castButton.gameObject.SetActive(false);
                repairButton.gameObject.SetActive(true);
            }
            else
            {
                EndGame();
            }
        }
    }

    public void EndGame()
    {
        gameoverScreen.SetActive(true);
        scoreSubmit.SetActive(true);
        scoreBoard.SetActive(false);
    }

    public void SumbitScore(TMP_InputField _input)
    {
        scoreBoard.SetActive(true);
        scoreSubmit.SetActive(false);

        InitializeLootLocker(_input.text);
    }

    public void CreateFish()
    {
        Instantiate(fishToSpawn[Random.Range(0, fishToSpawn.Length)], Vector3.zero, Quaternion.identity).GetComponent<FishBehavior>().Init();
    }

    public Vector3 MakeRarityDepthPos(float _rarity) 
    {
        float depth = Mathf.Abs(depthMinMax.x - depthMinMax.y);

        if (_rarity >= 0 && _rarity < commonUpperBound)
        {
            return new Vector3(Random.Range(widthMinMax.x, widthMinMax.y), -Random.Range(0, (depth / 4) * 1), 0);
        }

        else if(_rarity >= commonUpperBound && _rarity < uncommonUpperBound)
        {
            return new Vector3(Random.Range(widthMinMax.x, widthMinMax.y), -Random.Range((depth / 4) * 1, (depth / 4) * 2), 0);
        }

        else if (_rarity >= uncommonUpperBound && _rarity < rareUpperBound)
        {
            return new Vector3(Random.Range(widthMinMax.x, widthMinMax.y), -Random.Range((depth / 4) * 2, (depth / 4) * 3), 0);
        }

        else
        {
            return new Vector3(Random.Range(widthMinMax.x, widthMinMax.y), -Random.Range((depth / 4) * 3, (depth / 4) * 4), 0);
        }
    }

    public void ResetGame()
    {
        gameoverScreen.SetActive(false);
        Debug.Log("reset Game");
        //Application.reload
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(-100, seaLevel, 0), new Vector3(100, seaLevel, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-100, depthMinMax.x, 0), new Vector3(100, depthMinMax.x, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-100, depthMinMax.y, 0), new Vector3(100, depthMinMax.y, 0));

        Gizmos.color = Color.black;
        Gizmos.DrawLine(new Vector3(widthMinMax.x, -100,  0), new Vector3(widthMinMax.x, 100, 0));
        Gizmos.DrawLine(new Vector3(widthMinMax.y, -100, 0), new Vector3(widthMinMax.y, 100, 0));
    }
}

[System.Serializable]
public class ShopButton 
{
    [SerializeField]
    Button shopButton = null;
    [SerializeField]
    int buttonPushCost = 0;

    public void SetButtonActive(int _value) 
    {
        shopButton.interactable = buttonPushCost <= _value;
        Debug.Log(shopButton.interactable);
    } 
}

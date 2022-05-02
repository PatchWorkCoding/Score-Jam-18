using System.Collections;
using System.Collections.Generic;
using LootLocker.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    
    [SerializeField]
    float seaLevel = 10;

    int score = 0;
    [SerializeField]
    int money = 0;

    [SerializeField]
    GameObject LurePrefab = null;

    [SerializeField]
    TextMeshProUGUI scoreText = null;
    [SerializeField]
    Button castButton, repairButton = null;
    [SerializeField]
    int repairCost = 10;
    [SerializeField]
    GameObject gameoverScreen = null;
    [SerializeField]
    public GameObject camera = null;
    [SerializeField]
    GameObject canvas = null;
    [SerializeField]
    Transform drone = null;
    [SerializeField]
    Transform ballon = null, waterSurface = null;
    [SerializeField]
    List<string> bannedNames = null;

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

    List<GameObject> spawnedFish = null;

    [Header("UI Properties")]
    [SerializeField]
    ShopButton[] shopButtons = null;
    [SerializeField]
    GameObject scoreBoard = null;
    [SerializeField]
    GameObject scoreboardHolder =null, scoreElement = null, resetButton = null;
    [SerializeField]
    GameObject scoreSubmit = null;

    bool isResetingGame = false;

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
        spawnedFish = new List<GameObject>();

        for (int i = 0; i < shopButtons.Length; i++)
        {
            shopButtons[i].InitButton();
            shopButtons[i].SetButtonActive(money, false);
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
            LeanTween.delayedCall(Random.Range(timeVariance.x, timeVariance.y), MakeNewFish);
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
                EndGame(true);
            }
        }
    }

    public void EndGame(bool didDie)
    {
        gameoverScreen.SetActive(true);
        scoreSubmit.SetActive(true);
        if (didDie)
        {
            scoreSubmit.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = 
                "You Ran out of money\nYour Score: " + GetDisplayScore(score);
        }
        else
        {
            scoreSubmit.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Your Score: " + GetDisplayScore(score);
        }
        scoreBoard.SetActive(false);
    }

    public void SumbitScore(TMP_InputField _input)
    {
        if (!bannedNames.Contains(_input.text.ToLower()) && _input.text != "")
        {
            scoreBoard.SetActive(true);
            scoreSubmit.SetActive(false);

            InitializeLootLocker(_input.text);
        }
        else
        {
            _input.text = "";
        }
    }

    public void CreateFish()
    {

        GameObject _curFish = Instantiate(fishToSpawn[Random.Range(0, fishToSpawn.Length)], Vector3.zero, Quaternion.identity);
        _curFish.GetComponent<FishBehavior>().Init();
        spawnedFish.Add(_curFish);
    }

    public void MakeNewFish()
    {
        GameObject _curFish = Instantiate(fishToSpawn[Random.Range(0, fishToSpawn.Length)], Vector3.zero, Quaternion.identity);
        _curFish.GetComponent<FishBehavior>().Init();
        spawnedFish.Add(_curFish);

        if (spawnedFish.Count < maxPop)
        {
            LeanTween.delayedCall(Random.Range(timeVariance.x, timeVariance.y), MakeNewFish);
        }
    }

    public Vector3 MakeRarityDepthPos(float _rarity) 
    {
        float depth = Vector2.Distance(new Vector2(0, depthMinMax.x), new Vector2(0, depthMinMax.y));

        if (_rarity >= 0 && _rarity < commonUpperBound)
        {
            return new Vector3(Random.Range(widthMinMax.x, widthMinMax.y), depthMinMax.x - Random.Range(0, (depth / 4) * 1), 0);
        }

        else if(_rarity >= commonUpperBound && _rarity < uncommonUpperBound)
        {
            return new Vector3(Random.Range(widthMinMax.x, widthMinMax.y), depthMinMax.x - Random.Range((depth / 4) * 1, (depth / 4) * 2), 0);
        }

        else if (_rarity >= uncommonUpperBound && _rarity < rareUpperBound)
        {
            return new Vector3(Random.Range(widthMinMax.x, widthMinMax.y), depthMinMax.x - Random.Range((depth / 4) * 2, (depth / 4) * 3), 0);
        }

        else
        {
            return new Vector3(Random.Range(widthMinMax.x, widthMinMax.y), depthMinMax.x - Random.Range((depth / 4) * 3, (depth / 4) * 4), 0);
        }
    }

    public void ResetGame()
    {
        //gameoverScreen.SetActive(false);

        score = 0;
        money = 0;

        isResetingGame = false;
        
        for (int i = 0; i < spawnedFish.Count; i++)
        {
            Destroy(spawnedFish[i]);
        }

        isResetingGame = true;

        scoreText.text = "Score: " + GetDisplayScore(score) + "\nMoney: $" + GetDisplayScore(money);

        Destroy(drone.gameObject);
        drone = Instantiate(LurePrefab, ballon.transform.position, Quaternion.identity).transform;

        for (int i = 0; i < shopButtons.Length; i++)
        {
            shopButtons[i].SetButtonActive(money);
        }

        for (int i = 0; i < maxPop; i++)
        {
            CreateFish();
        }

        if (scoreboardHolder.transform.childCount > 0)
        {
            int scoreCount = scoreboardHolder.transform.childCount;

            for (int i = 0; i < scoreCount; i++)
            {
                Destroy(scoreboardHolder.transform.GetChild(i).gameObject);
            }
        }

        repairButton.gameObject.SetActive(false);
        castButton.gameObject.SetActive(true);

        gameoverScreen.SetActive(false);


        Debug.Log("reset Game");
        //Application.reload
    }

    public void RemoveFish(GameObject _fish)
    {
        if (!isResetingGame) 
        {
            if (spawnedFish.Contains(_fish))
            {
                spawnedFish.Remove(_fish);
                spawnedFish.TrimExcess();
            }
        }        
    }

    public void UpgradePlayerPullForce(float _value)
    {
        drone.GetComponent<LureBehavior>().IncreaseBasePullForce(_value);
    }

    public void UpgradePlayerOverdriveForce(float _value)
    {
        drone.GetComponent<LureBehavior>().IncreaseOverdriveForce(_value);
    }

    public void UpgradePlayerOverdriveFuel(float _value)
    {
        drone.GetComponent<LureBehavior>().IncreaseFuel(_value);
    }

    public void IncreasePopulationSize(int _value) 
    {
        maxPop += _value;
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
    string buttonName = "Updgrade";
    [SerializeField]
    Button shopButton = null;
    [SerializeField]
    int buttonPushCost = 0;
    [SerializeField]
    float upgradeCost = 1.1f;
    [SerializeField]
    int maxButtonUses = -1;

    int buttonUses = 0;

    public void InitButton()
    {
        shopButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = buttonName + "\nCost ($" + buttonPushCost + ")";
        shopButton.onClick.AddListener(() => GameManager.GM.SubtractMoney(buttonPushCost));
        shopButton.onClick.AddListener(ButtonClick);
    }

    public void SetButtonActive(int _value, bool _didPushButton = true) 
    {
        shopButton.interactable = buttonPushCost <= _value;
        Debug.Log(shopButton.interactable);
    } 

    public void ButtonClick()
    {
        buttonPushCost = Mathf.RoundToInt(buttonPushCost * upgradeCost);
        if (maxButtonUses > 0)
        {
            if (maxButtonUses < buttonUses)
            {
                shopButton.interactable = false;
                shopButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = buttonName + "\nMaxed";
            }

            else
            {
                buttonUses++;
                shopButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = buttonName + "\nCost ($" + buttonPushCost + ")";
            }
        }
        else
        {
            shopButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = buttonName + "\nCost ($" + buttonPushCost + ")";
        }
    }
}

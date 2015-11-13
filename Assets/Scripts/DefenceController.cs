using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DefenceController : LevelController {

    private GameObject ui, boxList;
    private Text waveText;
    private Image painPanel;
    private Text painText;
    private string[] painMessages;

    public string objective;
    public Vector3[] pathPoints;
    public int money = 2000, health, maxHealth = 100;
    //Wave controls
    public bool isWave;
    public int waveNumber = 0;
    public float enemyHealthBase = 50.0f, enemySpeedBase = 1.0f;
    public int enemyNumberBase = 20, enemyValueBase = 150,
        enemyHealthDoublePeriod = 5, enemySpeedDoublePeriod = 20, enemyNumberDoublePeriod = 10, enemyValueDoublePeriod = 10;
    public float curEnemyHealth, curEnemySpeed;
    public int curEnemyNumber, curEnemyValue,
        curEnemyCount, averageBossWaveSeparation, wavesUntilBoss;
    public float wavePauseTime = 10.0f, spawnPeriod = 3.0f;
    private float curSpawnTime;
    public bool isBossWave;

    public GameObject boxButton, enemy;
    public AudioClip ouchSound;

    public int[] placementCosts = new int[Global.BoxCount];
    private Text[] availableTexts = new Text[Global.BoxCount];
    private Text healthText, moneyText;
    private RectTransform selectionPanel;
    private Global.BoxType[] typesAvailable = null;

	// Use this for initialization
    new void Start()
    {
        base.Start();
        ui = GameObject.Find("Defence UI");
        boxList = ui.transform.FindChild("Box List").gameObject;
        waveText = ui.transform.FindChild("Wave Panel").FindChild("Wave Text").GetComponent<Text>();
        painPanel = ui.transform.FindChild("Pain Panel").GetComponent<Image>();
        painText = painPanel.transform.FindChild("Pain Text").GetComponent<Text>();
        painMessages = new string[] {
            "Ow!",
            "Ow!",
            "Ouch!",
            "Ouch!",
            "Argh!",
            "Oompf!",
            "Crikey!",
            "Oof!",
        };
        painPanel.color = new Color(painPanel.color.r, painPanel.color.g, painPanel.color.b, 0);
        painText.color = new Color(painText.color.r, painText.color.g, painText.color.b, 0);
        GameObject curButton;
        int totalAvailable = 0;
        float curY = -50.0f;
        for (int n = 1; n < Global.BoxCount; n++)
        {
            if (placementCosts[n] >= 0)
            {
                curButton = (GameObject)Instantiate(boxButton);
                curButton.transform.SetParent(boxList.transform, false);
                curButton.transform.FindChild("Button").GetChild(0).GetComponent<Text>().text//(Add spaces before non-first capital letters)
                    = ((Global.BoxType)n).ToString()[0]
                    + System.Text.RegularExpressions.Regex.Replace(((Global.BoxType)n).ToString().Remove(0, 1), "[A-Z]", " $&");
                curButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, curY);
                //curButton.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction())
                int index = n;
                curButton.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => BoxButtonPressed(index));
                availableTexts[n] = curButton.transform.FindChild("Text").GetComponent<Text>();
                availableTexts[n].text = placementCosts[n].ToString();
                //Set colour-coding of this button
                ColorBlock colourBlock =  curButton.transform.FindChild("Button").GetComponent<Button>().colors;
                colourBlock.normalColor = Color.Lerp(
                    gc.placementBoxes[n].transform.FindChild("Body").GetComponent<MeshRenderer>().sharedMaterial.color, Color.white, 0.5f);
                curButton.transform.FindChild("Button").GetComponent<Button>().colors = colourBlock;

                totalAvailable++;
                curY -= 37.5f;
            }
        }
        typesAvailable = new Global.BoxType[totalAvailable];
        int typeIndex = 0;
        for (int n = 1; n < Global.BoxCount; n++)
        {
            if (placementCosts[n] >= 0)
            {
                typesAvailable[typeIndex] = (Global.BoxType)n;
                typeIndex++;
            }
        }
        gc.SetTypesAvailable(typesAvailable);
        if (typesAvailable.Length > 0)
        {
            gc.SetPlacementType(typesAvailable[0]);
        }
        else
        {
            gc.SetPlacementType(Global.BoxType.None);
        }

        ui.transform.FindChild("Objective Panel").FindChild("Objective Text").GetComponent<Text>().text
            = Application.loadedLevelName + " - " + objective;
        selectionPanel = boxList.transform.FindChild("Selection Panel").GetComponent<RectTransform>();

        moneyText = boxList.transform.FindChild("Money Text").GetComponent<Text>();
        healthText = boxList.transform.FindChild("Health Text").GetComponent<Text>();
        health = maxHealth;

        wavesUntilBoss = averageBossWaveSeparation + Random.Range(-2, 2);
	}
	
	// Update is called once per frame
    new void Update()
    {
        if (health <= 0)
        {
            levelFinished = true;
            FinishLevel();
        }
        if (!levelFinished)
        {
            painPanel.color = new Color(painPanel.color.r, painPanel.color.g, painPanel.color.b, Mathf.Lerp(0, painPanel.color.a, 0.9f));
            painText.color = new Color(painText.color.r, painText.color.g, painText.color.b, Mathf.Lerp(0, painText.color.a, 0.9f));
        }
        /*for (int n = 1; n < Global.BoxCount; n++)
        {
            availableTexts[n].text = (placementAvailable[n] - placementMade[n]).ToString();
        }*/
        Global.BoxType type = gc.GetPlacementType();
        for (int n = typesAvailable.Length - 1; n >= 0; n--)
        {
            availableTexts[(int)typesAvailable[n]].text = placementCosts[(int)typesAvailable[n]].ToString();
            if (type == typesAvailable[n])
            {
                selectionPanel.anchoredPosition
                    = new Vector2(0.0f, selectionPanel.anchoredPosition.y * 0.6f + (-12.5f + (n + 1) * -37.5f) * 0.4f);
                if (money >= placementCosts[(int)typesAvailable[n]])
                {
                    selectionPanel.GetComponent<Image>().color
                        = Color.Lerp(new Color32(50, 50, 50, 125), selectionPanel.GetComponent<Image>().color, 0.5f);
                }
                else
                {
                    selectionPanel.GetComponent<Image>().color
                        = Color.Lerp(new Color32(150, 5, 5, 175), selectionPanel.GetComponent<Image>().color, 0.5f);
                }
            }
        }

        moneyText.text = "Money: £" + money;
        healthText.text = "Health: " + health + " / " + maxHealth;

        if (!levelFinished && pathPoints.Length > 1)
        {
            curSpawnTime += Time.deltaTime;
            if (isWave)
            {
                if (curEnemyCount < curEnemyNumber)
                {
                    if (curSpawnTime >= spawnPeriod)
                    {
                        curSpawnTime -= spawnPeriod;
                        SpawnEnemy();
                        curEnemyCount++;
                    }
                }
                else
                {
                    if (GameObject.FindGameObjectsWithTag("Enemy").Length <= 0)
                    {
                        isWave = false;
                        curSpawnTime = 0.0f;
                    }
                }
            }
            else
            {
                if (curSpawnTime >= wavePauseTime)
                {
                    StartNextWave();
                }
            }
        }
        if (isWave)
        {
            int remainingNumber = curEnemyNumber - curEnemyCount;
            if (remainingNumber > 1)
            {
                waveText.text = "Wave " + waveNumber + ": " + remainingNumber.ToString() + " zombies are still on their way!";
            }
            else if (remainingNumber == 1)
            {
                waveText.text = "Wave " + waveNumber + ": " + remainingNumber.ToString() + " zombie is still on its way!";
            }
            else
            {
                waveText.text = "Wave " + waveNumber + ": all of the zombies are here; kill them to complete the wave!";
            }
        }
        else
        {
            if (waveNumber > 0)
            {
                waveText.text = "Wave " + waveNumber + " complete: next wave begins in " + Mathf.RoundToInt(wavePauseTime - curSpawnTime).ToString()
                    + " (press [Space] to skip)";
                if (wavesUntilBoss == 1)
                {
                    waveText.text += " Boss wave inbound!";
                }
            }
            else
            {
                waveText.text = "Place a Boolean Source box on the map. Zombies will arrive in " + Mathf.RoundToInt(wavePauseTime - curSpawnTime).ToString()
                    + " (press [Space] to skip)";
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartNextWave();
            }

        }
    }

    void OnDrawGizmos()
    {
        if (pathPoints.Length > 0)
        {
            Gizmos.color = Color.white;
            for (int n = pathPoints.Length - 1; n > 0; n--)
            {
                Gizmos.DrawLine(pathPoints[n], pathPoints[n - 1]);
                Gizmos.DrawSphere(pathPoints[n], 0.1f);
            }
            Gizmos.DrawSphere(pathPoints[0], 0.1f);
        }
    }

    protected override void FinishLevel()
    {
        painPanel.color = new Color(painPanel.color.r, painPanel.color.g, painPanel.color.b, 0.8f);
        painText.color = new Color(painText.color.r, painText.color.g, painText.color.b, 1.0f);
        painText.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        painText.text = ("Game Over!#@You made it to wave " + waveNumber
            + "#@Press [Esc] to continue...").Replace("#@", System.Environment.NewLine);
        Time.timeScale = 0.25f;
    }

    void BoxButtonPressed(int type)
    {
        gc.SetPlacementType((Global.BoxType)type);
    }

    public override bool CanPlaceBox(Global.BoxType type)
    {
        if (money < placementCosts[(int)type])
        {
            return false;
        }
        return true;
    }
    public override bool CanMoveBox(Global.BoxType type) { return true; }

    public override void PlaceBox(Global.BoxType type)
    {
        money -= placementCosts[(int)type];
    }
    public override void DeleteBox(Global.BoxType type)
    {
        money += Mathf.FloorToInt(placementCosts[(int)type] * 0.75f);
    }

    void StartNextWave()
    {
        curSpawnTime = 0.0f;
        waveNumber++;
        curEnemyHealth = enemyHealthBase * (1 + ((float)waveNumber / (float)enemyHealthDoublePeriod));
        curEnemySpeed = enemySpeedBase * (1 + ((float)waveNumber / (float)enemySpeedDoublePeriod));
        curEnemyNumber = Mathf.RoundToInt((float)enemyNumberBase * (1 + ((float)waveNumber / (float)enemyNumberDoublePeriod)));
        curEnemyValue = Mathf.RoundToInt((float)enemyValueBase * (1 + ((float)waveNumber / (float)enemyValueDoublePeriod)));
        curEnemyCount = 1;
        curSpawnTime = spawnPeriod;
        wavesUntilBoss--;
        if (wavesUntilBoss == 0)
        {
            wavesUntilBoss = averageBossWaveSeparation + Random.Range(-2, 2);
            curEnemyHealth *= 4.0f;
            curEnemySpeed *= 0.4f;
            curEnemyNumber = Mathf.CeilToInt(curEnemyNumber / 5);
            curEnemyValue *= 4;
            isBossWave = true;
        }
        else
        {
            isBossWave = false;
        }
        isWave = true; 
    }

    public void SpawnEnemy()
    {
        GameObject curEnemy
            = (GameObject)Instantiate(enemy, pathPoints[0], Quaternion.LookRotation(pathPoints[1] - pathPoints[0], Vector3.up));
        curEnemy.GetComponent<EnemyController>().SetStats(curEnemyHealth, curEnemySpeed, curEnemyValue);
        if (isBossWave)
        {
            curEnemy.transform.localScale = Vector3.one * 1.5f;
        }
    }

    public Vector3 GetPathPoint(int pointIndex)
    {
        if (pathPoints.Length > 0 && pointIndex >= 0 && pointIndex < pathPoints.Length)
        {
            return pathPoints[pointIndex];
        }
        return Vector3.zero;
    }
    public int GetPathLength()
    {
        return pathPoints.Length;
    }
    public void ReduceHealth(int amount)
    {
        health -= amount;
        health = Mathf.Max(0, health);
        painPanel.color = new Color(painPanel.color.r, painPanel.color.g, painPanel.color.b, 0.8f);
        painText.color = new Color(painText.color.r, painText.color.g, painText.color.b, 1.0f);
        painText.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(-30.0f, 30.0f));
        painText.text = painMessages[Random.Range(0, painMessages.Length - 1)];
        if (ouchSound != null)
        {
            AudioSource.PlayClipAtPoint(ouchSound, Vector3.zero, 0.2f);
        }
    }
    public void AddMoney(int amount)
    {
        money += amount;
    }
}

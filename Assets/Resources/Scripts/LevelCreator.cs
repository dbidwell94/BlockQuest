using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LevelCreator : MonoBehaviour {

    public GameObject bot, player, wall, goal, patrolPoint;
    public Button botButton, playerButton, wallButton, goalButton, selectObjectButton, spawnButton;
    public Button mainMenuButton, saveDialogButton, loadButton, clearButton, optionsButton, selectBotButton;
    public GameObject saveDialog;
    public Button saveButton;
    public Button spawnPatrol;
    public Text levelName;
    public GameObject optionsMenu;
    private GameObject objectToSpawn;
    private GameObject objectMenu;
    private GameObject floor;
    private Vector3 objectPos;
    public GameObject botSelectMenu;
    private Dictionary<Vector3, GameObject> bots, goals, walls, players, allObjects;
    private int botSelection = 0;
    private bool optionsShowing = false;
    private Texture2D levelScreenshot;
    private List<GameObject> botList;
    private Dictionary<GameObject, List<Vector3>> botsWithPPoints;
    private GameObject selectedBot;
    private List<GameObject> selectedBotPPoints;

    private void Start()
    {
        selectedBotPPoints = new List<GameObject>();
        selectBotButton.interactable = false;
        bots = new Dictionary<Vector3, GameObject>();
        botList = new List<GameObject>();
        goals = new Dictionary<Vector3, GameObject>();
        walls = new Dictionary<Vector3, GameObject>();
        players = new Dictionary<Vector3, GameObject>();
        allObjects = new Dictionary<Vector3, GameObject>();
        floor = GameObject.Find("Play_Field");
        objectMenu = GameObject.Find("Object_Menu");
        objectMenu.SetActive(false);
        selectObjectButton.onClick.AddListener(delegate { ShowObjectMenu(); });
        botButton.onClick.AddListener(delegate { SelectObjectToDrag(bot); });
        playerButton.onClick.AddListener(delegate { SelectObjectToDrag(player); });
        wallButton.onClick.AddListener(delegate { SelectObjectToDrag(wall); });
        goalButton.onClick.AddListener(delegate { SelectObjectToDrag(goal); });
        spawnButton.onClick.AddListener(delegate { PlaceObject(objectToSpawn); });
        mainMenuButton.onClick.AddListener(delegate { UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); });
        optionsButton.onClick.AddListener(delegate {
            if (selectedBotPPoints.Count > 0)
            {
                foreach (GameObject obj in selectedBotPPoints)
                {
                    Destroy(obj);
                }
            }
            optionsShowing = !optionsShowing;
            if (optionsShowing)
            {
                selectObjectButton.interactable = false;
                optionsMenu.SetActive(true);
                ClearObjectToSpawn();
            }
            else
            {
                optionsMenu.SetActive(false);
                selectObjectButton.interactable = true;
            }
        });
        clearButton.onClick.AddListener(delegate { ClearLevel(); });
        saveDialogButton.onClick.AddListener(delegate
        {
            saveDialog.SetActive(true);
            Image screenshotImage = saveDialog.transform.GetChild(0).GetComponent<Image>();
            Texture2D screenSprite = Screenshot.Capture(GameObject.Find("Main Camera").GetComponent<Camera>(), 512, 512);
            screenshotImage.sprite = Sprite.Create(screenSprite, new Rect(0, 0, screenSprite.width, screenSprite.height), new Vector2());
            levelScreenshot = screenSprite;
        });
        saveButton.onClick.AddListener(delegate { SaveLevel(); });
        selectBotButton.onClick.AddListener(delegate { CycleBots(); });
        spawnPatrol.onClick.AddListener(delegate { SelectObjectToDrag(patrolPoint);  botSelectMenu.SetActive(false);
            if (botsWithPPoints != null && !botsWithPPoints.ContainsKey(selectedBot))
            {
                botsWithPPoints.Add(selectedBot, new List<Vector3>());
            }
            else if (botsWithPPoints == null)
            {
                botsWithPPoints = new Dictionary<GameObject, List<Vector3>>();
                botsWithPPoints.Add(selectedBot, new List<Vector3>());
            }
        });
    }

    void SelectObjectToDrag(GameObject obj)
    {
        objectToSpawn = Instantiate(obj, new Vector3(0, 1, 0), new Quaternion());
        objectToSpawn.name = obj.name;
        objectPos = objectToSpawn.transform.position;
        objectMenu.SetActive(false);
        selectObjectButton.interactable = true;
    }

    private void Update()
    {
        if (objectToSpawn != null)
        {
            MoveObject();
        }
    }

    void ClearObjectToSpawn()
    {
        if (objectToSpawn != null)
        {
            Destroy(objectToSpawn);
        }
    }

    void ShowObjectMenu()
    {
        if (selectedBotPPoints.Count > 0)
        {
            foreach (GameObject obj in selectedBotPPoints)
            {
                Destroy(obj);
            }
        }
        ClearObjectToSpawn();
        selectObjectButton.interactable = false;
        objectMenu.SetActive(true);
    }

    void MoveObject()
    {
        MeshCollider floorCol = floor.GetComponent<MeshCollider>();
        float x = GameManager._Instance.joystick.JoystickOutput.x * Time.deltaTime * 10;
        float y = GameManager._Instance.joystick.JoystickOutput.y * Time.deltaTime * 10;
        Vector3 tempPos = objectPos + new Vector3(x, objectToSpawn.transform.localScale.y, y);
        if (tempPos.x >= floorCol.bounds.min.x && tempPos.x <= floorCol.bounds.max.x)
        {
            if (tempPos.z >= floorCol.bounds.min.z && tempPos.z <= floorCol.bounds.max.z)
            {
                objectPos = tempPos;
                objectToSpawn.transform.position = new Vector3(Mathf.Round(objectPos.x), 1, Mathf.Round(objectPos.z));
            }
        }      
    }
    void PlaceObject(GameObject obj)
    {
        Vector3 toPlace = new Vector3(objectToSpawn.transform.position.x, obj.transform.localScale.y / 2, objectToSpawn.transform.position.z);
        if (!allObjects.ContainsKey(toPlace))
        {
            switch (obj.name)
            {
                default:
                    break;
                case "Bot_static":
                    GameObject newBot = Instantiate(obj, toPlace, new Quaternion());
                    bots.Add(newBot.transform.position, newBot);
                    allObjects.Add(newBot.transform.position, newBot);
                    selectBotButton.interactable = true;
                    botList.Add(newBot);
                    break;
                case "Goal-Prefab":
                    GameObject newGoal = Instantiate(obj, toPlace, new Quaternion());
                    goals.Add(newGoal.transform.position, newGoal);
                    allObjects.Add(newGoal.transform.position, newGoal);
                    break;
                case "Wall_prefab":
                    GameObject newWall = Instantiate(obj, toPlace, new Quaternion());
                    walls.Add(newWall.transform.position, newWall);
                    allObjects.Add(newWall.transform.position, newWall);
                    break;
                case "Player-prefab":
                    GameObject newPlayer = Instantiate(obj, toPlace, new Quaternion());
                    players.Add(newPlayer.transform.position, newPlayer);
                    allObjects.Add(newPlayer.transform.position, newPlayer);
                    break;
                case "Patrol-prefab":
                    botsWithPPoints[selectedBot].Add(toPlace);
                    GameObject newPPoint = Instantiate(obj, toPlace, new Quaternion());
                    selectedBotPPoints.Add(newPPoint);
                    break;
            }
        }
        if (players.Count >= 1)
        {
            playerButton.interactable = false;
            if (obj.transform.tag == "Player")
            {
                ClearObjectToSpawn();
            }
        }
        
    }

    void ClearLevel()
    {
        if (allObjects.Count > 0)
        {
            foreach (KeyValuePair<Vector3, GameObject> obj in allObjects)
            {
                Destroy(obj.Value);
            }
            allObjects.Clear();
            bots.Clear();
            players.Clear();
            walls.Clear();
            goals.Clear();
        }        
    }

    void SaveLevel()
    {
        XMLDataManager.Player playerToSave = new XMLDataManager.Player();
        foreach (KeyValuePair<Vector3, GameObject> p in players)
        {
            playerToSave.loc = p.Key;
            playerToSave.playerName = p.Value.name;
        }
        List<XMLDataManager.Bot> botsToSave = new List<XMLDataManager.Bot>();
        foreach (KeyValuePair<Vector3, GameObject> o in bots)
        {
            XMLDataManager.Bot bot = new XMLDataManager.Bot
            {
                botName = "Bot", location = o.Key
            };
            if (botsWithPPoints != null)
            {
                if (botsWithPPoints.ContainsKey(o.Value))
                {
                    bot.pPoints = botsWithPPoints[o.Value].ToArray();
                }
            }
            
            else bot.pPoints = null;
            botsToSave.Add(bot);
        }
        List<XMLDataManager.Wall> wallsToSave = new List<XMLDataManager.Wall>();
        foreach (KeyValuePair<Vector3, GameObject> o in walls)
        {
            XMLDataManager.Wall wall = new XMLDataManager.Wall
            {
                wallName = "wall", loc = o.Key
            };
            wallsToSave.Add(wall);
        }
        List<XMLDataManager.Goal> goalsToSave = new List<XMLDataManager.Goal>();
        foreach (KeyValuePair<Vector3, GameObject> g in goals)
        {
            XMLDataManager.Goal goal = new XMLDataManager.Goal
            {
                goalName = "goal", loc = g.Key
            };
            goalsToSave.Add(goal);
        }
        saveDialog.SetActive(false);
        XMLDataManager toSave = new XMLDataManager
        {
            bots = botsToSave.ToArray(), goals = goalsToSave.ToArray(), walls = wallsToSave.ToArray(),
            lName = levelName.text, player = playerToSave, timesPlayed = 0
        };
        XMLDataLoaderSaver.Save(toSave, levelScreenshot);
        LevelManager.RebuildLists();
    }

    void CycleBots()
    {
        ClearObjectToSpawn();
        if (botSelection > botList.Count - 1)
        {
            botSelection = 0;
        }
        botSelectMenu.SetActive(true);
        Vector3 botPos = botList[botSelection].transform.position;
        RectTransform menuTrans = botSelectMenu.GetComponent<RectTransform>();
        Vector3 toPut = new Vector3(botPos.x, botPos.y + 1, botPos.z + 2);
        selectedBot = botList[botSelection];

        if (selectedBotPPoints.Count > 0)
        {
            foreach (GameObject obj in selectedBotPPoints)
            {
                Destroy(obj);
            }
        }
        if (botsWithPPoints != null && botsWithPPoints.ContainsKey(botList[botSelection]))
        {
            foreach (Vector3 point in botsWithPPoints[botList[botSelection]])
            {
                GameObject pPoint = Instantiate(patrolPoint, point, new Quaternion());
                selectedBotPPoints.Add(pPoint);
            }
        }

        botSelection += 1;
        menuTrans.position = toPut;

    }
}

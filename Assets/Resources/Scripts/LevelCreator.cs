using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LevelCreator : MonoBehaviour {

    public GameObject bot, player, wall, goal, patrolPoint, delete;
    public Button botButton, playerButton, wallButton, goalButton, deleteButton, selectObjectButton, spawnButton;
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
    public GameObject loadLevelPanel;
    public GameObject levelButton;
    private List<GameObject> currentLevelButtons;
    private LevelManager.Level levelToLoad;
    public GameObject levelOptionsMenu;

    private Vector3 cameraPos;

    private void Start()
    {
        currentLevelButtons = new List<GameObject>();
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
        deleteButton.onClick.AddListener(delegate { SelectObjectToDrag(delete); });
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
            if (botSelectMenu.activeInHierarchy) botSelectMenu.SetActive(false);
        });
        clearButton.onClick.AddListener(delegate { ClearLevel(); });
        saveDialogButton.onClick.AddListener(delegate
        {
            saveDialog.SetActive(true);
            Image screenshotImage = saveDialog.transform.GetChild(0).GetComponent<Image>();
            Texture2D screenSprite = Screenshot.Capture(Camera.main, 512, 512);
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
        loadButton.onClick.AddListener(ShowLevelMenu);
        cameraPos = Camera.main.transform.position;
        levelOptionsMenu.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate {
            if (FirebaseManager.saveLoc == "Default_Levels")
            {
                FirebaseManager.UploadFileToFirebase(levelToLoad);
            }
            else FirebaseManager.UserUploadToFirebase(levelToLoad);
            levelOptionsMenu.SetActive(false);
            selectObjectButton.interactable = true;
            optionsButton.interactable = true;
        });
        levelOptionsMenu.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate {
            LoadLevel();
            levelOptionsMenu.SetActive(false);
            selectObjectButton.interactable = true;
            optionsButton.interactable = true;
        });
        levelOptionsMenu.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate {
            LevelManager.DeleteLevel(levelToLoad);
            levelOptionsMenu.SetActive(false);
            selectObjectButton.interactable = true;
            optionsButton.interactable = true;
        });
        loadLevelPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate {
            loadLevelPanel.SetActive(false);
            optionsShowing = false;
            optionsButton.interactable = true;
            selectObjectButton.interactable = true;
        });
    }

    void SelectObjectToDrag(GameObject obj)
    {
        objectToSpawn = Instantiate(obj, new Vector3(0, 2.5f, 0), new Quaternion());
        objectToSpawn.name = obj.name;
        objectPos = objectToSpawn.transform.position;
        objectMenu.SetActive(false);
        selectObjectButton.interactable = true;
        Camera.main.transform.SetParent(objectToSpawn.transform);
    }

    private void Update()
    {
        if (objectToSpawn != null)
        {
            MoveObject();
        }
        PinchAndZoom();
    }

    void PinchAndZoom()
    {
        if (Input.touches.Length >= 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            float newFOV = Camera.main.fieldOfView + deltaMagnitudeDiff * Time.deltaTime * 2.5f;

            newFOV = Mathf.Clamp(newFOV, 10, 60);
            Camera.main.fieldOfView = newFOV;
        }
       
    }

    void ClearObjectToSpawn()
    {
        Camera.main.transform.SetParent(null);
        Camera.main.transform.position = cameraPos;
        Camera.main.fieldOfView = 60;        
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
        if (botSelectMenu.activeInHierarchy) botSelectMenu.SetActive(false);
    }

    void MoveObject()
    {
        MeshCollider floorCol = floor.GetComponent<MeshCollider>();
        float x = DPad._Instance.DPadOutput.x * Time.deltaTime * 10;
        float z = DPad._Instance.DPadOutput.z * Time.deltaTime * 10;
        Vector3 tempPos = objectPos + new Vector3(x, 0, z);
        if (tempPos.x >= floorCol.bounds.min.x && tempPos.x <= floorCol.bounds.max.x)
        {
            if (tempPos.z >= floorCol.bounds.min.z && tempPos.z <= floorCol.bounds.max.z)
            {
                objectPos = tempPos;
                objectToSpawn.transform.position = new Vector3(Mathf.Round(objectPos.x), 2.5f, Mathf.Round(objectPos.z));
            }
        }      
    }

    // This method gets called when you press the select button in the level creator
    void PlaceObject(GameObject obj)
    {
        Vector3 toPlace = new Vector3(objectToSpawn.transform.position.x, obj.transform.localScale.y / 2, objectToSpawn.transform.position.z);
        if (!allObjects.ContainsKey(toPlace) && objectToSpawn.name != "Delete Object")
        {
            Camera.main.transform.parent = null;
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
        if (objectToSpawn.name == "Delete Object")
        {
            foreach (KeyValuePair<Vector3, GameObject> del in allObjects)
            {
                if (del.Key.x == toPlace.x && del.Key.z == toPlace.z)
                {
                    DeleteObject(del.Key);
                    break;
                }
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
        if (obj.transform.tag != "Player")
        {
            Camera.main.transform.parent = objectToSpawn.transform;
        }
    }

    // Clears all current gameobjects in the scene
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
        selectBotButton.interactable = false;
        playerButton.interactable = true;
    }

    // serializes the objects in the scene into an xml file
    void SaveLevel()
    {
        Camera.main.transform.SetParent(null);
        Camera.main.fieldOfView = 60;
        Camera.main.transform.position = cameraPos;
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

    // cycles through the current bots in the scene to select
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

    void ShowLevelMenu()
    {
        optionsShowing = false;
        optionsButton.interactable = false;
        if (currentLevelButtons.Count > 0)
        {
            foreach (GameObject obj in currentLevelButtons)
            {
                Destroy(obj);
            }
            currentLevelButtons = new List<GameObject>();
            
        }
        optionsMenu.SetActive(false);
        loadLevelPanel.SetActive(true);
        GameObject levelsContainer = loadLevelPanel.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).gameObject;
        foreach (LevelManager.Level level in LevelManager.MyLevels)
        {
            GameObject newButton = Instantiate(levelButton);
            //levelsContainer.GetComponent<RectTransform>().sizeDelta += new Vector2(0, newButton.GetComponent<RectTransform>().sizeDelta.y);
            newButton.transform.SetParent(levelsContainer.transform);
            newButton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            Sprite screenshot = Sprite.Create(level.LevelPic, new Rect(0, 0, level.LevelPic.width, level.LevelPic.height), new Vector2());
            newButton.transform.GetChild(0).GetComponent<Image>().sprite = screenshot;
            newButton.GetComponentInChildren<Text>().text = level.LevelName;
            newButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                levelToLoad = level;
                loadLevelPanel.SetActive(false);
                levelOptionsMenu.SetActive(true);
            });
            currentLevelButtons.Add(newButton);
        }
        if (currentLevelButtons.Count > 0)
        {
            RectTransform buttonTrans = currentLevelButtons[0].GetComponent<RectTransform>();
            float scale = Screen.height / Screen.width;
            float ySize = buttonTrans.sizeDelta.y * currentLevelButtons.Count;
            levelsContainer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ySize);
        }
    }

    void LoadLevel()
    {
        if (levelToLoad.LevelPath != null)
        {
            loadLevelPanel.SetActive(false);
            ClearLevel();
            playerButton.interactable = true;
            XMLDataManager container;
            XMLDataLoaderSaver.Load(levelToLoad, out container);
            foreach (XMLDataManager.Bot b in container.bots)
            {
                GameObject newBot = Instantiate(bot, b.location, new Quaternion());
                bots.Add(newBot.transform.position, newBot);
                if (b.pPoints != null)
                {
                    List<Vector3> botPPoints = new List<Vector3>();
                    foreach (Vector3 point in b.pPoints)
                    {
                        botPPoints.Add(point);
                    }
                    if (botsWithPPoints == null)
                    {
                        botsWithPPoints = new Dictionary<GameObject, List<Vector3>>();
                    }
                    botsWithPPoints.Add(newBot, botPPoints);
                }
                botList.Add(newBot);
                allObjects.Add(newBot.transform.position, newBot);
                selectBotButton.interactable = true;
            }
            foreach (XMLDataManager.Goal g in container.goals)
            {
                GameObject newGoal = Instantiate(goal, g.loc, new Quaternion());
                goals.Add(newGoal.transform.position, newGoal);
                allObjects.Add(newGoal.transform.position, newGoal);
            }
            foreach (XMLDataManager.Wall w in container.walls)
            {
                GameObject newWall = Instantiate(wall, w.loc, new Quaternion());
                walls.Add(newWall.transform.position, newWall);
                allObjects.Add(newWall.transform.position, newWall);
            }
            if (container.player.loc != null)
            {
                GameObject newPlayer = Instantiate(player, container.player.loc, new Quaternion());
                allObjects.Add(newPlayer.transform.position, newPlayer);
                players.Add(newPlayer.transform.position, newPlayer);
                playerButton.interactable = false;
            }
        }
        optionsShowing = false;
        selectObjectButton.interactable = true;
    }

    void DeleteObject(Vector3 keyLoc)
    {
        if (allObjects.Count > 0 && allObjects.ContainsKey(keyLoc))
        {
            
            if (botsWithPPoints != null && botsWithPPoints.ContainsKey(allObjects[keyLoc])) botsWithPPoints.Remove(allObjects[keyLoc]);
            if (bots != null && bots.ContainsKey(keyLoc))
            {
                bots.Remove(keyLoc);
                if (bots.Count <= 0) selectBotButton.interactable = false;
            }
            if (walls != null && walls.ContainsKey(keyLoc)) walls.Remove(keyLoc);
            if (players != null && players.ContainsKey(keyLoc))
            {
                players.Remove(keyLoc);
                playerButton.interactable = true;
            }
            if (goals != null && goals.ContainsKey(keyLoc)) goals.Remove(keyLoc);
            if (botList != null && botList.Contains(allObjects[keyLoc])) botList.Remove(allObjects[keyLoc]);
            Destroy(allObjects[keyLoc]);
            allObjects.Remove(keyLoc);
        }
    }
}

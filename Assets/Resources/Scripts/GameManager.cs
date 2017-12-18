using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using System.IO;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour{

    public static GameManager _Instance;
    public static List<GameObject> currentGameObjects;
    public Joystick joystick;
    private AudioClip[] bgmSounds;
    private AudioSource bgmPlayer;
    private bool isInGame = false;
    private bool level_complete = false;
    public GameObject player, bot, wall, goal;
    private int goalNum = 0;
    public Button optionsButton;
    public Button mainMenuButton;
    public Button resumeButton;
    public GameObject mainMenu;

    public Slider joySizer;
    public Slider joySensitivity;
    public Toggle joyToggle;
    private float joyBkgSize = 512, joyButtonSize = 175;
    private DateTime timeStarted;

	// Use this for initialization
	void Start () {
        if (PlayerPrefs.HasKey("Joy Size Multiplier"))
        {
            float mult = PlayerPrefs.GetFloat("Joy Size Multiplier");
            Vector2 bkgSize = new Vector2(joyBkgSize * mult, joyBkgSize * mult);
            Vector2 btnSize = new Vector2(joyButtonSize * mult, joyButtonSize * mult);

            joystick.GetComponent<RectTransform>().sizeDelta = bkgSize;
            joystick.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = btnSize;
        }
        _Instance = this;
        bgmSounds = Resources.LoadAll<AudioClip>("Sounds/BGM");
        bgmPlayer = GetComponent<AudioSource>();
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainGame"))
        {
            timeStarted = DateTime.Now;
            isInGame = true;
            LoadLevel(LevelManager.levelToLoad);
            optionsButton.onClick.AddListener(delegate
            {
                mainMenu.SetActive(true);
                if (PlayerPrefs.HasKey("Joy Size Multiplier"))
                {
                    joySizer.value = PlayerPrefs.GetFloat("Joy Size Multiplier");
                }
                else joySizer.value = joystick.GetComponent<RectTransform>().sizeDelta.x / joyBkgSize;
                if (PlayerPrefs.HasKey("Joystick Sensitivity"))
                {
                    joySensitivity.value = PlayerPrefs.GetFloat("Joystick Sensitivity");
                }
                if (PlayerPrefs.HasKey("Joystick Active"))
                {
                    joyToggle.isOn = (PlayerPrefs.GetString("Joystick Active") == "yes") ? true : false;
                }
            });
            mainMenuButton.onClick.AddListener(delegate
            {
                GooglePlayGames.PlayGamesPlatform.Instance.ReportScore((DateTime.Now - timeStarted).Milliseconds, GPGSIds.leaderboard_marathon, (bool success) => { });
                SceneManager.LoadScene("MainMenu");
            });
            resumeButton.onClick.AddListener(delegate
            {
                mainMenu.SetActive(false);
            });
            joySizer.onValueChanged.AddListener(delegate
            {
                Vector2 bkgSize = new Vector2(joyBkgSize * joySizer.value, joyBkgSize * joySizer.value);
                Vector2 btnSize = new Vector2(joyButtonSize * joySizer.value, joyButtonSize * joySizer.value);

                joystick.GetComponent<RectTransform>().sizeDelta = bkgSize;
                joystick.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = btnSize;

                PlayerPrefs.SetFloat("Joy Size Multiplier", joySizer.value);
                PlayerPrefs.Save();
            });
            joySensitivity.onValueChanged.AddListener(delegate
            {
                PlayerPrefs.SetFloat("Joystick Sensitivity", joySensitivity.value);
                player.GetComponent<PlayerController>().UpdateSensitivity();
                PlayerPrefs.Save();
            });
            joyToggle.onValueChanged.AddListener(delegate {
                SelectInputMethod(joyToggle.isOn);
            });
            if (PlayerPrefs.HasKey("Joystick Active"))
            {
                if (PlayerPrefs.GetString("Joystick Active") == "yes")
                {
                    DPad._Instance.gameObject.SetActive(false);
                    joystick.gameObject.SetActive(true);
                }
                else if (PlayerPrefs.GetString("Joystick Active") == "no")
                {
                    DPad._Instance.gameObject.SetActive(true);
                    joystick.gameObject.SetActive(false);
                }
            }
        }
        GenerateGrid();
        
    }
	
	// Update is called once per frame
	void Update () {
        if (isInGame)
        {
            PlayMusic();
            if (FirebaseManager.user != null)
            {
                TimeSpan timePlayed = DateTime.Now - timeStarted;
                if (timePlayed.Minutes >= 5)
                {
                    GooglePlayGames.PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_5_minute_man, 100.0, (bool success) => { });
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Texture2D goodPic = Screenshot.Capture(GameObject.Find("Main Camera").GetComponent<Camera>(), 2880, 1440);
            byte[] toSave = goodPic.EncodeToJPG();
            File.WriteAllBytes(Application.persistentDataPath + "/screenshot.jpg", toSave);
        }
	}

    void PlayMusic()
    {
        if (bgmPlayer.isPlaying == false)
        {
            bgmPlayer.PlayOneShot(bgmSounds[UnityEngine.Random.Range(0, bgmSounds.Length - 1)]);
        }
    }

    private void OnApplicationQuit()
    {
        GooglePlayGames.PlayGamesPlatform.Instance.ReportScore((DateTime.Now - timeStarted).Milliseconds, GPGSIds.leaderboard_marathon, (bool success) => { });
    }

    void LoadLevel(LevelManager.Level level)
    {
        if (currentGameObjects == null)
        {
            currentGameObjects = new List<GameObject>();
        }
        if (currentGameObjects.Count > 0)
        {
            foreach (GameObject obj in currentGameObjects)
            {
                Destroy(obj);
            }
            currentGameObjects.Clear();
        }
        XMLDataManager container;
        XMLDataLoaderSaver.Load(level, out container);
        foreach (XMLDataManager.Wall w in container.walls)
        {
            GameObject temp = Instantiate(wall, w.loc, new Quaternion());
            currentGameObjects.Add(temp);
        }
        foreach (XMLDataManager.Goal g in container.goals)
        {
            GameObject temp = Instantiate(goal, g.loc, new Quaternion());
            currentGameObjects.Add(temp);
        }
        player.transform.position = container.player.loc + new Vector3(0, 2, 0);
        foreach (XMLDataManager.Bot b in container.bots)
        {
            GameObject temp = Instantiate(bot, b.location, new Quaternion());
            if (b.pPoints != null)
            {
                temp.GetComponent<BotScript>().patrolPoints = b.pPoints;
            }
            currentGameObjects.Add(temp);
        }
        goalNum = container.goals.Length;
        player.GetComponent<PlayerController>().RefreshSpawn(player.transform.position);
    }

    void GenerateGrid()
    {
        GameObject Floor = GameObject.Find("Play_Field");
        Texture2D gridImage = new Texture2D(255, 255);
        float borderSize = 15;
        Color gridColor = Color.cyan;
        Color borderColor = Color.black;
        Collider floorCollider = Floor.GetComponent<Collider>();
        for (int x = 0; x < gridImage.width; x++)
        {
            for (int y = 0; y < gridImage.height; y++)
            {
                if (x < borderSize || x > gridImage.width - borderSize || y < borderSize || y > gridImage.height - borderSize)
                {
                    gridImage.SetPixel(x, y, new Color(borderColor.r, borderColor.g, borderColor.b, 50));
                }
                else gridImage.SetPixel(x, y, new Color(gridColor.r, gridColor.g, gridColor.b, 50));
            }
            gridImage.wrapMode = TextureWrapMode.Repeat;
            gridImage.Apply();
        }
        MeshRenderer floorRenderer = Floor.GetComponent<MeshRenderer>();
        Shader floorShader = floorRenderer.material.shader;
        floorRenderer.material.mainTexture = gridImage;
        floorRenderer.material.mainTextureScale = new Vector2(floorCollider.bounds.size.x, floorCollider.bounds.size.z);
        floorRenderer.material.mainTextureOffset = new Vector2(.5f, .5f);
    }

    public void HitGoal(GameObject goal)
    {
        Destroy(goal);
        goalNum--;
        if (goalNum <= 0)
        {
            LoadLevel(LevelManager.Levels[UnityEngine.Random.Range(0, LevelManager.Levels.Count - 1)]);
        }
    }

    void SelectInputMethod(bool isJoystick)
    {
        string active = null;
        if (isJoystick)
        {
            joystick.gameObject.SetActive(true);
            DPad._Instance.gameObject.SetActive(false);
            active = "yes";
        }
        else if (!isJoystick)
        {
            joystick.gameObject.SetActive(false);
            DPad._Instance.gameObject.SetActive(true);
            active = "no";
        }
        PlayerPrefs.SetString("Joystick Active", active);
        PlayerPrefs.Save();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;
using System.IO;

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

	// Use this for initialization
	void Start () {
        _Instance = this;
        bgmSounds = Resources.LoadAll<AudioClip>("Sounds/BGM");
        bgmPlayer = GetComponent<AudioSource>();
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainGame"))
        {
            isInGame = true;
            LoadLevel(LevelManager.levelToLoad);
        }
        GenerateGrid();
        Advertisement.Initialize("1267211");

    }
	
	// Update is called once per frame
	void Update () {
        if (isInGame)
        {
            PlayMusic();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Texture2D goodPic = Screenshot.Capture(GameObject.Find("Main Camera").GetComponent<Camera>(),1024, 500);
            byte[] toSave = goodPic.EncodeToJPG();
            File.WriteAllBytes(Application.persistentDataPath + "/screenshot.jpg", toSave);
        }
	}

    void PlayMusic()
    {
        if (bgmPlayer.isPlaying == false)
        {
            bgmPlayer.PlayOneShot(bgmSounds[Random.Range(0, bgmSounds.Length - 1)]);
        }
    }

    void LoadLevel(LevelManager.Level level)
    {
        Advertisement.Show();
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
            LoadLevel(LevelManager.Levels[Random.Range(0, LevelManager.Levels.Count - 1)]);
        }
    }
}

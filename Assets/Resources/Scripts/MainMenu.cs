using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject loadLevelMenu;
    public GameObject levelsContainer;
    public GameObject levelButton;
    public GameObject loadingScreen;
    public GameObject downloadingScreen;
    public Button loadLevelButton;
    public Button randomLevelButton;
    public Button quitGame;
    private List<GameObject> currentLevelButtons;

	// Use this for initialization
	void Start () {
        downloadingScreen = GameObject.Find("Downloading_Screen");
        currentLevelButtons = new List<GameObject>();
        loadLevelButton.onClick.AddListener(delegate { ShowLoadLevelMenu(); });
        ReloadLevels();
        randomLevelButton.onClick.AddListener(delegate {
            mainMenu.SetActive(false);
            loadingScreen.SetActive(true);
            LoadLevel(LevelManager.Levels[Random.Range(0, LevelManager.Levels.Count)]);
        });
        quitGame.onClick.AddListener(delegate { Application.Quit(); });
        Firebase.Unity.Editor.FirebaseEditorExtensions.SetEditorDatabaseUrl(Firebase.FirebaseApp.DefaultInstance, "https://blockquest-a1e16.firebaseio.com/");
        if (!FirebaseManager.filesDownloaded)
        {
            FirebaseManager.CheckNewLevels();
            ShowDownloadScreen();
        }
        else ShowMainMenu();
        FirebaseManager.onFilesDownloaded += ShowMainMenu;
        FirebaseManager.onFilesDownloaded += ReloadLevels;
        LogIn();
    }

    void LogIn()
    {
        if (FirebaseManager.GoogleConfigDone == false)
        {
            PlayGamesPlatform.InitializeInstance(FirebaseManager.config);
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate((bool success) => {
                if (success)
                {
                    FirebaseManager.GoogleConfigDone = true;
                    FirebaseManager.FirebaseLogin();
                }
            });
        }
    }

    void ShowDownloadScreen()
    {
        mainMenu.SetActive(false);
        loadLevelMenu.SetActive(false);
        loadingScreen.SetActive(false);
        downloadingScreen.SetActive(true);
    }

    public void CreateLevel()
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene("LevelCreator");
    }

    public void LoadLevel(LevelManager.Level level)
    {
        LevelManager.levelToLoad = level;
        SceneManager.LoadScene("MainGame");
    }

    public void ReloadLevels()
    {
        LevelManager.RebuildLists();
        if (LevelManager.Levels.Count <= 0)
        {
            if (LevelManager.Levels.Count <= 0)
            {
                loadLevelButton.interactable = false;
                randomLevelButton.interactable = false;
            }
        }
        if (LevelManager.Levels.Count > 0)
        {
            loadLevelButton.interactable = true;
            randomLevelButton.interactable = true;
        }
    }

    private void Update()
    {
        
    }

    void ShowLoadLevelMenu()
    {
        mainMenu.SetActive(false);
        loadLevelMenu.SetActive(true);

        if (currentLevelButtons.Count <= 0)
        {
            foreach (LevelManager.Level level in LevelManager.Levels)
            {
                GameObject newButton = Instantiate(levelButton);
                levelsContainer.GetComponent<RectTransform>().sizeDelta += new Vector2(0, newButton.GetComponent<RectTransform>().sizeDelta.y);
                newButton.transform.SetParent(levelsContainer.transform);
                newButton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                Sprite screenshot = Sprite.Create(level.LevelPic, new Rect(0, 0, level.LevelPic.width, level.LevelPic.height), new Vector2());
                newButton.transform.GetChild(0).GetComponent<Image>().sprite = screenshot;
                newButton.GetComponentInChildren<Text>().text = level.LevelName;
                newButton.GetComponent<Button>().onClick.AddListener(delegate
                {
                    LoadLevel(level);
                    loadingScreen.SetActive(true);
                });
                currentLevelButtons.Add(newButton);
            }
        }
        
    }

    public void ShowMainMenu()
    {
        if (downloadingScreen.activeInHierarchy)
        {
            downloadingScreen.SetActive(false);
        }
        if (loadLevelMenu.activeInHierarchy)
        {
            loadLevelMenu.SetActive(false);            
        }
        mainMenu.SetActive(true);
    }


}

public static class Screenshot
{
    public static Texture2D Capture(Camera camera, int width, int height)
    {
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        camera.targetTexture = renderTexture;
        Texture2D screenshot = new Texture2D(width, height);
        camera.Render();
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;
        camera.targetTexture = null;
        screenshot.Apply();
        return screenshot;
    }
}

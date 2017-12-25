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
    public GameObject cloudDataObject;
    public GameObject cloudButton;
    public GameObject cloudLevelOptions;
    public RectTransform cloudDataHolder;
    public Button loadLevelButton;
    public Button randomLevelButton;
    public Button browseLevelsButton;
    public Button myLevelsButton, userLevelsButton;
    public Button quitGame;
    private List<GameObject> currentLevelButtons, currentMyCloudLevels, currentUserCloudLevels;
    private bool myLevels, userLevels;
    private FirebaseManager.LevelQuery cloudLevelSelected;

	// Use this for initialization
	void Start () {
        downloadingScreen = GameObject.Find("Downloading_Screen");
        currentLevelButtons = new List<GameObject>();
        loadLevelButton.onClick.AddListener(delegate { ShowLoadLevelMenu(); });
        browseLevelsButton.onClick.AddListener(ShowCloudDataScreen);
        cloudDataObject.transform.GetChild(0).transform.GetChild(0).GetComponent<Button>().onClick
            .AddListener(delegate {
                FirebaseManager.QueryMyLevels();
                if (currentMyCloudLevels != null)
                {
                    foreach (GameObject obj in currentMyCloudLevels)
                    {
                        Destroy(obj);
                    }
                    currentMyCloudLevels.Clear();
                }
                if (currentUserCloudLevels != null)
                {
                    foreach (GameObject obj in currentUserCloudLevels)
                    {
                        Destroy(obj);
                    }
                    currentUserCloudLevels.Clear();
                }
            });
        cloudDataObject.transform.GetChild(0).transform.GetChild(1).GetComponent<Button>().onClick
            .AddListener(delegate {
                FirebaseManager.QueryAllLevels();
                if (currentUserCloudLevels != null)
                {
                    foreach (GameObject obj in currentUserCloudLevels)
                    {
                        Destroy(obj);
                    }
                    currentUserCloudLevels.Clear();
                }
                if (currentMyCloudLevels != null)
                {
                    foreach (GameObject obj in currentMyCloudLevels)
                    {
                        Destroy(obj);
                    }
                    currentMyCloudLevels.Clear();
                }
            });
        ReloadLevels();
        randomLevelButton.onClick.AddListener(delegate {
            mainMenu.SetActive(false);
            loadingScreen.SetActive(true);
            LoadLevel(LevelManager.Levels[Random.Range(0, LevelManager.Levels.Count)]);
        });
        quitGame.onClick.AddListener(delegate { Application.Quit(); });
        Firebase.Unity.Editor.FirebaseEditorExtensions.SetEditorDatabaseUrl(Firebase.FirebaseApp.DefaultInstance, "https://blockquest-a1e16.firebaseio.com/");
        if (!FirebaseManager.filesAreDownloaded)
        {
            FirebaseManager.CheckNewLevels();
            ShowDownloadScreen();
        }
        else ShowMainMenu();
        FirebaseManager.onFilesDownloaded += ShowMainMenu;
        FirebaseManager.onFilesDownloaded += ReloadLevels;
        LogIn();
        FirebaseManager.onMyFilesCached += LoadMyLevels;
        FirebaseManager.onUserFilesCached += LoadUserLevels;
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

    void ShowCloudDataScreen()
    {
        mainMenu.SetActive(false);
        cloudDataObject.SetActive(true);
    }

    public void LoadMyLevels()
    {
        if (currentMyCloudLevels == null) currentMyCloudLevels = new List<GameObject>();
        GameObject newCloudObject = Instantiate(cloudButton);
        newCloudObject.GetComponentInChildren<Button>().onClick.AddListener(delegate {
            cloudDataObject.SetActive(false);
            cloudLevelOptions.SetActive(true);
            cloudLevelSelected = FirebaseManager.levelsHolder[FirebaseManager.levelsHolder.Count - 1];
            ShowCloudLevelOptions(cloudLevelSelected, true);
        });
        RectTransform cloudTrans = newCloudObject.GetComponent<RectTransform>();
        newCloudObject.transform.SetParent(cloudDataHolder);
        newCloudObject.transform.Find("LevelName").GetComponent<Text>().text = FirebaseManager.levelsHolder[FirebaseManager.levelsHolder.Count - 1].lName;
        newCloudObject.transform.Find("AuthorName").GetComponent<Text>().text = FirebaseManager.levelsHolder[FirebaseManager.levelsHolder.Count - 1].levelAuthor;
        newCloudObject.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(FirebaseManager.levelsHolder[FirebaseManager.levelsHolder.Count - 1].screenshot, new Rect(0, 0, 512, 512), new Vector2());
        cloudTrans.localScale = new Vector3(1, 1, 1);
        currentMyCloudLevels.Add(newCloudObject);
    }

    public void LoadUserLevels()
    {
        if (currentUserCloudLevels == null) currentUserCloudLevels = new List<GameObject>();
        GameObject newCloudObject = Instantiate(cloudButton);
        newCloudObject.GetComponentInChildren<Button>().onClick.AddListener(delegate {
            cloudDataObject.SetActive(false);
            cloudLevelOptions.SetActive(true);
            cloudLevelSelected = FirebaseManager.levelsHolder[FirebaseManager.levelsHolder.Count - 1];
            ShowCloudLevelOptions(cloudLevelSelected, false);
        });
        RectTransform cloudTrans = newCloudObject.GetComponent<RectTransform>();
        newCloudObject.transform.SetParent(cloudDataHolder);
        newCloudObject.transform.Find("LevelName").GetComponent<Text>().text = FirebaseManager.levelsHolder[FirebaseManager.levelsHolder.Count - 1].lName;
        newCloudObject.transform.Find("AuthorName").GetComponent<Text>().text = FirebaseManager.levelsHolder[FirebaseManager.levelsHolder.Count - 1].levelAuthor;
        newCloudObject.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(FirebaseManager.levelsHolder[FirebaseManager.levelsHolder.Count - 1].screenshot, new Rect(0, 0, 512, 512), new Vector2());
        cloudTrans.localScale = new Vector3(1, 1, 1);
        currentUserCloudLevels.Add(newCloudObject);
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
        if (downloadingScreen.activeInHierarchy)
        {
            RectTransform baseImage = downloadingScreen.transform.GetChild(1).GetComponent<RectTransform>();
            RectTransform progressImage = baseImage.GetChild(0).GetComponent<RectTransform>();
            progressImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, baseImage.sizeDelta.x * FirebaseManager.fileRatio);
        }
    }

    void ShowLoadLevelMenu()
    {
        mainMenu.SetActive(false);
        loadLevelMenu.SetActive(true);
        LevelManager.RebuildLists();
        if (currentLevelButtons.Count > 0)
        {
            foreach (GameObject obj in currentLevelButtons)
            {
                Destroy(obj);
            }
            currentLevelButtons.Clear();
        }

        if (currentLevelButtons.Count <= 0)
        {
            foreach (LevelManager.Level level in LevelManager.UsersLevels)
            {
                GameObject newButton = Instantiate(levelButton);
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
            foreach (LevelManager.Level level in LevelManager.MyLevels)
            {
                GameObject newButton = Instantiate(levelButton);
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
        downloadingScreen.SetActive(false);
        loadLevelMenu.SetActive(false);
        cloudDataObject.SetActive(false);
        mainMenu.SetActive(true);
        if (currentMyCloudLevels != null)
        {
            foreach (GameObject obj in currentMyCloudLevels)
            {
                Destroy(obj);
            }
            currentMyCloudLevels.Clear();
        }
        if (currentUserCloudLevels != null)
        {
            foreach (GameObject obj in currentUserCloudLevels)
            {
                Destroy(obj);
            }
            currentUserCloudLevels.Clear();
        }
    }

    void ShowCloudLevelOptions(FirebaseManager.LevelQuery level, bool isMyLevel)
    {
        cloudLevelOptions.transform.GetChild(0).GetComponent<Text>().text = level.lName;
        cloudLevelOptions.transform.GetChild(1).GetComponent<Text>().text = level.levelAuthor;
        Button downloadButton = cloudLevelOptions.transform.GetChild(2).transform.GetChild(0)
            .GetComponent<Button>();
        cloudLevelOptions.transform.GetChild(2).transform.GetChild(1)
            .GetComponent<Button>().interactable = false;
        downloadButton.onClick.RemoveAllListeners();
        downloadButton.onClick.AddListener(delegate { FirebaseManager.DownloadLevel(level);
            cloudLevelOptions.SetActive(false);
            ShowMainMenu();
        });
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

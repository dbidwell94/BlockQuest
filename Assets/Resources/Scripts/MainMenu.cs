using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject loadLevelMenu;
    public GameObject levelsContainer;
    public GameObject levelButton;
    public GameObject loadingScreen;
    public Button loadLevelButton;
    public Button randomLevelButton;
    public Button quitGame;
    private List<GameObject> currentLevelButtons;

	// Use this for initialization
	void Start () {
        currentLevelButtons = new List<GameObject>();
        loadLevelButton.onClick.AddListener(delegate { ShowLoadLevelMenu(); });
        ReloadLevels();
        randomLevelButton.onClick.AddListener(delegate {
            LoadLevel(LevelManager.Levels[Random.Range(0, LevelManager.Levels.Count)]);
        });
        quitGame.onClick.AddListener(delegate { Application.Quit(); });
        Firebase.Unity.Editor.FirebaseEditorExtensions.SetEditorDatabaseUrl(Firebase.FirebaseApp.DefaultInstance, "https://blockquest-a1e16.firebaseio.com/");
        FirebaseManager.CheckNewLevels();
        LevelManager.OnLevelsChanged += ReloadLevels;
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
        if (LevelManager.Levels.Count <= 0)
        {
            LevelManager.RebuildLists();
            if (LevelManager.Levels.Count <= 0)
            {
                loadLevelButton.interactable = false;
                randomLevelButton.interactable = false;
            }
        }
        if (LevelManager.Levels.Count > 0)
        {
            Debug.Log(LevelManager.Levels.Count);
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
            newButton.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate
            {
                FirebaseManager.UploadFileToFirebase(level);
            });
            currentLevelButtons.Add(newButton);
        }
    }

    void ShowMainMenu()
    {
        foreach (GameObject obj in currentLevelButtons)
        {
            Destroy(obj);
        }
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

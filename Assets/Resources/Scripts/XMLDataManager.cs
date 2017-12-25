using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Auth;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

[XmlRoot("Level")]
public class XMLDataManager {

    [XmlAttribute("Times_Played")]
    public int timesPlayed;
    [XmlAttribute("Level_Name")]
    public string lName;
    [XmlElement("Player")]
    public Player player;
    [XmlArray("Bots")]
    [XmlArrayItem("Bot")]
    public Bot[] bots;
    [XmlArray("Walls")]
    [XmlArrayItem("Wall")]
    public Wall[] walls;
    [XmlArray("Goals")]
    [XmlArrayItem("Goal")]
    public Goal[] goals;

    public struct Bot
    {
        [XmlAttribute("Name")]
        public string botName;
        [XmlElement("Location")]
        public Vector3 location;
        [XmlArray("Patrol_Points")]
        [XmlArrayItem("Point")]
        public Vector3[] pPoints;
    }

    public struct Player
    {
        [XmlAttribute("Name")]
        public string playerName;
        [XmlElement("Spawn_Location")]
        public Vector3 loc;
    }
    
    public struct Goal
    {
        [XmlAttribute("Name")]
        public string goalName;
        [XmlElement("Goal_Location")]
        public Vector3 loc;
    }

    public struct Wall
    {
        [XmlAttribute("Name")]
        public string wallName;
        [XmlElement("Wall_Location")]
        public Vector3 loc;
    }

}

public static class XMLDataLoaderSaver
{
    public static string path = Application.persistentDataPath + "/UserLevels/";
    private static string user = (FirebaseManager.user == null) ? "Default_User" : FirebaseManager.FormattedUserName;
    public static string savePath = Path.Combine(path, user);

    public static void Load(LevelManager.Level level, out XMLDataManager Level_Container)
    {
        XmlSerializer xml = new XmlSerializer(typeof(XMLDataManager));
        try
        {
            using (FileStream stream = new FileStream(level.LevelPath, FileMode.Open))
            {
                Level_Container = xml.Deserialize(stream) as XMLDataManager;
            }
        }
        catch (System.Exception)
        {
            Level_Container = new XMLDataManager();
        }
    }

    public static bool Save(XMLDataManager levelData, Texture2D screenshot)
    {
        string levelSavePath = string.Format("{0}/{1}/{1}.xml", savePath, levelData.lName);
        string picSavePath = string.Format("{0}/{1}/{1}.png", savePath, levelData.lName);
        byte[] picToSave = screenshot.EncodeToPNG();
        bool success;
        XmlSerializer xml = new XmlSerializer(typeof(XMLDataManager));
        try
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            if (!Directory.Exists(savePath + "/" + levelData.lName))
            {
                Directory.CreateDirectory(savePath + "/" + levelData.lName);
            }           
            using (FileStream stream = new FileStream(levelSavePath, FileMode.Create))
            {
                xml.Serialize(stream, levelData);
                File.WriteAllBytes(picSavePath, picToSave);
            }
            success = true;
        }
        catch (System.Exception)
        {
            success = false;
        }
        return success;
    }
}

public static class FirebaseManager
{
    // IMPORTANT!! Modify this variable according to dev or user build!!
    public static string saveLoc = "User_Levels";

    public static float filesToDownload;
    public static float filesDownloaded;
    public static float fileRatio = 0f;
    public static bool filesAreDownloaded = false;

    public delegate void fileHandler();
    public static fileHandler onFilesDownloaded;
    public static fileHandler onMyFilesCached;
    public static fileHandler onUserFilesCached;

    public static List<LevelQuery> levelsHolder;

    private static FirebaseAuth auth = FirebaseAuth.DefaultInstance;
    public static FirebaseUser user;
    public static string FormattedUserName;
    public static bool GoogleConfigDone = false;
    public static PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .RequestIdToken()
        .RequestServerAuthCode(false)
        .Build();

    public struct Level
    {
       public string levelName;
       public string filePath;
       public string picturePath;
        public Level(string name, string path, string pic)
        {
            levelName = name;
            filePath = path;
            picturePath = pic;
        }
    }
    public struct LevelQuery
    {
        public Texture2D screenshot;
        public string filePath;
        public string picPath;
        public string lName;
        public string levelAuthor;
    }
    private static FirebaseStorage storage = FirebaseStorage.GetInstance("gs://blockquest-a1e16.appspot.com");
    private static StorageReference root = storage.GetReferenceFromUrl("gs://blockquest-a1e16.appspot.com");
    public static FirebaseDatabase instance;
    public static void UploadFileToFirebase(LevelManager.Level level)
    {
        DatabaseReference data = FirebaseDatabase.DefaultInstance.GetReferenceFromUrl("https://blockquest-a1e16.firebaseio.com/");

        StorageReference levelFolder = root.Child(saveLoc);
        StorageReference userLevel = levelFolder.Child(level.LevelName);
        StorageReference levelFile = userLevel.Child(level.LevelName + ".xml");
        StorageReference levelPic = userLevel.Child(level.LevelName + ".png");

        levelFile.PutFileAsync(level.LevelPath);
        levelPic.PutBytesAsync(level.LevelPic.EncodeToPNG());

        Level newLevel = new Level(level.LevelName, levelFile.Path, levelPic.Path);

        data.Child(saveLoc).Child(level.LevelName).Child("File_Path").SetValueAsync(newLevel.filePath);
        data.Child(saveLoc).Child(level.LevelName).Child("Picture_Path").SetValueAsync(newLevel.picturePath);
        if (saveLoc == "Default_Levels")
        {
            data.Child("Base_Level_Last_Changed").SetValueAsync(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }
    }

    public static void UserUploadToFirebase(LevelManager.Level level)
    {
        DatabaseReference data = FirebaseDatabase.DefaultInstance.GetReferenceFromUrl("https://blockquest-a1e16.firebaseio.com/");

        StorageReference levelFolder = root.Child(saveLoc);
        StorageReference userName = levelFolder.Child(FormattedUserName);
        StorageReference userLevel = userName.Child(level.LevelName);
        StorageReference levelFile = userLevel.Child(level.LevelName + ".xml");
        StorageReference levelPic = userLevel.Child(level.LevelName + ".png");

        levelFile.PutFileAsync(level.LevelPath);
        levelPic.PutBytesAsync(level.LevelPic.EncodeToPNG());

        Level newLevel = new Level(level.LevelName, levelFile.Path, levelPic.Path);
        Debug.Log(levelFile.Path);
        Debug.Log(levelPic.Path);

        data.Child(saveLoc).Child(FirebaseManager.FormattedUserName).Child(level.LevelName).Child("File_Path").SetValueAsync(newLevel.filePath);
        data.Child(saveLoc).Child(FirebaseManager.FormattedUserName).Child(level.LevelName).Child("Picture_Path").SetValueAsync(newLevel.picturePath);
    }

    public static void DownloadBaseLevels()
    {
        string defaultLevelPath = Path.Combine(Application.persistentDataPath, "Levels/");

        DatabaseReference data = FirebaseDatabase.DefaultInstance.RootReference;
        data.Child("Default_Levels").GetValueAsync().ContinueWith(x =>
       {
           if (x.IsCompleted)
           {
               DataSnapshot shot = x.Result;
               List<string> keyNames = new List<string>();
               foreach (var item in shot.Children)
               {
                   keyNames.Add(item.Key);                  
               }
               List<string> fileLocs = new List<string>();
               List<string> picLocs = new List<string>();
               foreach (string loc in keyNames)
               {
                   fileLocs.Add(shot.Child(loc).Child("File_Path").Value.ToString());
                   picLocs.Add(shot.Child(loc).Child("Picture_Path").Value.ToString());
               }
               if (!Directory.Exists(Application.persistentDataPath + "/Levels"))
               {
                   Directory.CreateDirectory(Application.persistentDataPath + "/Levels");
               }
               for (int i = 0; i < fileLocs.Count; i++)
               {
                   filesToDownload += 2;
                   Directory.CreateDirectory(defaultLevelPath + keyNames[i]);
               root.Child(fileLocs[i]).GetFileAsync(defaultLevelPath + keyNames[i] + "/" + keyNames[i] + ".xml").ContinueWith(done => {
                   filesDownloaded += 1;
                   UpdateDownloadProgress();
               });
                   root.Child(picLocs[i]).GetFileAsync(defaultLevelPath + keyNames[i] + "/" + keyNames[i] + ".png").ContinueWith(done2 => {
                       filesDownloaded += 1;
                       UpdateDownloadProgress();
                   });
               }
               LevelData levelData = new LevelData();
               data.Child("Base_Level_Last_Changed").GetValueAsync().ContinueWith(y => {
                   if (x.IsCompleted)
                   {
                       DateTime changed = DateTime.Parse(y.Result.Value.ToString());
                       levelData.timeChanged = changed;
                       using (FileStream stream = new FileStream(Application.persistentDataPath + "/leveldata.xml", FileMode.Create))
                       {
                           XmlSerializer xml = new XmlSerializer(typeof(LevelData));
                           xml.Serialize(stream, levelData);
                       }
                   }
               });
           }
           else if (x.IsFaulted)
           {
               Debug.Log("Fail!");
           }
       });
    }

    public static void CheckNewLevels()
    {
        DatabaseReference data = FirebaseDatabase.DefaultInstance.RootReference;
        data.Child("Base_Level_Last_Changed").GetValueAsync().ContinueWith(x => {
            if (x.IsCompleted)
            {
                DateTime changed = DateTime.Parse(x.Result.Value.ToString());
                if (File.Exists(Application.persistentDataPath + "/leveldata.xml"))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(LevelData));
                    DateTime current;
                    using (FileStream stream = new FileStream(Application.persistentDataPath + "/leveldata.xml", FileMode.Open))
                    {
                        LevelData levelData = xml.Deserialize(stream) as LevelData;
                        current = levelData.timeChanged;
                    }
                    if (current < changed)
                    {
                        DownloadBaseLevels();
                    }
                    else
                    {
                        filesAreDownloaded = true;
                        onFilesDownloaded();
                    }
                }
                else if (!File.Exists(Application.persistentDataPath + "/leveldata.xml"))
                {
                    DownloadBaseLevels();
                }
            }
        });
    }

    public static void UpdateDownloadProgress()
    {
        if (filesDownloaded == filesToDownload)
        {
            filesAreDownloaded = true;
            onFilesDownloaded();
        }
        fileRatio = (filesDownloaded == 0) ? 0.1f : filesDownloaded / filesToDownload;
    }

    public static void FirebaseLogin()
    {
        Credential cred = GoogleAuthProvider.GetCredential(PlayGamesPlatform.Instance.GetIdToken(),
            PlayGamesPlatform.Instance.GetServerAuthCode());
        auth.SignInWithCredentialAsync(cred).ContinueWith(done => {
            user = done.Result;
            PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_team_player, 100.0, (bool success) => { });
            FormattedUserName = (user.DisplayName.Contains(" ")) ? user.DisplayName.Replace(' ', '_') : user.DisplayName;
        });
    }

    public static void QueryMyLevels()
    {
        if (levelsHolder == null)
        {
            levelsHolder = new List<LevelQuery>();
        }
        else levelsHolder.Clear();

        const long maxAllowedSize = 4 * 1024 * 1024;

        FirebaseDatabase.DefaultInstance.RootReference.Child("User_Levels").Child(FirebaseManager.FormattedUserName).GetValueAsync().ContinueWith(x => {
            foreach (DataSnapshot child in x.Result.Children)
            {
                string levelName = child.Key;
                string filePath = child.Child("File_Path").Value.ToString();
                string picPath = child.Child("Picture_Path").Value.ToString();
                root.Child(picPath).GetBytesAsync(maxAllowedSize).ContinueWith(done => {
                    Texture2D screenTex = new Texture2D(512, 512);
                    screenTex.LoadImage(done.Result);
                    screenTex.Apply();
                    LevelQuery newLevel = new LevelQuery {
                        lName = levelName,
                        filePath = filePath,
                        picPath = picPath,
                        screenshot = screenTex,
                        levelAuthor = FirebaseManager.FormattedUserName
                    };
                    levelsHolder.Add(newLevel);
                    onMyFilesCached();
                });
            }
        });
    }

    public static void QueryAllLevels()
    {
        if (levelsHolder == null) levelsHolder = new List<LevelQuery>();
        else levelsHolder.Clear();

        const long maxAllowedSize = 4 * 1024 * 1024;

        FirebaseDatabase.DefaultInstance.RootReference.Child("User_Levels").GetValueAsync().ContinueWith(x => {

            foreach (DataSnapshot item in x.Result.Children)
            {
                if (user == null || FormattedUserName != item.Key)
                {
                    string author = item.Key;
                    Debug.Log(author);
                    foreach (DataSnapshot level in item.Children)
                    {
                        Debug.Log(level.Key);
                        string lName = level.Key;
                        string fPath = level.Child("File_Path").Value.ToString();
                        string pPath = level.Child("Picture_Path").Value.ToString();
                        root.Child(pPath).GetBytesAsync(maxAllowedSize).ContinueWith(done => {
                            Texture2D screenShot = new Texture2D(512, 512);
                            screenShot.LoadImage(done.Result);
                            screenShot.Apply();
                            LevelQuery newLevel = new LevelQuery
                            {
                                levelAuthor = author,
                                filePath = fPath,
                                picPath = pPath,
                                screenshot = screenShot,
                                lName = lName
                            };
                            levelsHolder.Add(newLevel);
                            onUserFilesCached();
                        });
                    }
                }
            }
        });


    }

    public static void DownloadLevel(LevelQuery level)
    {
        string authorDir = Application.persistentDataPath + "/UserLevels/" + level.levelAuthor + "/";
        string levelDir = Path.Combine(authorDir, level.lName);
        string fileDir = Path.Combine(levelDir, level.lName + ".xml");
        string picDir = Path.Combine(levelDir, level.lName + ".png");

        if (!Directory.Exists(authorDir))
        {
            Directory.CreateDirectory(authorDir);
        }
        if (!Directory.Exists(levelDir))
        {
            Directory.CreateDirectory(levelDir);
        }
        root.Child(level.filePath).GetFileAsync(fileDir);
        root.Child(level.picPath).GetFileAsync(picDir);
        LevelManager.RebuildLists();
    }
}

[XmlRoot("Root_Level_Data")]
public class LevelData
{
    [XmlElement("Changed_Time")]
    public DateTime timeChanged;
}

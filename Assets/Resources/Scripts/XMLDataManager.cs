using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Auth;

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
    public static string savePath = Application.persistentDataPath + "/Levels/";

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
    private static string saveLoc = "Default_Levels";

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
    }

    public static void DownloadBaseLevels()
    {
        DatabaseReference data = FirebaseDatabase.DefaultInstance.RootReference;
        var snap = data.Child("Default_Levels").GetValueAsync().ContinueWith(x =>
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
               for (int i = 0; i < fileLocs.Count; i++)
               {
                   Directory.CreateDirectory(XMLDataLoaderSaver.savePath + keyNames[i]);
                   root.Child(fileLocs[i]).GetFileAsync(XMLDataLoaderSaver.savePath + keyNames[i] + "/" + keyNames[i] + ".xml");
                   root.Child(picLocs[i]).GetFileAsync(XMLDataLoaderSaver.savePath + keyNames[i] + "/" + keyNames[i] + ".png");
               }
           }
           else if (x.IsFaulted)
           {
               Debug.Log("Fail!");
           }
       });
    }

}

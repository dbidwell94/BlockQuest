using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Firebase;
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
    

   public static void UploadFileToFirebase(LevelManager.Level level)
    {
        FirebaseStorage storage = FirebaseStorage.GetInstance("gs://blockquest-a1e16.appspot.com");
        StorageReference root = storage.GetReferenceFromUrl("gs://blockquest-a1e16.appspot.com");
        StorageReference levelFolder = root.Child("User_Levels");
        StorageReference userLevel = levelFolder.Child(level.LevelName);
        StorageReference levelFile = userLevel.Child(level.LevelName + ".xml");
        StorageReference levelPic = userLevel.Child(level.LevelName + ".png");

        levelFile.PutFileAsync(level.LevelPath);
        levelPic.PutBytesAsync(level.LevelPic.EncodeToPNG());

        Debug.Log(levelPic.Path);
    }

   public static void DownloadFileFromDatabase()
    {

    }
}

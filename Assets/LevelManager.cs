using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelManager {

    public struct Level {
        public string LevelName;
        public string LevelPath;
        public Texture2D LevelPic;

        public Level(string name, string path, Texture2D picture)
        {
            LevelName = name;
            LevelPath = path;
            LevelPic = picture;
        }
    }

    public static List<Level> Levels = new List<Level>();

    public static Level levelToLoad;

    public static void RebuildLists()
    {
        Levels = new List<Level>();
        try
        {
            foreach (string file in System.IO.Directory.GetDirectories(Application.persistentDataPath + "/Levels"))
            {
                string[] picLocation = System.IO.Directory.GetFiles(file, "*.png");
                string[] levelLocation = System.IO.Directory.GetFiles(file, "*.xml");
                byte[] picToLoad = System.IO.File.ReadAllBytes(picLocation[0]);
                Texture2D screenshotPic = new Texture2D(256, 256, TextureFormat.RGB24, false);
                screenshotPic.LoadImage(picToLoad);
                Level level = new Level(System.IO.Path.GetFileNameWithoutExtension(levelLocation[0]), levelLocation[0], screenshotPic);
                Levels.Add(level);
            }
        }
        catch (System.Exception)
        {

        }
    }

}


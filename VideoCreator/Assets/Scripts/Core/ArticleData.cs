using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static TextToSpeech;

[System.Serializable]
public class ArticleData
{
    public string jsonText;
    public string topic;
    public string title;
    public string uuid;
    public string lang;
    public ContentData intro;
    public MainData[] main;
    public ContentData conclusion;

    [System.Serializable]
    public class ContentData
    {
        public string content;
        public string[] images;
        public List<GameObject> imgObjs = new List<GameObject>();
        public AudioClip audioClip;
        public List<TimepointData> timepoints;
    }

    [System.Serializable]
    public class MainData
    {
        public string heading;
        public string tag;
        public ContentData detail;
    }

    public static ArticleData LoadData(string filePath)
    {
        var jsonText = File.ReadAllText(filePath);
        var articleData = JsonUtility.FromJson<ArticleData>(jsonText);
        articleData.jsonText = jsonText;
        return articleData;
    }
}
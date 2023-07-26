using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class DataContainer : MonoBehaviour
{
    public static string s_DataOutputPath = "Assets/__DataOutput";
    public string JsonDataPath;
    public ArticleData Data;

    public void SetData(string jsonDataPath, ArticleData data)
    {
        JsonDataPath = jsonDataPath;
        if (Data?.jsonText != data.jsonText)
        {
            Data = data;
            LoadAssets();
        }
    }

    public void LoadAssets()
    {
        CreateAllImages(Path.GetDirectoryName(JsonDataPath));
        SaveAsset();
    }
    
    public void SaveAsset()
    {
        PrefabUtility.ApplyPrefabInstance(this.gameObject, InteractionMode.AutomatedAction);
    }

    public List<string> CreateAllImages(string dir)
    {
        List<string> allImages = new List<string>();

        // Add images from intro
        if (Data.intro != null && Data.intro.images != null)
        {
            Data.intro.imgObjs.AddRange(CreateImageObjects(dir, Data.intro.images, "intro"));
        }

        // Add images from heading
        if (Data.main != null)
        {
            int i = 0;
            foreach (var mainData in Data.main)
            {
                mainData.detail.imgObjs.AddRange(CreateImageObjects(dir, mainData.detail.images, "main" + (++i)));
            }
        }

        // Add images from end
        if (Data.conclusion != null && Data.conclusion.images != null)
        {
            Data.conclusion.imgObjs.AddRange(CreateImageObjects(dir, Data.conclusion.images, "conclusion"));
        }

        return allImages;
    }

    List<GameObject> CreateImageObjects(string dir, string[] relPaths, string name)
    {
        var objs = new List<GameObject>();
        int i = 0;
        foreach (var relPath in relPaths)
        {
            string imagePath = Path.Combine(dir, relPath);
            var texture = Utils.LoadImageTexture(imagePath, Path.Combine(GetDataStoreFolderPath(), "Images"));
            objs.Add(AddChildImage(texture, name + "_" + (++i)));
        }
        return objs;
    }

    GameObject AddChildImage(Texture2D texture, string name)
    {
        if (texture == null)
            return null;

        var newObject = new GameObject(name);
        var spriteRenderer = newObject.AddComponent<SpriteRenderer>();
        // if texture is in asset database, get sprite from texture asset. otherwise create one
        var sprite = AssetDatabase.GetAssetPath(texture) != "" ? AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(texture)) : Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = sprite;
        newObject.transform.SetParent(this.transform, false);

        Utils.FitImage(newObject);

        return newObject;
    }

    static public string GetDataStoreFolderPath(ArticleData data)
    {
        var dir = s_DataOutputPath;
        var subdirs = new string[] { data?.uuid, data?.lang };
        foreach (var subdir in subdirs)
            dir = Path.Combine(dir, subdir);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return dir;
    }

    public string GetDataStoreFolderPath()
    {
        return GetDataStoreFolderPath(Data);
    }

}
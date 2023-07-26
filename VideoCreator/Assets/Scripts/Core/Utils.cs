using System.IO;
using UnityEditor;
using UnityEngine;

public class Utils
{
    static public void FitImage(GameObject go)
    {
        go.SetActive(true);
        go.transform.localScale = new Vector3(1, 1, 1);
        var spriteRenderer = go.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        float screenWidth = mainCamera.aspect * mainCamera.orthographicSize * 2;
        float screenHeight = mainCamera.orthographicSize * 2;
        float screenRatio = screenWidth / screenHeight;

        float spriteWidth = spriteRenderer.bounds.size.x;
        float spriteHeight = spriteRenderer.bounds.size.y;
        float spriteRatio = spriteWidth / spriteHeight;

        var scale = Vector3.one;
        if (screenRatio < spriteRatio)
        {
            scale.x = screenHeight / spriteHeight;
            scale.y = scale.x;
        }
        else
        {
            scale.y = screenWidth / spriteWidth;
            scale.x = scale.y;
        }
        go.transform.localScale = scale;

        Vector3 cameraPosition = mainCamera.transform.position;
        go.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, go.transform.position.z);
    }

    static public Texture2D LoadImageTexture(string path, string destAssetFolder)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"Not able to find path {path}");
            return null;
        }
        // create folder if not exist
        if (!Directory.Exists(destAssetFolder))
            Directory.CreateDirectory(destAssetFolder);
            
        string destFilePath = Path.Combine(destAssetFolder, Path.GetFileName(path));
        File.Copy(path, destFilePath, true);

        AssetDatabase.ImportAsset(destFilePath, ImportAssetOptions.Default);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var importer = AssetImporter.GetAtPath(destFilePath);
        importer.SaveAndReimport();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(destFilePath);
        return texture;
    }
}

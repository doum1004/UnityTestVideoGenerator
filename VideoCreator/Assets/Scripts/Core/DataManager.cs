using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class DataManager : MonoBehaviour
{
    public string ReadOnlyUUID;
    public string DataJsonPath;
    public ArticleData data => DataContainer ? DataContainer.Data : null;
    public DataContainer DataContainer;

    public void LoadData(string jsonPath)
    {
        DataJsonPath = jsonPath;
        LoadDataContainer(false);
    }

    [ContextMenu("LoadData")]
    public void LoadData()
    {
        LoadDataContainer(false);
    }

    public void LoadDataContainer(bool force)
    {
        var _data = ArticleData.LoadData(DataJsonPath);
        if (string.IsNullOrEmpty(_data?.uuid))
            return;

        if (!force && DataContainer && DataContainer.Data?.jsonText == _data.jsonText)
        {
            ReadOnlyUUID = DataContainer.Data.uuid;
            return;
        }

        DeleteContainer();
        string dataContainerPath = GetDataContainerPrefabPath(_data);
        if (File.Exists(dataContainerPath))
        {
            var assetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dataContainerPath);
            if (assetPrefab)
            {
                var newGameObject = PrefabUtility.InstantiatePrefab(assetPrefab) as GameObject;
                DataContainer = newGameObject.GetComponent<DataContainer>();
            }
        }

        if (!DataContainer)
        {
            var dataContainer = new GameObject("dataContainer").AddComponent<DataContainer>();
            var assetPrefab = PrefabUtility.SaveAsPrefabAsset(dataContainer.gameObject, dataContainerPath);
            var newGameObject = PrefabUtility.InstantiatePrefab(assetPrefab) as GameObject;
            DataContainer = newGameObject.GetComponent<DataContainer>();
            Undo.DestroyObjectImmediate(dataContainer.gameObject);
        }

        DataContainer.SetData(DataJsonPath, _data);
        DataContainer.transform.SetParent(transform);
        ReadOnlyUUID = DataContainer.Data.uuid;
    }

    void DeleteContainer()
    {
        if (DataContainer && DataContainer.gameObject)
        {
            DataContainer.SaveAsset();
            DestroyImmediate(DataContainer.gameObject);
        }
        DataContainer = null;

        // Get the transform component of the parent GameObject
        Transform parentTransform = gameObject.transform;
        // Loop through all the children and destroy them
        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = parentTransform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    }

    string GetDataContainerPrefabPath(ArticleData data)
    {
        return Path.Combine(DataContainer.GetDataStoreFolderPath(data), "dataContainer.prefab");
    }

    public void SaveDataAsset()
    {
        if (!DataContainer)
            return;
        DataContainer.SaveAsset();
    }
}
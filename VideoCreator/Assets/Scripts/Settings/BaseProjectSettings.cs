using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;

public abstract class BaseProjectSettings<T> : ScriptableObject where T : ScriptableObject
{
    internal static bool Exists => s_Instance != null;
    protected static T s_Instance;

    protected static T CreateOrLoad(string filePath)
    {
        //try load else create
        s_Instance = InternalEditorUtility.LoadSerializedFileAndForget(filePath).OfType<T>().FirstOrDefault();
        if (s_Instance == null)
        {
            s_Instance = CreateInstance<T>();
            s_Instance.hideFlags = HideFlags.HideAndDontSave;
            Save(filePath);
        }

        return s_Instance;
    }

    protected static void Save(string filePath)
    {
        if (s_Instance == null)
        {
            Debug.LogError("Failed to save (no instance)");
            return;
        }

        var folderPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(folderPath) && !Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { s_Instance }, filePath, allowTextSerialization: true);
    }
}
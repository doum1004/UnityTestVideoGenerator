using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;

public static class JsonSettings
{
    [System.Serializable]
    public class SettingsData
    {
        public bool SucceedInRecord = false;
    }
    public static string JsonSettingsPath = "C:/Workspace/Personal/Unity/UnityTestVideoGenerator/VideoCreator/Assets/__DataOutput/JsonSettings.json";

    public static SettingsData Instance => LoadSettings();

    public static bool SucceedInRecord
    {
        get
        {
            return Instance.SucceedInRecord;
        }
        set
        {
            var data = Instance;
            data.SucceedInRecord = value;
            SaveSettings(data);
            Logger.Log("SucceedInRecord " + value);
        }
    }

    public static SettingsData LoadSettings()
    {
        SettingsData settingsData = null;
        if (!File.Exists(JsonSettingsPath))
        {
            // Create new settings file
            settingsData = new SettingsData();
        }
        else
        { 
            string json = File.ReadAllText(JsonSettingsPath);
            settingsData = JsonUtility.FromJson<SettingsData>(json);
        }
        return settingsData;
    }

    public static void SaveSettings(SettingsData settingsData)
    {
        string json = JsonUtility.ToJson(settingsData);
        File.WriteAllText(JsonSettingsPath, json);
    }
}
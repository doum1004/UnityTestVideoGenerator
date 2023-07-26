using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;

public class VidiNomProjectSettings : BaseProjectSettings<VidiNomProjectSettings>
{
    /// <summary>
    /// Stores the instance of the singleton.
    /// </summary>
    public static VidiNomProjectSettings Instance => s_Instance ? s_Instance : CreateOrLoad(filePath);

    internal static readonly string filePath = "ProjectSettings/VidiNomProjectSettings.asset";

    static void Save() => Save(filePath);

    [SerializeField]
    string m_GOOGLE_API_KEY = "";
    public static string GOOGLE_API_KEY
    {
        get => Instance.m_GOOGLE_API_KEY;
        set
        {
            Instance.m_GOOGLE_API_KEY = value;
            Save();
        }
    }
}
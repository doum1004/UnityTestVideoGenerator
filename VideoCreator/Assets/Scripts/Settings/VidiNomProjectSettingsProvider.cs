using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

class VidiNomProjectSettingsProvider : SettingsProvider
{
    const string k_ProjectSettingsPath = "Project/VidiNom";

    VidiNomProjectSettingsProvider(string path = k_ProjectSettingsPath, SettingsScope scope = SettingsScope.Project) : base(path, scope) { }

    /// <inheritdoc />
    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        var settings = VidiNomProjectSettings.Instance;
        if (settings == null)
            return;

        var container = new VisualElement();
        container.AddToClassList("settings");
        //container.styleSheets.AddRange(UIResourceServices.GetProjectSettingsStyleSheets());
        rootElement.Add(container);

        var title = new Label() { text = "VidiNom" };
        title.AddToClassList("title");
        container.Add(title);

        var googleAPIKeyTextField = new TextField("Google API KEY");
        googleAPIKeyTextField.value = VidiNomProjectSettings.GOOGLE_API_KEY;
        googleAPIKeyTextField.RegisterValueChangedCallback(evt => VidiNomProjectSettings.GOOGLE_API_KEY = evt.newValue);
        container.Add(googleAPIKeyTextField);
    }

    [SettingsProvider]
    static SettingsProvider CreateSettingsProvider()
    {
        var settings = new VidiNomProjectSettingsProvider();
        settings.keywords = new List<string> { "VidiNom" };
        settings.label = "VidiNom";
        return settings;
    }
}
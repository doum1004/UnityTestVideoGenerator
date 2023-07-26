using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
[RequireComponent(typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    [SerializeField]
    string m_TitleHeaderKO = "꼭 알아야 할";

    [SerializeField]
    string m_TitleHeaderEN = "Must Know";

    [SerializeField]
    float m_HeaderIndex = -1f;

    public bool ShowInfoPanel = false;

    public DataManager DataMgr;

    UIDocument GetUIDocument() => GetComponent<UIDocument>();
    VisualElement GetRootVisualElement() => GetUIDocument().rootVisualElement;

    void Update()
    {
        StartCoroutine(UICouroutine());
    } 

    void OnValidate()
    {
        //StartCoroutine(UICouroutine());
    }

    IEnumerator UICouroutine()
    {
        UpdateUI();
        yield break;
    }

    public void UpdateUI()
    {
        if (DataMgr == null || DataMgr.data == null)
            return;

        var root = GetRootVisualElement();
        if (root == null)
            return;

        var Hedaer = m_TitleHeaderEN;
        if (DataMgr.data.lang.Contains("ko") || DataMgr.data.lang.Contains("ko"))
            Hedaer = m_TitleHeaderKO;
        Hedaer = Hedaer.ToUpper();
        var titleHeader = root.Q<Label>("TitleHeader");
        if (titleHeader != null)
            titleHeader.text = Hedaer;
        var conclusionHeader = root.Q<TextElement>("ConclusionHeader");
        if (conclusionHeader != null)
            conclusionHeader.text = Hedaer;

        var infoPanel = root.Q<VisualElement>("InfoPanel");
        if (infoPanel != null)
            infoPanel.style.display = ShowInfoPanel ? DisplayStyle.Flex : DisplayStyle.None;

        var info = "";
        var mainLength = DataMgr.data.main.Length;
        for (int i = 0; i < mainLength; i++)
        {
            info += DataMgr.data.main[i].heading;
            if (i < mainLength - 1)
                info += "\n";
        }
        var infoLabel = root.Q<TextElement>("InfoLabel");
        if (infoLabel != null)
            infoLabel.text = info;

        var showScene2 = m_HeaderIndex > DataMgr.data.main.Length;
        var showHeader = (m_HeaderIndex >= 1 && m_HeaderIndex <= DataMgr.data.main.Length);

        var scene1 = root.Q<VisualElement>("Scene1");
        if (scene1 != null)
            scene1.style.display = !showScene2 ? DisplayStyle.Flex : DisplayStyle.None;
        var scene2 = root.Q<VisualElement>("Scene2");
        if (scene2 != null)
            scene2.style.display = showScene2 ? DisplayStyle.Flex : DisplayStyle.None;

        var headerStr = showHeader ? DataMgr.data.main[(int)m_HeaderIndex - 1].heading : "";
        var header = root.Q<Label>("MainHeader");
        if (header != null)
        {
            header.style.display = showHeader ? DisplayStyle.Flex : DisplayStyle.None;
            header.text = headerStr;
        }

        var topic = DataMgr.data.topic.ToUpper();
        var titleTopic = root.Q<TextElement>("TitleTopic");
        if (titleTopic != null)
            titleTopic.text = topic;
        var conclusionTopic = root.Q<TextElement>("ConclusionTopic");
        if (conclusionTopic != null)
            conclusionTopic.text = topic;

    }
}

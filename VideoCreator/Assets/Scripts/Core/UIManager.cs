using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static TextToSpeech;

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

    [SerializeField]
    float m_SubtitleIndex = -1f;

    public bool ShowInfoPanel = false;
    public bool ShowSubtitlePanel = false;

    public DataManager DataMgr;

    public int nbWordsToShow = 1;
    public int prevIndex = 0;
    public int curStartA = 0;

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

        int headerIndexInt = (int)m_HeaderIndex;

        var showScene2 = headerIndexInt > DataMgr.data.main.Length;
        var showHeader = (m_HeaderIndex >= 1 && headerIndexInt <= DataMgr.data.main.Length);

        var scene1 = root.Q<VisualElement>("Scene1");
        if (scene1 != null)
            scene1.style.display = !showScene2 ? DisplayStyle.Flex : DisplayStyle.None;
        var scene2 = root.Q<VisualElement>("Scene2");
        if (scene2 != null)
            scene2.style.display = showScene2 ? DisplayStyle.Flex : DisplayStyle.None;

        var headerStr = showHeader ? DataMgr.data.main[headerIndexInt - 1].heading : "";
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

        var subtitleIndexInt = (int)m_SubtitleIndex;
        var subtitle = "";
        if (ShowSubtitlePanel)
        {
            var a = subtitleIndexInt % 2;
            if (subtitleIndexInt >= 0)
            {
                if (headerIndexInt == 0)
                {
                    subtitle = GetSubTitle(DataMgr.data.intro.timepoints, subtitleIndexInt);
                }
                else if (headerIndexInt <= DataMgr.data.main.Length)
                {
                    subtitle = GetSubTitle(DataMgr.data.main[headerIndexInt - 1].detail.timepoints, subtitleIndexInt);
                }
                else if (subtitleIndexInt < DataMgr.data.conclusion.timepoints.Count)
                {
                    subtitle = GetSubTitle(DataMgr.data.conclusion.timepoints, subtitleIndexInt);
                }
            }

            var subtitleLabel = root.Q<TextElement>("SubtitleLabel");
            if (subtitleLabel != null)
                subtitleLabel.text = subtitle;
        }

        var subtitlePanel = root.Q<VisualElement>("SubtitlePanel");
        if (subtitlePanel != null)
            subtitlePanel.style.display = !string.IsNullOrEmpty(subtitle) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    string GetSubTitle(List<TimepointData> timepoints, int index)
    {
        if (index >= timepoints.Count)
            return "";

        // get two word from timepoints using index. If index % n == 0, get from index to index + n - 1 timepoints.word

        if (prevIndex > index)
            curStartA = 0;

        var a = (index - curStartA) % nbWordsToShow;
        if (a == 0 && prevIndex != index)
        {
            nbWordsToShow = UnityEngine.Random.Range(1, 3);
            curStartA = index % nbWordsToShow;
            a = (index - curStartA) % nbWordsToShow;
        }
        prevIndex = index;

        var startIndex = index - a;
        var endIndex = startIndex + nbWordsToShow - 1;

        var subtitle = "";
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i >= timepoints.Count)
                break;

            var timepoint = timepoints[i];
            if (string.IsNullOrEmpty(timepoint.word))
                continue;

            subtitle += timepoint.word;
            if (i < endIndex)
                subtitle += " ";
        }

        return subtitle;
    }
}

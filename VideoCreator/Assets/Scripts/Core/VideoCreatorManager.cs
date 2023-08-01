using System.Collections;
using UnityEditor;
using UnityEngine;

public class VideoCreatorManager : MonoBehaviour
{
    public string DataJsonPath;
    public DataManager DataMgr;
    public TextToSpeech TTS;
    public UIManager UI;
    public UpdateTimeline TimelineManager;
    public VideoRecordings VideoRecordings;

    [ContextMenu("Work")]
    void Work()
    {
        StartCoroutine(WorkCoroutine());
    }

    public IEnumerator WorkCoroutine()
    {
        yield return LoadCoroutine();
        Produce();
    }

    [ContextMenu("Load")]
    void Load()
    {
        StartCoroutine(LoadCoroutine());
    }

    public IEnumerator LoadCoroutine()
    {
        LoadData();
        yield return TTSWorkCoroutine();
        Timeline();
    }

    [ContextMenu("LoadData")]
    void LoadData()
    {
        DataMgr.LoadData(DataJsonPath);
        DataJsonPath = DataMgr.DataJsonPath;
    }

    [ContextMenu("TTS")]
    void TTSWork()
    {
        StartCoroutine(TTSWorkCoroutine());
    }

    public IEnumerator TTSWorkCoroutine()
    {
        var folder = DataMgr.DataContainer.GetDataStoreFolderPath();

        if (DataMgr.data.intro.audioClip == null)
        {
            yield return TTS.TextToSpeechClipCoroutine(DataMgr.data.intro.content, DataMgr.data.lang, folder, "intro");
            DataMgr.data.intro.audioClip = TTS.OutputAudioClip;
            DataMgr.data.intro.timepoints = TTS.OutputTimePoints;
        }

        for (int i=0; i<DataMgr.data.main.Length; i++)
        {
            var main = DataMgr.data.main[i];
            if (main.detail.audioClip == null)
            {
                yield return TTS.TextToSpeechClipCoroutine(main.heading + ".\n\n" + main.detail.content, DataMgr.data.lang, folder, "main" + (i + 1));
                main.detail.audioClip = TTS.OutputAudioClip;
                main.detail.timepoints = TTS.OutputTimePoints;
            }
        }

        if (DataMgr.data.conclusion.audioClip == null)
        {
            yield return TTS.TextToSpeechClipCoroutine(DataMgr.data.conclusion.content, DataMgr.data.lang, folder, "conclusion");
            DataMgr.data.conclusion.audioClip = TTS.OutputAudioClip;
            DataMgr.data.conclusion.timepoints = TTS.OutputTimePoints;
        }

        DataMgr.SaveDataAsset();
    }

    [ContextMenu("Timeline")]
    void Timeline()
    {
        UI.DataMgr = DataMgr;
        TimelineManager.DataMgr = DataMgr;
        TimelineManager.UI = UI;
        TimelineManager.TestCreate();
    }

    [ContextMenu("Produce")]
    void Produce()
    {
        SetupVideoRecordings();
        EditorApplication.EnterPlaymode();
    }    

    public void SetupVideoRecordings()
    {
        VideoRecordings.RecordVideo = true;
        VideoRecordings.ShortsVideo = true;
        VideoRecordings.CapsureImage = true;
        VideoRecordings.timeFrames = TimelineManager.timeFrames;
        VideoRecordings.playableDirector = TimelineManager.playableDirector;
        VideoRecordings.DataMgr = DataMgr;
        VideoRecordings.UI = UI;
    }

    [ContextMenu("Produce - Video")]
    void ProduceVideo()
    {
        SetupVideoRecordings();
        VideoRecordings.ShortsVideo = false;
        VideoRecordings.CapsureImage = false;
        EditorApplication.EnterPlaymode();
    }

    [ContextMenu("Produce - Video Shorts")]
    void ProduceShorts()
    {
        SetupVideoRecordings();
        VideoRecordings.CapsureImage = false;
        VideoRecordings.RecordVideo = false;
        EditorApplication.EnterPlaymode();
    }

    [ContextMenu("Produce - Image")]
    void ProduceImage()
    {
        SetupVideoRecordings();
        VideoRecordings.ShortsVideo = false;
        VideoRecordings.RecordVideo = false;
        EditorApplication.EnterPlaymode();
    }
}
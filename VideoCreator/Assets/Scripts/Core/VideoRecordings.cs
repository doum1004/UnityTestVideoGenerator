using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// This example shows how to set up a recording session via script.
/// To use this example, add the MultipleRecordingsExample component to a GameObject.
///
/// Enter the Play Mode to start the recording.
/// The recording automatically stops when you exit the Play Mode or when you disable the component.
///
/// This script saves the recording outputs in [Project Folder]/SampleRecordings (except for the recorded animation,
/// which is saved in Assets/SampleRecordings).
/// </summary>
public class VideoRecordings : MonoBehaviour
{
    public PlayableDirector playableDirector;
    
    [System.Serializable]
    public class OutputResolution
    {
        public int OutputWidth = 1280;
        public int OutputHeight = 720;
    }

    public DataManager DataMgr;
    public List<float> timeFrames = new List<float>();
    public UIManager UI;

    public string renderFolder => DataMgr ? Path.Combine(DataMgr.DataContainer.GetDataStoreFolderPath(), "Render") : "";
    public List<OutputResolution> VideoResolutions = new List<OutputResolution> { new OutputResolution() { OutputWidth = 1920, OutputHeight = 1080 } };
    public List<OutputResolution> ImageResolutions = new List<OutputResolution> { new OutputResolution() { OutputWidth = 1280, OutputHeight = 720 }, new OutputResolution() { OutputWidth = 1200, OutputHeight = 1200 } };

    RecorderControllerSettings m_RecorderControllerSettings;
    RecorderController m_RecorderController;

    public bool RecordVideo = true;
    public bool CapsureImage = true;

    void OnEnable()
    {
        StartCoroutine(StartWorkCoroutine());
    }

    IEnumerator StartWorkCoroutine()
    {
        if (CapsureImage)
            yield return StartImageCaptureCoroutine();
        if (RecordVideo)
            yield return StartVideoRecordCoroutine();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    IEnumerator StartVideoRecordCoroutine()
    {
        foreach (var resolution in VideoResolutions)
        {
            playableDirector.Stop();
            playableDirector.time = 0;
            yield return Wait();

            StartRecordVideo(resolution);
            // playableDirector move to begin and play
            playableDirector.Play();

            yield return Wait();
            while (playableDirector.state == PlayState.Playing)
                yield return null;

            m_RecorderController.StopRecording();
        }
        yield break;
    }

    IEnumerator StartImageCaptureCoroutine()
    {
        foreach (var resolution in ImageResolutions)
        {
            playableDirector.Stop();
            playableDirector.time = 0;
            yield return null;

            for (int i = 0; i < timeFrames.Count - 1; i++)
            {
                UI.ShowInfoPanel = i == 0;
                var begin = timeFrames[i];
                var end = timeFrames[i + 1];

                var time = begin + ((end - begin) / 10f);
                playableDirector.time = time;
                yield return Wait();

                StartImageCapture(resolution, i);
                playableDirector.Play();
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
                yield return Wait();

                playableDirector.Pause();
                m_RecorderController.StopRecording();
            }
        }
        yield break;
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
    }

    string GetOutputPath(string prefix, string suffix)
    {
        var path = Path.Combine(renderFolder, $"{prefix}_{suffix}_{DataMgr.data.lang}") + "_v" + DefaultWildcard.Take;
        return path;
        //var i = 0;
        //string path;
        //do
        //{
        //    path = Path.Combine(renderFolder, $"{prefix}_{suffix}_{DataMgr.data.lang}_{++i}_") + "_v" + DefaultWildcard.Take;
        //} while (File.Exists(path));
        //return path;
    }

    void StartRecordVideo(OutputResolution resolution)
    {
        m_RecorderControllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(m_RecorderControllerSettings);

        // Video
        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        videoRecorder.name = "My Video Recorder";
        videoRecorder.Enabled = true;

        videoRecorder.EncoderSettings = new CoreEncoderSettings
        {
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
            Codec = CoreEncoderSettings.OutputCodec.MP4
        };
        videoRecorder.CaptureAudio = true;
        videoRecorder.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = resolution.OutputWidth,
            OutputHeight = resolution.OutputHeight
        };

        string path = GetOutputPath("video", $"{resolution.OutputWidth}x{resolution.OutputHeight}");
        Logger.Log("RecordVideo at: " + path);
        videoRecorder.OutputFile = path;

        // Setup Recording
        m_RecorderControllerSettings.AddRecorderSettings(videoRecorder);
        m_RecorderControllerSettings.SetRecordModeToManual();
        m_RecorderControllerSettings.FrameRate = 60.0f;

        RecorderOptions.VerboseMode = false;
        m_RecorderController.PrepareRecording();
        m_RecorderController.StartRecording();
    }

    void StartImageCapture(OutputResolution resolution, int index)
    {
        m_RecorderControllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(m_RecorderControllerSettings);

        // Image Sequence
        var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        imageRecorder.name = "My Image Recorder";
        imageRecorder.Enabled = true;

        imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.JPEG;
        imageRecorder.CaptureAlpha = false;
        imageRecorder.imageInputSettings = new GameViewInputSettings
        {
            OutputWidth = resolution.OutputWidth,
            OutputHeight = resolution.OutputHeight
        };

        string path = GetOutputPath("image", $"{resolution.OutputWidth}x{resolution.OutputHeight}_{index}");
        Logger.Log("ImageCapture at: " + path);
        imageRecorder.OutputFile = path;

        // Setup Recording
        m_RecorderControllerSettings.AddRecorderSettings(imageRecorder);
        m_RecorderControllerSettings.SetRecordModeToManual();
        m_RecorderControllerSettings.FrameRate = 60.0f;

        RecorderOptions.VerboseMode = false;
        m_RecorderController.PrepareRecording();
        m_RecorderController.StartRecording();
    }

    void Animation()
    {
        // animation output is an asset that must be created in Assets folder
        //// Animation
        //var animationRecorder = ScriptableObject.CreateInstance<AnimationRecorderSettings>();
        //animationRecorder.name = "My Animation Recorder";
        //animationRecorder.Enabled = true;

        //var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //animationRecorder.AnimationInputSettings = new AnimationInputSettings
        //{
        //    gameObject = sphere,
        //    Recursive = true,
        //};

        //animationRecorder.AnimationInputSettings.AddComponentToRecord(typeof(Transform));
        //animationRecorder.OutputFile = Path.Combine(renderFolder, "anim_") + DefaultWildcard.GeneratePattern("GameObject") + "_v" + DefaultWildcard.Take;
        //controllerSettings.AddRecorderSettings(animationRecorder);

    }

    void OnDisable()
    {
        m_RecorderController.StopRecording();
    }
}
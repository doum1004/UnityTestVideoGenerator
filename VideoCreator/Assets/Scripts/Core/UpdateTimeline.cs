using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder.Timeline;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using static TextToSpeech;

public class UpdateTimeline : MonoBehaviour
{
    public PlayableDirector playableDirector;
    TimelineAsset GetTimelineAsset() => playableDirector.playableAsset as TimelineAsset;

    public DataManager DataMgr;

    public List<AnimationClip> clips;
    public MovieRecorderSettings m_Settings = null;
    public UIManager UI;

    public List<float> timeFrames = new List<float>();
    public AudioClip backgroundMusic;
    public float backgroundMusicVolume = 0.12f;
    public string renderFolder => Path.Combine(DataMgr.DataContainer.GetDataStoreFolderPath(), "Render");
    public string clipsFolder => Path.Combine(DataMgr.DataContainer.GetDataStoreFolderPath(), "AnimationClip");

    bool isDirectorPlayed = false;
    public float BeginPauseSec = 2f;
    public float SoundTrackEndPauseSec = 1f;
    public float EndPauseSec = 3f;

    void Update()
    {
        ExitPlayingWhenPlayIsOver();
    }

    void ExitPlayingWhenPlayIsOver()
    {
        if (!playableDirector)
            return;

        if (Application.isPlaying)
        {
            isDirectorPlayed |= playableDirector.state == PlayState.Playing;
            if (isDirectorPlayed && (playableDirector.time >= playableDirector.duration - 0.5f))
                UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    void RefreshTimeline()
    {
        playableDirector.RebuildGraph();
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
    }

    [ContextMenu("Create")]
    public void TestCreate()
    {
        DeleteAllTrack();

        AddAudioTracks();
        AddUITracks();
        //CraeteRecordTrack(timeFrames[timeFrames.Count - 1]);
    }

    void DeleteAllTrack()
    {
        clips.Clear();
        var timeline = GetTimelineAsset();
        foreach (var track in timeline.GetRootTracks())
            timeline.DeleteTrack(track);
        RefreshTimeline();
    }

    void AddAudioTracks()
    {
        AddSpeechAudioSource();
        AddBackgroundAudioSource();
        RefreshTimeline();
    }

    void AddUITracks()
    {
        CreateImageAnimations();

        var animationTrack = CrateAnimationTrack(UI.gameObject, "UI Animation Track");
        CreateUICurveAnimations(animationTrack);
        RefreshTimeline();
    }

    void CreateImageTracks(List<GameObject> images, float start, float end)
    {
        var imageCount = images.Count;
        var duration = (end - start) / imageCount;

        for (int i=0; i < imageCount; ++i)
        {
            var image = images[i];
            var time1 = start + duration * i;
            CreateImageTrack(image, time1, duration, UnityEngine.Random.Range(1.1f, 1.5f));
        }
    }

    AnimationClip CraeteAnimationClip()
    {
        var animClip = new AnimationClip();
        clips.Add(animClip);
        
        if (!Directory.Exists(clipsFolder))
            Directory.CreateDirectory(clipsFolder);
        string clipAssetPath = Path.Combine(clipsFolder, $"AnimationClip{clips.Count}.asset");
        AssetDatabase.CreateAsset(animClip, clipAssetPath);
        AssetDatabase.SaveAssets();

        animClip.legacy = false;

        return animClip;
    }

    public void CreateImageTrack(GameObject go, float start, float duration, float scale)
    {
        Utils.FitImage(go);
        if (!go.TryGetComponent<Animator>(out var _))
            go.AddComponent<Animator>();

        var timeline = GetTimelineAsset();

        // Activation
        var activationTrack = timeline.CreateTrack<ActivationTrack>("Activation Track");
        playableDirector.SetGenericBinding(activationTrack, go);

        var activationClip = activationTrack.CreateDefaultClip();
        activationClip.displayName = "Activation Clip";
        activationClip.start = start;
        activationClip.duration = duration;

        // Animation
        var animationTrack = timeline.CreateTrack<AnimationTrack>("Animation Track");
        playableDirector.SetGenericBinding(animationTrack, go);

        var scaleXCurve = AnimationCurve.Linear(0, go.transform.localScale.x, duration, go.transform.localScale.x * scale);
        var scaleYCurve = AnimationCurve.Linear(0, go.transform.localScale.y, duration, go.transform.localScale.y * scale);
        var scaleZCurve = AnimationCurve.Linear(0, go.transform.localScale.z, duration, go.transform.localScale.z * scale);

        var scaleClip = CraeteAnimationClip();
        scaleClip.SetCurve("", typeof(Transform), "localScale.x", scaleXCurve);
        scaleClip.SetCurve("", typeof(Transform), "localScale.y", scaleYCurve);
        scaleClip.SetCurve("", typeof(Transform), "localScale.z", scaleZCurve);

        // Add the AnimationClip to the AnimationTrack
        var timelineClip = animationTrack.CreateClip(scaleClip);
        timelineClip.displayName = "Animation time clip";
        timelineClip.start = start;
        timelineClip.duration = duration;
    }

    AnimationTrack CrateAnimationTrack(GameObject go, string trackName)
    {
        if (!go.TryGetComponent<Animator>(out var _))
            go.AddComponent<Animator>();

        var timeline = GetTimelineAsset();
        var animationTrack = timeline.CreateTrack<AnimationTrack>(trackName);
        playableDirector.SetGenericBinding(animationTrack, go);
        return animationTrack;
    }

    public void SetAnimClipCurve(AnimationTrack at, string propertyName, float start, float end, int value)
    {
        var animClip = CraeteAnimationClip();
        var elipse = 0.05f;
        //animClip.SetCurve(string.Empty, typeof(UIManager), propertyName, AnimationCurve.Linear(0, value, duration, value));
        animClip.SetCurve("", typeof(UIManager), propertyName, AnimationCurve.Linear(start, 0, start + elipse, 0));
        animClip.SetCurve("", typeof(UIManager), propertyName, AnimationCurve.Linear(start + elipse, value, end, value));

        // Add the AnimationClip to the AnimationTrack
        var timelineClip = at.CreateClip(animClip);
        timelineClip.displayName = "Animation time clip";
        timelineClip.start = start;
        timelineClip.duration = end - start;

        //animClip.SetCurve(string.Empty, typeof(UIManager), propertyName, AnimationCurve.Constant(start, end, value));
        //animClip.SetCurve(string.Empty, typeof(UIManager), propertyName, AnimationCurve.Linear(start, defaultValue, end, value));
        //animClip.SetCurve(string.Empty, typeof(UIManager), propertyName, AnimationCurve.Linear(start, value, end, value));
    }

    public void AddAnimClipKey(AnimationCurve animCurve, float start, float end, int value)
    {
        AddKeyFrame(animCurve, start, value);
        //AddKeyFrame(animCurve, start + elipse, value);
        //AddKeyFrame(animCurve, end - elipse, value);
        AddKeyFrame(animCurve, end, value);
    }

    void AddKeyFrame(AnimationCurve animCurve, float time, float value)
    {
        var keyFrame = new Keyframe(time, value);
        animCurve.AddKey(keyFrame);
        AnimationUtility.SetKeyLeftTangentMode(animCurve, animCurve.length - 1, AnimationUtility.TangentMode.Constant);
        AnimationUtility.SetKeyRightTangentMode(animCurve, animCurve.length - 1, AnimationUtility.TangentMode.Constant);
    }

    public void AddAnimClipKey(AnimationCurve animCurve, float start, int value)
    {
        AddKeyFrame(animCurve, start, value);
    }

    public void AddTimepointsAnimClipKey(AnimationCurve animCurve, List<TimepointData> timepoints, float start)
    {
        AddAnimClipKey(animCurve, start, -1);

        for (int i1 = 0; i1 < timepoints.Count; i1++)
        {
            var timepoint = timepoints[i1];
            AddAnimClipKey(animCurve, start + timepoint.timeSeconds, i1);
        }
    }

    void AddSpeechAudioSource()
    {
        timeFrames.Clear();
        timeFrames.Add(0f);

        var start = timeFrames[timeFrames.Count - 1];
        var at1 = CreateAudioTrack(DataMgr.data.intro.audioClip, start + BeginPauseSec);
        var end = (float)at1.start + (float)at1.duration;
        timeFrames.Add(end);

        foreach (var main in DataMgr.data.main)
        {
            start = timeFrames[timeFrames.Count - 1];
            at1 = CreateAudioTrack(main.detail.audioClip, start);
            end = (float)at1.start + (float)at1.duration;
            timeFrames.Add(end);
        }

        start = timeFrames[timeFrames.Count - 1];
        at1 = CreateAudioTrack(DataMgr.data.conclusion.audioClip, start);
        end = (float)at1.start + (float)at1.duration + EndPauseSec;
        timeFrames.Add(end);
    }

    void AddBackgroundAudioSource()
    {
        var duration = timeFrames.Count > 0 ? timeFrames[timeFrames.Count - 1] : 30;
        var timeline = GetTimelineAsset();

        var audioTrack = timeline.CreateTrack<AudioTrack>("Audio Track");

        // Add the AudioClip to the AudioTrack
        TimelineClip timelineClip = audioTrack.CreateClip(backgroundMusic);
        timelineClip.start = 0;
        timelineClip.duration = duration;

        audioTrack.CreateCurves("nameOfAnimationClip");
        //audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "volume", AnimationCurve.Linear(0, 0.1f, duration, 0.1f));
        audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "volume", AnimationCurve.Constant(0, duration, backgroundMusicVolume));

        // Set the target GameObject as the track's binding
        playableDirector.SetGenericBinding(audioTrack, gameObject);
    }

    void CreateImageAnimations()
    {
        int i = 0;
        var start = timeFrames[i++];
        var end = timeFrames[i++];
        CreateImageTracks(DataMgr.data.intro.imgObjs, start, end);

        foreach (var main in DataMgr.data.main)
        {
            start = end;
            end = timeFrames[i++];
            CreateImageTracks(main.detail.imgObjs, start, end);
        }

        start = end;
        end = timeFrames[i++];
        CreateImageTracks(DataMgr.data.conclusion.imgObjs, start, end);
    }

    void CreateUICurveAnimations(AnimationTrack at)
    {
        var headingAnimCurve = new AnimationCurve();
        var subtitleAnimCurve = new AnimationCurve();

        int i = 0;
        var start = timeFrames[i++];
        var end = timeFrames[i++];
        AddAnimClipKey(headingAnimCurve, start, 0);
        AddTimepointsAnimClipKey(subtitleAnimCurve, DataMgr.data.intro.timepoints, start + 2);

        for (int i1 = 0; i1 < DataMgr.data.main.Length; i1++)
        {
            start = end;
            end = timeFrames[i++];
            AddAnimClipKey(headingAnimCurve, start, i1 + 1);
            AddTimepointsAnimClipKey(subtitleAnimCurve, DataMgr.data.main[i1].detail.timepoints, start);
        }

        start = end;
        end = timeFrames[i++];
        AddAnimClipKey(headingAnimCurve, start, DataMgr.data.main.Length + 1);
        AddTimepointsAnimClipKey(subtitleAnimCurve, DataMgr.data.conclusion.timepoints, start);

        var headingAnimClip = CraeteAnimationClip();
        var timelineClip = at.CreateClip(headingAnimClip);
        timelineClip.displayName = "Animation heading time clip";
        timelineClip.start = 0;
        timelineClip.duration = timeFrames.LastOrDefault();

        headingAnimClip.SetCurve("", typeof(UIManager), "m_HeaderIndex", headingAnimCurve);
        headingAnimClip.SetCurve("", typeof(UIManager), "m_SubtitleIndex", subtitleAnimCurve);
    }

    AudioTrack CreateAudioTrack(AudioClip audioClip, float startTime)
    {
        if (audioClip == null)
            return null;

        var timeline = GetTimelineAsset();

        var duration = audioClip.length;
        var audioTrack = timeline.CreateTrack<AudioTrack>("Audio Track");

        // Add the AudioClip to the AudioTrack
        TimelineClip timelineClip = audioTrack.CreateClip(audioClip);
        timelineClip.start = startTime;
        timelineClip.duration = duration + SoundTrackEndPauseSec;

        // Set the target GameObject as the track's binding
        playableDirector.SetGenericBinding(audioTrack, gameObject);
        return audioTrack;
    }
}

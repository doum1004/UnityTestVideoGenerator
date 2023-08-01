using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    [System.Serializable]
    public class VoiceOption
    {
        public string LangCode = "";
        public List<string> VoiceNames = new List<string> { "" };
        public string Gender = "";
        public int CurrentVoiceName = 0;
        public VoiceOption(string langCode, List<string> voiceNames, string gender)
        {
            LangCode = langCode;
            VoiceNames = voiceNames;
            Gender = gender;
        }
    }

    static string s_URL = "https://texttospeech.googleapis.com/v1/text:synthesize?key=";
    static string s_URLBeta = "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=";
    Dictionary<string, int> langVoiceOptionIndexDict = new Dictionary<string, int> { { "en", 0 }, { "ko", 1 } };
    public List<VoiceOption> VoiceOptions => new List<VoiceOption> {
        new VoiceOption("en-US", new List<string> { "en-US-Neural2-F" }, "FEMALE"), //en-US-Studio-O
        new VoiceOption("ko-KR", new List<string> { "ko-KR-Neural2-B" }, "FEMALE")
    };

    public string audioEncoding = "LINEAR16";
    public string pitch = "0";
    public float speakingRate = 1.1f;

    public TTSResponseData ResponseData;
    public AudioClip OutputAudioClip;
    public List<TimepointData> OutputTimePoints;
    List<string> m_Words = new List<string>();

    public void TextToSpeechClip(string text, string lang, string folder, string clipName)
    {
        //StartCoroutine(TextToSpeechClipCoroutine(text, lang, folder, clipName));
        StartCoroutine(TextToSpeechClipCoroutineV1(text, lang, folder, clipName));
    }

    public IEnumerator TextToSpeechClipCoroutineV1(string text, string lang, string folder, string clipName)
    {
        var curVoiceOption = VoiceOptions[langVoiceOptionIndexDict[lang]];
        var langCode = curVoiceOption.LangCode;
        var voiceName = curVoiceOption.VoiceNames[curVoiceOption.CurrentVoiceName];
        var gender = curVoiceOption.Gender;
        using (UnityWebRequest www = new UnityWebRequest(s_URL + VidiNomProjectSettings.GOOGLE_API_KEY, "POST"))
        {
            var requestBody = $"{{\"input\":{{\"text\":\"{text}\"}},\"voice\":{{\"languageCode\":\"{langCode}\",\"name\":\"{voiceName}\"}},\"audioConfig\":{{\"audioEncoding\":\"{audioEncoding}\",\"speakingRate\":{speakingRate},\"pitch\":{pitch}}}}}";
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogError("Error: " + www.error);
                yield break;
            }

            byte[] responseData = www.downloadHandler.data;
            HandleResponseData(responseData, GetFilePath(folder, clipName));
        }
    }

    public IEnumerator TextToSpeechClipCoroutine(string text, string lang, string folder, string clipName)
    {
        var curVoiceOption = VoiceOptions[langVoiceOptionIndexDict[lang]];
        var langCode = curVoiceOption.LangCode;
        var voiceName = curVoiceOption.VoiceNames[curVoiceOption.CurrentVoiceName];
        var gender = curVoiceOption.Gender;
        using (UnityWebRequest www = new UnityWebRequest(s_URLBeta + VidiNomProjectSettings.GOOGLE_API_KEY, "POST"))
        {
            m_Words.Clear();
            var sentences = text.Split('.');
            var i = 1;
            var ssmlText = "<speak>";
            foreach (var sentence in sentences)
            {
                ssmlText += "<p>";
                var sentenceWords = sentence.Split(' ');
                foreach (var word in sentenceWords)
                {
                    var trimedWord = word.Trim();
                    if (string.IsNullOrEmpty(trimedWord))
                        continue;

                    var htmlCode = trimedWord.Replace("'", "&apos;").Replace("\"", "&quot;");
                    ssmlText += $@"<mark name='{htmlCode}_{i++}' /> {trimedWord} ";

                    m_Words.Add(trimedWord);
                }
                ssmlText += "</p>";
            }
            ssmlText += "</speak>";

            var requestBody = $@"
{{
    ""input"": {{
		""ssml"": ""{ssmlText}""

    }},
	""voice"": {{
        ""languageCode"": ""{langCode}"",
		""name"": ""{voiceName}"",
		""ssmlGender"": ""{gender}""

    }},
	""enableTimePointing"": [""SSML_MARK""],
	""audioConfig"": {{
        ""audioEncoding"": ""{audioEncoding}"",
		""speakingRate"": {speakingRate},
        ""pitch"": {pitch}
    }}
}}";
            Logger.Log(ssmlText);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogError("Error: " + www.error);
                yield break;
            }

            byte[] responseData = www.downloadHandler.data;
            HandleResponseData(responseData, GetFilePath(folder, clipName));
        }
    }

    void HandleResponseData(byte[] responseDataByte, string filePath)
    {
        ResponseData = GetResponseData(responseDataByte);

        foreach (var a in ResponseData.timepoints)
        {
            Logger.Log(a.markName);
        }

        var decodedAudioData = System.Convert.FromBase64String(ResponseData.audioContent);
        OutputAudioClip = SaveAudioAsWav(decodedAudioData, filePath);
        OutputTimePoints = ResponseData.timepoints;

        for (int i = 0; i < OutputTimePoints.Count; i++)
        {
            OutputTimePoints[i].word = m_Words[i];
        }
    }

    TTSResponseData GetResponseData(byte[] responseDataByte)
    {
        string responseDataStr = System.Text.Encoding.Default.GetString(responseDataByte);
        return JsonUtility.FromJson<TTSResponseData>(responseDataStr);
    }

    string GetFilePath(string dir, string clipName)
    {
        var folderPath = Path.Combine(dir, "AudioClips");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        return Path.Combine(folderPath, clipName + ".wav");
    }

    AudioClip SaveAudioAsWav(byte[] audioData, string filePath)
    {
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fileStream))
                writer.Write(audioData);
        }
        Logger.Log("WAV audio file downloaded and saved at: " + filePath);

        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.Default);
        AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(filePath);
        return audioClip;
    }

    AudioClip ConvertToAudioClip(byte[] audioData, string filePath)
    {
        int sampleRate = 24000;
        int channels = 1;
        int dataSize = BitConverter.ToInt32(audioData, 40);
        float[] samples = new float[dataSize / 2];

        for (int i = 0; i < dataSize; i += 2)
        {
            short sample = BitConverter.ToInt16(audioData, 44 + i);
            samples[i / 2] = sample / 32768.0f;
        }
        AudioClip clip = AudioClip.Create(new FileInfo(filePath).Name, samples.Length, channels, sampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    AudioClip SaveAudioClipAsAsset(AudioClip clip, string filePath)
    {
        // Save the AudioClip as an asset
        AssetDatabase.CreateAsset(clip, filePath);
        AudioImporter audioImporter = AssetImporter.GetAtPath(filePath) as AudioImporter;
        if (audioImporter != null)
        {
            // Set your desired import options for the AudioClip
            // For example, you can modify the import settings here
            audioImporter.loadInBackground = true;
            // Apply the changes to the importer
            audioImporter.SaveAndReimport();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Logger.Log("AudioClip saved as asset at: " + filePath);
        var assetClip = AssetDatabase.LoadAssetAtPath<AudioClip>(filePath);
        if (assetClip == null)
            Logger.LogError("Failed to load AudioClip: " + filePath);

        return assetClip;
    }

    [System.Serializable]
    public class TTSResponseData
    {
        public string audioContent;
        public List<TimepointData> timepoints;
        public string audioConfig;
    }

    [System.Serializable]
    public class TimepointData
    {
        public string markName;
        public float timeSeconds;
        public string word;
    }
}
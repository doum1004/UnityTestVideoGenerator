using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class TextToSpeech : MonoBehaviour
{

    private string apiEndpoint = "https://texttospeech.googleapis.com/v1/text:synthesize?key=";
    public string GOOGLE_API_KEY;
    public AudioSource audioSource;
    public string text;
    public string outputFilePath = @"C:\Workspace\output.wav";
    public float speakingRate = 1.1f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    [ContextMenu("Speak")]
    public void Speak()
    {
        StartCoroutine(SendRequest(text));
    }

    private IEnumerator SendRequest(string text)
    {
        using (UnityWebRequest www = new UnityWebRequest(apiEndpoint + GOOGLE_API_KEY, "POST"))
        {
            var requestBody = $"{{\"input\":{{\"text\":\"{text}\"}},\"voice\":{{\"languageCode\":\"ko-KR\",\"name\":\"ko-KR-Neural2-B\"}},\"audioConfig\":{{\"audioEncoding\":\"LINEAR16\",\"speakingRate\":{speakingRate}}}}}";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                byte[] audioData = www.downloadHandler.data;
                SaveAudioAsWav(audioData, outputFilePath);
                Debug.Log("WAV audio file downloaded and saved as: " + outputFilePath);
                OpenFileFolder(outputFilePath);
            }
        }

        //using (UnityWebRequest www = new UnityWebRequest(apiEndpoint, "POST"))
        //{
        //    www.SetRequestHeader("Content-Type", "application/json");


        //    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
        //    www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //    www.downloadHandler = new DownloadHandlerBuffer();

        //    yield return www.SendWebRequest();

        //    if (www.result == UnityWebRequest.Result.Success)
        //    {
        //        string response = www.downloadHandler.text;
        //        string base64Audio = ExtractBase64Audio(response);
        //        AudioClip audioClip = Base64ToAudioClip(base64Audio);
        //        audioSource.clip = audioClip;
        //        audioSource.Play();
        //    }
        //    else
        //    {
        //        UnityEngine.Debug.LogError($"Error: {www.error}");
        //    }
        //}
    }

    [System.Serializable]
    private class AudioResponse
    {
        public string audioContent;
    }

    void OpenFileFolder(string filePath)
    {
        string folderPath = Path.GetDirectoryName(filePath);
        Process.Start("explorer.exe", folderPath);
    }

    private void SaveAudioAsWav(byte[] audioData, string filePath)
    {
        string audioContent = System.Text.Encoding.Default.GetString(audioData);
        var responseJson = JsonUtility.FromJson<AudioResponse>(audioContent);
        byte[] decodedAudioData = System.Convert.FromBase64String(responseJson.audioContent);

        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                writer.Write(decodedAudioData);
            }
        }
    }

}
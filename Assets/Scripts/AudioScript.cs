using System.Collections;
using System.IO;
using System.Text;
using HuggingFace.API;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SpeechRecognitionTest : MonoBehaviour
{
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button stopButton;

    [SerializeField]
    private TextMeshProUGUI text;

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
    }

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
    }

    private void StopRecording()
    {
        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;
        SendRecording();
    }

    private void SendRecording()
    {
        HuggingFaceAPI.AutomaticSpeechRecognition(
            bytes,
            response =>
            {
                Debug.Log(response);
                text.color = Color.white;
                text.text = response;

                string jsonData = "{\"prompt\": \"" + response + "\"}";

                byte[] postData = Encoding.UTF8.GetBytes(jsonData);

                StartCoroutine(PostRequest("https://for-gemini.onrender.com/generate", postData));
            },
            error =>
            {
                text.color = Color.red;
                text.text = error;
            }
        );
    }

    IEnumerator PostRequest(string url, byte[] postData)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");

            webRequest.uploadHandler = new UploadHandlerRaw(postData);

            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
                text.color = Color.red;
                text.text = "Error sending request";
            }
            else
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
                text.color = Color.green;
                text.text = webRequest.downloadHandler.text;
            }
        }
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
}

using UnityEngine;
using UnityEngine.Android;

public class MicrophoneManager : MonoBehaviour
{
    private bool isMicrophoneInitialized = false;

    void Start()
    {
        // Check if the microphone permission is already granted
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // Permission is granted, proceed with microphone initialization
            InitializeMicrophone();
        }
        else
        {
            // Request microphone permission
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }

    void Update()
    {
        // Check if the permission has been granted after the request
        if (!isMicrophoneInitialized && Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // Permission is granted, proceed with microphone initialization
            InitializeMicrophone();
            isMicrophoneInitialized = true;
            Debug.Log("Microphone initialized and recording started.");
        }
        else if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // Permission is denied, handle accordingly
            Debug.LogError("Microphone permission denied.");
        }
    }

    void InitializeMicrophone()
    {
        // Check if the device has a microphone
        if (Microphone.devices.Length > 0)
        {
            // Start recording with the microphone
            AudioSource audioSource = GetComponent<AudioSource>();
            audioSource.clip = Microphone.Start(null, true, 10, 44100);
            while (!(Microphone.GetPosition(null) > 0)) { }
            audioSource.Play();
        }
        else
        {
            Debug.LogError("No microphone found on this device.");
        }
    }
}

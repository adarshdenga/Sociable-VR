using System;
using UnityEngine;


#if !SCRIPT_JSON_DEBUG_ONLY
public class SpeechRecognizer : IDisposable {
    private Recorder recorder;
    private WhisperBridge whisper;
    private RecognizerLL dummy; // only for signaling start / end / existence of voice input

    private bool sessionRequested;

    public event Action<bool, string> OnAttempt;
    public event Action OnSpeaking;


    public SpeechRecognizer() {
        recorder = new Recorder();

        whisper = new WhisperBridge();

        dummy = new RecognizerLL();
        dummy.OnSpeaking += () => {
            if(sessionRequested)
                OnSpeaking?.Invoke();
        };
        dummy.OnCompleted += OnSessionCompleted;
    }


    private void OnSessionCompleted(bool ok) {
        sessionRequested = false;
        recorder.Stop();
        if(!ok || recorder.Data == null) {
            OnAttempt?.Invoke(false, "");
            return;
        }
        var text = whisper.RequestSTT(recorder.Data);
        OnAttempt?.Invoke(true, text);
    }


    public void RequestSession() {
        if(sessionRequested)
            return;
        sessionRequested = true;
        recorder.Start();
        dummy.Start();
    }


    public void EndSession() {
        dummy.Stop();
        recorder.Stop();
        sessionRequested = false;
    }


    public void Dispose() {
        sessionRequested = true;
        OnSpeaking = null;
        OnAttempt = null;
        dummy.Dispose();
        dummy = null;
        whisper.Dispose();
        whisper = null;
        recorder.Dispose();
        recorder = null;
    }
}
#else
public class SpeechRecognizer : IDisposable {
    public event Action<bool, string> OnAttempt;
    public event Action OnSpeaking;


    public SpeechRecognizer() {
    }


    public void RequestSession() {
        OnSpeaking?.Invoke();
        OnAttempt?.Invoke(true, Extensions.BypassToken);
    }


    public void EndSession() {
    }


    public void Dispose() {
        OnSpeaking = null;
        OnAttempt = null;
    }
}
#endif


internal class Recorder : IDisposable {
    private AudioClip recording;

    public AudioClip Data => recording;


    public Recorder() {
        foreach(var device in Microphone.devices)
            Debug.Log($"Found microphone {device}");
    }


    private void End() {
        if(Microphone.IsRecording(null))
            Microphone.End(null);
    }


    public void Start() {
        End();
        recording = Microphone.Start(null, false, 20, 44100);
    }


    public void Stop() {
        //var pos = Microphone.GetPosition(null);
        End();
        // todo truncate (or not ?)
    }


    public void Dispose() {
        End();
        recording = null;
    }
}

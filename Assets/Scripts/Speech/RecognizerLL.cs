using System;
using UnityEngine;


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UnityEngine.Windows.Speech;


internal class RecognizerLL : IDisposable {
    private DictationRecognizer dummy; // literally dummy I mean

    public event Action<bool> OnCompleted;
    public event Action OnSpeaking;


    public RecognizerLL() {
        dummy = new DictationRecognizer(ConfidenceLevel.Low) {
            AutoSilenceTimeoutSeconds = 1.1f,
            InitialSilenceTimeoutSeconds = 10f
        };

        dummy.DictationError += (error, hresult) => Debug.LogError($"RecognizerLL error {error} ({hresult})");
        dummy.DictationHypothesis += _ => OnSpeaking?.Invoke();
        dummy.DictationComplete += OnDictationComplete;
    }


    private void OnDictationComplete(DictationCompletionCause cause) {
        var ok = cause == DictationCompletionCause.TimeoutExceeded;
        if(ok)
            Debug.Log("RecognizerLL completed with TLE (expected behaviour)");
        else
            Debug.Log($"RecognizerLL completed due to {cause}");
        OnCompleted?.Invoke(ok);
    }


    public void Start() {
        if(dummy.Status != SpeechSystemStatus.Running)
            dummy.Start();
    }


    public void Stop() {
        if(dummy.Status != SpeechSystemStatus.Stopped)
            dummy.Stop();
    }


    public void Dispose() {
        OnCompleted = null;
        OnSpeaking = null;
        if(dummy == null)
            return;
        Stop();
        dummy.Dispose();
        dummy = null;
    }
}
#else
internal class RecognizerLL : IDisposable {
    public event Action<bool> OnCompleted;
    public event Action OnSpeaking;


    public RecognizerLL() {
        Debug.LogWarning("Not supported on this system yet");
    }


    public void Start() {
        OnCompleted?.Invoke(true);
    }


    public void Stop() {
    }


    public void Dispose() {
        OnCompleted = null;
        OnSpeaking = null;
    }
}
#endif

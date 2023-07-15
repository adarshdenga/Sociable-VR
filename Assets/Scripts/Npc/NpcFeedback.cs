using UnityEngine;


public abstract class NpcFeedback : MonoBehaviour {
    public struct SpeechInfo {
        public string name;
        public string utterance;
        public AudioClip clip;
    }

    public abstract void StartSpeaking(SpeechInfo info);
    public abstract void StopSpeaking();
}

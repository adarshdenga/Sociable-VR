using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DanceTalk : NpcFeedback {
    private Animation anime;

    //private AudioSource source;
    //public bool IsSpeaking;


    private void Start() {
        anime = GetComponent<Animation>();
        anime.playAutomatically = false;
        anime.Stop();
    }


    public override void StartSpeaking(SpeechInfo info) {
        StopAllCoroutines();
        anime.Play("Dance", PlayMode.StopAll);
    }


    public override void StopSpeaking() {
        anime.Stop();
        StartCoroutine(ResetToDefault());
    }


    private IEnumerator ResetToDefault() {
        anime.Play("Idle", PlayMode.StopAll);
        yield return new WaitForSeconds(.2f);
        anime.Stop();
    }
}

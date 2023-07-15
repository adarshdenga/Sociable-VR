using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DefaultNpcAnime : NpcFeedback {
    private Animation anime;
    private Material renderMat;

    private Coroutine blinking;


    private void Start() {
        anime = GetComponent<Animation>();
        anime.playAutomatically = false;
        anime.Stop();
        StartCoroutine(DelayedStart());

        var ren = GetComponentInChildren<SkinnedMeshRenderer>();
        renderMat = ren.material;
    }


    public override void StartSpeaking(SpeechInfo info) {
        if(blinking != null) {
            StopCoroutine(blinking);
            blinking = null;
        }
        blinking = StartCoroutine(Blink());
    }


    public override void StopSpeaking() {
        if(blinking != null) {
            StopCoroutine(blinking);
            blinking = null;
        }
        renderMat.color = Color.white;
    }


    private IEnumerator DelayedStart() {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1, 11) / 5.0f);
        anime.Play("Idle", PlayMode.StopAll);
    }


    private IEnumerator Blink() {
        while(true) {
            renderMat.color = Color.white * 2;
            yield return new WaitForSeconds(.9f);
            renderMat.color = Color.white;
            yield return new WaitForSeconds(.9f);
        }
    }
}

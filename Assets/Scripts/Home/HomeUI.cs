using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class HomeUI : MonoBehaviour {
    [SerializeField]
    private Button playButton, helpButton, settingsButton, quitButton;

    [SerializeField]
    private RectTransform frame;

    [SerializeField]
    private GameObject playPanelPrefab, helpPanelPrefab, settingsPanelPrefab;


    private void ClearPanel() {
        var children = (from RectTransform child in frame select child.gameObject).ToList();
        children.ForEach(Destroy);
    }


    private void Start() {
        playButton.onClick.AddListener(() => {
            ClearPanel();
            GameObject.Instantiate(playPanelPrefab, frame);
        });

        helpButton.onClick.AddListener(() => {
            ClearPanel();
            GameObject.Instantiate(helpPanelPrefab, frame);
        });

        settingsButton.onClick.AddListener(() => {
            ClearPanel();
            GameObject.Instantiate(settingsPanelPrefab, frame);
        });

        quitButton.onClick.AddListener(() => {
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
    }
}

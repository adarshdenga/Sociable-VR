using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class SettingsPanelUI : MonoBehaviour {
    [SerializeField]
    private TMP_InputField nameField;

    [SerializeField]
    private Button subtitleToggle;

    [SerializeField]
    private TMP_Text subtitleState;


    private void Start() {
        nameField.text = GameConfig.playerName;
        subtitleState.text = GameConfig.showSubtitle ? "show" : "hide";

        nameField.onEndEdit.AddListener((text) => { GameConfig.playerName = text; });

        subtitleToggle.onClick.AddListener(() => {
            GameConfig.showSubtitle = !GameConfig.showSubtitle;
            subtitleState.text = GameConfig.showSubtitle ? "show" : "hide";
        });
    }
}

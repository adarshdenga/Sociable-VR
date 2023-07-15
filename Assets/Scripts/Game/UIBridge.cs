using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIBridge : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI speakerTMP, utteranceTMP;

    [SerializeField]
    private Button micButton;

    [SerializeField]
    private RectTransform optionList;

    [SerializeField]
    private GameObject optionPrefab;


    private float micColorResetCd;

    private IEnumerator MicColorChange() {
        var image = micButton.GetComponent<Image>();
        image.color = new Color(0, 1, 0, .3f);
        while(micColorResetCd > 0) {
            yield return new WaitForSeconds(.3f);
            micColorResetCd -= .3f;
        }
        image.color = new Color(1, 1, 1, .3f);
    }

    public void SignalSpeaking() {
        if(micColorResetCd > 0) {
            micColorResetCd += .3f;
            return;
        }
        micColorResetCd += .3f;
        StartCoroutine(MicColorChange());
    }


    public Button.ButtonClickedEvent OnMicBtnClick => micButton.onClick;


    public void SetSubtitle(string speaker = "", string utterance = "") {
        speakerTMP.text = speaker;
        utteranceTMP.text = utterance;
    }


    public Button AddOptions(string text) {
        var option = GameObject.Instantiate(optionPrefab, optionList);
        option.GetComponentInChildren<TextMeshProUGUI>().text = text;
        var button = option.GetComponent<Button>();
        return button;
    }


    public void ClearOptions() {
        var options = (from RectTransform child in optionList select child.gameObject).ToList();
        options.ForEach(Destroy);
    }
}

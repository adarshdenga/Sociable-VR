using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ScoreUI : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI scoreText;

    public static int score;


    private void Start() {
        scoreText.text = $"{score}";
        StartCoroutine(WaitThenHome());
    }


    private static IEnumerator WaitThenHome() {
        yield return new WaitForSeconds(5f);
        score = 0;
        SceneManager.LoadScene("Scenes/00-Home");
    }
}

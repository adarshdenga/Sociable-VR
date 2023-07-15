using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayPanelUI : MonoBehaviour {
    // wired to Event Trigger on Image
    public void OnMeetingSceneClicked() {
        SceneManager.LoadScene("Scenes/01-Meeting");
    }


    // wired to Event Trigger on Image
    public void OnRestaurantSceneClicked() {
        SceneManager.LoadScene("Scenes/02-Restaurant");
    }


    // wired to Event Trigger on Image
    public void OnInterviewSceneClicked() {
        SceneManager.LoadScene("Scenes/03-Bus");
        //SceneManager.LoadScene("Scenes/03-Interview");
    }
}

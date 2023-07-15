using UnityEngine;


public class CameraCtrl : MonoBehaviour {
    [SerializeField]
    private Transform cameraParent;

    private float mouseSensitivity = 300;

    private Vector2 mouseDelta = Vector2.zero;
    private Vector2 mouseDeltaVelocity = Vector2.zero;
    private float mouseSmoothTime = .03f;


    private void LateUpdate() {
        if(Input.GetMouseButton(0)) {
            var targetDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            targetDelta *= Time.deltaTime * mouseSensitivity;
            mouseDelta = Vector2.SmoothDamp(mouseDelta, targetDelta, ref mouseDeltaVelocity, mouseSmoothTime);

            var euler = cameraParent.localEulerAngles;
            var pitch = euler.x + -mouseDelta.y;
            if(pitch > 270)
                pitch -= 360;
            euler.x = Mathf.Clamp(pitch, -89.9f, 89.9f);
            euler.y += mouseDelta.x;
            euler.z = 0;
            cameraParent.localEulerAngles = euler;
        }

        // todo move camera position with wasdqe
    }
}

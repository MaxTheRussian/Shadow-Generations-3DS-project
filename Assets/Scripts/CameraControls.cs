using UnityEngine;
using UnityEngine.N3DS;

public class CameraControls : MonoBehaviour {

    public Transform ToFollow;
    public Transform LookAt;

    public Vector2 CamHandlerInput;
    public Vector3 Offset;
    public Vector3 LookAtOffset;
    float Angle = 0f;
    Vector3 UpDir;
    public void Start()
    {
        // Here you can add code that needs to be called when script is created, just before the first game update
        UnityEngine.Application.targetFrameRate = 60;
    }

    public void FixedUpdate()
    {
        CamHandlerInput += (GamePad.CirclePadPro + new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) + (Input.touchCount > 0 ? Input.GetTouch(0).deltaPosition.normalized : Vector2.zero)).normalized * 7f * Time.deltaTime;
        CamHandlerInput.y = Mathf.Clamp(CamHandlerInput.y, -2f, 2f);
        UpDir = Vector3.up;
        transform.position = ToFollow.position + Quaternion.FromToRotation(Vector3.up, UpDir) * new Vector3(Offset.z * Mathf.Cos(CamHandlerInput.x), Offset.y + CamHandlerInput.y, Offset.z * Mathf.Sin(CamHandlerInput.x));

        transform.rotation = Quaternion.LookRotation(LookAtOffset + LookAt.position - transform.position, Vector3.up);
        //DebugDraw.DrawCircle(ToFollow.Position + Vector3.up * Offset.Y * Quaternion.FindBetween(Vector3.up, ToFollow.Transform.up), ToFollow.Transform.up, Offset.Z, Color.Red, 0.1f, true);
    }
}

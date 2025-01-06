using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ExpertDynamic
{
    [DefaultExecutionOrder(999)]
    public class ExpertDynamicHeart : MonoBehaviour
    {
        public ExpertDynamicHandler activeCamera;
        Transform activeCamTransform;
        public float TransitionTime = 0.2f;
        public EasingStyles EasingType;
        bool transitioning = false;
        [SerializeField] private List<ExpertDynamicHandler> camerasInScene = new List<ExpertDynamicHandler>();
        public enum EasingStyles
        {
            Linear,
            Sine
        }

        public void OnEnable()
        {

        }

        public void LateUpdate()
        {
            if (!transitioning)
                UpdateCameraTransform();
        }

        private void UpdateCameraTransform() // FromHandler
        {
            if (activeCamTransform == null)
            {
                activeCamTransform = activeCamera.transform;
                if (activeCamTransform == null)
                {
                    Debug.LogError("No Initial camera set!");
                    return;
                }
            }
            transform.position = activeCamTransform.position;
            transform.rotation = activeCamTransform.rotation;
        }

        public void RegisterNewCamera(ExpertDynamicHandler registeredCam) // Включилась новая 
        {
            camerasInScene.Add(registeredCam);
            if (registeredCam.Priority >= activeCamera.Priority)
                StartCoroutine(SetActiveCamera(registeredCam));
        }


        public void RemoveThisCamera(ExpertDynamicHandler WasThisYourCurrent)  // Менять вообще весь метод
        {
            camerasInScene.Remove(WasThisYourCurrent);
            if (WasThisYourCurrent == activeCamera)
            {
                int i = 0, maxPrior = 0;
                for (i = 0; i < camerasInScene.Count; ++i)
                {
                    if (camerasInScene[i].Priority >= camerasInScene[maxPrior].Priority)
                        maxPrior = i;
                }
                StartCoroutine(SetActiveCamera(camerasInScene[maxPrior]));
            }            
        }

        IEnumerator SetActiveCamera(ExpertDynamicHandler newCam) // Make a transition not a snap, idiot
        {
            if (!newCam.gameObject.activeInHierarchy)
                yield break;
            float a = 0f;
            float t = 0f;
            Camera camera = gameObject.GetComponent<Camera>();
            transitioning = true;
            float ogFOV = camera.fieldOfView;
            float ogNP = camera.nearClipPlane;
            float ogFP = camera.farClipPlane;
            Vector3 ogPs = camera.transform.position;
            Quaternion ogRt = camera.transform.rotation;

            activeCamera = newCam;
            activeCamTransform = newCam.transform;

            while (t < 1f)
            {
                t = Mathf.Clamp(a / TransitionTime, 0f, 1f);
                transform.position = Vector3.Lerp(ogPs, newCam.transform.position, t);
                transform.rotation = Quaternion.Lerp(ogRt, newCam.transform.rotation, t);
                camera.fieldOfView = Mathf.Lerp(ogFOV, newCam.fieldOfView, t);
                camera.nearClipPlane = Mathf.Lerp(ogNP, newCam.nearPlane, t);
                camera.farClipPlane = Mathf.Lerp(ogFP, newCam.farPlane, t);
                yield return new WaitForEndOfFrame();
                a += Time.deltaTime;
            }

            transitioning = false;
        }


#if UNITY_EDITOR
    public void OnDebugDrawSelected()
    {
        UpdateCameraTransform();
    }
#endif
    }
}

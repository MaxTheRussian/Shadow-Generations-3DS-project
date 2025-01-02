using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExpertDynamic
{

    /// <summary>
    /// ExpertDynamicHandler Script.
    /// </summary>
    public class ExpertDynamicHandler : MonoBehaviour
    {
        public int Priority = 0;
        public float fieldOfView = 70f;
        public float nearPlane = 7f;
        public float farPlane = 12750f;
        [Range(0f, 10f)] public float CustomAspectRatio = 0f;
        public StandByUpdateType StandByUpdate;

        public enum StandByUpdateType
        {
            Never,
            Always
        }

        void OnEnable()
        {
            Camera.main.GetComponent<ExpertDynamicHeart>().RegisterNewCamera(this);
        }

        void OnDisable()
        {
            Camera.main.GetComponent<ExpertDynamicHeart>().RemoveThisCamera(this);
        }

    }
}
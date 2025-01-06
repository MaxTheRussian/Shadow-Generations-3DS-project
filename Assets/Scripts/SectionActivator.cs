using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SectionActivator : MonoBehaviour
{
    public UnityEvent OnEnter;

    public IEnumerator Activate()
    {
        OnEnter.Invoke();
        yield return new WaitForEndOfFrame();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerDetector : MonoBehaviour
{
    public UnityEvent onExitEvent;
    public Transform tokenTray;
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out MathToken token))
        {
            token.ReturnToTray(tokenTray);
        }
        else if (other.TryGetComponent(out CountryMarker marker))
        {
            marker.ReturnToTray(tokenTray);
        }
    }
}

using UnityEngine;
using System;

public class CapybaraEvent : MonoBehaviour
{
    public static Action<GameObject> OnCapybaraCreation;

    void Start()
    {
        OnCapybaraCreation?.Invoke(this.gameObject);
    }
}

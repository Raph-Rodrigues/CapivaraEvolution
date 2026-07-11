using UnityEngine;
using System;

public class OrangeEvent : MonoBehaviour
{
    public static Action<GameObject> OnOrangeCreation;

    void Start()
    {
        OnOrangeCreation?.Invoke(this.gameObject);
    }
}


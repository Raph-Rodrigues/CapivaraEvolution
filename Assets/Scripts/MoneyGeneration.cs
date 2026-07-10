using System.Collections.Generic;
using UnityEngine;

using System.Collections;

public class MoneyGeneration : MonoBehaviour
{
    [Header("Ajustes")]
    [SerializeField] private List<int> tierMoneyGeneration = new List<int>(); // 5, 13, 35, 90, 235
    [SerializeField] private float cooldown = 10f;

    private int money = 0;
    private List<GameObject> Capivaras = new List<GameObject>();
    
    public void CapivaraAdded(GameObject cap)
    {
        if (cap.CompareTag("Capivara"))
        {
            Capivaras.Add(cap);
            // int tier = cap.GetComponent<>(); // pegar tier
            StartCoroutine(MoneyCoroutine(cap, 1)); // ajustar 1 pelo tier
        }
    }

    public void CapivaraRemoved(GameObject cap)
    {
        if (cap.CompareTag("Capivara") && Capivaras.Contains(cap))
        {
            Capivaras.Remove(cap);
        }
    }

    IEnumerator MoneyCoroutine(GameObject cap, int tier)
    {
        while (Capivaras.Contains(cap))
        {
            yield return new WaitForSeconds(cooldown);
            money += tierMoneyGeneration[tier];
        }
    }
    
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class MoneyGeneration : MonoBehaviour
{
    [Header("Variaveis externas")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [Header("Ajustes")]
    [SerializeField] private List<int> tierMoneyGeneration = new List<int>(); // 5, 13, 35, 90, 235
    [SerializeField] private float cooldown = 1f;
    [Header("Visualização de debug")]
    [SerializeField] private float money = 0;
    private List<GameObject> Capivaras = new List<GameObject>();
    
    public void CapivaraAdded(GameObject cap)
    {
        if (cap.CompareTag("Capivara"))
        {
            Capivaras.Add(cap);
            CapybaraBehaviors capScript = cap.GetComponent<CapybaraBehaviors>();
            int tier = capScript.EvolutionLevel;
            StartCoroutine(MoneyCoroutine(cap, tier)); // ajustar 1 pelo tier
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
            money += tierMoneyGeneration[tier] / 10f;
            Debug.Log(money);
        }
    }

    public bool CheckIfHasMoney(int v)
    {
        if (money >= v)
        {
            money -= v;
            return true;
        }
        else
        {
            return false;
        }
    }

    void Update()
    {
        float arredondamento = (Mathf.Round(money * 100f) / 100f); // 2 casas decimais no maximo
        moneyText.text = arredondamento.ToString(); 
    }
    
}

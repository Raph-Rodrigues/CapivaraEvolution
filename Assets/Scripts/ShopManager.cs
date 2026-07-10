using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Variaveis externas")]
    [SerializeField] private MoneyGeneration moneyScript;
    [SerializeField] private CapivaraBoxDrops boxScript;

    [Header("Variaveis internas")]
    [SerializeField] private GameObject openShopButton;
    [SerializeField] private GameObject lockUi;
    [SerializeField] private GameObject shopUI;
    [SerializeField] private List<GameObject> prefabsList = new List<GameObject>();

    [Header("Ajustes")]
    [SerializeField] private List<int> listaDePrecos = new List<int>();
    private bool isShopEnabled = false;

    public void SetShopEnable()
    {
        isShopEnabled = true;
        lockUi.SetActive(false);
        openShopButton.SetActive(true);
    }

    public void OpenShop()
    {
        if (isShopEnabled)
        {
            shopUI.SetActive(!shopUI.active);
        }
    }

    public void NewUnlock(int tier)
    {
        if (prefabsList[tier] && prefabsList[tier].active == false)
        {
            prefabsList[tier].SetActive(true);
        }
    }

    public void BuyClick(int tier)
    {
        int price = listaDePrecos[tier];
        bool bought = moneyScript.CheckIfHasMoney(price);

        if (bought)
        {
            boxScript.DropCustomBox(tier);
        }
    }
}

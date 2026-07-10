using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CapivaraBoxDrops : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private MoneyGeneration moneyScript;
    [SerializeField] private float BoxDropsCooldown = 10f;
    [SerializeField] private float BoxHeightOffSet = 10f;
    [SerializeField] private float LeftLimit = -10f;
    [SerializeField] private float RightLimit = 10f;
    [SerializeField] private float TopLimit = 5f;
    [SerializeField] private float BottomLimit = -5f;
    [Header("Prefabs")]
    [SerializeField] private GameObject DropBoxPrefab;
    [SerializeField] private List<GameObject> CapivaraPrefabs = new List<GameObject>();

    private float counter;

    void Start()
    {
        if (PlayerPrefs.HasKey("DropBoxCooldown"))
        {
            BoxDropsCooldown = PlayerPrefs.GetFloat("DropBoxCooldown");
        }

        counter = BoxDropsCooldown;
    }

    void FixedUpdate()
    {
        if (counter > 0)
        {
            counter -= Time.fixedDeltaTime;
        }
        else if (counter <= 0)
        {
            counter = BoxDropsCooldown;

            int chance = UnityEngine.Random.Range(1, 100);
            
            if (chance <= 5)
            {
                DropBox(6);
            }
            else
            {
                DropBox(0);
            }
            
        }
    }

    void DropBox(int tier) // pega uma posição aleatória dentro dos limites e spawna a caixa com um offset para cima
    {
        float randomX = UnityEngine.Random.Range(LeftLimit, RightLimit);
        float randomY = UnityEngine.Random.Range(BottomLimit, TopLimit);

        GameObject spawnedBox = Instantiate(DropBoxPrefab);
        spawnedBox.transform.position = new Vector3(randomX, randomY + BoxHeightOffSet, 0);
        StartCoroutine(BoxFalling(spawnedBox.transform, randomY, tier));
        
    }
    
    void SpawnCapivara(GameObject box, int tier) // deleta caixa e spawna capivara no lugar dela
    {
        GameObject spawnedCapivara = Instantiate(CapivaraPrefabs[tier]);
        Vector2 spawnPos = box.transform.position;

        Destroy(box); // substituir por efeito de quebrar caixa depois

        spawnedCapivara.transform.position = spawnPos;
        moneyScript.CapivaraAdded(spawnedCapivara);
    }

    IEnumerator BoxFalling(Transform boxTransform, float randomY, int tier) // detecta quando a caixa chega no chão
    {
        bool onGround = false;

        while (onGround == false)
        {
            if (boxTransform.position.y <= randomY)
            {
                onGround = true;
                boxTransform.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0f; // supõe que prefab da caixa tem rigidyBody para cair
                boxTransform.gameObject.GetComponent<Rigidbody2D>().linearVelocityY = 0f;
                yield return new WaitForSeconds(0.5f);
                SpawnCapivara(boxTransform.gameObject, tier);
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return 0;

    }

    public void DropCustomBox(int tier)
    {
        DropBox(tier);
    }
}

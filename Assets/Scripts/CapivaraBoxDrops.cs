using System.Collections;
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
    [SerializeField] private GameObject CapivaraPrefab;

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
            DropBox();
        }
    }

    void DropBox() // pega uma posição aleatória dentro dos limites e spawna a caixa com um offset para cima
    {
        float randomX = UnityEngine.Random.Range(LeftLimit, RightLimit);
        float randomY = UnityEngine.Random.Range(BottomLimit, TopLimit);

        GameObject spawnedBox = Instantiate(DropBoxPrefab);
        spawnedBox.transform.position = new Vector3(randomX, randomY + BoxHeightOffSet, 0);
        StartCoroutine(BoxFalling(spawnedBox.transform, randomY));
        
    }
    
    void SpawnCapivara(GameObject box) // deleta caixa e spawna capivara no lugar dela
    {
        GameObject spawnedCapivara = Instantiate(CapivaraPrefab);
        Vector2 spawnPos = box.transform.position;

        Destroy(box); // substituir por efeito de quebrar caixa depois

        spawnedCapivara.transform.position = spawnPos;
        moneyScript.CapivaraAdded(spawnedCapivara);
    }

    IEnumerator BoxFalling(Transform boxTransform, float randomY) // detecta quando a caixa chega no chão
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
                SpawnCapivara(boxTransform.gameObject);
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return 0;

    }


}

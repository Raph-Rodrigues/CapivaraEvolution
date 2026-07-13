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
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioClip BoxSFX;
    [SerializeField] private float BoxDropsCooldown = 10f;
    [SerializeField] private float BoxHeightOffSet = 10f;
    [SerializeField] private float LeftLimit = -10f;
    [SerializeField] private float RightLimit = 10f;
    [SerializeField] private float TopLimit = 5f;
    [SerializeField] private float BottomLimit = -5f;
    [Header("Prefabs")]
    [SerializeField] private GameObject DropBoxPrefab;
    [SerializeField] private List<GameObject> CapivaraPrefabs = new List<GameObject>();
    [SerializeField] private List<Sprite> particles = new List<Sprite>();
    [SerializeField] private GameObject ParticlePrefab;

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

            DropBox(0);
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
        //ParticleEffect(spawnPos);
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
                yield return new WaitForSeconds(0.1f);
                Vector2 spawnPos = boxTransform.position;
                ParticleEffect(spawnPos);
                boxTransform.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                SFXSource.clip = BoxSFX;
                SFXSource.Play();
                yield return new WaitForSeconds(0.4f);
                SpawnCapivara(boxTransform.gameObject, tier);
            }
            yield return new WaitForSeconds(0.1f);
            
        }
        yield return 0;

    }

    public void DropCustomBox(int tier)
    {
        DropBox(tier+1);
    }

    private void ParticleEffect(Vector2 spawnPosition)
    {
        if (ParticlePrefab == null || particles.Count == 0) return;

        GameObject effectGo = Instantiate(ParticlePrefab, spawnPosition, Quaternion.identity);
        ParticleSystem ps = effectGo.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            var textureSheet = ps.textureSheetAnimation;
            textureSheet.enabled = true;
            textureSheet.mode = ParticleSystemAnimationMode.Sprites;

            int spriteCount = textureSheet.spriteCount;
            for (int i = spriteCount - 1; i >= 0; i--)
            {
                textureSheet.RemoveSprite(i);
            }
            foreach (Sprite sprite in particles)
            {
                if (sprite != null)
                {
                    textureSheet.AddSprite(sprite);
                }
            }

            var mainModule = ps.main;
            mainModule.stopAction = ParticleSystemStopAction.Destroy;
            
            ps.Play();
        }
    }
}

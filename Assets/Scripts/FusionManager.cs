using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

[System.Serializable]
public class CapybaraEvolution
{
  [Header("Capivara atual")]
  public string currentType;
  public Sprite currentSpr;
  public GameObject currentPrefab;

  [Header("Evolução")]
  public string nextType;
  public Sprite nextSpr;
  public GameObject nextPrefab;
}

[RequireComponent(typeof(InputHandler))]
public class FusionManager : MonoBehaviour
{
  [Header("Variaveis externas")]
  [SerializeField] private MoneyGeneration moneyScript;
  [SerializeField] private ShopManager shopScript;
  [SerializeField] private AudioSource SFXSource;
  [SerializeField] private AudioClip mergeSFX;

  [Header("Configurações de Interação")]
  [SerializeField] private LayerMask _spawnLayer;

  [Header("Configurações Dev Vara")]
  [SerializeField] float spawnChance = 14f;
  [SerializeField] private GameObject DevVara;
  [SerializeField] private GameObject _achievementUI;
  [SerializeField] private GameObject _devVaraAlertUI;
  [SerializeField] private GameObject _boxCounter;
  [SerializeField] private TextMeshProUGUI _devVaraCounterTxt;
  [SerializeField] private int _devVaraCounter;

  [Header("Tabela de Evoluções")]
  [SerializeField] private List<CapybaraEvolution> _evolutionList;
  public List<CapybaraEvolution> EvolutionList => _evolutionList;

  private InputHandler _input;
  private Camera _mainCamera;

  private CapybaraBehaviors _draggedCapybara;
  [SerializeField] private bool _isFusionHappen = false;

  void Awake()
  {
    _input = GetComponent<InputHandler>();
    _mainCamera = Camera.main;

    if (_achievementUI != null) _achievementUI.SetActive(false);
    if (_devVaraAlertUI != null) _devVaraAlertUI.SetActive(false);
    if (_boxCounter != null) _boxCounter.SetActive(false);
    if (_devVaraCounterTxt != null) _devVaraCounterTxt.text = "0";
  }

  void Update()
  {
    if (_isFusionHappen) return;

    HandleDragAndDrop();
  }

  private void HandleDragAndDrop()
  {
    // Converte a posição do mouse na tela para a posição no mundo 2D
    Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(_input.PointerPosition);

    // TENTATIVA DE PEGAR A CAPIVARA
    if (_input.IsPointerPressed && _draggedCapybara == null)
    {
      Debug.Log($"[DragAndDrop] Clicou segurando. Posição do Mouse no Mundo: {mouseWorldPos}");

      Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, _spawnLayer);

      if (hit != null)
      {
        Debug.Log($"[DragAndDrop] Mouse tocou no colisor do objeto: {hit.gameObject.name}");
        _draggedCapybara = hit.GetComponent<CapybaraBehaviors>();

        if (_draggedCapybara != null)
        {
          Debug.Log("[DragAndDrop] SUCESSO! Capivara conectada ao mouse.");
          _draggedCapybara.IsDragged = true;
        }
        else
        {
          Debug.LogWarning("[DragAndDrop] O objeto clicado tem colisor, mas NÃO tem o script CapybaraBehaviors!");
        }
      }
      else
      {
        Debug.LogWarning("[DragAndDrop] O mouse não tocou em nenhum colisor pertencente à SpawnLayer configurada.");
      }
    }

    // ARRASTANDO A CAPIVARA
    if (_input.IsPointerPressed && _draggedCapybara != null)
    {
      _draggedCapybara.transform.position = mouseWorldPos;
    }

    // SOLTANDO A CAPIVARA
    if (_input.WasPointerReleasedThisFrame && _draggedCapybara != null)
    {
      Debug.Log($"[DragAndDrop] Soltou a capivara {_draggedCapybara.gameObject.name} na posição {mouseWorldPos}");
      CheckFoFusionOnDrop(mouseWorldPos);
    }
  }

  private void CheckFoFusionOnDrop(Vector2 dropPosition)
  {
    Collider2D[] hits = Physics2D.OverlapPointAll(dropPosition, _spawnLayer);
    CapybaraBehaviors target = null;

    Debug.Log($"[Fusão] Checando colisão no ponto solto. Quantidade de objetos na mesma posição: {hits.Length}");

    foreach (var hit in hits)
    {
      CapybaraBehaviors capy = hit.GetComponent<CapybaraBehaviors>();
      if (capy != null && capy != _draggedCapybara)
      {
        target = capy;
        break;
      }
    }

    if (target != null)
    {
      if ((_draggedCapybara.EvolutionLevel == target.EvolutionLevel) && (_draggedCapybara.EvolutionLevel < _evolutionList.Count - 1))
      {
        Debug.Log($"[Fusão] Par compatível! {_draggedCapybara.Name} + {target.Name}");
        StartCoroutine(FusionRoutine(_draggedCapybara, target));
      }
      else
      {
        Debug.Log("[Fusão] Níveis diferentes ou nível máximo alcançado. Cancelando.");
        _draggedCapybara.IsDragged = false;
        _draggedCapybara = null;
      }
    }
    else
    {
      Debug.Log("[Fusão] Soltou no vazio. Voltando a capivara ao normal.");
      _draggedCapybara.IsDragged = false;
      _draggedCapybara = null;
    }
  }

  private void HandleDevVaraSpawnUI()
  {
    _devVaraCounter++;

    // atualiza o texto do contador
    if (_devVaraCounterTxt != null)
    {
      _devVaraCounterTxt.text = $"{_devVaraCounter}";
    }

    if (_devVaraCounter == 1)
    {
      // animação da conquista da dev vara
      if (_achievementUI != null)
      {
        _achievementUI.SetActive(true);
        _achievementUI.transform.localScale = Vector3.zero;
        _boxCounter.SetActive(true);
        _boxCounter.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(_achievementUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack)); // aparece
        seq.AppendInterval(2.5f);
        seq.Append(_achievementUI.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack)); // some
        seq.OnComplete(() => Destroy(_achievementUI)); // destroi permanentemente

        _boxCounter.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
      }
    }
    else // alerta de DevVara sendo spawnada
    {
      if (_devVaraAlertUI != null)
      {
        _devVaraAlertUI.SetActive(true);
        _devVaraAlertUI.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(_devVaraAlertUI.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)); // aparece rápido
        seq.AppendInterval(1.5f);
        seq.Append(_devVaraAlertUI.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)); // some rápido
        seq.OnComplete(() => _devVaraAlertUI.SetActive(false)); // desativa para reutilizar
      }
    }
  }

  private IEnumerator FusionRoutine(CapybaraBehaviors capy1, CapybaraBehaviors capy2)
  {
    _isFusionHappen = true;
    _draggedCapybara = null;

    capy1.IsFusing = true;
    capy1.IsDragged = false;
    capy2.IsFusing = true;

    capy1.StopAllTweens();
    capy2.StopAllTweens();

    Vector2 centerPoint = (capy1.transform.position + capy2.transform.position) / 2f;
    Vector2 dir1 = ((Vector2)capy1.transform.position - centerPoint).normalized;
    Vector2 dir2 = ((Vector2)capy2.transform.position - centerPoint).normalized;

    // afastamento
    float pullBackTime = 0.15f;
    capy1.transform.DOMove((Vector2)capy1.transform.position + dir1 * 0.4f, pullBackTime).SetEase(Ease.OutQuad);
    capy2.transform.DOMove((Vector2)capy2.transform.position + dir2 * 0.4f, pullBackTime).SetEase(Ease.OutQuad);

    yield return new WaitForSeconds(pullBackTime);

    // colisão forte
    float dashTime = 0.15f;
    capy1.transform.DOMove(centerPoint, dashTime).SetEase(Ease.InExpo);
    Tween dash2 = capy2.transform.DOMove(centerPoint, dashTime).SetEase(Ease.InExpo);

    yield return dash2.WaitForCompletion(); // espera eles baterem no meio

    // camera shake
    _mainCamera.DOShakePosition(0.2f, strength: 0.15f, vibrato: 10, randomness: 90, fadeOut: true);

    int currentLevel = capy1.EvolutionLevel;
    GameObject survivor = null;

    if (currentLevel < _evolutionList.Count)
    {
      CapybaraEvolution evData = _evolutionList[currentLevel];

      if (evData.nextPrefab != null)
      {
        if (currentLevel == 3)
        {
          float randomI = Random.Range(0, 100);

          if (randomI < spawnChance)
          {
            // DevVara
            survivor = Instantiate(DevVara, centerPoint, Quaternion.identity);
            Destroy(capy1.gameObject);
            Destroy(capy2.gameObject);
            moneyScript.CapivaraRemoved(capy1.gameObject);
            moneyScript.CapivaraRemoved(capy2.gameObject);

            // logica da UI 
            HandleDevVaraSpawnUI();
          }
          else
          {
            survivor = Instantiate(evData.nextPrefab, centerPoint, Quaternion.identity);
            Destroy(capy1.gameObject);
            Destroy(capy2.gameObject);
            moneyScript.CapivaraRemoved(capy1.gameObject);
            moneyScript.CapivaraRemoved(capy2.gameObject);
          }
        }
        else
        {
          survivor = Instantiate(evData.nextPrefab, centerPoint, Quaternion.identity);
          Destroy(capy1.gameObject);
          Destroy(capy2.gameObject);
          moneyScript.CapivaraRemoved(capy1.gameObject);
          moneyScript.CapivaraRemoved(capy2.gameObject);
        }
      }
      else
      {
        Debug.Log("Esqueceu de colocar o prefab seu BURRÃO");
        capy1.transform.position = centerPoint;
        Destroy(capy2.gameObject);
        capy1.IsFusing = false;
        survivor = capy1.gameObject;
      }
    }
    else
    {
      Debug.Log("Fusão TIER Máximo");
      capy1.transform.position = centerPoint;
      Destroy(capy2.gameObject);
      capy1.IsFusing = false;
      survivor = capy1.gameObject;
    }

    // animação de conclusão com sucesso
    if (survivor != null)
    {
      CapybaraBehaviors survivorBehavior = survivor.GetComponent<CapybaraBehaviors>();
      SFXSource.clip = mergeSFX;
      SFXSource.Play();
      // Impede que a capivara sobrevivente/nova tente andar durante o POP
      if (survivorBehavior != null) survivorBehavior.IsFusing = true;

      Vector3 baseScale = survivor.transform.localScale;
      survivor.transform.localScale = Vector3.zero;

      // FIX animação de conclusão de fusão
      Sequence doublePop = DOTween.Sequence();

      // 1° POP
      doublePop.Append(survivor.transform.DOScale(baseScale * 1.2f, 0.1f).SetEase(Ease.OutQuad));
      doublePop.Append(survivor.transform.DOScale(baseScale, 0.05f).SetEase(Ease.InQuad));
      // 2° POP
      doublePop.Append(survivor.transform.DOScale(baseScale * 1.2f, 0.1f).SetEase(Ease.OutQuad));
      doublePop.Append(survivor.transform.DOScale(baseScale, 0.05f).SetEase(Ease.InQuad));

      moneyScript.CapivaraAdded(survivor);

      int survivorTier;

      if (currentLevel + 1 > _evolutionList.Count - 1)
      {
        survivorTier = currentLevel;
      }
      else
      {
        survivorTier = currentLevel + 1;
      }

      if (survivorTier == 1)
      {
        shopScript.SetShopEnable();
      }
      shopScript.NewUnlock(currentLevel);

      yield return doublePop.WaitForCompletion(); // Espera o POP terminar

      // Libera a capivara e forçar a entrar em Squeeze/Idle
      if (survivorBehavior != null)
      {
        survivorBehavior.StartIdleAfterFusion();
      }
    }
    else
    {
      yield return new WaitForSeconds(0.15f);
    }

    _isFusionHappen = false;
  }
}

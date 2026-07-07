using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class CapybaraEvolution
{
  [Header("Capivara atual")]
  public string currentType;
  public SpriteRenderer currentSpr;
  public GameObject currentPrefab;

  [Header("Evolução")]
  public string nextType;
  public SpriteRenderer nextSpr;
  public GameObject nextPrefab;
}

[RequireComponent(typeof(InputHandler))]
public class FusionManager : MonoBehaviour
{
  [Header("Configurações de Interação")]
  [SerializeField] private LayerMask _spawnLayer;
  [SerializeField] private float _fusionSpeed = 5f;

  [Header("Tabela de Evoluções")]
  [SerializeField] private List<CapybaraEvolution> _evolutionList;

  private InputHandler _input;
  private Camera _mainCamera;

  private CapybaraBehaviors _draggedCapybara;
  [SerializeField] private bool _isFusionHappen = false;

  void Awake()
  {
    _input = GetComponent<InputHandler>();
    _mainCamera = Camera.main;
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
      if (_draggedCapybara.EvolutionLevel == target.EvolutionLevel)
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

  private IEnumerator FusionRoutine(CapybaraBehaviors capy1, CapybaraBehaviors capy2)
  {
    _isFusionHappen = true;
    _draggedCapybara = null;

    capy1.IsFusing = true;
    capy1.IsDragged = false;
    capy2.IsFusing = true;

    Vector2 centerPoint = (capy1.transform.position + capy2.transform.position) / 2f;

    while (Vector2.Distance(capy1.transform.position, centerPoint) > 0.1f ||
           Vector2.Distance(capy2.transform.position, centerPoint) > 0.1f)
    {
      capy1.transform.position = Vector2.MoveTowards(capy1.transform.position, centerPoint, _fusionSpeed * Time.deltaTime);
      capy2.transform.position = Vector2.MoveTowards(capy2.transform.position, centerPoint, _fusionSpeed * Time.deltaTime);
      yield return null;
    }

    int currentLevel = capy1.EvolutionLevel;

    if (currentLevel < _evolutionList.Count)
    {
      CapybaraEvolution evData = _evolutionList[currentLevel];

      if (evData.nextPrefab != null)
      {
        Instantiate(evData.nextPrefab, centerPoint, Quaternion.identity);
        Destroy(capy1.gameObject);
        Destroy(capy2.gameObject);
      }
      else
      {
        Debug.Log("Esqueceu de colocar o prefab seu BURRÃO");
        capy1.transform.position = centerPoint;
        Destroy(capy2.gameObject);
        capy1.IsFusing = false;
      }
    }
    else
    {
      Debug.Log("Fusão TIER Máximo");
      capy1.transform.position = centerPoint;
      Destroy(capy2.gameObject);
      capy1.IsFusing = false;
    }

    _isFusionHappen = false;
  }
}

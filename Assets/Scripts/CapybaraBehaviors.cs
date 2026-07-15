using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class CapybaraBehaviors : MonoBehaviour
{
  [Header("Configurações de movimento")]
  [SerializeField] private float _speed = 2f;
  [Tooltip("Raio máximo que pode se moviementar a partir do ponto inicial")]
  [SerializeField] private float _movementRadius = 5f;
  [Tooltip("Tempo mínimo que fica parada")]
  [SerializeField] private float _minIdleTime = 1f;
  [Tooltip("Tempo máximo que fica parada")]
  [SerializeField] private float _maxIdleTime = 3f;

  [Header("Ciclo de vida / Auto-Fusão")]
  [SerializeField] private float _lifeTime = 60f;
  [SerializeField] private bool _isDevVara = false;
  [SerializeField] private float _currentLifeTimer;
  [SerializeField] private int _maxTier5 = 12;
  [SerializeField] private bool _isDying = false;
  [SerializeField] private bool _hasOvercrowdingAlert = false;

  [Header("Configurações de Colisão com a Parede")]
  [SerializeField] private LayerMask _wallLayer;
  [Tooltip("Distância de segurança para não encostar na parede")]
  [SerializeField] private float _obstacleOffset = 0.2f;

  [Header("Sistema de Direção da Sprite")]
  [SerializeField] private bool _isLookingRight = false;
  private SpriteRenderer _capySprite;
  private int _originalSortingOrder;

  [Header("Debug da Fusão")]
  [SerializeField] private bool _isFusing;
  public bool IsFusing
  {
    get { return _isFusing; }
    set
    {
      _isFusing = value;
      if (_capySprite != null)
      {
        // traz para frente quando está fundindo e retorna para trás após a fusão
        _capySprite.sortingOrder = _isFusing ? 100 : _originalSortingOrder;
      }
    }
  }
  [SerializeField] private bool _isDragged;
  public bool IsDragged
  {
    get { return _isDragged; }
    set
    {
      _isDragged = value;
      if (_capySprite != null)
      {
        _capySprite.sortingOrder = _isDragged ? 100 : _originalSortingOrder;
      }

      if (_isDragged)
      {
        _lastValidPos = transform.position;

        StopAllTweens();

        transform.DORotate(Vector3.zero, 0.2f);
        transform.DOScale(new Vector3(_baseScale.x * 1.2f, _baseScale.y * 1.2f, _baseScale.z), 0.2f).SetEase(Ease.OutBack);
      }
      else
      {
        StopAllTweens();
        transform.DOScale(_baseScale, 0.2f).SetEase(Ease.OutQuad);

        if (!_isFusing)
        {
          Collider2D hitWall = Physics2D.OverlapCircle(transform.position, _obstacleOffset, _wallLayer);

          if (hitWall != null)
          {
            transform.DOMove(_lastValidPos, 0.3f).SetEase(Ease.OutBack).OnComplete(ResetMovementState);
            _startPos = _lastValidPos;
          }
          else
          {
            _startPos = transform.position;
            ResetMovementState();
          }
        }
      }
    }
  }

  [Header("Status de evolução")]
  [SerializeField] private int _evolutionLevel = 0;
  public int EvolutionLevel
  {
    get { return _evolutionLevel; }
    set { _evolutionLevel = value; }
  }
  [SerializeField] private string _name = "Tier 1";
  public string Name
  {
    get { return _name; }
    set { _name = value; }
  }

  private Vector3 _baseScale;
  private Vector2 _startPos;
  private Vector2 _targetPos;
  private Vector2 _lastValidPos;

  private bool _isMoving;
  private float _idleTimer;

  private Tween _idleTween;
  private Tween _moveTween;

  void Awake()
  {
    _baseScale = transform.localScale;

    _capySprite = GetComponent<SpriteRenderer>();
    if (_capySprite == null)
    {
      _capySprite = GetComponentInChildren<SpriteRenderer>();
    }

    if (_capySprite != null)
    {
      _originalSortingOrder = _capySprite.sortingOrder;
    }
  }

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    _startPos = transform.position;
    _isFusing = false;
    _isDying = false;
    _hasOvercrowdingAlert = false;
    _currentLifeTimer = _lifeTime;

    // registra na lista global do jogo assim que nascer
    if (FusionManager.Instance != null)
    {
      FusionManager.Instance.ActiveCapybaras.Add(this);
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (IsFusing || _isDragged || _isDying) return;

    HandleLifeCycleTimer();

    if (_isMoving)
    {
      HandleMove();
    }
    else
    {
      HandleIdleTimer();
    }
  }

  private void HandleLifeCycleTimer()
  {
    if (_isDevVara) return;

    if (FusionManager.Instance == null) return;

    _currentLifeTimer -= Time.deltaTime;

    if (_currentLifeTimer <= 0)
    {
      // Tiers 1, 2 e 3 tentão a fusão automática
      if (_evolutionLevel < 4)
      {
        CapybaraBehaviors target = FindNearestSameTier();
        if (target != null)
        {
          if (FusionManager.Instance.ForceAutoMerge(this, target))
          {
            _currentLifeTimer = _lifeTime;
          }
          else
          {
            _currentLifeTimer = 1.5f;
          }
        }
        else
        {
          _currentLifeTimer = 2f; // Tenta novamente em 2 segundos se não achar um par
        }
      }
      else if (_evolutionLevel == 4) // tier 5 morre por super lotação
      {
        if (FusionManager.Instance.GetTier5Count() > _maxTier5)
        {
          // dispara o alerta e concede 15s de vida extra se ainda não recebeu o aviso
          if (!_hasOvercrowdingAlert)
          {
            FusionManager.Instance.ShowTier5OvercrowdingAlert();
            _hasOvercrowdingAlert = true;
            _currentLifeTimer = 15f; // tempo extra

            // Efeito visual de perigo (fica piscando vermelho claro)
            if (_capySprite != null)
            {
              _capySprite.DOColor(new Color(1f, 0.6f, 0.6f), 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
          }
          else
          {
            DieFromOvercrowding(); // 15s passaram e ainda tem muita capivara, então morre
          }
        }
        else
        {
          // se o jogador resolveu a superlotação antes dos 15s acabarem 
          if (_hasOvercrowdingAlert)
          {
            _hasOvercrowdingAlert = false;
            if (_capySprite != null)
            {
              _capySprite.DOKill(); // mata o efeito visual de piscar 
              _capySprite.DOColor(Color.white, 0.2f); // volta a cor normal
            }
          }

          _currentLifeTimer = 15f;
        }
      }
    }
  }

  private void DieFromOvercrowding()
  {
    _isDying = true;
    _isMoving = false;
    StopAllTweens();

    // pinta de vermelho
    if (_capySprite != null)
    {
      _capySprite.DOColor(Color.red, 0.4f);
    }

    Sequence deathSeq = DOTween.Sequence();

    // da um pulinho e rotaciona para ficar no Chão
    deathSeq.Append(transform.DOJump(transform.position, 0.5f, 1, 0.5f));
    deathSeq.Join(transform.DORotate(new Vector3(0, 0, 180f), 0.5f).SetEase(Ease.InQuad));

    // afunda um pouco no chão e encolhe até sumir completamente
    deathSeq.Append(transform.DOMoveY(transform.position.y - 1f, 0.4f).SetEase(Ease.InExpo));
    deathSeq.Join(transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InExpo));

    // quando terminar tudo, destroi esse objeto
    deathSeq.OnComplete(() =>
    {
      if (FusionManager.Instance != null)
      {
        FusionManager.Instance.RemoveAndDestroyCapybara(this);
      }
    });
  }

  private CapybaraBehaviors FindNearestSameTier()
  {
    if (FusionManager.Instance == null) return null;

    CapybaraBehaviors nearest = null;
    float minDistance = float.MaxValue;

    foreach (var capy in FusionManager.Instance.ActiveCapybaras)
    {
      if (capy == null) continue;

      // Ignora a si mesma, capivaras em fusão, agarradas ou de nível diferente
      if (capy == this || capy.IsFusing || capy.IsDragged || capy.EvolutionLevel != this.EvolutionLevel)
      {
        continue;
      }

      float dist = Vector2.Distance(transform.position, capy.transform.position);
      if (dist < minDistance)
      {
        minDistance = dist;
        nearest = capy;
      }
    }

    return nearest;
  }

  private void SetNewDestination()
  {
    // define ponto inicial aleatório
    Vector2 randomPoint = _startPos + Random.insideUnitCircle * _movementRadius;

    // Coleta a posição atual e calcula o ponto alvo
    Vector2 currentPos = transform.position;
    Vector2 dir = (randomPoint - currentPos).normalized;
    float distanceToTarget = Vector2.Distance(currentPos, randomPoint);

    // dispara o Raycast para definir a rota
    RaycastHit2D hit = Physics2D.Raycast(currentPos, dir, distanceToTarget, _wallLayer);
    if (hit.collider != null)
    {
      if (hit.distance <= _obstacleOffset)
      {
        _isMoving = false;
        _idleTimer = Random.Range(_minIdleTime, _maxIdleTime);
        return;
      }
      _targetPos = hit.point - (dir * _obstacleOffset); // se tiver parede o caminho traçado, o alvo será um pouco antes da parede
    }
    else
    {
      _targetPos = randomPoint; // se o caminho tiver livre, o alvo é o ponto aleatório
    }

    StopAllTweens();
    _isMoving = true;

    // FIX: capivara olhando pra direção que está se movendo
    if (_capySprite != null)
    {
      if (dir.x < 0) // esquerda
      {
        _isLookingRight = false;
      }
      else if (dir.x > 0) // direita
      {
        _isLookingRight = true;
      }

      _capySprite.flipX = _isLookingRight;
    }

    transform.localRotation = Quaternion.identity;

    // animação de movimentação
    _moveTween = transform.DORotate(new Vector3(0, 0, 10f), 0.15f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
  }

  private void HandleMove()
  {
    Debug.DrawLine(transform.position, _targetPos, Color.blue);

    // Failsafe: Se ela detectar parede a milímetros dela enquanto caminha, ELA PARA NA HORA!
    Vector2 dir = (_targetPos - (Vector2)transform.position).normalized;
    RaycastHit2D instantHit = Physics2D.Raycast(transform.position, dir, 0.15f, _wallLayer);

    if (instantHit.collider != null)
    {
      _isMoving = false;
      _idleTimer = Random.Range(_minIdleTime, _maxIdleTime);
      _moveTween?.Kill();
      transform.DORotate(Vector3.zero, 0.2f);
      return;
    }

    transform.position = Vector2.MoveTowards(transform.position, _targetPos, _speed * Time.deltaTime);

    if (Vector2.Distance(transform.position, _targetPos) < 0.1f)
    {
      ResetMovementState();
    }
  }

  private void HandleIdleTimer()
  {
    _idleTimer -= Time.deltaTime;

    if (_idleTween == null || !_idleTween.IsActive())
    {
      _idleTween = transform.DOScale(new Vector3(_baseScale.x * 1.1f, _baseScale.y * 0.9f, _baseScale.z), 0.5f)
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine);
    }

    if (_idleTimer <= 0)
    {
      SetNewDestination();
    }
  }

  private void ResetMovementState()
  {
    _isMoving = false;
    _idleTimer = Random.Range(_minIdleTime, _maxIdleTime);
    StopAllTweens();
    transform.DORotate(Vector3.zero, 0.2f);
  }

  public void StartIdleAfterFusion()
  {
    _isFusing = false;
    _isMoving = false;
    _startPos = transform.position;
    _idleTimer = Random.Range(_minIdleTime, _maxIdleTime);
    transform.DORotate(Vector3.zero, 0.2f);
  }

  public int GetSortingOrder()
  {
    return _capySprite != null ? _capySprite.sortingOrder : 0;
  }

  public void StopAllTweens()
  {
    _idleTween?.Kill();
    _moveTween?.Kill();
    transform.DOKill();
  }

  private void OnDestroy()
  {
    transform.DOKill();
    // remove da lista ao ser destruído
    if (FusionManager.Instance != null && FusionManager.Instance.ActiveCapybaras.Contains(this))
    {
      FusionManager.Instance.ActiveCapybaras.Remove(this);
    }
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Vector2 center = Application.isPlaying ? _startPos : (Vector2)transform.position;
    Gizmos.DrawWireSphere(center, _movementRadius);

    if (Application.isPlaying && _isMoving)
    {
      Gizmos.color = Color.blue;
      Gizmos.DrawLine(transform.position, _targetPos);
    }
  }
}

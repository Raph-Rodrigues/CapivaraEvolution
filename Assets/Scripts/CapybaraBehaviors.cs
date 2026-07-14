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

  [Header("Configurações de Colisão com a Parede")]
  [SerializeField] private LayerMask _wallLayer;
  [Tooltip("Distância de segurança para não encostar na parede")]
  [SerializeField] private float _obstacleOffset = 0.2f;

  [Header("Sistema de Direção da Sprite")]
  [SerializeField] private bool _isLookingRight = false;
  private SpriteRenderer _capySprite;

  [Header("Debug da Fusão")]
  [SerializeField] private bool _isFusing;
  public bool IsFusing
  {
    get { return _isFusing; }
    set { _isFusing = value; }
  }
  [SerializeField] private bool _isDragged;
  public bool IsDragged
  {
    get { return _isDragged; }
    set
    {
      _isDragged = value;

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
  }

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    _startPos = transform.position;
    _isFusing = false;
  }

  // Update is called once per frame
  void Update()
  {
    if (IsFusing || _isDragged) return;

    if (_isMoving)
    {
      HandleMove();
    }
    else
    {
      HandleIdleTimer();
    }
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

  public void StopAllTweens()
  {
    _idleTween?.Kill();
    _moveTween?.Kill();
    transform.DOKill();
  }

  private void OnDestroy()
  {
    transform.DOKill();
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

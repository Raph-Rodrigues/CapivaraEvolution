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
  [SerializeField] private float _obstacleOffset = 0.5f;

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
        StopAllTweens();
        transform.DOScale(_baseScale * 1.2f, 0.2f).SetEase(Ease.OutBack);
      }
      else
      {
        StopAllTweens();
        transform.DOScale(_baseScale, 0.2f).SetEase(Ease.OutQuad);
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
  private bool _isMoving;
  private float _idleTimer;
  private Tween _idleTween;

  void Awake()
  {
    _baseScale = transform.localScale;
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

    _isMoving = true;

    StopAllTweens();
    transform.DOScale(_baseScale, 0.1f);
  }

  private void HandleMove()
  {
    Debug.DrawLine(transform.position, _targetPos, Color.blue);

    transform.position = Vector2.MoveTowards(transform.position, _targetPos, _speed * Time.deltaTime);
    if (Vector2.Distance(transform.position, _targetPos) < 0.1f)
    {
      _isMoving = false;
      _idleTimer = Random.Range(_minIdleTime, _maxIdleTime); // define aleatóriamente o tempo de descanço
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

  public void StartIdleAfterFusion()
  {
    _isFusing = false;
    _isMoving = false;
    _startPos = transform.position;
    _idleTimer = Random.Range(_minIdleTime, _maxIdleTime);
  }

  public void StopAllTweens()
  {
    _idleTween?.Kill();
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

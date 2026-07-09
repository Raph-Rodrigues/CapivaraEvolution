using UnityEngine;
using DG.Tweening;

public class TutorialCursor : MonoBehaviour
{
    [SerializeField] private RectTransform hand;
    private Camera mainCam;
    private Sequence handSequence;
    private Sequence dragSequence;
    private Transform offCanvasTarget;
    private Vector3 currentOffset;
    public static TutorialCursor Instance {get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        mainCam = Camera.main;
    }

    private void Update()
    {
        if(offCanvasTarget != null && hand.gameObject.activeSelf)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(offCanvasTarget.position);
            hand.position = screenPos + currentOffset;
        }
    }

    public void InsideCanvasPointer(RectTransform UIElement, Vector3 offset = default)
    {
        offCanvasTarget = null;
        hand.SetParent(UIElement);
        hand.anchoredPosition = offset;

        LoopSequence();
    }

    public void OutsideCanvasPointer(Transform target, Vector3 offset = default)
    {
        hand.SetParent(transform);
        offCanvasTarget = target;
        currentOffset = offset;

        LoopSequence();
    }

    public void DragCanvasPointer(Transform startTarget, Transform endTarget)
    {
        offCanvasTarget = null; 
        hand.SetParent(transform);
        
        CursorState(true);
        PlayDragCycle(startTarget, endTarget);
    }

    private void PlayDragCycle(Transform start, Transform end)
    {
        handSequence?.Kill();
        dragSequence?.Kill();

        if (start == null || end == null)
        {
            CursorState(false);
            return;
        }

        dragSequence = DOTween.Sequence();

        dragSequence.AppendCallback(() => 
        {
            if (start != null) 
                hand.position = mainCam.WorldToScreenPoint(start.position);
        });

        dragSequence.Append(hand.DOScale(0.85f, 0.2f))
                    .Join(hand.DORotate(new Vector3(0, 0, 8), 0.2f));

        dragSequence.Append(DOVirtual.Float(0f, 1f, 1.2f, (t) =>
        {
            if (start != null && end != null)
            {
                Vector3 startPos = mainCam.WorldToScreenPoint(start.position);
                Vector3 endPos = mainCam.WorldToScreenPoint(end.position);
                hand.position = Vector3.Lerp(startPos, endPos, t);
            }
        }).SetEase(Ease.InOutSine));

        dragSequence.Append(hand.DOScale(1f, 0.2f))
                    .Join(hand.DORotate(new Vector3(0, 0, 3), 0.2f));

        dragSequence.AppendInterval(0.3f)
                    .OnComplete(() => PlayDragCycle(start, end));
    }
   
    private void LoopSequence()
    {
        handSequence?.Kill();
        dragSequence?.Kill();

        CursorState(true);
        handSequence = DOTween.Sequence();
        handSequence
            .Append(DOTween.Sequence())
                .Join(hand.DOScale(0.85f, 0.2f))
                .Join(hand.DORotate(new Vector3(0, 0, 8), 0.2f))
            .AppendInterval(0.1f)
            .Append(DOTween.Sequence()
                .Join(hand.DOScale(1f, 0.2f)))
                .Join(hand.DORotate(new Vector3(0, 0, 3), 0.2f));
        
        handSequence.SetLoops(-1, LoopType.Restart);

    }

    public void CursorState(bool state)
    {
        hand.gameObject.SetActive(state);

        if (!state)
        {
            handSequence?.Kill();
            dragSequence?.Kill();
            offCanvasTarget = null;
        }
    }
    
}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("Select Effect")]
    private int originalPos;
    //[SerializeField] private float verticalMoveAmount = 0.3f;
    [SerializeField] private float moveTime = 0.1f;
    [Range(0f, 2f), SerializeField] private float scaleAmount = 1.1f;
    //private Vector3 startPos;
    private Vector3 startScale;

    private void Start()
    {
        //startPos = transform.position;
        startScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(SelectionEffect(true));
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(SelectionEffect(false));

    }

    private IEnumerator SelectionEffect (bool startAnimation)
    {
        //startPos = transform.position;
        //Vector3 endPosition;
        Vector3 endScale;

        float elapsedTime = 0f;

        while(elapsedTime <= moveTime)
        {
            elapsedTime += Time.deltaTime;

            if(startAnimation)
            {
                //endPosition = startPos + new Vector3(0f, verticalMoveAmount, 0f);
                endScale = startScale * scaleAmount;
            }

            else
            {
                //endPosition = startPos;
                endScale = startScale;
            }

            //calculos de lerp

            //Vector3 lerpedPos = Vector3.Lerp(transform.position, endPosition, (elapsedTime/moveTime));
            Vector3 lerpedScale = Vector3.Lerp(transform.localScale, endScale, (elapsedTime/moveTime));

            //transform.position = lerpedPos;
            transform.localScale = lerpedScale;

            yield return null;
        }

    }
}
using System.Collections;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Code.Runtime.UI.Combat
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DamageNumberView : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected TextMeshProUGUI damageNumberText;
        [SerializeField] protected AnimationCurve fadeCurve;
        [SerializeField] protected float fadeDuration = 1f;
        [SerializeField] protected float height = 1f;
        [SerializeField, ReadOnly] protected float startTime;
        
        void Start()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            
            damageNumberText.transform.position = transform.position;
            damageNumberText.transform.localScale = Vector3.one;
        }
        

        [ContextMenu("ShowRandomDamageNumber")]
        void ShowRandomDamageNumber() => Show(Random.Range(1, 100));
        
        void Show(float damage)
        {
            damageNumberText.text = $"{damage:F0}";
            damageNumberText.transform.position = transform.position;
            
            StartCoroutine(Animate());
        }
        
        private IEnumerator Animate()
        {
            startTime = Time.time;
            var currentTime = 0f;
            var posChange = height / fadeDuration;

            while (currentTime < 1f)
            {
                yield return new WaitForEndOfFrame();
                currentTime = (Time.time - startTime) / fadeDuration;
                var currentValue = fadeCurve.Evaluate( currentTime );
                
                canvasGroup.alpha = currentValue;
                damageNumberText.transform.localScale = Vector3.one * currentValue;
                damageNumberText.transform.position += Vector3.up * (posChange * Time.deltaTime);
            }
        }
    }
}
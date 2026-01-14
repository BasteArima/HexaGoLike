using UnityEngine;
using System.Collections;
using System.Linq;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private RectTransform _handIcon;
    [SerializeField] private CanvasGroup _handCanvasGroup;
    [SerializeField] private float _inactivityDelay = 2.0f;

    private bool _isCompleted;
    private bool _isDragging;
    private Coroutine _inactivityCoroutine;

    private void Start()
    {
        _handCanvasGroup.alpha = 0;

        StackController.OnDragStarted += OnDragStarted;
        StackController.OnDragCanceled += OnDragCanceled;
        StackController.OnStackPlaced += OnStackPlaced;
        
        StackSpawnController.OnStacksGenerated += OnStacksGeneratedHandler;

        if (StackSpawnController.Instance != null && StackSpawnController.Instance.ActiveStacks.Count > 0)
        {
            StartTutorialSequence();
        }
    }

    private void OnDestroy()
    {
        StackController.OnDragStarted -= OnDragStarted;
        StackController.OnDragCanceled -= OnDragCanceled;
        StackController.OnStackPlaced -= OnStackPlaced;
        StackSpawnController.OnStacksGenerated -= OnStacksGeneratedHandler;
        
        if (_handIcon != null)
            LeanTween.cancel(_handIcon.gameObject);
    }

    private void OnStacksGeneratedHandler()
    {
        StartTutorialSequence();
    }

    private void OnDragStarted()
    {
        if (_isCompleted) return;
        
        _isDragging = true;
        
        StopInactivityTimer();
        HideHand();
    }

    private void OnDragCanceled()
    {
        if (_isCompleted) return;

        _isDragging = false;

        RestartInactivityTimer();
    }

    private void OnStackPlaced(FieldSlot slot)
    {
        CompleteTutorial();
    }

    private void StartTutorialSequence()
    {
        if (_isCompleted || _isDragging) return;

        var allStacks = StackSpawnController.Instance.ActiveStacks;

        if (allStacks == null || allStacks.Count == 0)
        {
            RestartInactivityTimer();
            return;
        }

        var startStack = allStacks.OrderBy(s => s.transform.position.x).ElementAt(allStacks.Count / 2);

        var targetSlot = FieldCreator.Instance.AllSlots
            .Where(s => !s.IsOccupied)
            .OrderBy(s => s.transform.position.sqrMagnitude) 
            .FirstOrDefault();

        if (targetSlot == null)
        {
            RestartInactivityTimer(); 
            return;
        }

        AnimateHand(startStack.transform.position, targetSlot.transform.position);
    }

    private void AnimateHand(Vector3 worldStart, Vector3 worldEnd)
    {
        LeanTween.cancel(_handIcon.gameObject);

        Vector2 screenStart = Camera.main.WorldToScreenPoint(worldStart);
        Vector2 screenEnd = Camera.main.WorldToScreenPoint(worldEnd);

        _handIcon.position = screenStart;
        _handCanvasGroup.alpha = 0f;
        _handIcon.localScale = Vector3.one * 1.5f;

        var seq = LeanTween.sequence();
        seq.append(() => 
        {
            LeanTween.alphaCanvas(_handCanvasGroup, 1f, 0.3f);
            LeanTween.scale(_handIcon, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack);
        });
        seq.append(0.5f);
        seq.append(LeanTween.move(_handIcon.gameObject, screenEnd, 1.0f).setEase(LeanTweenType.easeInOutQuad));
        seq.append(LeanTween.alphaCanvas(_handCanvasGroup, 0f, 0.3f));
        seq.append(0.5f);
        seq.append(StartTutorialSequence); 
    }
    
    private void HideHand()
    {
        LeanTween.cancel(_handIcon.gameObject);
        LeanTween.alphaCanvas(_handCanvasGroup, 0f, 0.2f);
    }

    private void CompleteTutorial()
    {
        _isCompleted = true;
        HideHand();
        StackSpawnController.OnStacksGenerated -= OnStacksGeneratedHandler;
    }

    private void RestartInactivityTimer()
    {
        StopInactivityTimer();
        _inactivityCoroutine = StartCoroutine(WaitAndShowRoutine());
    }

    private void StopInactivityTimer()
    {
        if (_inactivityCoroutine != null)
            StopCoroutine(_inactivityCoroutine);
    }

    private IEnumerator WaitAndShowRoutine()
    {
        yield return new WaitForSeconds(_inactivityDelay);
        StartTutorialSequence();
    }
}
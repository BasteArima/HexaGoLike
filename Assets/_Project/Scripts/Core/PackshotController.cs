using UnityEngine;
using UnityEngine.UI;
using Luna.Unity;

public class PackshotController : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _playNowButton;
    [SerializeField] private Button _fullscreenButton;

    private bool _isShown;

    private void Start()
    {
        _canvasGroup.gameObject.SetActive(false);
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        _playNowButton.onClick.AddListener(OnInstallClicked);
        _fullscreenButton.onClick.AddListener(OnInstallClicked);
    }

    public void Show()
    {
        if (_isShown) return;
        _isShown = true;

        _canvasGroup.gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;

        LeanTween.alphaCanvas(_canvasGroup, 1f, 0.5f)
            .setEase(LeanTweenType.easeInOutQuad);

        LifeCycle.GameEnded();
        Debug.Log("Luna: Game Ended");
    }

    private void OnInstallClicked()
    {
        Playable.InstallFullGame();
        Debug.Log("Luna: Install Full Game clicked");
    }
}
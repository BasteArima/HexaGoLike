using UnityEngine;
using UnityEngine.UI;
using Luna.Unity;
using TMPro;

public class PackshotController : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _playNowButton;
    [SerializeField] private Button _fullscreenButton;

    [SerializeField] private TMP_Text _titleText;

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

    public void Show(bool isWin)
    {
        if (_isShown) return;
        _isShown = true;
        _titleText.text = isWin ? "LEVEL COMPLETE!" : "GAME OVER";

        _canvasGroup.gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;

        LeanTween.alphaCanvas(_canvasGroup, 1f, 0.5f)
            .setEase(LeanTweenType.easeInOutQuad);

        LifeCycle.GameEnded();
        Debug.Log($"Luna: Game Ended. Result: {(isWin ? "Win" : "Lose")}");
    }

    private void OnInstallClicked()
    {
        Playable.InstallFullGame();
        Debug.Log("Luna: Install Full Game clicked");
    }
}
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Collider _collider;

    private MaterialPropertyBlock _propBlock;
    private static readonly int ColorPropertyId = Shader.PropertyToID("_BaseColor");

    public HexagonStack HexagonStack { get; private set; }

    public ColorType Type { get; private set; }

    public void Init(ColorType type, Color visualColor)
    {
        Type = type;
        SetVisualColor(visualColor);
    }

    public void Configure(HexagonStack hexagonStack) => HexagonStack = hexagonStack;
    public void SetParent(Transform parent) => transform.SetParent(parent);
    public void DisableCollider() => _collider.enabled = false;

    private void SetVisualColor(Color color)
    {
        if (_propBlock == null)
            _propBlock = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorPropertyId, color);
        _renderer.SetPropertyBlock(_propBlock);
    }

    public void Vanish(float delay, float duration)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.zero, duration)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(delay)
            .setOnComplete(() => Destroy(gameObject));
    }

    public void MoveToLocal(Vector3 targetLocalPos, float duration)
    {
        LeanTween.cancel(gameObject);
        transform.localRotation = Quaternion.identity;

        var delay = transform.GetSiblingIndex() * 0.01f;

        LeanTween.moveLocal(gameObject, targetLocalPos, duration)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay);

        var diff = targetLocalPos - transform.localPosition;
        diff.y = 0;

        if (diff.sqrMagnitude > 0.001f)
        {
            var direction = diff.normalized;
            var rotationAxis = Vector3.Cross(Vector3.up, direction);

            LeanTween.rotateAroundLocal(gameObject, rotationAxis, 180f, duration)
                .setEase(LeanTweenType.easeInOutSine)
                .setDelay(delay)
                .setOnComplete(() => { transform.localRotation = Quaternion.identity; });
        }
    }
}
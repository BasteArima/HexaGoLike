using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FieldSlot : MonoBehaviour
{
    [SerializeField] private ColorsConfig _colorConfig;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _highlightColor;
    [SerializeField] private float _highlightScaleY;
    [SerializeField] private float _highlightDuration;
    [SerializeField] private Hexagon _hexagonPrefab;
    [SerializeField, OnValueChanged("GenerateInitialHexagons")]
    private ColorType[] _initialStack;

    public List<FieldSlot> Neighbors { get; set; } = new List<FieldSlot>();

    public HexagonStack Stack { get; private set; }
    public bool IsOccupied => Stack != null;

    private Color _originalColor;
    private Vector3 _originalScale;
    private bool _isHighlighted;

    private void Awake()
    {
        _originalColor = _renderer.material.color;
        _originalScale = transform.localScale;
    }

    private void Start()
    {
        BakeNeighbors();
        if (_initialStack.Length > 0)
            GenerateInitialHexagons();
    }

    public void Highlight()
    {
        if (_isHighlighted) return;
        _isHighlighted = true;

        LeanTween.cancel(gameObject);
        LeanTween.color(_renderer.gameObject, _highlightColor, _highlightDuration)
            .setEase(LeanTweenType.easeOutQuad);

        Vector3 targetScale = _originalScale;
        targetScale.y = _highlightScaleY;

        LeanTween.scale(_renderer.gameObject, targetScale, _highlightDuration)
            .setEase(LeanTweenType.easeOutBack);
    }

    public void ResetHighlight()
    {
        if (!_isHighlighted) return;
        _isHighlighted = false;

        LeanTween.cancel(gameObject);


        LeanTween.color(_renderer.gameObject, _originalColor, _highlightDuration)
            .setEase(LeanTweenType.easeOutQuad);

        LeanTween.scale(_renderer.gameObject, _originalScale, _highlightDuration)
            .setEase(LeanTweenType.easeOutQuad);
    }

    private void GenerateInitialHexagons()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.GetComponent<HexagonStack>() || child.GetComponent<Hexagon>())
                DestroyImmediate(child.gameObject);
        }

        Stack = new GameObject("Initial Stack").AddComponent<HexagonStack>();
        Stack.transform.SetParent(transform);
        Stack.transform.localPosition = Vector3.up * .2f;

        for (int i = 0; i < _initialStack.Length; i++)
        {
            var spawnPosition = Stack.transform.TransformPoint(Vector3.up * i * .2f);
            var hexagonInstance = Instantiate(_hexagonPrefab, spawnPosition, Quaternion.identity);

            var type = _initialStack[i];
            var visualColor = _colorConfig.GetColor(type);

            hexagonInstance.Init(type, visualColor);

            Stack.Add(hexagonInstance);
        }
    }

    public void AssignStack(HexagonStack stack) => Stack = stack;

    public void BakeNeighbors()
    {
        Neighbors.Clear();
        var colliders = Physics.OverlapSphere(transform.position, 1.4f);
        foreach (var col in colliders)
        {
            var slot = col.GetComponent<FieldSlot>();
            if (slot != null && slot != this) Neighbors.Add(slot);
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var neighbor in Neighbors)
            if (neighbor != null)
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
    }

    #if UNITY_EDITOR
    [SerializeField] private ColorType _fastColorType;
    [Button]
    private void FastSetColor()
    {
        for (var index = 0; index < _initialStack.Length; index++)
            _initialStack[index] = _fastColorType;
        GenerateInitialHexagons();
    }
    #endif
}
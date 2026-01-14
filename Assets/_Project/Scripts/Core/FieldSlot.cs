using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FieldSlot : MonoBehaviour
{
    [SerializeField] private Hexagon _hexagonPrefab;
    [SerializeField] private ColorsConfig _colorConfig;
    
    [SerializeField, OnValueChanged("GenerateInitialHexagons")]
    private ColorType[] _hexagonTypes; 
    
    public List<FieldSlot> Neighbors { get; set; } = new List<FieldSlot>();

    public HexagonStack Stack { get; private set; }
    public bool IsOccupied => Stack != null;

    public void Init(ColorsConfig config)
    {
        _colorConfig = config;
        BakeNeighbors();
    }
    
    private void Start()
    {
        BakeNeighbors();
        if (_hexagonTypes.Length > 0)
            GenerateInitialHexagons();
    }

    private void GenerateInitialHexagons()
    {
        if (_colorConfig == null) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.GetComponent<HexagonStack>() || child.GetComponent<Hexagon>())
                DestroyImmediate(child.gameObject);
        }

        Stack = new GameObject("Initial Stack").AddComponent<HexagonStack>();
        Stack.transform.SetParent(transform);
        Stack.transform.localPosition = Vector3.up * .2f;

        for (int i = 0; i < _hexagonTypes.Length; i++)
        {
            var spawnPosition = Stack.transform.TransformPoint(Vector3.up * i * .2f);
            var hexagonInstance = Instantiate(_hexagonPrefab, spawnPosition, Quaternion.identity);
            
            var type = _hexagonTypes[i];
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
            if (neighbor != null) Gizmos.DrawLine(transform.position, neighbor.transform.position);
    }
}
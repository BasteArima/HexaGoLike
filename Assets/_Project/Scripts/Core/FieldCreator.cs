using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

public class FieldCreator : MonoBehaviour
{
    public static FieldCreator Instance { get; private set; }
    
    [SerializeField] private ColorsConfig _colorConfig;
    [SerializeField] private Grid _grid;
    [SerializeField] private FieldSlot _fieldSlotPrefab;
    [SerializeField] private Transform _fieldSlotsParent;
    [SerializeField, OnValueChanged("CreateField")]
    private int _fieldSize;
    [SerializeField] private List<FieldSlot> _fieldSlots;

    public List<FieldSlot> AllSlots => _fieldSlots;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void CreateField()
    {
        ClearField();

        for (int x = -_fieldSize; x <= _fieldSize; x++)
        {
            for (int y = -_fieldSize; y <= _fieldSize; y++)
            {
                var spawnPos = _grid.CellToWorld(new Vector3Int(x, y, 0));
                if (spawnPos.magnitude > _grid.CellToWorld(new Vector3Int(1, 0, 0)).magnitude * _fieldSize)
                    continue;

                var fieldSlot = Instantiate(_fieldSlotPrefab, _fieldSlotsParent);
                fieldSlot.transform.position = spawnPos;
                fieldSlot.Init(_colorConfig);
                
                _fieldSlots.Add(fieldSlot);
            }
        }

        BakeNeighbors();
    }

    [Button]
    private void ClearField()
    {
        _fieldSlots ??= new List<FieldSlot>();

        foreach (var fieldSlot in _fieldSlots)
        {
            DestroyImmediate(fieldSlot.gameObject);
        }

        _fieldSlots.Clear();
    }

    [Button]
    private void BakeNeighbors()
    {
        foreach (var fieldSlot in _fieldSlots)
            fieldSlot.BakeNeighbors();
    }
}
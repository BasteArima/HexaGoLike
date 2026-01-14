using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

public class FieldCreator : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _fieldSlotPrefab;
    [SerializeField] private Transform _fieldSlotsParent;
    [SerializeField] private int _fieldSize;

    [SerializeField, ReadOnly] private List<GameObject> _fieldSlots;
    
    [Button]
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
                _fieldSlots.Add(fieldSlot);
            }
        }
    }

    private void ClearField()
    {
        _fieldSlots ??= new List<GameObject>();

        foreach (var fieldSlot in _fieldSlots)
        {
            DestroyImmediate(fieldSlot);
        }
        
        _fieldSlots.Clear();
    }
}

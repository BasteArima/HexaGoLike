using System.Collections.Generic;
using UnityEngine;

public class HexagonStack : MonoBehaviour
{
    public List<Hexagon> Hexagons { get; private set; }
    private FieldSlot _currentSlot;

    public void Add(Hexagon hexagon)
    {
        Hexagons ??= new List<Hexagon>();
        Hexagons.Add(hexagon);
        hexagon.SetParent(transform);
        _currentSlot = transform.GetComponentInParent<FieldSlot>();
    }

    public void Place()
    {
        if (Hexagons == null) return;
        foreach (var hexagon in Hexagons)
            hexagon.DisableCollider();
        _currentSlot = transform.GetComponentInParent<FieldSlot>();
    }

    public void TryDestroy()
    {
        if (Hexagons == null || Hexagons.Count <= 0)
        {
            if (_currentSlot != null)
            {
                _currentSlot.ClearStackReference();
            }

            Destroy(gameObject);
        }
    }

    public ColorType GetTopHexagonType()
    {
        if (Hexagons == null || Hexagons.Count == 0) return ColorType.White;
        return Hexagons[Hexagons.Count - 1].Type;
    }

    public void Remove(Hexagon hexagon) => Hexagons.Remove(hexagon);

    public bool Contains(Hexagon hexagon) => Hexagons != null && Hexagons.Contains(hexagon);
}
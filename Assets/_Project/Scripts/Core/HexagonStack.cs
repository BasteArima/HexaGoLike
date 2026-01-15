using System.Collections.Generic;
using UnityEngine;

public class HexagonStack : MonoBehaviour
{
    public List<Hexagon> Hexagons { get; private set; }

    public void Add(Hexagon hexagon)
    {
        Hexagons ??= new List<Hexagon>();
        Hexagons.Add(hexagon);
        hexagon.SetParent(transform);
    }

    public void Place()
    {
        if (Hexagons == null) return;
        foreach (Hexagon hexagon in Hexagons)
            hexagon.DisableCollider();
    }

    public void Remove(Hexagon hexagon)
    {
        Hexagons.Remove(hexagon);
        if (Hexagons.Count <= 0)
            DestroyImmediate(gameObject);
    }

    public bool Contains(Hexagon hexagon) => Hexagons != null && Hexagons.Contains(hexagon);

    public ColorType GetTopHexagonType()
    {
        if (Hexagons == null || Hexagons.Count == 0) return ColorType.White;
        return Hexagons[Hexagons.Count - 1].Type; 
    }
}
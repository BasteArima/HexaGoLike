using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MergeController : MonoBehaviour
{
    public static bool IsMerging { get; private set; }
    private const float BASE_DURATION = 0.25f;
    
    [SerializeField] private PackshotController _packshot;
    [SerializeField] private int _numsOfHexToMerge = 10;

    private List<FieldSlot> _updatedSlots = new List<FieldSlot>();
    private float _speedMultiplier = 1f;

    private void Awake() => StackController.OnStackPlaced += StackPlacedCallback;
    private void OnDestroy() => StackController.OnStackPlaced -= StackPlacedCallback;

    private void StackPlacedCallback(FieldSlot fieldSlot) => StartCoroutine(StackPlacedCoroutine(fieldSlot));

    private IEnumerator StackPlacedCoroutine(FieldSlot fieldSlot)
    {
        IsMerging = true;
        _speedMultiplier = 1f;
        _updatedSlots.Add(fieldSlot);

        while (_updatedSlots.Count > 0)
            yield return CheckForMerge(_updatedSlots[0]);

        IsMerging = false;
    }

    private IEnumerator CheckForMerge(FieldSlot fieldSlot)
    {
        _updatedSlots.Remove(fieldSlot);

        if (!fieldSlot.IsOccupied) yield break;

        var neighborFieldSlots = GetOccupiedNeighbors(fieldSlot);
        if (neighborFieldSlots.Count <= 0) yield break;

        ColorType topType = fieldSlot.Stack.GetTopHexagonType();

        var similarNeighborFieldSlots = GetSimilarNeighborFieldSlots(topType, neighborFieldSlots.ToArray());

        if (similarNeighborFieldSlots.Count <= 0) yield break;

        _updatedSlots.AddRange(similarNeighborFieldSlots);

        var hexagonsToAdd = GetHexagonsToAdd(topType, similarNeighborFieldSlots.ToArray());
        RemoveHexagonsFromStacks(hexagonsToAdd, similarNeighborFieldSlots.ToArray());

        float currentDuration = BASE_DURATION / _speedMultiplier;
        MoveHexagons(fieldSlot, hexagonsToAdd, currentDuration);

        yield return new WaitForSeconds(currentDuration + (hexagonsToAdd.Count * 0.01f));

        yield return CheckForCompleteStack(fieldSlot, topType);

        _speedMultiplier *= 1.3f;
        if (!AreMovesAvailable()) 
        {
            Debug.Log("Game Over");
            _packshot.Show();
        }
    }

    private List<FieldSlot> GetSimilarNeighborFieldSlots(ColorType targetType, FieldSlot[] neighborFieldSlots)
    {
        var similar = new List<FieldSlot>();
        foreach (var neighbor in neighborFieldSlots)
        {
            if (neighbor.Stack.GetTopHexagonType() == targetType)
                similar.Add(neighbor);
        }
        return similar;
    }

    private List<Hexagon> GetHexagonsToAdd(ColorType targetType, FieldSlot[] similarNeighborFieldSlots)
    {
        var hexagonsToAdd = new List<Hexagon>();
        foreach (var neighborSlot in similarNeighborFieldSlots)
        {
            var stack = neighborSlot.Stack;
            for (int i = stack.Hexagons.Count - 1; i >= 0; i--)
            {
                var hex = stack.Hexagons[i];
                if (hex.Type != targetType) break;

                hexagonsToAdd.Add(hex);
                hex.SetParent(null);
            }
        }
        return hexagonsToAdd;
    }

    private IEnumerator CheckForCompleteStack(FieldSlot fieldSlot, ColorType targetType)
    {
        if (fieldSlot.Stack.Hexagons.Count < _numsOfHexToMerge) yield break;

        var similarHexagons = new List<Hexagon>();
        for (int i = fieldSlot.Stack.Hexagons.Count - 1; i >= 0; i--)
        {
            var hex = fieldSlot.Stack.Hexagons[i];
            if (hex.Type != targetType) break;
            similarHexagons.Add(hex);
        }

        if (similarHexagons.Count < _numsOfHexToMerge) yield break;

        float delay = 0;
        float duration = BASE_DURATION / _speedMultiplier;

        while (similarHexagons.Count > 0)
        {
            var hex = similarHexagons[0];
            hex.SetParent(null);
            hex.Vanish(delay, duration);
            delay += 0.01f / _speedMultiplier;
            fieldSlot.Stack.Remove(hex);
            similarHexagons.RemoveAt(0);
        }

        _updatedSlots.Add(fieldSlot);
        yield return new WaitForSeconds(duration + delay);
    }

    private void MoveHexagons(FieldSlot fieldSlot, List<Hexagon> hexagonsToAdd, float duration)
    {
        float initialY = fieldSlot.Stack.Hexagons.Count * 0.2f;
        for (int i = 0; i < hexagonsToAdd.Count; i++)
        {
            var hexagon = hexagonsToAdd[i];
            var targetY = initialY + i * 0.2f;
            var targetLocalPosition = Vector3.up * targetY;
            fieldSlot.Stack.Add(hexagon);
            hexagon.MoveToLocal(targetLocalPosition, duration);
        }
    }
    
    private List<FieldSlot> GetOccupiedNeighbors(FieldSlot fieldSlot)
    {
        var list = new List<FieldSlot>();
        foreach (var neighbor in fieldSlot.Neighbors)
        {
            if (neighbor != null && neighbor.IsOccupied) list.Add(neighbor);
        }
        return list;
    }
    
    private void RemoveHexagonsFromStacks(List<Hexagon> hexagonsToAdd, FieldSlot[] similarNeighborFieldSlots)
    {
        foreach (var neighborSlot in similarNeighborFieldSlots)
        {
            var stack = neighborSlot.Stack;
            foreach (var hexagon in hexagonsToAdd)
            {
                if (stack.Contains(hexagon)) stack.Remove(hexagon);
            }
        }
    }

    private bool AreMovesAvailable()
    {
        var allFieldSlots = FieldCreator.Instance.AllSlots;

        foreach (var fieldSlot in allFieldSlots)
            if (!fieldSlot.IsOccupied) return true;

        foreach (var cell in allFieldSlots)
        {
            if (cell.IsOccupied)
            {
                var type = cell.Stack.GetTopHexagonType();
                var neighbors = GetOccupiedNeighbors(cell);
                var similar = GetSimilarNeighborFieldSlots(type, neighbors.ToArray());
                if (similar.Count > 0) return true;
            }
        }

        Debug.Log($"Moves not available");
        return false;
    }
}
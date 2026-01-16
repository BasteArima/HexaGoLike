using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MergeController : MonoBehaviour
{
    public static bool IsMerging { get; private set; }
    public static event Action OnMergeOccurred;

    [SerializeField] private PackshotController _packshot;
    [SerializeField] private int _numsOfHexToMerge;
    [SerializeField] private float _mergeBaseDuration;

    private List<FieldSlot> _updatedSlots = new List<FieldSlot>();
    private float _speedMultiplier = 1f;

    private void Awake() => StackController.OnStackPlaced += StackPlacedCallback;
    private void OnDestroy() => StackController.OnStackPlaced -= StackPlacedCallback;

    private void StackPlacedCallback(FieldSlot fieldSlot) => StartCoroutine(StackPlacedCoroutine(fieldSlot));

    private IEnumerator StackPlacedCoroutine(FieldSlot initialSlot)
    {
        IsMerging = true;
        _speedMultiplier = 1f;
        _updatedSlots.Add(initialSlot);

        while (_updatedSlots.Count > 0)
        {
            var currentSlot = _updatedSlots[0];
            _updatedSlots.RemoveAt(0);

            if (!currentSlot.IsOccupied)
            {
                continue;
            }

            var neighborFieldSlots = GetOccupiedNeighbors(currentSlot);
            if (neighborFieldSlots.Count <= 0) continue;

            var topType = currentSlot.Stack.GetTopHexagonType();
            var similarNeighbors = GetSimilarNeighborFieldSlots(topType, neighborFieldSlots.ToArray());

            if (similarNeighbors.Count <= 0) continue;

            FieldSlot targetForMove = null;
            List<Hexagon> hexesToMove = null;

            if (similarNeighbors.Count > 1)
            {
                _updatedSlots.AddRange(similarNeighbors);
                _updatedSlots.Add(currentSlot);

                hexesToMove = GetHexagonsFromNeighbors(topType, similarNeighbors.ToArray());
                RemoveHexagonsFromStacks(hexesToMove, similarNeighbors.ToArray());
                targetForMove = currentSlot;
            }
            else
            {
                var targetNeighbor = similarNeighbors[0];
                hexesToMove = GetHexagonsFromSingleStack(currentSlot.Stack, topType);

                RemoveHexagonsFromSingleStack(hexesToMove, currentSlot.Stack);

                _updatedSlots.Add(targetNeighbor);
                _updatedSlots.Add(currentSlot);
                targetForMove = targetNeighbor;
            }

            var currentDuration = _mergeBaseDuration / _speedMultiplier;
            MoveHexagons(targetForMove, hexesToMove, currentDuration);

            var moveTimer = 0f;
            var moveTotalTime = currentDuration + (hexesToMove.Count * 0.01f);
            while (moveTimer < moveTotalTime)
            {
                moveTimer += Time.deltaTime;
                yield return null;
            }

            yield return CheckForCompleteStack(targetForMove, topType);

            _speedMultiplier *= 1.3f;

            yield return null;
        }

        IsMerging = false;
        CheckGameEndState();
    }

    private IEnumerator CheckForCompleteStack(FieldSlot fieldSlot, ColorType targetType)
    {
        if (fieldSlot.Stack == null) yield break;

        var stackToProcess = fieldSlot.Stack;
        int totalCount = stackToProcess.Hexagons.Count;

        if (totalCount < _numsOfHexToMerge) yield break;

        var similarHexagons = GetHexagonsFromSingleStack(stackToProcess, targetType);
        if (similarHexagons.Count < _numsOfHexToMerge) yield break;

        float delay = 0;
        float duration = _mergeBaseDuration / _speedMultiplier;

        OnMergeOccurred?.Invoke();

        while (similarHexagons.Count > 0)
        {
            var hex = similarHexagons[0];
            hex.SetParent(null);

            LeanTween.cancel(hex.gameObject);
            LeanTween.scale(hex.gameObject, Vector3.zero, duration)
                .setEase(LeanTweenType.easeInBack)
                .setDelay(delay);

            delay += 0.01f / _speedMultiplier;
            stackToProcess.Remove(hex);
            similarHexagons.RemoveAt(0);
        }

        float animTimer = 0f;
        float animTotalTime = duration + delay;
        while (animTimer < animTotalTime)
        {
            animTimer += Time.deltaTime;
            yield return null;
        }

        stackToProcess.TryDestroy();

        yield return null;

        if (fieldSlot.IsOccupied)
        {
            _updatedSlots.Add(fieldSlot);
        }
    }

    private void CheckGameEndState()
    {
        if (IsFieldEmpty())
        {
            _packshot.Show(true);
            return;
        }

        bool hasStacks = StackSpawnController.Instance.ActiveStacks.Count > 0;
        bool hasWaves = StackSpawnController.Instance.HasWavesLeft;
        bool canMove = AreMovesAvailable();

        if (!hasStacks && !hasWaves)
        {
            _packshot.Show(false);
            return;
        }

        if (!canMove)
        {
            _packshot.Show(false);
            return;
        }
    }

    private bool IsFieldEmpty()
    {
        foreach (var slot in FieldCreator.Instance.AllSlots)
            if (slot.IsOccupied)
                return false;
        return true;
    }

    private List<Hexagon> GetHexagonsFromNeighbors(ColorType targetType, FieldSlot[] neighbors)
    {
        var list = new List<Hexagon>();
        foreach (var slot in neighbors) list.AddRange(GetHexagonsFromSingleStack(slot.Stack, targetType));
        return list;
    }

    private List<Hexagon> GetHexagonsFromSingleStack(HexagonStack stack, ColorType targetType)
    {
        var list = new List<Hexagon>();
        if (stack == null || stack.Hexagons == null) return list;
        for (int i = stack.Hexagons.Count - 1; i >= 0; i--)
        {
            var hex = stack.Hexagons[i];
            if (hex.Type != targetType) break;
            list.Add(hex);
        }

        return list;
    }

    private void RemoveHexagonsFromStacks(List<Hexagon> hexs, FieldSlot[] slots)
    {
        foreach (var slot in slots) RemoveHexagonsFromSingleStack(hexs, slot.Stack);
    }

    private void RemoveHexagonsFromSingleStack(List<Hexagon> hexs, HexagonStack stack)
    {
        if (stack == null) return;
        foreach (var h in hexs)
        {
            if (stack.Contains(h))
            {
                h.SetParent(null);
                stack.Remove(h);
            }
        }

        stack.TryDestroy();
    }

    private List<FieldSlot> GetSimilarNeighborFieldSlots(ColorType targetType, FieldSlot[] neighborFieldSlots)
    {
        var similar = new List<FieldSlot>();
        foreach (var neighbor in neighborFieldSlots)
        {
            if (neighbor.Stack.GetTopHexagonType() == targetType) similar.Add(neighbor);
        }

        return similar;
    }

    private void MoveHexagons(FieldSlot targetSlot, List<Hexagon> hexagonsToAdd, float duration)
    {
        float initialY = targetSlot.Stack.Hexagons.Count * 0.2f;
        for (int i = 0; i < hexagonsToAdd.Count; i++)
        {
            var hexagon = hexagonsToAdd[i];
            var targetY = initialY + i * 0.2f;
            var targetLocalPosition = Vector3.up * targetY;
            targetSlot.Stack.Add(hexagon);
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

    private bool AreMovesAvailable()
    {
        var allFieldSlots = FieldCreator.Instance.AllSlots;
        foreach (var fieldSlot in allFieldSlots)
            if (!fieldSlot.IsOccupied)
                return true;
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

        return false;
    }
}
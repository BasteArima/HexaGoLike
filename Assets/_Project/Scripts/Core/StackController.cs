using System;
using UnityEngine;

public class StackController : MonoBehaviour
{
    public static Action<FieldSlot> OnStackPlaced;
    public static Action OnDragStarted;
    public static Action OnDragCanceled;

    [SerializeField] private LayerMask _hexagonLayerMask;
    [SerializeField] private LayerMask _fieldSlotLayerMask;
    [SerializeField] private LayerMask _groundLayerMask;

    private HexagonStack _currentStack;
    private Vector3 _currentStackInitialPos;
    private FieldSlot _targetSlot;

    private void Update()
    {
        ManageControl();
    }

    private void ManageControl()
    {
        if (MergeController.IsMerging) return;

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleInputDown(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (_currentStack != null)
                        HandleInputDrag(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (_currentStack != null)
                        HandleInputUp();
                    break;
            }
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        else
        {
            if (Input.GetMouseButtonDown(0))
                HandleInputDown(Input.mousePosition);
            else if (Input.GetMouseButton(0) && _currentStack != null)
                HandleInputDrag(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0) && _currentStack != null)
                HandleInputUp();
        }
#endif
    }

    private void HandleInputDown(Vector2 screenPosition)
    {
        Physics.Raycast(GetRay(screenPosition), out var hit, 500, _hexagonLayerMask);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<Hexagon>(out var hexagon))
            {
                _currentStack = hexagon.HexagonStack;
                _currentStackInitialPos = _currentStack.transform.position;
                OnDragStarted?.Invoke();
            }
        }
    }

    private void HandleInputDrag(Vector2 screenPosition)
    {
        var ray = GetRay(screenPosition);
        Physics.Raycast(ray, out var hit, 500, _fieldSlotLayerMask);

        if (hit.collider == null)
            DraggingAboveGround(ray);
        else
            DraggingAboveGridCell(hit, ray);
    }

    private void DraggingAboveGround(Ray ray)
    {
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 500, _groundLayerMask);

        if (hit.collider == null)
        {
            return;
        }

        var currentStackTargetPos = hit.point.With(y: 2);

        _currentStack.transform.position = Vector3.MoveTowards(
            _currentStack.transform.position,
            currentStackTargetPos,
            Time.deltaTime * 30);

        _targetSlot = null;
    }

    private void DraggingAboveGridCell(RaycastHit hit, Ray ray)
    {
        var gridCell = hit.collider.GetComponent<FieldSlot>();

        if (gridCell.IsOccupied)
            DraggingAboveGround(ray);
        else
            DraggingAboveNonOccupiedGridCell(gridCell);
    }

    private void DraggingAboveNonOccupiedGridCell(FieldSlot fieldSlot)
    {
        var currentStackTargetPos = fieldSlot.transform.position.With(y: 2);

        _currentStack.transform.position = Vector3.MoveTowards(
            _currentStack.transform.position,
            currentStackTargetPos,
            Time.deltaTime * 30);

        _targetSlot = fieldSlot;
    }

    private void HandleInputUp()
    {
        if (_targetSlot == null)
        {
            _currentStack.transform.position = _currentStackInitialPos;
            _currentStack = null;
            OnDragCanceled?.Invoke();
            return;
        }

        _currentStack.transform.position = _targetSlot.transform.position.With(y: .2f);
        _currentStack.transform.SetParent(_targetSlot.transform);
        _currentStack.Place();

        _targetSlot.AssignStack(_currentStack);

        OnStackPlaced?.Invoke(_targetSlot);

        _targetSlot = null;
        _currentStack = null;
    }

    private Ray GetRay(Vector2 screenPos) => Camera.main.ScreenPointToRay(screenPos);
}
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// モバイルタッチおよびマウス入力を検知し、ゲーム内のアクション（移動予約、カード使用）をトリガーするクラス。
/// </summary>
[RequireComponent(typeof(UnityEngine.Transform))]
public class InputHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask gridLayerMask;

    private GameManager _gameManager;
    private GridManager _gridManager;
    private CardManager _cardManager;

    private void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _gridManager = FindFirstObjectByType<GridManager>();
        _cardManager = FindFirstObjectByType<CardManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, gridLayerMask))
        {
            GridCell cell = hit.collider.GetComponent<GridCell>();
            if (cell == null)
                return;

            Vector2Int gridPos = cell.GridPosition;

            switch (GameManager.CurrentPhase)
            {
                case GamePhase.Spawn:
                    if (_cardManager.IsCardSelected)
                    {
                        _cardManager.UseSelectedCard(gridPos);
                    }
                    break;

                case GamePhase.MoveReservation:
                    ProcessMoveReservation(gridPos);
                    break;
            }
        }
    }

    private void ProcessMoveReservation(Vector2Int targetPos)
    {
        if (_gridManager.SelectedUnit == null)
        {
            var unit = _gridManager.GetUnitAt(targetPos);
            if (unit != null && unit.IsPlayerOwned)
            {
                _gridManager.SelectUnit(unit);
            }
        }
        else
        {
            if (_gridManager.IsValidMove(targetPos))
            {
                _gridManager.ReserveMove(_gridManager.SelectedUnit, targetPos);
                _gridManager.DeselectUnit();
            }
            else
            {
                _gridManager.DeselectUnit();
            }
        }
    }
}
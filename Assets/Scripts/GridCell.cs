// Responsibility: Represents an individual grid cell, stores its coordinate on the grid, and handles direct click input via OnMouseDown.
// Attachment Note: Attach to GridCell prefabs.

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// グリッド上の各セル（マス）にアタッチされ、自身の座標情報を保持し、クリック入力を処理するコンポーネント。
/// GridCellのプレハブにアタッチしてください。
/// </summary>
[RequireComponent(typeof(Collider))]
public class GridCell : MonoBehaviour
{
    [SerializeField] private Vector2Int gridPosition;

    public Vector2Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }

    private GameManager _gameManager;
    private GridManager _gridManager;
    private CardManager _cardManager;

    private void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _gridManager = FindFirstObjectByType<GridManager>();
        _cardManager = FindFirstObjectByType<CardManager>();
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        switch (GameManager.CurrentPhase)
        {
            case GameManager.GamePhase.Spawn:
                if (_cardManager != null && _cardManager.IsCardSelected)
                {
                    _cardManager.UseSelectedCard(gridPosition);
                }
                break;

            case GameManager.GamePhase.MoveReservation:
                ProcessMoveReservation(gridPosition);
                break;
        }
    }

    private void ProcessMoveReservation(Vector2Int targetPos)
    {
        if (_gridManager == null)
            return;

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

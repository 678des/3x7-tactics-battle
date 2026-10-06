using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// モバイルタッチおよびマウス入力を検知し、ゲーム内のアクション（移動予約、カード使用）をトリガーするクラス。
/// </summary>
public class InputHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask gridLayerMask;

    private GameManager _gameManager;
    private GridManager _gridManager;
    private CardManager _cardManager;

    private void Awake()
    {
        // シーン内の主要マネージャーをキャッシュ
        _gameManager = FindFirstObjectByType<GameManager>();
        _gridManager = FindFirstObjectByType<GridManager>();
        _cardManager = FindFirstObjectByType<CardManager>();
    }

    private void Update()
    {
        // 入力処理は現在のフェーズに応じて分岐
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        // UIクリックを無視
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, gridLayerMask))
        {
            // ヒットしたグリッド座標を取得
            Vector2Int gridPos = hit.collider.GetComponent<GridCell>().GridPosition;

            switch (_gameManager.CurrentPhase)
            {
                case GamePhase.Spawn:
                    // カード選択中であれば召喚処理
                    if (_cardManager.IsCardSelected)
                    {
                        _cardManager.UseSelectedCard(gridPos);
                    }
                    break;

                case GamePhase.MoveReservation:
                    // キャラクター選択または移動予約処理
                    ProcessMoveReservation(gridPos);
                    break;
            }
        }
    }

    /// <summary>
    /// 移動予約フェーズにおける入力処理
    /// </summary>
    private void ProcessMoveReservation(Vector2Int targetPos)
    {
        // 既に選択中のユニットがあるか確認
        if (_gridManager.SelectedUnit == null)
        {
            // ユニットを選択
            var unit = _gridManager.GetUnitAt(targetPos);
            if (unit != null && unit.IsPlayerOwned)
            {
                _gridManager.SelectUnit(unit);
            }
        }
        else
        {
            // 移動先として予約
            if (_gridManager.IsValidMove(targetPos))
            {
                _gridManager.ReserveMove(_gridManager.SelectedUnit, targetPos);
                _gridManager.DeselectUnit();
            }
            else
            {
                // 選択解除
                _gridManager.DeselectUnit();
            }
        }
    }
}

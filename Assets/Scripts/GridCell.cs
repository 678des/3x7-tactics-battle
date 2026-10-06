using UnityEngine;

/// <summary>
/// グリッド上の各セル（マス）にアタッチされ、自身の座標情報を保持するコンポーネント。
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
}
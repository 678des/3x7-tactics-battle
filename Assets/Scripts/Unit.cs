// Responsibility: Represents an in-game unit that handles player clicks during the Planning phase to schedule movement via GridManager.
// Attachment Note: Attach this script to Unit GameObjects that have a Collider for mouse clicking.

using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum ElementType { Fire, Water, Grass }


    [Header("Unit Status")]
    [SerializeField] private ElementType element = ElementType.Fire;
    [SerializeField] private int power = 10;
    private Material _materialInstance;

    [Header("Visual Settings")]
    [SerializeField] private MeshRenderer _unitMesh;
    private bool _isSelected = false;

    //[SerializeField] private Color _selectionColor = Color.cyan;
    //private float _minIntensity = 0f;
    //private float _maxIntensity = 1.0f;
    //private float _pulseSpeed = 4.0f; // 点滅の速さ

    public bool IsPlayerOwned = true;
    public ElementType Element => element;
    public Vector2Int CurrentPosition { get; set; }

    public int CurrentPower
    {
        get => power;
        set => power = Mathf.Max(0, value);
    }

    public Vector2Int? PlannedDestination { get; set; } = null;

    [Header("Ranges (Relative Offsets)")]
    [Tooltip("移動できるマスの相対座標リスト")]
    public List<Vector2Int> MovePattern = new List<Vector2Int>
    {
        new Vector2Int(0,0),
        new Vector2Int( 0,  1), // 上
        new Vector2Int( 0, -1), // 下
        new Vector2Int( 1,  0), // 右
        new Vector2Int(-1,  0), // 左
        new Vector2Int( 1,  1), // 右上
        new Vector2Int(-1,  1), // 左上
        new Vector2Int( 1, -1), // 右下
        new Vector2Int(-1, -1)  // 左下
    };

    [Tooltip("攻撃できるマスの相対座標リスト")]
    public List<Vector2Int> AttackPattern = new List<Vector2Int>
    {
        new Vector2Int(0,0),
        new Vector2Int( 0,  1), // 上
        new Vector2Int( 0, -1), // 下
        new Vector2Int( 1,  0), // 右
        new Vector2Int(-1,  0), // 左
        new Vector2Int( 1,  1), // 右上
        new Vector2Int(-1,  1), // 左上
        new Vector2Int( 1, -1), // 右下
        new Vector2Int(-1, -1)  // 左下
    };

    [Header("Visual Settings")]
    [SerializeField] private Material _defaultMaterial;   // 通常時のマテリアル
    [SerializeField] private Material _selectedMaterial;  // 選択時の光るマテリアル

    private void Awake()
    {

        if (_unitMesh == null) _unitMesh = GetComponent<MeshRenderer>();
        if (_unitMesh != null) _materialInstance = _unitMesh.material;

    }
    private void Update()
    {
        //// 選択されている間、毎フレーム発光の強さを脈動させる
        //if (_isSelected && _materialInstance != null)
        //{
        //    // サイン波を使って強度を滑らかに往復させる (-1〜1 を 0〜1 に変換しつつ倍率調整)
        //    float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f;
        //    float currentIntensity = Mathf.Lerp(_minIntensity, _maxIntensity, t);

        //    _materialInstance.SetColor("_EmissionColor", _selectionColor * currentIntensity);
        //}
    }


    /// <summary>
    /// このユニットが選択されたかどうかに応じて、見た目を更新する。
    /// GridManagerから呼び出される。
    /// </summary>
    /// <param name="isSelected">選択された場合はtrue, そうでない場合はfalse</param>
    public void SetSelectionState(bool isSelected)
    {
        if (_unitMesh != null && _selectedMaterial != null && _defaultMaterial != null)
        {
            _isSelected = isSelected;
            //if (_materialInstance == null) return;

            //if (_isSelected)
            //{
            //    _materialInstance.EnableKeyword("_EMISSION");
            //    _materialInstance.SetColor("_EmissionColor", _selectionColor * _maxIntensity);
            //}
            //else
            //{
            //    _materialInstance.DisableKeyword("_EMISSION");
            //    _materialInstance.SetColor("_EmissionColor", Color.black);
            //}
        }
    }


    public void ClearDestination()
    {
        PlannedDestination = null;
    }

    public List<Vector2Int> GetAttackablePositions(Vector2Int currentPos)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var offset in AttackPattern)
        {
            Vector2Int targetPos = currentPos + offset;
            result.Add(targetPos);
        }

        return result;
    }
    public List<Vector2Int> GetMovalePositions(Vector2Int currentPos)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var offset in MovePattern)
        {
            Vector2Int targetPos = currentPos + offset;
            if (targetPos.y >= GridManager.Rows || targetPos.x >= GridManager.Cols || targetPos.y < 0 || targetPos.x < 0) continue;
            result.Add(targetPos);
        }
        return result;
    }

    /// <summary>
    /// 指定した位置（myPos）にいるとき、相手の位置（targetPos）を攻撃できるか？
    /// </summary>
    public bool CanAttackTarget(Vector2Int myPos, Vector2Int targetPos)
    {
        List<Vector2Int> attackableCells = GetAttackablePositions(myPos);
        return attackableCells.Contains(targetPos);
    }
    private void OnMouseDown()
    {
        if (!IsPlayerOwned || GameManager.CurrentPhase != GameManager.GamePhase.MoveReservation) return;
        //もう一度クリックしたら削除
        if (GridManager.Instance.SelectedUnit == this)
        {
            GridManager.Instance.DeselectUnit();
            return;
        }
        else
        {

            GridManager.Instance.SelectUnit(this);
        }




    }
}
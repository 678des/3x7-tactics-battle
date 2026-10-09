using System.Collections.Generic;
using UnityEngine;

public class BattleProcessor : MonoBehaviour
{
    public static BattleProcessor Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public void ExecuteBattle()
    {
        // 1. 場にいるすべてのユニットを取得
        List<Unit> allUnits = GridManager.Instance.GetAllUnits();
        List<Unit> unitsToDestroy = new List<Unit>();

        Debug.Log("ユニット");

        foreach (var attacker in allUnits)
        {
            if (attacker == null || unitsToDestroy.Contains(attacker)) continue;

            // 2. 自身の現在地から攻撃範囲（絶対座標のリスト）を算出
            List<Vector2Int> attackablePositions = attacker.GetAttackablePositions(attacker.CurrentPosition);

            foreach (var position in attackablePositions)
            {
                Debug.Log($"{position}攻撃範囲！");
            }
            Unit targetEnemy = null;
            bool isInBaseRange = false;

            // 3. 攻撃範囲内のマスをチェック
            foreach (var pos in attackablePositions)
            {
                Unit occupant = GridManager.Instance.GetUnitAtPosition(pos);

                if (occupant != null && occupant.IsPlayerOwned != attacker.IsPlayerOwned)
                {
                    targetEnemy = occupant;
                    Debug.Log($"{attacker.IsPlayerOwned}敵を発見{targetEnemy}");


                    break;
                }

                // 拠点に届いているかチェック（例：プレイヤーなら行7、敵なら行-1）
                if (attacker.IsPlayerOwned && pos.y >= GridManager.Rows) isInBaseRange = true;
                else if (!attacker.IsPlayerOwned && pos.y <= -1) isInBaseRange = true;

            }

            // 4. 判定・処理の実行
            if (targetEnemy != null)
            {
                ResolveCombat(attacker, targetEnemy, unitsToDestroy);
            }
            else if (isInBaseRange)
            {
                if (attacker.IsPlayerOwned) GameManager.Instance.DamageBase(TeamType.Enemy, attacker.CurrentPower);
                else GameManager.Instance.DamageBase(TeamType.Player, attacker.CurrentPower);

                Debug.Log($"{attacker.name} が拠点を攻撃！ ダメージ: {attacker.CurrentPower}");
            }
        }

        // 5. 敗北したユニットを削除
        foreach (var deadUnit in unitsToDestroy)
        {
            if (deadUnit != null)
            {
                Destroy(deadUnit.gameObject);
            }
        }
    }



    private void ResolveCombat(Unit attacker, Unit defender, List<Unit> destroyList)
    {
        // 1. 属性の相性をチェックする（例：水は火に強い、火は草に強い、草は水に強い）
        int attackerAdvantage = GetElementAdvantage(attacker.Element, defender.Element);
        // 1: 攻撃側が有利, -1: 攻撃側が不利, 0: 互角（相性なし）

        // 2. 「パワー ＋ 属性補正」で最終的な戦闘力を計算する
        // ※ 属性が有利なら、パワーに +5 などのボーナスを与える（これなら 10 vs 10 でも水が勝つ！）
        int attackerFinalPower = attacker.CurrentPower + (attackerAdvantage * 5);
        int defenderFinalPower = defender.CurrentPower - (attackerAdvantage * 5); // 相手の補正

        if (attackerFinalPower > defenderFinalPower)
        {
            destroyList.Add(defender); // 攻撃側の勝ち（防御側を破壊）
        }
        else if (attackerFinalPower < defenderFinalPower)
        {
            destroyList.Add(attacker); // 防御側の勝ち（攻撃側を破壊）
        }
        else
        {
            // パワーも相性も完全に互角の場合のみ相打ち
            destroyList.Add(attacker);
            destroyList.Add(defender);
        }
    }

    // 属性のじゃんけん判定をする関数
    public int GetElementAdvantage(Unit.ElementType attackerElement, Unit.ElementType defenderElement)
    {
        // 例: Water(水) > Fire(火) > Wood(木) > Water(水)
        if (attackerElement == Unit.ElementType.Water && defenderElement == Unit.ElementType.Fire) return 1;
        if (attackerElement == Unit.ElementType.Fire && defenderElement == Unit.ElementType.Grass) return 1;
        if (attackerElement == Unit.ElementType.Grass && defenderElement == Unit.ElementType.Water) return 1;

        // 逆に負けている場合
        if (attackerElement == Unit.ElementType.Fire && defenderElement == Unit.ElementType.Water) return -1;
        if (attackerElement == Unit.ElementType.Grass && defenderElement == Unit.ElementType.Fire) return -1;
        if (attackerElement == Unit.ElementType.Water && defenderElement == Unit.ElementType.Grass) return -1;

        return 0; // 同属性の場合
    }


}
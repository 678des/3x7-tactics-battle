using System.Collections.Generic;
using UnityEngine;

public class BattleProcessor : MonoBehaviour
{
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

                // 拠点に届いているかチェック（例：プレイヤーなら行6、敵なら行0）
                if (attacker.IsPlayerOwned && pos.y >= 6)
                {
                    isInBaseRange = true;
                }
                else if (!attacker.IsPlayerOwned && pos.y <= 0)
                {
                    isInBaseRange = true;
                }
            }

            // 4. 判定・処理の実行
            if (targetEnemy != null)
            {
                ResolveCombat(attacker, targetEnemy, unitsToDestroy);
            }
            else if (isInBaseRange)
            {
                ApplyBaseDamage(attacker);
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
        if (attacker.CurrentPower > defender.CurrentPower)
        {
            destroyList.Add(defender);
        }
        else if (attacker.CurrentPower < defender.CurrentPower)
        {
            destroyList.Add(attacker);
        }
        else
        {
            destroyList.Add(attacker);
            destroyList.Add(defender);
        }
    }

    private void ApplyBaseDamage(Unit attacker)
    {
        Debug.Log($"{attacker.name} が拠点を攻撃！ ダメージ: {attacker.CurrentPower}");
    }
}
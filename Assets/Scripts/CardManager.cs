using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手札の管理、コスト消費、カード効果の適用、および初期配置を行うマネージャークラス。
/// </summary>
public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Initial Setup (Decks)")]
    [SerializeField] private List<GameObject> playerStartingCards;
    [SerializeField] private List<GameObject> enemyStartingCards;

    [Header("Hand / UI Settings")]
    [SerializeField] private Vector3 handOffset = new Vector3(0, 0.3f, -0.4f);
    [SerializeField] private Quaternion handRotation = Quaternion.Euler(0, 90, 30);

    //現在選択しているカード
    public Card currentSelectedCard;

    public List<GameObject> EnemyStartingCards => enemyStartingCards;

    private void Awake()
    {
        if (gridManager == null)
            gridManager = GetComponent<GridManager>();

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// ゲームスタート時にプレイヤーと敵のカードをそれぞれ生成・配置する
    /// </summary>
    public void DistributeStartingCards()
    {
        SpawnCardsForTeam(playerStartingCards, TeamType.Player);
        SpawnCardsForTeam(enemyStartingCards, TeamType.Enemy);
    }

    /// <summary>
    /// チームごとにカードプレハブを生成する（味方・敵を明確に分離）
    /// </summary>
    private void SpawnCardsForTeam(List<GameObject> cardPrefabs, TeamType team)
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0) return;

        bool isPlayer = (team == TeamType.Player);

        for (int i = 0; i < cardPrefabs.Count; i++)
        {
            if (cardPrefabs[i] == null) continue;

            // 手札エリアへの配置（※もしグリッド上に直接置く場合はここをグリッド座標に書き換えます）
            Vector3 spawnPos = handOffset + new Vector3(i * 1.5f, 0, isPlayer ? 0 : 5f); // 味方と敵で少し位置を分ける例
            GameObject cardObj = Instantiate(cardPrefabs[i], spawnPos, handRotation, this.transform);
        }
    }

    public void UseSelectedCard(Vector2Int targetPosition)
    {
        Debug.Log("カードを使おうと");
        if (TryUseCard(currentSelectedCard, targetPosition))
        {
            if (currentSelectedCard != null)
            {
                currentSelectedCard.SetSelected(false);
                Destroy(currentSelectedCard.gameObject);
            }
            currentSelectedCard = null;
        }
    }

    public bool TryUseCard(Card card, Vector2Int targetPosition)
    {
        Debug.Log("カードを使おうと重い");
        if (card == null) return false;
        Debug.Log("カードを使おうと思いました");
        if (GameManager.Instance.CurrentPlayerCost < card.CardData.cost)
        {
            Debug.LogWarning("コスト不足です。");
            return false;
        }

        switch (card.CardData.type)
        {
            case CardType.Summon:
                // 味方による召喚を想定（敵のAIが使う場合はチーム判定が必要になります）
                Debug.Log("今からカード使いますよ");
                gridManager.SpawnUnit(card.CardData.unitPrefab, targetPosition, Quaternion.Euler(-90, 0, 0), true);
                break;

            case CardType.Buff:
                // TODO: バフ効果の適用
                break;

            case CardType.Debuff:
                // TODO: デバフ効果の適用
                break;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TryConsumeCost(TeamType.Player, card.CardData.cost);
        }

        return true;
    }
}
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PokerSceneDirector : MonoBehaviour
{
    // エディタから設定
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Button buttonBetCoin;
    [SerializeField] Button buttonPlay;
    [SerializeField] Button buttonChange;
    [SerializeField] Text textGameInfo;
    [SerializeField] Text textRate;
    // ボタン内のテキスト
    Text textButtonBetCoin;
    Text textButtonChange;
    // 全カード
    List<CardController> cards;
    // 手札
    List<CardController> hand;
    // 交換するカード
    List<CardController> selectCards;
    // 山札のインデックス番号
    int dealCardCount;
    // プレイヤーの持ちコイン
    [SerializeField] int playerCoin;
    // 交換できる回数
    [SerializeField] int cardChangeCountMax;
    // ベットしたコイン
    int betCoin;
    // 交換した回数
    int cardChangeCount;
    // 倍率設定
    int straightFlushRate = 10;
    int fourCardRate = 8;
    int fullHouseRate = 6;
    int flushRate = 5;
    int straightRate = 4;
    int threeCardRate = 3;
    int twoPairRate = 2;
    int onePairhRate = 1;
    // アニメーション時間
    const float SortHandTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        // カード取得
        cards = cardsDirector.GetShuffleCards();

        // 配列データ初期化
        hand = new List<CardController>();
        selectCards = new List<CardController>();

        // ボタン内のテキスト取得
        textButtonBetCoin = buttonBetCoin.GetComponentInChildren<Text>();
        textButtonChange = buttonChange.GetComponentInChildren<Text>();

        // 山札初期化
        restartGame(false);
        // テキストとボタンを初期化
        updateTexts();
        setButtonsInPlay(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // Rayを作成して投射
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // ヒットしたオブジェクトからCardControllerを取得
                CardController card = hit.collider.gameObject.GetComponent<CardController>();
                // カード選択処理
                setSelectCard(card);
            }
        }
    }

    // 手札を加える
    CardController addHand()
    {
        // 山札からカードを取得してインデックスを進める
        CardController card = cards[dealCardCount++];
        // 手札に加える
        hand.Add(card);
        // 引いたカードを返す
        return card;
    }

    // 手札をめくる
    void openHand(CardController card)
    {
        // 回転アニメーション
        card.transform.DORotate(Vector3.zero, SortHandTime)
            .OnComplete(() => { card.FlipCard(); });
    }

    // 手札を並べる
    void sortHand()
    {
        // 初期位置
        float x = -CardController.Width * 2;
        // 手札を枚数分並べる
        foreach (var item in hand)
        {
            // 表示位置へアニメーションして移動
            Vector3 pos = new Vector3(x, 0, 0);
            item.transform.DOMove(pos, SortHandTime);
            // 次回の表示位置x
            x += CardController.Width;
        }
    }

    // ゲームをスタート
    void restartGame(bool deal = true)
    {
        // 手札、選択カードをリセット
        hand.Clear();
        selectCards.Clear();
        // カードを引ける回数をリセット
        cardChangeCount = cardChangeCountMax;
        // 山札から引いた枚数をリセット
        dealCardCount = 0;

        // カードシャッフル
        cardsDirector.ShuffleCards(cards);

        // カード初期設定
        foreach (var item in cards)
        {
            // 捨て札は非表示状態なので表示する
            item.gameObject.SetActive(true);
            // 裏向きにする
            item.FlipCard(false);
            // 山札の場所へ
            item.transform.position = new Vector3(0, 0, 0.18f);
        }

        // ここから下は配る処理
        if (!deal) return;

        // 5枚配って表向きにする
        for (int i = 0; i < 5; i++)
        {
            openHand(addHand());
        }

        // カードを並べる
        sortHand();
    }

    // レート表を更新
    void updateTexts()
    {
        textButtonBetCoin.text = "手持ちコイン " + playerCoin;
        textGameInfo.text = "BET枚数 " + betCoin;

        textRate.text = "ストレートフラッシュ " + (straightFlushRate * betCoin) + "\n"
            + "フォーカード " + (fourCardRate * betCoin) + "\n"
            + "フルハウス " + (fullHouseRate * betCoin) + "\n"
            + "フラッシュ " + (flushRate * betCoin) + "\n"
            + "ストレート " + (straightRate * betCoin) + "\n"
            + "スリーカード " + (threeCardRate * betCoin) + "\n"
            + "ツーペア " + (twoPairRate * betCoin) + "\n"
            + "ワンペア " + (onePairhRate * betCoin) + "\n";
    }

    // ゲーム中のボタンを表示する
    void setButtonsInPlay(bool disp = true)
    {
        textButtonChange.text = "終了";
        // 交換ボタン表示設定（ゲームプレイ）
        buttonChange.gameObject.SetActive(disp);
        // ベットとプレイボタン表示設定（非ゲームプレイ）
        buttonBetCoin.gameObject.SetActive(!disp);
        buttonPlay.gameObject.SetActive(!disp);
    }

    // コインをベットする
    public void OnClickBetCoin()
    {
        if (1 > playerCoin) return;

        // コインを減らしてテキストを更新
        playerCoin--;
        betCoin++;
        updateTexts();
    }

    // ゲームプレイボタン
    public void OnClickPlay()
    {
        // デッキと手札を初期化
        restartGame();
        // ゲーム中のボタンとテキストの更新
        setButtonsInPlay();
        updateTexts();
    }

    // カード選択状態
    void setSelectCard(CardController card)
    {
        // 選択できないカードなら終了
        if (!card || !card.isFrontUp) return;

        // カードの現在地
        Vector3 pos = card.transform.position;

        // 2回目選択されたら非選択
        if (selectCards.Contains(card))
        {
            pos.z -= 0.02f;
            selectCards.Remove(card);
        }
        // 選択状態（カード上限を超えないように）
        else if (cards.Count > dealCardCount + selectCards.Count)
        {
            pos.z += 0.02f;
            selectCards.Add(card);
        }

        // 更新された場所
        card.transform.position = pos;

        // ボタン更新（選択枚数が0枚なら終了ボタンに変更）
        textButtonChange.text = "交換";
        if (1 > selectCards.Count)
        {
            textButtonChange.text = "終了";
        }
    }

    // カード交換
    public void OnClickChange()
    {
        // 交換しないなら1回で終了
        if (1 > selectCards.Count)
        {
            cardChangeCount = 0;
        }

        // 捨てカードを手札から削除
        foreach (var item in selectCards)
        {
            item.gameObject.SetActive(false);
            hand.Remove(item);
            // 捨てた分カードを追加
            openHand(addHand());
        }
        selectCards.Clear();

        // 並べる
        sortHand();
        setButtonsInPlay();

        // カード交換可能回数
        cardChangeCount--;
        if (1 > cardChangeCount)
        {
            // 役を精算する
            checkHandRank();
        }
    }

    // 役を精算する
    void checkHandRank()
    {
        // フラッシュチェック
        bool flush = true;
        // 1枚目のカードのマーク
        SuitType suit = hand[0].Suit;

        foreach (var item in hand)
        {
            // 1枚目と違ったら終了
            if (suit != item.Suit)
            {
                flush = false;
                break;
            }
        }

        // ストレートチェック
        bool straight = false;
        for (int i = 0; i < hand.Count; i++)
        {
            // 何枚数字が連続したか
            int straightcount = 0;
            // 現在のカード番号
            int cardno = hand[i].No;

            // 1枚目から連続しているか調べる
            for (int j = 0; j < hand.Count; j++)
            {
                // 同じカードはスキップ
                if (i == j) continue;

                // 見つけたい数字は現在の数字+1
                int targetno = cardno + 1;
                // 13の次は1
                if (13 < targetno) targetno = 1;

                // ターゲットの数字発見
                if (targetno == hand[j].No)
                {
                    // 連続回数をカウント
                    straightcount++;
                    // 今回のカード番号(次回+1される)
                    cardno = hand[j].No;
                    // jはまた0から始める
                    j = -1;
                }
            }

            if (3 < straightcount)
            {
                straight = true;
                break;
            }
        }

        // 同じ数字のチェック
        int pair = 0;
        bool threecard = false;
        bool fourcard = false;
        List<CardController> checkcards = new List<CardController>();

        for (int i = 0; i < hand.Count; i++)
        {
            if (checkcards.Contains(hand[i])) continue;

            // 同じ数字のカード枚数
            int samenocount = 0;
            int cardno = hand[i].No;

            for (int j = 0; j < hand.Count; j++)
            {
                if (i == j) continue;
                if (cardno == hand[j].No)
                {
                    samenocount++;
                    checkcards.Add(hand[j]);
                }
            }

            // ワンペア、ツーペア、スリーカード、フォーカード判定
            if (1 == samenocount)
            {
                pair++;
            }
            else if (2 == samenocount)
            {
                threecard = true;
            }
            else if (3 == samenocount)
            {
                fourcard = true;
            }
        }

        // フルハウス
        bool fullhouse = false;
        if (1 == pair && threecard)
        {
            fullhouse = true;
        }

        // ストレートフラッシュ
        bool straightflush = false;
        if (flush && straight)
        {
            straightflush = true;
        }

        // 役の判定
        int addcoin = 0;
        string infotext = "役無し... ";

        if (straightflush)
        {
            addcoin = straightFlushRate * betCoin;
            infotext = "ストレートフラッシュ!! ";
        }
        else if (fourcard)
        {
            addcoin = fourCardRate * betCoin;
            infotext = "フォーカード!! ";
        }
        else if (fullhouse)
        {
            addcoin = fullHouseRate * betCoin;
            infotext = "フルハウス!! ";
        }
        else if (flush)
        {
            addcoin = flushRate * betCoin;
            infotext = "フラッシュ!! ";
        }
        else if (straight)
        {
            addcoin = straightRate * betCoin;
            infotext = "ストレート!! ";
        }
        else if (threecard)
        {
            addcoin = threeCardRate * betCoin;
            infotext = "スリーカード!! ";
        }
        else if (2 == pair)
        {
            addcoin = twoPairRate * betCoin;
            infotext = "ツーペア!! ";
        }
        else if (1 == pair)
        {
            addcoin = onePairhRate * betCoin;
            infotext = "ワンペア!! ";
        }

        // コイン取得
        playerCoin += addcoin;

        // テキスト更新
        updateTexts();
        textGameInfo.text = infotext + addcoin;

        // 次回のゲーム用
        betCoin = 0;
        setButtonsInPlay(false);
    }
}

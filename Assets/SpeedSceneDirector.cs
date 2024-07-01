using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpeedSceneDirector : MonoBehaviour
{
    // 共通カード管理クラス
    [SerializeField] CardsDirector cardsDirector;
    // 勝敗表示
    [SerializeField] Text textResultInfo;
    // 全カード
    List<CardController> cards;
    // プレイヤー手札
    List<CardController> hand1p, hand2p;
    // プレイヤー場札
    List<CardController> layout1p, layout2p;
    // 台札(0左1右)
    CardController[] leadCards;
    // 選択中のカード
    CardController selectCard;
    // CPU思考時間
    float cpuTimer;
    // ゲームクリアフラグ
    bool isGameEnd;

    // 台札に置かれたカードのプレイヤー番号
    const int LeadCardNo = 100;
    // CPUランダム行動時間
    const float CpuRandomTimerMin = 3;
    const float CpuRandomTimerMax = 6;
    // 台札リセットスピード
    const float RestartMoveSpeed = 0.8f;
    const float RestartRotateSpeed = 0.2f;
    // 手札→場札スピード
    const float DealCardMoveSpeed = 0.5f;
    // 場札→台札スピード
    const float PlayCardMoveSpeed = 0.8f;
    // 1P手札ポジション
    const float HandPositionX = 0.15f;
    const float HandPositionZ = -0.1f;
    // 1P場札スタートポジション
    const float LayoutPositionX = -0.12f;
    const float LayoutPositionZ = -0.13f;
    // 台札左側ポジション
    const float LeadPositionX = -0.05f;
    // カードを重ねる時のサイズ
    const float StackCardHeight = 0.0001f;

    // Start is called before the first frame update
    void Start()
    {
        // シャッフルされたカードを取得
        cards = cardsDirector.GetShuffleCards();

        // データ初期化
        hand1p = new List<CardController>();
        hand2p = new List<CardController>();
        layout1p = new List<CardController>();
        layout2p = new List<CardController>();
        leadCards = new CardController[2];
        textResultInfo.text = "";

        // 山札を並べる
        CardController firstcard = addCardHand(hand1p, cards[0]);
        // 1枚目に追加されたカード同じ色を1Pへ
        foreach (var item in cards)
        {
            List<CardController> hand = hand1p;
            // 1枚目と違うなら2Pの手札へ
            if (firstcard.SuitColor != item.SuitColor) hand = hand2p;

            addCardHand(hand, item);
        }

        // 場札を並べる
        shuffleLayout(layout1p);
        shuffleLayout(layout2p);

        // 台札を並べる
        playCardFromHand(hand1p);
        playCardFromHand(hand2p);

        // CPU行動タイマー
        cpuTimer = Random.Range(CpuRandomTimerMin, CpuRandomTimerMax);
    }

    // Update is called once per frame
    void Update()
    {
        // 勝敗がついていたら終了
        if (isGameEnd) return;

        // 台札がリセットされたら抜ける
        if (tryResetLeadCards()) return;

        // プレイヤー操作
        if (Input.GetMouseButtonUp(0))
        {
            playerSelectCard();
        }

        // CPU操作
        cpuTimer -= Time.deltaTime;
        if (0 > cpuTimer)
        {
            autoSelectCard(layout2p);
            // 次のCPU行動時間
            cpuTimer = Random.Range(CpuRandomTimerMin, CpuRandomTimerMax);
        }
    }

    // 手札にカードを追加する
    CardController addCardHand(List<CardController> hand, CardController card)
    {
        // 1P設定
        int playerno = 0;
        int dir = 1;
        // 2P設定
        if (hand2p == hand)
        {
            playerno = 1;
            dir *= -1;
        }

        // カードがないか既に追加済み
        if (!card || hand.Contains(card)) return null;

        card.transform.position = new Vector3(HandPositionX * dir, 0, HandPositionZ * dir);
        card.PlayerNo = playerno;
        card.FlipCard(false);

        hand.Add(card);
        // 追加したカードを返す
        return card;
    }

    // 手札から1枚カードを引く
    CardController getCardHand(List<CardController> hand)
    {
        CardController card = null;
        if (0 < hand.Count)
        {
            card = hand[0];
            hand.Remove(card);
        }
        return card;
    }

    // カードを移動して表向きにする
    void moveCardOpen(CardController card, Vector3 pos, float speed)
    {
        card.transform.DOKill();

        card.transform.DORotate(Vector3.zero, speed);
        card.transform.DOMove(pos, speed).OnComplete(() => card.FlipCard());
    }

    // 手札のカードを一枚引いて場札へ移動する
    void dealCard(List<CardController> hand)
    {
        // 手札を1枚取得
        CardController card = getCardHand(hand);
        if (!card) return;

        // 1P設定
        List<CardController> layout = layout1p;
        int dir = 1;
        // 2P設定
        if (hand2p == hand)
        {
            layout = layout2p;
            dir *= -1;
        }

        // 手札の空いている場所へ1枚カードを引く
        for (int i = 0; i < layout.Count; i++)
        {
            // カードがあるならスキップ
            if (layout[i]) continue;

            // 内部データ更新
            layout[i] = card;

            // 目標位置
            float x = (i * CardController.Width + LayoutPositionX) * dir;
            float z = LayoutPositionZ * dir;
            Vector3 pos = new Vector3(x, 0, z);
            // 元位置を保存する
            card.HandPosition = pos;

            // アニメーション
            float dist = Vector3.Distance(card.transform.position, pos);
            moveCardOpen(card, pos, dist / DealCardMoveSpeed);
            // 1枚追加されたら終了
            break;
        }
    }

    // 場札を入れ替える
    void shuffleLayout(List<CardController> layout)
    {
        // 場札にアニメーション中のものがあれば処理しない
        foreach (var item in layout)
        {
            if (!item) continue;
            if (DOTween.IsTweening(item.transform)) return;
        }

        // 1P設定
        List<CardController> hand = hand1p;
        // 2P設定
        if (layout == layout2p) hand = hand2p;

        // 場札を手札に戻す
        foreach (var item in layout)
        {
            addCardHand(hand, item);
        }
        layout.Clear();

        // カードシャッフル
        cardsDirector.ShuffleCards(hand);

        // 4枚並べる
        for (int i = 0; i < 4; i++)
        {
            layout.Add(null);
            dealCard(hand);
        }
    }

    // 台札を更新する
    void updateLead(int index, CardController card)
    {
        // プレイヤー番号を台札に設定
        card.PlayerNo = LeadCardNo;
        // 0左1右
        card.Index = index;
        // データ更新
        leadCards[index] = card;
    }

    // 強制的に手札から1枚台札に出す、なければ場札から出す
    void playCardFromHand(List<CardController> hand)
    {
        // 1P設定
        int index = 0;
        int dir = 1;
        List<CardController> layout = layout1p;
        // 2P設定
        if (hand2p == hand)
        {
            index = 1;
            dir *= -1;
            layout = layout2p;
        }

        // カードを手札から取得
        CardController card = getCardHand(hand);

        // 手札がなければ場札からランダムで強制的に出して終了
        if (!card)
        {
            foreach (var item in layout)
            {
                if (!item) continue;
                playCardFromLayout(item, index, true);
                return;
            }
        }

        // すでにカードがあれば少し上に置く
        float y = 0;
        if (leadCards[index])
        {
            y = leadCards[index].transform.position.y + StackCardHeight;
        }

        // 目的地
        Vector3 pos = new Vector3(LeadPositionX * dir, y, 0);
        // 距離
        float dist = Vector3.Distance(card.transform.position, pos);
        // アニメーション
        card.transform.DORotate(Vector3.zero, RestartRotateSpeed);
        card.transform.DOMove(pos, dist / RestartMoveSpeed)
        .OnComplete(() =>
        {
            updateLead(index, card);
            card.FlipCard();
        });
    }

    // カード選択
    void setSelectCard(CardController card = null)
    {
        // 一旦非選択状態
        if (selectCard)
        {
            selectCard.gameObject.transform.position = selectCard.HandPosition;
            selectCard = null;
        }

        if (!card) return;

        // 選んだ感じにする
        Vector3 pos = card.transform.position;
        pos.z += 0.02f;
        card.transform.position = pos;

        selectCard = card;
    }

    // プレイヤーの処理
    void playerSelectCard()
    {
        // Rayを投射
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // 当たり判定がなければ終了
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // ヒットしたゲームオブジェクトからCardControllerを取得
        CardController card = hit.collider.gameObject.GetComponent<CardController>();

        // カードじゃないか伏せてあれば終了
        if (!card || !card.isFrontUp) return;

        // カード選択状態で台札のカードが押された
        if (selectCard && LeadCardNo == card.PlayerNo)
        {
            playCardFromLayout(selectCard, card.Index);
            setSelectCard();
        }
        // 手札
        else if (0 == card.PlayerNo)
        {
            setSelectCard(card);
        }
    }

    // 置けるかどうかチェック
    bool isMovable(CardController movecard, CardController leadcard)
    {
        // カード情報がない
        if (!movecard || !leadcard) return false;
        // 置こうとしてるカードがアニメーション中は置けない
        if (DOTween.IsTweening(movecard.transform)) return false;

        // おける数字のチェック
        int min = leadcard.No - 1;
        if (1 > min) min = 13;

        int max = leadcard.No + 1;
        if (13 < max) max = 1;

        // 1小さいか1大きければおける
        if (min == movecard.No || max == movecard.No)
        {
            return true;
        }

        return false;
    }

    // 場札のカード枚数を返す
    int cardCount(List<CardController> layout)
    {
        int count = 0;
        foreach (var item in layout)
        {
            if (item) count++;
        }
        return count;
    }

    // 勝敗チェック
    void checkResult()
    {
        // 既に勝敗がついてる
        if (isGameEnd) return;

        if (1 > hand1p.Count && 1 > cardCount(layout1p))
        {
            textResultInfo.text = "1P勝利!!";
            isGameEnd = true;
        }
        else if (1 > hand2p.Count && 1 > cardCount(layout2p))
        {
            textResultInfo.text = "2P勝利!!";
            isGameEnd = true;
        }
    }

    // 場札から台札へ1枚出す
    void playCardFromLayout(CardController card, int index, bool force = false)
    {
        // 1P設定
        List<CardController> layout = layout1p;
        List<CardController> hand = hand1p;
        // 2P設定
        if (1 == card.PlayerNo)
        {
            layout = layout2p;
            hand = hand2p;
        }

        // 目的地
        Vector3 pos = leadCards[index].transform.position;
        // カードの上にのるように
        pos.y += StackCardHeight;
        // 場札と台札の距離
        float dist = Vector3.Distance(card.transform.position, pos);

        // ランダムで角度をずらす
        float ry = Random.Range(-15.0f, 15.0f);
        card.transform.DORotate(new Vector3(0, ry, 0), dist / PlayCardMoveSpeed);

        // 移動完了時に台札の状態をチェックするためにOnCompleteをつける
        card.transform.DOMove(pos, dist / PlayCardMoveSpeed)
        .OnComplete(() =>
        {
            // 移動完了後、台札に置けるか調べる
            if (isMovable(card, leadCards[index]) || force)
            {
                // 台札更新
                updateLead(leadCards[index].Index, card);
                // 場札更新
                layout[layout.IndexOf(card)] = null;
                // 手札から1枚引く
                dealCard(hand);
                // 勝敗チェック
                checkResult();
            }
            // おけなかった時は場札に戻す
            else
            {
                moveCardOpen(card, card.HandPosition, dist / PlayCardMoveSpeed);
            }
        });

    }

    // シャッフルボタン
    public void OnClickShuffle()
    {
        setSelectCard();
        shuffleLayout(layout1p);
    }

    // 指定された場札から出せるカードがあれば出す
    void autoSelectCard(List<CardController> layout)
    {
        // 場札
        foreach (var layoutcard in layout)
        {
            // 台札
            foreach (var leadcard in leadCards)
            {
                // 置けないカードをスキップ
                if (!isMovable(layoutcard, leadcard)) continue;

                // 置けたら終了
                playCardFromLayout(layoutcard, leadcard.Index);
                return;
            }
        }

        // おけなかったら場札をシャッフル
        shuffleLayout(layout);
    }

    // 全カードが出せなければ台札をリセットする
    bool tryResetLeadCards()
    {
        // なにかアニメーション中
        if (null != DOTween.PlayingTweens()) return false;

        // 1Pと2Pの場札のリストを作成
        List<CardController> alllayout = new List<CardController>(layout1p);
        alllayout.AddRange(layout2p);

        // 全てのカードが移動可能か調べる
        foreach (var layoutcard in alllayout)
        {
            foreach (var leadcard in leadCards)
            {
                if (isMovable(layoutcard, leadcard)) return false;
            }
        }

        // 今までの台札を非表示
        foreach (var item in cards)
        {
            if (LeadCardNo == item.PlayerNo) item.gameObject.SetActive(false);
        }

        // 動かせなければ台札をリセット
        setSelectCard();
        playCardFromHand(hand1p);
        playCardFromHand(hand2p);

        return true;
    }

    // シーン再読み込み
    public void OnClickRestart()
    {
        SceneManager.LoadScene("SpeedScene");
    }
}

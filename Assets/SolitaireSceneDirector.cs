using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SolitaireSceneDirector : MonoBehaviour
{
    // カード管理クラス
    [SerializeField] CardsDirector cardsDirector;
    // ゲームタイマー
    [SerializeField] Text textTimer;

    // 山札土台
    [SerializeField] GameObject stock;
    // 組札土台
    [SerializeField] List<Transform> foundation;
    // 場札土台
    [SerializeField] List<Transform> column;

    // 全てのカード
    List<CardController> cards;
    // 山札のカード
    List<CardController> stockCards;
    // めくり札のカード
    List<CardController> wasteCards;
    // 選択中のカード
    CardController selectCard;
    // ドラッグ開始位置
    Vector3 startPosition;
    // ゲーム終了フラグ
    bool isGameEnd;
    // ゲームタイマー
    float gameTimer;
    int oldSecond;
    // カードを並べる時のサイズ
    const float StackCardHeight = 0.0001f;
    const float StackCardWidth = 0.02f;
    // アニメーション時間
    const float SortWasteCardTime = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        // シャッフルされたカードを取得
        cards = cardsDirector.GetShuffleCards();

        // データ初期化
        wasteCards = new List<CardController>();
        stockCards = new List<CardController>();

        // カード初期設定
        foreach (var item in cards)
        {
            item.PlayerNo = 0;
            item.FlipCard(false);
            // 山札へ追加
            stockCards.Add(item);
        }

        // 場札を並べる
        int cardindex = 0;
        int columncount = 0;

        foreach (var item in column)
        {
            // 場札に何枚置くか
            columncount++;
            for (int i = 0; i < columncount; i++)
            {
                // カードが足りなかった時は終了
                if (cards.Count - 1 < cardindex) break;

                // 今回追加するカード
                CardController card = cards[cardindex];
                // 親オブジェクト
                CardController parent = item.GetComponent<CardController>();
                // 1番下以外は1つ前のカードの上に置く
                if (0 != i) parent = cards[cardindex - 1];

                // カードをセットする
                putCard(parent, card);
                // 追加したカードを山札から削除
                stockCards.Remove(card);

                // カードを次へ送る
                cardindex++;
            }

            // 最後に追加したカードをめくる
            cards[cardindex - 1].FlipCard();
        }

        // 山札を並べる
        stackStockCards();
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームクリア
        if (isGameEnd) return;

        // 経過時間
        gameTimer += Time.deltaTime;
        // タイマー表示
        textTimer.text = getTimerText(gameTimer);

        // マウスが押された
        if (Input.GetMouseButtonDown(0))
        {
            setSelectCard();
        }
        // ドラッグ中
        else if (Input.GetMouseButton(0))
        {
            moveCard();
        }
        // 離された
        else if (Input.GetMouseButtonUp(0))
        {
            releaseCard();
        }
    }

    // カードを移動させる
    void putCard(CardController parent, CardController child)
    {
        // 親オブジェクト指定
        child.transform.parent = parent.transform;
        // 移動先
        Vector3 pos = parent.transform.position;
        // 上にずらす
        pos.y += StackCardHeight;

        // 場札のカードならｚをずらす（土台以外）
        if (column.Contains(parent.transform.root) && !column.Contains(parent.transform))
        {
            pos.z -= StackCardWidth;
        }

        // 更新された場所
        child.transform.position = pos;

        // めくり札だったらリストから削除
        wasteCards.Remove(child);
    }

    // 山札を並べる
    void stackStockCards()
    {
        // 山札の上に並べる
        for (int i = 0; i < stockCards.Count; i++)
        {
            CardController card = stockCards[i];
            card.FlipCard(false);

            Vector3 pos = stock.transform.position;
            pos.y += (i + 1) * StackCardHeight;
            card.transform.position = pos;
            card.transform.parent = stock.transform;
        }
    }

    // タイマー表示を取得する
    string getTimerText(float timer)
    {
        // 秒数を取得
        int sec = (int)timer % 60;
        // 前回と秒数が同じなら前回の表示値を返す
        string ret = textTimer.text;

        // 前回と秒数に違いがあるか
        if (oldSecond != sec)
        {
            // 分数を計算
            int min = (int)timer / 60;
            // minとsecを00のような文字列にする（0埋め、ゼロパディング）
            string pmin = string.Format("{0:D2}", min);
            string psec = string.Format("{0:D2}", sec);

            ret = pmin + ":" + psec;
            oldSecond = sec;
        }

        return ret;
    }

    // スクリーンからワールドポジションへ変換
    Vector3 getScreenToWorldPosition()
    {
        // マウスの座標（スクリーン座標）
        Vector3 cameraposition = Input.mousePosition;
        // カメラのｙ座標を設定
        cameraposition.z = Camera.main.transform.position.y;
        // スクリーン座標→ワールド座標
        Vector3 worldposition = Camera.main.ScreenToWorldPoint(cameraposition);

        return worldposition;
    }

    // めくり札を並べる
    void sortWasteCards()
    {
        // 山札の左側を開始位置に設定
        float startx = stock.transform.position.x - CardController.Width * 2;

        for (int i = 0; i < wasteCards.Count; i++)
        {
            CardController card = wasteCards[i];
            // 枚数分右に
            float x = startx + i * StackCardWidth;
            // 枚数分上に
            float y = i * StackCardHeight;
            // アニメーション
            card.transform.DOMove(new Vector3(x, y, stock.transform.position.z), SortWasteCardTime);
        }
    }

    // カード選択
    void setSelectCard()
    {
        // 当たり判定がなければ終了
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // 当たり判定があったオブジェクトとカード
        GameObject obj = hit.collider.gameObject;
        CardController card = obj.GetComponent<CardController>();

        // 選択状態解除
        selectCard = null;

        // 山札が最後までめくられた
        if (obj == stock)
        {
            // めくり札と捨てられたカードをデッキへ戻す
            stockCards.AddRange(wasteCards);
            foreach (var item in cards)
            {
                if (item.gameObject.activeSelf) continue;

                item.gameObject.SetActive(true);
                stockCards.Add(item);
            }
            wasteCards.Clear();

            // シャッフルして再度並べる
            cardsDirector.ShuffleCards(stockCards);
            stackStockCards();
        }

        // カードじゃないか土台なら終了
        if (!card || 0 > card.PlayerNo) return;

        // オープンされているカード
        if (card.isFrontUp)
        {
            // めくり札のカードだったら1番手前のみ選択可能
            if (wasteCards.Contains(card) && card != wasteCards[wasteCards.Count - 1]) return;

            // おけなかった時に戻る場所（現在地）
            card.HandPosition = card.transform.position;
            // カード選択状態
            selectCard = card;
            // ドラッグ用に現在のポジションを取得
            startPosition = getScreenToWorldPosition();
        }
        // 未オープンのカード
        else
        {
            // 1番手前ならオープン（共通）
            if (1 > card.transform.childCount)
            {
                card.transform.DORotate(Vector3.zero, SortWasteCardTime)
                    .OnComplete(() => { card.FlipCard(); });
            }

            // 山札のカードをめくり札の場所へ
            if (card.transform.root == stock.transform)
            {
                // 4枚目以降は1番古いカードを捨てて非表示
                if (3 < wasteCards.Count + 1)
                {
                    wasteCards[0].gameObject.SetActive(false);
                    wasteCards.RemoveAt(0);
                }

                // 山札からめくり札へ移動
                stockCards.Remove(card);
                wasteCards.Add(card);

                // 並べ直す
                sortWasteCards();
                stackStockCards();
            }
        }
    }

    // カード移動
    void moveCard()
    {
        if (!selectCard) return;

        // 動いたポジションの差分
        Vector3 diff = getScreenToWorldPosition() - startPosition;
        Vector3 pos = selectCard.transform.position + diff;
        // 少し浮かせて選択した状態にする
        pos.y = 0.01f;
        selectCard.transform.position = pos;
        // ポジションを更新
        startPosition = getScreenToWorldPosition();
    }

    // カードが離された
    void releaseCard()
    {
        if (!selectCard) return;

        // 1番手前のカードを代入
        CardController frontcard = null;

        // 全ての当たり判定を取得
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            // 当たり判定があったカード
            CardController card = hit.transform.gameObject.GetComponent<CardController>();

            // カードじゃないか選択中のカードならスキップ
            if (!card || card == selectCard) continue;

            // 1番手前の(子オブジェクトが少ない)カードを取得
            if (!frontcard || frontcard.transform.childCount > card.transform.childCount)
            {
                frontcard = card;
            }
        }

        // 組札に置けるカード
        if (frontcard && foundation.Contains(frontcard.transform.root)
            && 1 > selectCard.transform.childCount
            && frontcard.No + 1 == selectCard.No
            && frontcard.Suit == selectCard.Suit)
        {
            putCard(frontcard, selectCard);

            // クリア判定
            bool fieldend = true;
            foreach (var item in column)
            {
                if (0 < item.childCount) fieldend = false;
            }

            // 場札、めくり札、山札のカードが1枚もなければクリア
            isGameEnd = fieldend && 1 > wasteCards.Count && 1 > stockCards.Count;
        }
        // 場札に置けるカード
        else if (frontcard && column.Contains(frontcard.transform.root)
            && 1 > frontcard.transform.childCount
            && frontcard.No - 1 == selectCard.No
            && frontcard.SuitColor != selectCard.SuitColor)
        {
            putCard(frontcard, selectCard);
        }
        // 置けなかったら元に戻す
        else
        {
            selectCard.transform.position = selectCard.HandPosition;
        }
    }

    // シーン再読み込み
    public void OnClickRestart()
    {
        SceneManager.LoadScene("SolitaireScene");
    }
}

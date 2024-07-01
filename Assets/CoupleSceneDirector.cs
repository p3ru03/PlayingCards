using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoupleSceneDirector : MonoBehaviour
{
    // カード管理クラス
    [SerializeField] CardsDirector cardsDirector;
    // ゲームタイマー
    [SerializeField] Text textTimer;
    // 全カード
    List<CardController> cards;
    // 縦横何枚並べるか
    int width = 4;
    int height = 4;
    // 選択中のカード
    CardController selectCard;
    // フィールドに並んでいるカード
    List<CardController> fieldCards;
    // ゲームタイマー
    float gameTimer;
    int oldSecond;
    // フィールドをリセットする時間
    const float FieldResetTime = 1;
    // ゲーム終了
    bool isGameEnd;

    [SerializeField] Text textClear;

    // Start is called before the first frame update
    void Start()
    {
        // シャッフルされたカードを取得
        cards = cardsDirector.GetShuffleCards();

        // フィールド初期化
        resetField(FieldResetTime);
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

        // ボタンが押されたら時の処理
        if (Input.GetMouseButtonUp(0))
        {
            matchCards();
        }
    }

    // カードを等間隔に並べる
    List<CardController> sortFieldCards(List<CardController> cards, int width, int height, float speed)
    {
        List<CardController> ret = new List<CardController>();

        // カード全体を真ん中にずらすためのオフセット
        Vector2 offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f);

        // カードを並べる
        for (int i = 0; i < width * height; i++)
        {
            // カードが足りなければ終了
            if (cards.Count - 1 < i) break;
            // カードデータがない場合はスキップ
            if (!cards[i]) continue;

            // 位置（インデックス）
            Vector2Int index = new Vector2Int(i % width, i / width);
            cards[i].Index = i;
            cards[i].IndexPosition = index;

            // 表示位置
            Vector3 pos = new Vector3(
                (index.x - offset.x) * CardController.Width,
                0,
                (index.y - offset.y) * CardController.Height);

            // カードの現在地と表示位置の距離
            float dist = Vector3.Distance(cards[i].transform.position, pos);

            // アニメーション（移動しながら回転）
            cards[i].transform.DOMove(pos, dist / speed);
            cards[i].transform.DORotate(new Vector3(0, 0, 0), dist / speed);

            // 並べたカードを保存
            ret.Add(cards[i]);
        }

        return ret;
    }

    // デッキからフィールドへ
    void resetField(float speed)
    {
        // デッキへ移動（内部データリセット）
        foreach (var item in cards)
        {
            item.PlayerNo = -1;
            item.IndexPosition = new Vector2Int(-100, -100);
            item.transform.position = new Vector3(0.2f, 0, -0.15f);
            item.FlipCard(false);
        }

        // 等間隔に並べたカードを取得
        fieldCards = sortFieldCards(cards, width, height, speed);

        // フィールドカードを選択できるようにする
        foreach (var item in fieldCards)
        {
            // 選択可能な状態
            item.PlayerNo = 0;
            // デッキから削除
            cards.Remove(item);
        }
    }

    // カード選択
    void setSelectCard(CardController card = null)
    {
        Vector3 pos;
        // いったん非選択状態
        if (selectCard)
        {
            pos = selectCard.transform.position;
            selectCard.transform.position = new Vector3(pos.x, 0, pos.z);
            selectCard = null;
        }

        // カード情報がなければ終了
        if (!card) return;

        // 選んだ状態にする
        selectCard = card;
        pos = selectCard.transform.position;
        pos.y += 0.02f;
        selectCard.transform.position = pos;
    }

    // カード判定
    void matchCards()
    {
        // レイを投射
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        // ヒットしたオブジェクトからCardControllerを取得
        CardController card = hit.collider.gameObject.GetComponent<CardController>();
        // カードじゃないなら終了
        if (!card) return;

        // カード同士のインデックスの距離
        int dist = 0;
        if (selectCard)
        {
            dist = (int)Vector2Int.Distance(selectCard.IndexPosition, card.IndexPosition);
        }

        // 2枚目選択
        if (1 == dist && selectCard.No == card.No)
        {
            // フィールドのインデックスを更新
            fieldCards[selectCard.Index] = null;
            fieldCards[card.Index] = null;

            // 揃ったカードを非表示
            selectCard.gameObject.SetActive(false);
            card.gameObject.SetActive(false);

            // カードをつめる
            for (int i = 0; i < fieldCards.Count; i++)
            {
                if (fieldCards[i]) continue;

                // カードを探す
                for (int j = i + 1; j < fieldCards.Count; j++)
                {
                    if (!fieldCards[j]) continue;
                    fieldCards[i] = fieldCards[j];
                    fieldCards[j] = null;
                    break;
                }
            }

            // 選択解除
            setSelectCard();

            // フィールドカードを並べ直す
            sortFieldCards(fieldCards, width, height, FieldResetTime / 2);

            // ゲーム終了チェック
            bool endfield = true;
            foreach (var item in fieldCards)
            {
                if (item)
                {
                    endfield = false;
                    break;
                }
            }

            isGameEnd = endfield && (1 > cards.Count);

            if (isGameEnd)
            {
                textClear.gameObject.SetActive(true);
            }
        }
        // カード選択
        else if (0 == card.PlayerNo)
        {
            setSelectCard(card);
        }
        // デッキからフィールドへ1枚出す
        else
        {
            // フィールドへカードを補填
            for (int i = 0; i < fieldCards.Count; i++)
            {
                if (fieldCards[i]) continue;
                fieldCards[i] = cards[0];
                fieldCards[i].PlayerNo = 0;
                cards.RemoveAt(0);
                break;
            }

            // 選択解除
            setSelectCard();
            // カードを並べ直す
            sortFieldCards(fieldCards, width, height, FieldResetTime);
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

    // シャッフルボタン
    public void OnClickShuffle()
    {
        // アニメーションを全て終了（連打対策）
        if (null != DOTween.PlayingTweens())
        {
            foreach (var item in DOTween.PlayingTweens())
            {
                item.Kill();
            }
        }

        // フィールドのカードをデッキへ
        foreach (var item in fieldCards)
        {
            if (!item) continue;
            cards.Add(item);
        }
        fieldCards.Clear();

        // デッキをシャッフル
        cardsDirector.ShuffleCards(cards);
        // フィールドリセット
        resetField(FieldResetTime / 2);
    }

    // シーン再読み込み
    public void OnClickRestart()
    {
        SceneManager.LoadScene("CoupleScene");
    }
}

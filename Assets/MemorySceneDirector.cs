using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MemorySceneDirector : MonoBehaviour
{
    //共通カード管理クラス
    [SerializeField] CardsDirector cardsDirector;
    //タイマー
    [SerializeField] Text textTimer;
    //ゲームで使うカード
    List<CardController> cards;
    //縦横何枚並べるか
    int width = 5;
    int height = 4;
    //選んだカード
    List<CardController> selectCards;
    int selectCountMax = 2;
    //ゲーム終了フラグ
    bool isGameEnd;
    //経過時間
    float gameTimer;
    //前回の秒数
    int oldSecond;
    void Start()
    {
        //シャッフルされたカードを取得
        cards = cardsDirector.GetMemoryCards();

        //カード全体を真ん中にずらすためのオフセット
        Vector2 offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f);

        //カード枚数が足りないとき、エラーを表示する
        if (cards.Count < width * height)
        {
            Debug.LogError("カードが足りません");
        }

        //カードを並べる
        for (int i = 0; i < width * height; i++)
        {
            //表示位置
            float x = (i % width - offset.x) * CardController.Width;
            float y = (i / width - offset.y) * CardController.Height;

            //場所と角度
            cards[i].transform.position = new Vector3(x, 0, y);
            cards[i].FlipCard(false);
        }

        //各種フラグ初期化
        selectCards = new List<CardController>();
        oldSecond = -1;
    }

    // Update is called once per frame
    void Update()
    {
        //ゲーム終了なら処理をしない
        if (isGameEnd) return;

        //経過時間を足す
        gameTimer += Time.deltaTime;

        //タイマー表示更新
        textTimer.text = getTimerText(gameTimer);

        //マウスが離されたとき
        if (Input.GetMouseButtonDown(0))
        {
            //３回目のタップ
            if (!canOpen()) return;
            //Rayを飛ばして当たり判定をとる
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //ヒットしたゲームオブジェクトからCardControllerを取得
                CardController card = hit.collider.gameObject.GetComponent<CardController>();
                //カードじゃないかもうめくったカードなら終了
                if (!card || selectCards.Contains(card)) return;
                //カードオープン
                card.FlipCard();
                //選択したカードを保存
                selectCards.Add(card);
            }
        }
    }

    //タイマー表示を取得する
    string getTimerText(float timer)
    {
        //秒数を取得
        int sec = (int)timer % 60;
        //前回と秒数が同じなら前回の表示値を返す
        string ret = textTimer.text;
        //前回と秒数に違いがあるか
        if (oldSecond != sec)
        {
            //分数を計算
            int min = (int)timer / 60;
            //minとsecを00のような文字列にする（０埋め、ゼロパディング）
            string pmin = string.Format("{0:D2}", min);
            string psec = string.Format("{0:D2}", sec);

            ret = pmin + ":" + psec;
            oldSecond = sec;
        }

        return ret;
    }

    //もう１枚めくれるかどうか
    bool canOpen()
    {
        //２枚揃っていない場合は、カードをめくることができる
        if (selectCards.Count < selectCountMax) return true;

        //２枚揃っている場合は、選択したカードが同じ数字かどうかチェック
        bool equal = true;
        foreach (var item in selectCards)
        {
            //選択したカードを裏返しにする
            item.FlipCard(false);

            //同じ数字かどうかをチェック
            if (item.No != selectCards[0].No)
            {
                equal = false;
            }
        }

        //全部同じ数字だったらカードを消す
        if (equal)
        {
            //めくったカードを非表示
            foreach (var item in selectCards)
            {
                item.gameObject.SetActive(false);
            }

            //全部のカードが非表示になっていたらゲーム終了
            isGameEnd = true;
            foreach (var item in cards)
            {
                if (item.gameObject.activeSelf)
                {
                    isGameEnd = false;
                    break;
                }
            }

            //ゲームクリア秒数を表示
            if (isGameEnd)
            {
                textTimer.text = "クリア!!" + getTimerText(gameTimer);
            }

        }

        //選択したカードをクリア
        selectCards.Clear();

        return false;
    }
}

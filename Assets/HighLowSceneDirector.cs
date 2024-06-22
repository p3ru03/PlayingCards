using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighLowSceneDirector : MonoBehaviour
{
    //共通カード管理クラス
    [SerializeField] CardsDirector cardsDirector;
    //UI
    [SerializeField] GameObject buttonHigh;
    [SerializeField] GameObject buttonLow;
    [SerializeField] Text textInfo;
    //ゲームで使うカード
    List<CardController> cards;
    //現在のインデックス
    int cardIndex;
    //勝利数
    int winCount;
    //ウェイト時間
    const float NextWaitTimer = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        //シャッフルされたカードを取得
        cards = cardsDirector.GetHighLowCards();
        //初期位置と向きを設定
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.position = new Vector3(0, 0, 0.15f);
            cards[i].FlipCard(false);
        }
        //２枚配る
        dealCards();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void dealCards()
    {
        cards[cardIndex].transform.position = new Vector3(-0.05f, 0, 0);
        cards[cardIndex].GetComponent<CardController>().FlipCard();
        cards[cardIndex + 1].transform.position = new Vector3(0.05f, 0, 0);
        setHighLowButtons(true);
    }

    //ゲームボタンの表示/非表示
    void setHighLowButtons(bool active)
    {
        buttonHigh.SetActive(active);
        buttonLow.SetActive(active);
    }

    public void OnClickHigh()
    {
        setHighLowButtons(false);
        checkHighLow(true);
    }

    public void OnClickLow()
    {
        setHighLowButtons(false);
        checkHighLow(false);
    }

    //ゲーム判定
    void checkHighLow(bool high)
    {
        //右のカードをオープン
        cards[cardIndex + 1].GetComponent<CardController>().FlipCard();
        //デフォルトの文字列
        string result = "LOSE... : ";
        //左と右のカード番号
        int lno = cards[cardIndex].GetComponent<CardController>().No;
        int rno = cards[cardIndex + 1].GetComponent<CardController>().No;

        //引き分け
        if (lno == rno)
        {
            result = "NO GAME!!";
        }
        //HIGHを選んだ
        else if (high)
        {
            if (lno < rno)
            {
                winCount++;
                result = "WIN!! : ";
                GetComponent<AudioSource>().Play();
            }
        }
        //LOWを選んだ
        else
        {
            if (lno > rno)
            {
                winCount++;
                result = "WIN!! : ";
                GetComponent<AudioSource>().Play();
            }
        }

        //インフォ更新
        textInfo.text = result + winCount;

        //次のゲーム
        StartCoroutine(nextCards());
    }

    //次のゲーム
    IEnumerator nextCards()
    {
        //指定秒数待つ
        yield return new WaitForSeconds(NextWaitTimer);

        //前回のカードを片づける
        cards[cardIndex].gameObject.SetActive(false);
        cards[cardIndex + 1].gameObject.SetActive(false);

        //次のカードの一枚目
        cardIndex += 2;

        //カードが足りない
        if (cards.Count - 1 <= cardIndex)
        {
            textInfo.text = "終了!! " + winCount;
        }
        //次のゲーム
        else
        {
            dealCards();
        }
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("HighLowScene");
    }
}

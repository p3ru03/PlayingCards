using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BlackJackSceneDirector : MonoBehaviour
{
    //共通カード管理クラス
    [SerializeField] CardsDirector cardsDirector;
    //UI
    [SerializeField] GameObject buttonHit;
    [SerializeField] GameObject buttonStay;
    [SerializeField] Text textPlayerInfo;
    [SerializeField] Text textDealerInfo;
    //ゲームで使うカード
    List<CardController> cards;
    //手札
    List<CardController> playerHand;
    List<CardController> dealerHand;
    //山札のインデックス
    int cardIndex;
    //ウェイト時間
    const float NextWaitTime = 1;

    AudioSource audioPlayer;
    [SerializeField] AudioClip win;
    [SerializeField] AudioClip lose;


    // Start is called before the first frame update
    void Start()
    {
        //シャッフルしたカードを取得
        cards = cardsDirector.GetShuffleCards();
        //初期位置
        foreach (var item in cards)
        {
            item.transform.position = new Vector3(100, 0, 0);
            item.FlipCard(false);
        }

        //手札の初期化
        playerHand = new List<CardController>();
        dealerHand = new List<CardController>();

        cardIndex = 0;

        //ディーラーの手札を追加
        CardController card;

        card = hitCard(dealerHand);
        card = hitCard(dealerHand);
        card.FlipCard();

        //プレイヤーの手札を追加
        hitCard(playerHand).FlipCard();
        hitCard(playerHand).FlipCard();

        //テキストを更新
        textPlayerInfo.text = "" + getScore(playerHand);

        audioPlayer = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    //ヒット
    CardController hitCard(List<CardController> hand)
    {
        float x = -0.1f;
        float z = -0.05f;

        //ディーラーの初期位置
        if (dealerHand == hand)
        {
            z = 0.1f;
        }
        //カードがあるなら右に並べる
        if (0 < hand.Count)
        {
            x = hand[hand.Count - 1].transform.position.x;
            z = hand[hand.Count - 1].transform.position.z;
        }
        //山札のインデックスからカードを取得
        CardController card = cards[cardIndex];
        card.transform.position = new Vector3(x + CardController.Width, 0, z);
        hand.Add(card);
        cardIndex++;

        return card;
    }

    //手札をカウント
    int getScore(List<CardController> hand)
    {
        int score = 0;
        List<CardController> ace = new List<CardController>();

        //手札を計算
        foreach (var item in hand)
        {
            int no = item.No;

            if (1 == no)
            {
                ace.Add(item);
            }
            //J Q K の計算
            else if (10 < no)
            {
                no = 10;
            }

            score += no;
        }

        //Aを11にする
        foreach (var item in ace)
        {
            if ((score + 10) < 22)
            {
                score += 10;
            }
        }

        return score;
    }
    //プレイヤーヒットボタン
    public void OnClickHit()
    {
        //カードを一枚引く
        CardController card = hitCard(playerHand);
        card.FlipCard();
        //テキスト更新
        int score = getScore(playerHand);
        textPlayerInfo.text = "" + score;
        //21を超えていないか
        if (21 < score)
        {
            textPlayerInfo.text = "バースト!!　敗北...";

            buttonHit.gameObject.SetActive(false);
            buttonStay.gameObject.SetActive(false);

            audioPlayer.PlayOneShot(lose);
        }
    }

    //プレイヤーステイボタン
    public void OnClickStay()
    {
        //ボタンを押せないように非表示
        buttonHit.gameObject.SetActive(false);
        buttonStay.gameObject.SetActive(false);
        //伏せられている１枚目をオープン
        dealerHand[0].FlipCard();
        //テキスト更新
        int score = getScore(dealerHand);
        textDealerInfo.text = "" + score;

        //ディーラーが一枚引く
        StartCoroutine(dealerHit());
    }

    //ディーラーの番
    IEnumerator dealerHit()
    {
        //指定秒数待つ
        yield return new WaitForSeconds(NextWaitTime);
        //現在のカウント
        int score = getScore(dealerHand);
        //17以下なら強制的に引く
        if (18 > score)
        {
            CardController card = hitCard(dealerHand);
            card.FlipCard();

            textDealerInfo.text = "" + getScore(dealerHand);
        }
        //勝敗チェック
        score = getScore(dealerHand);

        //バーストしてたらプレイヤーの勝ち
        if (21 < score)
        {
            textDealerInfo.text += " バースト";
            textPlayerInfo.text = "勝利!!";
            audioPlayer.PlayOneShot(win);
        }
        //18以上ならディーラーステイで勝敗チェック
        else if (17 < score)
        {
            string textplayer = "勝利!!";
            if (getScore(playerHand) < getScore(dealerHand))
            {
                textplayer = "敗北...";
            }
            else if (getScore(playerHand) == getScore(dealerHand))
            {
                textplayer = "引き分け!!";
            }
            textPlayerInfo.text = textplayer;

            if (textplayer.Contains("勝利"))
            {
                audioPlayer.PlayOneShot(win);
            }
            else if (textplayer.Contains("敗北"))
            {
                audioPlayer.PlayOneShot(lose);
            }
        }
        else
        {
            StartCoroutine(dealerHit());
        }
    }

    //リスタート
    public void OnClickRestart()
    {
        SceneManager.LoadScene("BlackJackScene");
    }
}

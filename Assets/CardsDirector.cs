using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsDirector : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabSpades;
    [SerializeField] List<GameObject> prefabClubs;
    [SerializeField] List<GameObject> prefabDiamonds;
    [SerializeField] List<GameObject> prefabHearts;
    [SerializeField] List<GameObject> prefabJokers;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //ハイ＆ローで使うカードを作成
    public List<CardController> GetHighLowCards()
    {
        List<CardController> ret = new List<CardController>();

        ret.AddRange(createCards(SuitType.Spade));
        ret.AddRange(createCards(SuitType.Club));
        ret.AddRange(createCards(SuitType.Diamond));
        ret.AddRange(createCards(SuitType.Heart));

        ShuffleCards(ret);

        return ret;
    }

    //シャッフルしたカードを返す
    public List<CardController> GetShuffleCards()
    {
        List<CardController> ret = new List<CardController>();

        ret.AddRange(createCards(SuitType.Spade));
        ret.AddRange(createCards(SuitType.Club));
        ret.AddRange(createCards(SuitType.Diamond));
        ret.AddRange(createCards(SuitType.Heart));

        ShuffleCards(ret);

        return ret;
    }

    //神経衰弱で使うカードを返す
    public List<CardController> GetMemoryCards()
    {
        List<CardController> ret = new List<CardController>();

        ret.AddRange(createCards(SuitType.Spade, 10));
        ret.AddRange(createCards(SuitType.Diamond, 10));

        ShuffleCards(ret);

        return ret;
    }

    //シャッフル
    public void ShuffleCards(List<CardController> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int rnd = Random.Range(0, cards.Count);
            CardController tmp = cards[i];

            cards[i] = cards[rnd];
            cards[rnd] = tmp;
        }
    }

    //カード作成
    List<CardController> createCards(SuitType suitType, int count = -1)
    {
        List<CardController> ret = new List<CardController>();

        //カードの種類(デフォルト)
        List<GameObject> prefabcards = prefabSpades;
        Color suitcolor = Color.black;

        if (SuitType.Club == suitType)
        {
            prefabcards = prefabClubs;
        }
        else if (SuitType.Diamond == suitType)
        {
            prefabcards = prefabDiamonds;
            suitcolor = Color.red;
        }
        else if (SuitType.Heart == suitType)
        {
            prefabcards = prefabHearts;
            suitcolor = Color.red;
        }
        else if (SuitType.Joker == suitType)
        {
            prefabcards = prefabJokers;
        }

        //枚数に指定がなければすべてのカードを作成する
        if (0 > count)
        {
            count = prefabcards.Count;
        }

        //カード生成
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefabcards[i]);

            //当たり判定追加
            BoxCollider bc = obj.AddComponent<BoxCollider>();
            //当たり判定検知用
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            //カード同士の当たり判定と物理演算を行わない
            bc.isTrigger = true;
            rb.isKinematic = true;

            //カードにデータをセット
            CardController ctrl = obj.AddComponent<CardController>();

            ctrl.Suit = suitType;
            ctrl.SuitColor = suitcolor;
            ctrl.PlayerNo = -1;
            ctrl.No = i + 1;

            ret.Add(ctrl);
        }

        return ret;
    }
}

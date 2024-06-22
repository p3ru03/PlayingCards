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

    //�n�C�����[�Ŏg���J�[�h���쐬
    public List<CardController> GetHighLowCards()
    {
        List<CardController> ret =new List<CardController>();

        ret.AddRange(createCards(SuitType.Spade));
        ret.AddRange(createCards(SuitType.Club));
        ret.AddRange(createCards(SuitType.Diamond));
        ret.AddRange(createCards(SuitType.Heart));

        ShuffleCards(ret);

        return ret;
    }
    //�V���b�t��
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

    //�J�[�h�쐬
    List<CardController> createCards(SuitType suitType)
    {
        List<CardController> ret = new List<CardController>();

        //�J�[�h�̎��(�f�t�H���g)
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

        //�J�[�h����
        for (int i = 0; i < prefabcards.Count; i++)
        {
            GameObject obj = Instantiate(prefabcards[i]);

            //�����蔻��ǉ�
            BoxCollider bc = obj.AddComponent<BoxCollider>();
            //�����蔻�茟�m�p
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            //�J�[�h���m�̓����蔻��ƕ������Z���s��Ȃ�
            bc.isTrigger = true;
            rb.isKinematic = true;

            //�J�[�h�Ƀf�[�^���Z�b�g
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

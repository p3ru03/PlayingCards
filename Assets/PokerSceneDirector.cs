using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PokerSceneDirector : MonoBehaviour
{
    // �G�f�B�^����ݒ�
    [SerializeField] CardsDirector cardsDirector;
    [SerializeField] Button buttonBetCoin;
    [SerializeField] Button buttonPlay;
    [SerializeField] Button buttonChange;
    [SerializeField] Text textGameInfo;
    [SerializeField] Text textRate;
    // �{�^�����̃e�L�X�g
    Text textButtonBetCoin;
    Text textButtonChange;
    // �S�J�[�h
    List<CardController> cards;
    // ��D
    List<CardController> hand;
    // ��������J�[�h
    List<CardController> selectCards;
    // �R�D�̃C���f�b�N�X�ԍ�
    int dealCardCount;
    // �v���C���[�̎����R�C��
    [SerializeField] int playerCoin;
    // �����ł����
    [SerializeField] int cardChangeCountMax;
    // �x�b�g�����R�C��
    int betCoin;
    // ����������
    int cardChangeCount;
    // �{���ݒ�
    int straightFlushRate = 10;
    int fourCardRate = 8;
    int fullHouseRate = 6;
    int flushRate = 5;
    int straightRate = 4;
    int threeCardRate = 3;
    int twoPairRate = 2;
    int onePairhRate = 1;
    // �A�j���[�V��������
    const float SortHandTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        // �J�[�h�擾
        cards = cardsDirector.GetShuffleCards();

        // �z��f�[�^������
        hand = new List<CardController>();
        selectCards = new List<CardController>();

        // �{�^�����̃e�L�X�g�擾
        textButtonBetCoin = buttonBetCoin.GetComponentInChildren<Text>();
        textButtonChange = buttonChange.GetComponentInChildren<Text>();

        // �R�D������
        restartGame(false);
        // �e�L�X�g�ƃ{�^����������
        updateTexts();
        setButtonsInPlay(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // Ray���쐬���ē���
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // �q�b�g�����I�u�W�F�N�g����CardController���擾
                CardController card = hit.collider.gameObject.GetComponent<CardController>();
                // �J�[�h�I������
                setSelectCard(card);
            }
        }
    }

    // ��D��������
    CardController addHand()
    {
        // �R�D����J�[�h���擾���ăC���f�b�N�X��i�߂�
        CardController card = cards[dealCardCount++];
        // ��D�ɉ�����
        hand.Add(card);
        // �������J�[�h��Ԃ�
        return card;
    }

    // ��D���߂���
    void openHand(CardController card)
    {
        // ��]�A�j���[�V����
        card.transform.DORotate(Vector3.zero, SortHandTime)
            .OnComplete(() => { card.FlipCard(); });
    }

    // ��D����ׂ�
    void sortHand()
    {
        // �����ʒu
        float x = -CardController.Width * 2;
        // ��D�𖇐������ׂ�
        foreach (var item in hand)
        {
            // �\���ʒu�փA�j���[�V�������Ĉړ�
            Vector3 pos = new Vector3(x, 0, 0);
            item.transform.DOMove(pos, SortHandTime);
            // ����̕\���ʒux
            x += CardController.Width;
        }
    }

    // �Q�[�����X�^�[�g
    void restartGame(bool deal = true)
    {
        // ��D�A�I���J�[�h�����Z�b�g
        hand.Clear();
        selectCards.Clear();
        // �J�[�h��������񐔂����Z�b�g
        cardChangeCount = cardChangeCountMax;
        // �R�D������������������Z�b�g
        dealCardCount = 0;

        // �J�[�h�V���b�t��
        cardsDirector.ShuffleCards(cards);

        // �J�[�h�����ݒ�
        foreach (var item in cards)
        {
            // �̂ĎD�͔�\����ԂȂ̂ŕ\������
            item.gameObject.SetActive(true);
            // �������ɂ���
            item.FlipCard(false);
            // �R�D�̏ꏊ��
            item.transform.position = new Vector3(0, 0, 0.18f);
        }

        // �������牺�͔z�鏈��
        if (!deal) return;

        // 5���z���ĕ\�����ɂ���
        for (int i = 0; i < 5; i++)
        {
            openHand(addHand());
        }

        // �J�[�h����ׂ�
        sortHand();
    }

    // ���[�g�\���X�V
    void updateTexts()
    {
        textButtonBetCoin.text = "�莝���R�C�� " + playerCoin;
        textGameInfo.text = "BET���� " + betCoin;

        textRate.text = "�X�g���[�g�t���b�V�� " + (straightFlushRate * betCoin) + "\n"
            + "�t�H�[�J�[�h " + (fourCardRate * betCoin) + "\n"
            + "�t���n�E�X " + (fullHouseRate * betCoin) + "\n"
            + "�t���b�V�� " + (flushRate * betCoin) + "\n"
            + "�X�g���[�g " + (straightRate * betCoin) + "\n"
            + "�X���[�J�[�h " + (threeCardRate * betCoin) + "\n"
            + "�c�[�y�A " + (twoPairRate * betCoin) + "\n"
            + "�����y�A " + (onePairhRate * betCoin) + "\n";
    }

    // �Q�[�����̃{�^����\������
    void setButtonsInPlay(bool disp = true)
    {
        textButtonChange.text = "�I��";
        // �����{�^���\���ݒ�i�Q�[���v���C�j
        buttonChange.gameObject.SetActive(disp);
        // �x�b�g�ƃv���C�{�^���\���ݒ�i��Q�[���v���C�j
        buttonBetCoin.gameObject.SetActive(!disp);
        buttonPlay.gameObject.SetActive(!disp);
    }

    // �R�C�����x�b�g����
    public void OnClickBetCoin()
    {
        if (1 > playerCoin) return;

        // �R�C�������炵�ăe�L�X�g���X�V
        playerCoin--;
        betCoin++;
        updateTexts();
    }

    // �Q�[���v���C�{�^��
    public void OnClickPlay()
    {
        // �f�b�L�Ǝ�D��������
        restartGame();
        // �Q�[�����̃{�^���ƃe�L�X�g�̍X�V
        setButtonsInPlay();
        updateTexts();
    }

    // �J�[�h�I�����
    void setSelectCard(CardController card)
    {
        // �I���ł��Ȃ��J�[�h�Ȃ�I��
        if (!card || !card.isFrontUp) return;

        // �J�[�h�̌��ݒn
        Vector3 pos = card.transform.position;

        // 2��ڑI�����ꂽ���I��
        if (selectCards.Contains(card))
        {
            pos.z -= 0.02f;
            selectCards.Remove(card);
        }
        // �I����ԁi�J�[�h����𒴂��Ȃ��悤�Ɂj
        else if (cards.Count > dealCardCount + selectCards.Count)
        {
            pos.z += 0.02f;
            selectCards.Add(card);
        }

        // �X�V���ꂽ�ꏊ
        card.transform.position = pos;

        // �{�^���X�V�i�I�𖇐���0���Ȃ�I���{�^���ɕύX�j
        textButtonChange.text = "����";
        if (1 > selectCards.Count)
        {
            textButtonChange.text = "�I��";
        }
    }

    // �J�[�h����
    public void OnClickChange()
    {
        // �������Ȃ��Ȃ�1��ŏI��
        if (1 > selectCards.Count)
        {
            cardChangeCount = 0;
        }

        // �̂ăJ�[�h����D����폜
        foreach (var item in selectCards)
        {
            item.gameObject.SetActive(false);
            hand.Remove(item);
            // �̂Ă����J�[�h��ǉ�
            openHand(addHand());
        }
        selectCards.Clear();

        // ���ׂ�
        sortHand();
        setButtonsInPlay();

        // �J�[�h�����\��
        cardChangeCount--;
        if (1 > cardChangeCount)
        {
            // ���𐸎Z����
            checkHandRank();
        }
    }

    // ���𐸎Z����
    void checkHandRank()
    {
        // �t���b�V���`�F�b�N
        bool flush = true;
        // 1���ڂ̃J�[�h�̃}�[�N
        SuitType suit = hand[0].Suit;

        foreach (var item in hand)
        {
            // 1���ڂƈ������I��
            if (suit != item.Suit)
            {
                flush = false;
                break;
            }
        }

        // �X�g���[�g�`�F�b�N
        bool straight = false;
        for (int i = 0; i < hand.Count; i++)
        {
            // �����������A��������
            int straightcount = 0;
            // ���݂̃J�[�h�ԍ�
            int cardno = hand[i].No;

            // 1���ڂ���A�����Ă��邩���ׂ�
            for (int j = 0; j < hand.Count; j++)
            {
                // �����J�[�h�̓X�L�b�v
                if (i == j) continue;

                // �������������͌��݂̐���+1
                int targetno = cardno + 1;
                // 13�̎���1
                if (13 < targetno) targetno = 1;

                // �^�[�Q�b�g�̐�������
                if (targetno == hand[j].No)
                {
                    // �A���񐔂��J�E���g
                    straightcount++;
                    // ����̃J�[�h�ԍ�(����+1�����)
                    cardno = hand[j].No;
                    // j�͂܂�0����n�߂�
                    j = -1;
                }
            }

            if (3 < straightcount)
            {
                straight = true;
                break;
            }
        }

        // ���������̃`�F�b�N
        int pair = 0;
        bool threecard = false;
        bool fourcard = false;
        List<CardController> checkcards = new List<CardController>();

        for (int i = 0; i < hand.Count; i++)
        {
            if (checkcards.Contains(hand[i])) continue;

            // ���������̃J�[�h����
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

            // �����y�A�A�c�[�y�A�A�X���[�J�[�h�A�t�H�[�J�[�h����
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

        // �t���n�E�X
        bool fullhouse = false;
        if (1 == pair && threecard)
        {
            fullhouse = true;
        }

        // �X�g���[�g�t���b�V��
        bool straightflush = false;
        if (flush && straight)
        {
            straightflush = true;
        }

        // ���̔���
        int addcoin = 0;
        string infotext = "�𖳂�... ";

        if (straightflush)
        {
            addcoin = straightFlushRate * betCoin;
            infotext = "�X�g���[�g�t���b�V��!! ";
        }
        else if (fourcard)
        {
            addcoin = fourCardRate * betCoin;
            infotext = "�t�H�[�J�[�h!! ";
        }
        else if (fullhouse)
        {
            addcoin = fullHouseRate * betCoin;
            infotext = "�t���n�E�X!! ";
        }
        else if (flush)
        {
            addcoin = flushRate * betCoin;
            infotext = "�t���b�V��!! ";
        }
        else if (straight)
        {
            addcoin = straightRate * betCoin;
            infotext = "�X�g���[�g!! ";
        }
        else if (threecard)
        {
            addcoin = threeCardRate * betCoin;
            infotext = "�X���[�J�[�h!! ";
        }
        else if (2 == pair)
        {
            addcoin = twoPairRate * betCoin;
            infotext = "�c�[�y�A!! ";
        }
        else if (1 == pair)
        {
            addcoin = onePairhRate * betCoin;
            infotext = "�����y�A!! ";
        }

        // �R�C���擾
        playerCoin += addcoin;

        // �e�L�X�g�X�V
        updateTexts();
        textGameInfo.text = infotext + addcoin;

        // ����̃Q�[���p
        betCoin = 0;
        setButtonsInPlay(false);
    }
}

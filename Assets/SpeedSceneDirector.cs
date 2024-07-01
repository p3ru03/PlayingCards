using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpeedSceneDirector : MonoBehaviour
{
    // ���ʃJ�[�h�Ǘ��N���X
    [SerializeField] CardsDirector cardsDirector;
    // ���s�\��
    [SerializeField] Text textResultInfo;
    // �S�J�[�h
    List<CardController> cards;
    // �v���C���[��D
    List<CardController> hand1p, hand2p;
    // �v���C���[��D
    List<CardController> layout1p, layout2p;
    // ��D(0��1�E)
    CardController[] leadCards;
    // �I�𒆂̃J�[�h
    CardController selectCard;
    // CPU�v�l����
    float cpuTimer;
    // �Q�[���N���A�t���O
    bool isGameEnd;

    // ��D�ɒu���ꂽ�J�[�h�̃v���C���[�ԍ�
    const int LeadCardNo = 100;
    // CPU�����_���s������
    const float CpuRandomTimerMin = 3;
    const float CpuRandomTimerMax = 6;
    // ��D���Z�b�g�X�s�[�h
    const float RestartMoveSpeed = 0.8f;
    const float RestartRotateSpeed = 0.2f;
    // ��D����D�X�s�[�h
    const float DealCardMoveSpeed = 0.5f;
    // ��D����D�X�s�[�h
    const float PlayCardMoveSpeed = 0.8f;
    // 1P��D�|�W�V����
    const float HandPositionX = 0.15f;
    const float HandPositionZ = -0.1f;
    // 1P��D�X�^�[�g�|�W�V����
    const float LayoutPositionX = -0.12f;
    const float LayoutPositionZ = -0.13f;
    // ��D�����|�W�V����
    const float LeadPositionX = -0.05f;
    // �J�[�h���d�˂鎞�̃T�C�Y
    const float StackCardHeight = 0.0001f;

    // Start is called before the first frame update
    void Start()
    {
        // �V���b�t�����ꂽ�J�[�h���擾
        cards = cardsDirector.GetShuffleCards();

        // �f�[�^������
        hand1p = new List<CardController>();
        hand2p = new List<CardController>();
        layout1p = new List<CardController>();
        layout2p = new List<CardController>();
        leadCards = new CardController[2];
        textResultInfo.text = "";

        // �R�D����ׂ�
        CardController firstcard = addCardHand(hand1p, cards[0]);
        // 1���ڂɒǉ����ꂽ�J�[�h�����F��1P��
        foreach (var item in cards)
        {
            List<CardController> hand = hand1p;
            // 1���ڂƈႤ�Ȃ�2P�̎�D��
            if (firstcard.SuitColor != item.SuitColor) hand = hand2p;

            addCardHand(hand, item);
        }

        // ��D����ׂ�
        shuffleLayout(layout1p);
        shuffleLayout(layout2p);

        // ��D����ׂ�
        playCardFromHand(hand1p);
        playCardFromHand(hand2p);

        // CPU�s���^�C�}�[
        cpuTimer = Random.Range(CpuRandomTimerMin, CpuRandomTimerMax);
    }

    // Update is called once per frame
    void Update()
    {
        // ���s�����Ă�����I��
        if (isGameEnd) return;

        // ��D�����Z�b�g���ꂽ�甲����
        if (tryResetLeadCards()) return;

        // �v���C���[����
        if (Input.GetMouseButtonUp(0))
        {
            playerSelectCard();
        }

        // CPU����
        cpuTimer -= Time.deltaTime;
        if (0 > cpuTimer)
        {
            autoSelectCard(layout2p);
            // ����CPU�s������
            cpuTimer = Random.Range(CpuRandomTimerMin, CpuRandomTimerMax);
        }
    }

    // ��D�ɃJ�[�h��ǉ�����
    CardController addCardHand(List<CardController> hand, CardController card)
    {
        // 1P�ݒ�
        int playerno = 0;
        int dir = 1;
        // 2P�ݒ�
        if (hand2p == hand)
        {
            playerno = 1;
            dir *= -1;
        }

        // �J�[�h���Ȃ������ɒǉ��ς�
        if (!card || hand.Contains(card)) return null;

        card.transform.position = new Vector3(HandPositionX * dir, 0, HandPositionZ * dir);
        card.PlayerNo = playerno;
        card.FlipCard(false);

        hand.Add(card);
        // �ǉ������J�[�h��Ԃ�
        return card;
    }

    // ��D����1���J�[�h������
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

    // �J�[�h���ړ����ĕ\�����ɂ���
    void moveCardOpen(CardController card, Vector3 pos, float speed)
    {
        card.transform.DOKill();

        card.transform.DORotate(Vector3.zero, speed);
        card.transform.DOMove(pos, speed).OnComplete(() => card.FlipCard());
    }

    // ��D�̃J�[�h���ꖇ�����ď�D�ֈړ�����
    void dealCard(List<CardController> hand)
    {
        // ��D��1���擾
        CardController card = getCardHand(hand);
        if (!card) return;

        // 1P�ݒ�
        List<CardController> layout = layout1p;
        int dir = 1;
        // 2P�ݒ�
        if (hand2p == hand)
        {
            layout = layout2p;
            dir *= -1;
        }

        // ��D�̋󂢂Ă���ꏊ��1���J�[�h������
        for (int i = 0; i < layout.Count; i++)
        {
            // �J�[�h������Ȃ�X�L�b�v
            if (layout[i]) continue;

            // �����f�[�^�X�V
            layout[i] = card;

            // �ڕW�ʒu
            float x = (i * CardController.Width + LayoutPositionX) * dir;
            float z = LayoutPositionZ * dir;
            Vector3 pos = new Vector3(x, 0, z);
            // ���ʒu��ۑ�����
            card.HandPosition = pos;

            // �A�j���[�V����
            float dist = Vector3.Distance(card.transform.position, pos);
            moveCardOpen(card, pos, dist / DealCardMoveSpeed);
            // 1���ǉ����ꂽ��I��
            break;
        }
    }

    // ��D�����ւ���
    void shuffleLayout(List<CardController> layout)
    {
        // ��D�ɃA�j���[�V�������̂��̂�����Ώ������Ȃ�
        foreach (var item in layout)
        {
            if (!item) continue;
            if (DOTween.IsTweening(item.transform)) return;
        }

        // 1P�ݒ�
        List<CardController> hand = hand1p;
        // 2P�ݒ�
        if (layout == layout2p) hand = hand2p;

        // ��D����D�ɖ߂�
        foreach (var item in layout)
        {
            addCardHand(hand, item);
        }
        layout.Clear();

        // �J�[�h�V���b�t��
        cardsDirector.ShuffleCards(hand);

        // 4�����ׂ�
        for (int i = 0; i < 4; i++)
        {
            layout.Add(null);
            dealCard(hand);
        }
    }

    // ��D���X�V����
    void updateLead(int index, CardController card)
    {
        // �v���C���[�ԍ����D�ɐݒ�
        card.PlayerNo = LeadCardNo;
        // 0��1�E
        card.Index = index;
        // �f�[�^�X�V
        leadCards[index] = card;
    }

    // �����I�Ɏ�D����1����D�ɏo���A�Ȃ���Ώ�D����o��
    void playCardFromHand(List<CardController> hand)
    {
        // 1P�ݒ�
        int index = 0;
        int dir = 1;
        List<CardController> layout = layout1p;
        // 2P�ݒ�
        if (hand2p == hand)
        {
            index = 1;
            dir *= -1;
            layout = layout2p;
        }

        // �J�[�h����D����擾
        CardController card = getCardHand(hand);

        // ��D���Ȃ���Ώ�D���烉���_���ŋ����I�ɏo���ďI��
        if (!card)
        {
            foreach (var item in layout)
            {
                if (!item) continue;
                playCardFromLayout(item, index, true);
                return;
            }
        }

        // ���łɃJ�[�h������Ώ�����ɒu��
        float y = 0;
        if (leadCards[index])
        {
            y = leadCards[index].transform.position.y + StackCardHeight;
        }

        // �ړI�n
        Vector3 pos = new Vector3(LeadPositionX * dir, y, 0);
        // ����
        float dist = Vector3.Distance(card.transform.position, pos);
        // �A�j���[�V����
        card.transform.DORotate(Vector3.zero, RestartRotateSpeed);
        card.transform.DOMove(pos, dist / RestartMoveSpeed)
        .OnComplete(() =>
        {
            updateLead(index, card);
            card.FlipCard();
        });
    }

    // �J�[�h�I��
    void setSelectCard(CardController card = null)
    {
        // ��U��I�����
        if (selectCard)
        {
            selectCard.gameObject.transform.position = selectCard.HandPosition;
            selectCard = null;
        }

        if (!card) return;

        // �I�񂾊����ɂ���
        Vector3 pos = card.transform.position;
        pos.z += 0.02f;
        card.transform.position = pos;

        selectCard = card;
    }

    // �v���C���[�̏���
    void playerSelectCard()
    {
        // Ray�𓊎�
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // �����蔻�肪�Ȃ���ΏI��
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // �q�b�g�����Q�[���I�u�W�F�N�g����CardController���擾
        CardController card = hit.collider.gameObject.GetComponent<CardController>();

        // �J�[�h����Ȃ��������Ă���ΏI��
        if (!card || !card.isFrontUp) return;

        // �J�[�h�I����Ԃő�D�̃J�[�h�������ꂽ
        if (selectCard && LeadCardNo == card.PlayerNo)
        {
            playCardFromLayout(selectCard, card.Index);
            setSelectCard();
        }
        // ��D
        else if (0 == card.PlayerNo)
        {
            setSelectCard(card);
        }
    }

    // �u���邩�ǂ����`�F�b�N
    bool isMovable(CardController movecard, CardController leadcard)
    {
        // �J�[�h��񂪂Ȃ�
        if (!movecard || !leadcard) return false;
        // �u�����Ƃ��Ă�J�[�h���A�j���[�V�������͒u���Ȃ�
        if (DOTween.IsTweening(movecard.transform)) return false;

        // �����鐔���̃`�F�b�N
        int min = leadcard.No - 1;
        if (1 > min) min = 13;

        int max = leadcard.No + 1;
        if (13 < max) max = 1;

        // 1��������1�傫����΂�����
        if (min == movecard.No || max == movecard.No)
        {
            return true;
        }

        return false;
    }

    // ��D�̃J�[�h������Ԃ�
    int cardCount(List<CardController> layout)
    {
        int count = 0;
        foreach (var item in layout)
        {
            if (item) count++;
        }
        return count;
    }

    // ���s�`�F�b�N
    void checkResult()
    {
        // ���ɏ��s�����Ă�
        if (isGameEnd) return;

        if (1 > hand1p.Count && 1 > cardCount(layout1p))
        {
            textResultInfo.text = "1P����!!";
            isGameEnd = true;
        }
        else if (1 > hand2p.Count && 1 > cardCount(layout2p))
        {
            textResultInfo.text = "2P����!!";
            isGameEnd = true;
        }
    }

    // ��D�����D��1���o��
    void playCardFromLayout(CardController card, int index, bool force = false)
    {
        // 1P�ݒ�
        List<CardController> layout = layout1p;
        List<CardController> hand = hand1p;
        // 2P�ݒ�
        if (1 == card.PlayerNo)
        {
            layout = layout2p;
            hand = hand2p;
        }

        // �ړI�n
        Vector3 pos = leadCards[index].transform.position;
        // �J�[�h�̏�ɂ̂�悤��
        pos.y += StackCardHeight;
        // ��D�Ƒ�D�̋���
        float dist = Vector3.Distance(card.transform.position, pos);

        // �����_���Ŋp�x�����炷
        float ry = Random.Range(-15.0f, 15.0f);
        card.transform.DORotate(new Vector3(0, ry, 0), dist / PlayCardMoveSpeed);

        // �ړ��������ɑ�D�̏�Ԃ��`�F�b�N���邽�߂�OnComplete������
        card.transform.DOMove(pos, dist / PlayCardMoveSpeed)
        .OnComplete(() =>
        {
            // �ړ�������A��D�ɒu���邩���ׂ�
            if (isMovable(card, leadCards[index]) || force)
            {
                // ��D�X�V
                updateLead(leadCards[index].Index, card);
                // ��D�X�V
                layout[layout.IndexOf(card)] = null;
                // ��D����1������
                dealCard(hand);
                // ���s�`�F�b�N
                checkResult();
            }
            // �����Ȃ��������͏�D�ɖ߂�
            else
            {
                moveCardOpen(card, card.HandPosition, dist / PlayCardMoveSpeed);
            }
        });

    }

    // �V���b�t���{�^��
    public void OnClickShuffle()
    {
        setSelectCard();
        shuffleLayout(layout1p);
    }

    // �w�肳�ꂽ��D����o����J�[�h������Ώo��
    void autoSelectCard(List<CardController> layout)
    {
        // ��D
        foreach (var layoutcard in layout)
        {
            // ��D
            foreach (var leadcard in leadCards)
            {
                // �u���Ȃ��J�[�h���X�L�b�v
                if (!isMovable(layoutcard, leadcard)) continue;

                // �u������I��
                playCardFromLayout(layoutcard, leadcard.Index);
                return;
            }
        }

        // �����Ȃ��������D���V���b�t��
        shuffleLayout(layout);
    }

    // �S�J�[�h���o���Ȃ���Α�D�����Z�b�g����
    bool tryResetLeadCards()
    {
        // �Ȃɂ��A�j���[�V������
        if (null != DOTween.PlayingTweens()) return false;

        // 1P��2P�̏�D�̃��X�g���쐬
        List<CardController> alllayout = new List<CardController>(layout1p);
        alllayout.AddRange(layout2p);

        // �S�ẴJ�[�h���ړ��\�����ׂ�
        foreach (var layoutcard in alllayout)
        {
            foreach (var leadcard in leadCards)
            {
                if (isMovable(layoutcard, leadcard)) return false;
            }
        }

        // ���܂ł̑�D���\��
        foreach (var item in cards)
        {
            if (LeadCardNo == item.PlayerNo) item.gameObject.SetActive(false);
        }

        // �������Ȃ���Α�D�����Z�b�g
        setSelectCard();
        playCardFromHand(hand1p);
        playCardFromHand(hand2p);

        return true;
    }

    // �V�[���ēǂݍ���
    public void OnClickRestart()
    {
        SceneManager.LoadScene("SpeedScene");
    }
}

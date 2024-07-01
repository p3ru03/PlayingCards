using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SolitaireSceneDirector : MonoBehaviour
{
    // �J�[�h�Ǘ��N���X
    [SerializeField] CardsDirector cardsDirector;
    // �Q�[���^�C�}�[
    [SerializeField] Text textTimer;

    // �R�D�y��
    [SerializeField] GameObject stock;
    // �g�D�y��
    [SerializeField] List<Transform> foundation;
    // ��D�y��
    [SerializeField] List<Transform> column;

    // �S�ẴJ�[�h
    List<CardController> cards;
    // �R�D�̃J�[�h
    List<CardController> stockCards;
    // �߂���D�̃J�[�h
    List<CardController> wasteCards;
    // �I�𒆂̃J�[�h
    CardController selectCard;
    // �h���b�O�J�n�ʒu
    Vector3 startPosition;
    // �Q�[���I���t���O
    bool isGameEnd;
    // �Q�[���^�C�}�[
    float gameTimer;
    int oldSecond;
    // �J�[�h����ׂ鎞�̃T�C�Y
    const float StackCardHeight = 0.0001f;
    const float StackCardWidth = 0.02f;
    // �A�j���[�V��������
    const float SortWasteCardTime = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        // �V���b�t�����ꂽ�J�[�h���擾
        cards = cardsDirector.GetShuffleCards();

        // �f�[�^������
        wasteCards = new List<CardController>();
        stockCards = new List<CardController>();

        // �J�[�h�����ݒ�
        foreach (var item in cards)
        {
            item.PlayerNo = 0;
            item.FlipCard(false);
            // �R�D�֒ǉ�
            stockCards.Add(item);
        }

        // ��D����ׂ�
        int cardindex = 0;
        int columncount = 0;

        foreach (var item in column)
        {
            // ��D�ɉ����u����
            columncount++;
            for (int i = 0; i < columncount; i++)
            {
                // �J�[�h������Ȃ��������͏I��
                if (cards.Count - 1 < cardindex) break;

                // ����ǉ�����J�[�h
                CardController card = cards[cardindex];
                // �e�I�u�W�F�N�g
                CardController parent = item.GetComponent<CardController>();
                // 1�ԉ��ȊO��1�O�̃J�[�h�̏�ɒu��
                if (0 != i) parent = cards[cardindex - 1];

                // �J�[�h���Z�b�g����
                putCard(parent, card);
                // �ǉ������J�[�h���R�D����폜
                stockCards.Remove(card);

                // �J�[�h�����֑���
                cardindex++;
            }

            // �Ō�ɒǉ������J�[�h���߂���
            cards[cardindex - 1].FlipCard();
        }

        // �R�D����ׂ�
        stackStockCards();
    }

    // Update is called once per frame
    void Update()
    {
        // �Q�[���N���A
        if (isGameEnd) return;

        // �o�ߎ���
        gameTimer += Time.deltaTime;
        // �^�C�}�[�\��
        textTimer.text = getTimerText(gameTimer);

        // �}�E�X�������ꂽ
        if (Input.GetMouseButtonDown(0))
        {
            setSelectCard();
        }
        // �h���b�O��
        else if (Input.GetMouseButton(0))
        {
            moveCard();
        }
        // �����ꂽ
        else if (Input.GetMouseButtonUp(0))
        {
            releaseCard();
        }
    }

    // �J�[�h���ړ�������
    void putCard(CardController parent, CardController child)
    {
        // �e�I�u�W�F�N�g�w��
        child.transform.parent = parent.transform;
        // �ړ���
        Vector3 pos = parent.transform.position;
        // ��ɂ��炷
        pos.y += StackCardHeight;

        // ��D�̃J�[�h�Ȃ炚�����炷�i�y��ȊO�j
        if (column.Contains(parent.transform.root) && !column.Contains(parent.transform))
        {
            pos.z -= StackCardWidth;
        }

        // �X�V���ꂽ�ꏊ
        child.transform.position = pos;

        // �߂���D�������烊�X�g����폜
        wasteCards.Remove(child);
    }

    // �R�D����ׂ�
    void stackStockCards()
    {
        // �R�D�̏�ɕ��ׂ�
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

    // �^�C�}�[�\�����擾����
    string getTimerText(float timer)
    {
        // �b�����擾
        int sec = (int)timer % 60;
        // �O��ƕb���������Ȃ�O��̕\���l��Ԃ�
        string ret = textTimer.text;

        // �O��ƕb���ɈႢ�����邩
        if (oldSecond != sec)
        {
            // �������v�Z
            int min = (int)timer / 60;
            // min��sec��00�̂悤�ȕ�����ɂ���i0���߁A�[���p�f�B���O�j
            string pmin = string.Format("{0:D2}", min);
            string psec = string.Format("{0:D2}", sec);

            ret = pmin + ":" + psec;
            oldSecond = sec;
        }

        return ret;
    }

    // �X�N���[�����烏�[���h�|�W�V�����֕ϊ�
    Vector3 getScreenToWorldPosition()
    {
        // �}�E�X�̍��W�i�X�N���[�����W�j
        Vector3 cameraposition = Input.mousePosition;
        // �J�����̂����W��ݒ�
        cameraposition.z = Camera.main.transform.position.y;
        // �X�N���[�����W�����[���h���W
        Vector3 worldposition = Camera.main.ScreenToWorldPoint(cameraposition);

        return worldposition;
    }

    // �߂���D����ׂ�
    void sortWasteCards()
    {
        // �R�D�̍������J�n�ʒu�ɐݒ�
        float startx = stock.transform.position.x - CardController.Width * 2;

        for (int i = 0; i < wasteCards.Count; i++)
        {
            CardController card = wasteCards[i];
            // �������E��
            float x = startx + i * StackCardWidth;
            // ���������
            float y = i * StackCardHeight;
            // �A�j���[�V����
            card.transform.DOMove(new Vector3(x, y, stock.transform.position.z), SortWasteCardTime);
        }
    }

    // �J�[�h�I��
    void setSelectCard()
    {
        // �����蔻�肪�Ȃ���ΏI��
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // �����蔻�肪�������I�u�W�F�N�g�ƃJ�[�h
        GameObject obj = hit.collider.gameObject;
        CardController card = obj.GetComponent<CardController>();

        // �I����ԉ���
        selectCard = null;

        // �R�D���Ō�܂ł߂���ꂽ
        if (obj == stock)
        {
            // �߂���D�Ǝ̂Ă�ꂽ�J�[�h���f�b�L�֖߂�
            stockCards.AddRange(wasteCards);
            foreach (var item in cards)
            {
                if (item.gameObject.activeSelf) continue;

                item.gameObject.SetActive(true);
                stockCards.Add(item);
            }
            wasteCards.Clear();

            // �V���b�t�����čēx���ׂ�
            cardsDirector.ShuffleCards(stockCards);
            stackStockCards();
        }

        // �J�[�h����Ȃ����y��Ȃ�I��
        if (!card || 0 > card.PlayerNo) return;

        // �I�[�v������Ă���J�[�h
        if (card.isFrontUp)
        {
            // �߂���D�̃J�[�h��������1�Ԏ�O�̂ݑI���\
            if (wasteCards.Contains(card) && card != wasteCards[wasteCards.Count - 1]) return;

            // �����Ȃ��������ɖ߂�ꏊ�i���ݒn�j
            card.HandPosition = card.transform.position;
            // �J�[�h�I�����
            selectCard = card;
            // �h���b�O�p�Ɍ��݂̃|�W�V�������擾
            startPosition = getScreenToWorldPosition();
        }
        // ���I�[�v���̃J�[�h
        else
        {
            // 1�Ԏ�O�Ȃ�I�[�v���i���ʁj
            if (1 > card.transform.childCount)
            {
                card.transform.DORotate(Vector3.zero, SortWasteCardTime)
                    .OnComplete(() => { card.FlipCard(); });
            }

            // �R�D�̃J�[�h���߂���D�̏ꏊ��
            if (card.transform.root == stock.transform)
            {
                // 4���ڈȍ~��1�ԌÂ��J�[�h���̂ĂĔ�\��
                if (3 < wasteCards.Count + 1)
                {
                    wasteCards[0].gameObject.SetActive(false);
                    wasteCards.RemoveAt(0);
                }

                // �R�D����߂���D�ֈړ�
                stockCards.Remove(card);
                wasteCards.Add(card);

                // ���ג���
                sortWasteCards();
                stackStockCards();
            }
        }
    }

    // �J�[�h�ړ�
    void moveCard()
    {
        if (!selectCard) return;

        // �������|�W�V�����̍���
        Vector3 diff = getScreenToWorldPosition() - startPosition;
        Vector3 pos = selectCard.transform.position + diff;
        // �����������đI��������Ԃɂ���
        pos.y = 0.01f;
        selectCard.transform.position = pos;
        // �|�W�V�������X�V
        startPosition = getScreenToWorldPosition();
    }

    // �J�[�h�������ꂽ
    void releaseCard()
    {
        if (!selectCard) return;

        // 1�Ԏ�O�̃J�[�h����
        CardController frontcard = null;

        // �S�Ă̓����蔻����擾
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            // �����蔻�肪�������J�[�h
            CardController card = hit.transform.gameObject.GetComponent<CardController>();

            // �J�[�h����Ȃ����I�𒆂̃J�[�h�Ȃ�X�L�b�v
            if (!card || card == selectCard) continue;

            // 1�Ԏ�O��(�q�I�u�W�F�N�g�����Ȃ�)�J�[�h���擾
            if (!frontcard || frontcard.transform.childCount > card.transform.childCount)
            {
                frontcard = card;
            }
        }

        // �g�D�ɒu����J�[�h
        if (frontcard && foundation.Contains(frontcard.transform.root)
            && 1 > selectCard.transform.childCount
            && frontcard.No + 1 == selectCard.No
            && frontcard.Suit == selectCard.Suit)
        {
            putCard(frontcard, selectCard);

            // �N���A����
            bool fieldend = true;
            foreach (var item in column)
            {
                if (0 < item.childCount) fieldend = false;
            }

            // ��D�A�߂���D�A�R�D�̃J�[�h��1�����Ȃ���΃N���A
            isGameEnd = fieldend && 1 > wasteCards.Count && 1 > stockCards.Count;
        }
        // ��D�ɒu����J�[�h
        else if (frontcard && column.Contains(frontcard.transform.root)
            && 1 > frontcard.transform.childCount
            && frontcard.No - 1 == selectCard.No
            && frontcard.SuitColor != selectCard.SuitColor)
        {
            putCard(frontcard, selectCard);
        }
        // �u���Ȃ������猳�ɖ߂�
        else
        {
            selectCard.transform.position = selectCard.HandPosition;
        }
    }

    // �V�[���ēǂݍ���
    public void OnClickRestart()
    {
        SceneManager.LoadScene("SolitaireScene");
    }
}

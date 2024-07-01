using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoupleSceneDirector : MonoBehaviour
{
    // �J�[�h�Ǘ��N���X
    [SerializeField] CardsDirector cardsDirector;
    // �Q�[���^�C�}�[
    [SerializeField] Text textTimer;
    // �S�J�[�h
    List<CardController> cards;
    // �c���������ׂ邩
    int width = 4;
    int height = 4;
    // �I�𒆂̃J�[�h
    CardController selectCard;
    // �t�B�[���h�ɕ���ł���J�[�h
    List<CardController> fieldCards;
    // �Q�[���^�C�}�[
    float gameTimer;
    int oldSecond;
    // �t�B�[���h�����Z�b�g���鎞��
    const float FieldResetTime = 1;
    // �Q�[���I��
    bool isGameEnd;

    [SerializeField] Text textClear;

    // Start is called before the first frame update
    void Start()
    {
        // �V���b�t�����ꂽ�J�[�h���擾
        cards = cardsDirector.GetShuffleCards();

        // �t�B�[���h������
        resetField(FieldResetTime);
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

        // �{�^���������ꂽ�玞�̏���
        if (Input.GetMouseButtonUp(0))
        {
            matchCards();
        }
    }

    // �J�[�h�𓙊Ԋu�ɕ��ׂ�
    List<CardController> sortFieldCards(List<CardController> cards, int width, int height, float speed)
    {
        List<CardController> ret = new List<CardController>();

        // �J�[�h�S�̂�^�񒆂ɂ��炷���߂̃I�t�Z�b�g
        Vector2 offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f);

        // �J�[�h����ׂ�
        for (int i = 0; i < width * height; i++)
        {
            // �J�[�h������Ȃ���ΏI��
            if (cards.Count - 1 < i) break;
            // �J�[�h�f�[�^���Ȃ��ꍇ�̓X�L�b�v
            if (!cards[i]) continue;

            // �ʒu�i�C���f�b�N�X�j
            Vector2Int index = new Vector2Int(i % width, i / width);
            cards[i].Index = i;
            cards[i].IndexPosition = index;

            // �\���ʒu
            Vector3 pos = new Vector3(
                (index.x - offset.x) * CardController.Width,
                0,
                (index.y - offset.y) * CardController.Height);

            // �J�[�h�̌��ݒn�ƕ\���ʒu�̋���
            float dist = Vector3.Distance(cards[i].transform.position, pos);

            // �A�j���[�V�����i�ړ����Ȃ����]�j
            cards[i].transform.DOMove(pos, dist / speed);
            cards[i].transform.DORotate(new Vector3(0, 0, 0), dist / speed);

            // ���ׂ��J�[�h��ۑ�
            ret.Add(cards[i]);
        }

        return ret;
    }

    // �f�b�L����t�B�[���h��
    void resetField(float speed)
    {
        // �f�b�L�ֈړ��i�����f�[�^���Z�b�g�j
        foreach (var item in cards)
        {
            item.PlayerNo = -1;
            item.IndexPosition = new Vector2Int(-100, -100);
            item.transform.position = new Vector3(0.2f, 0, -0.15f);
            item.FlipCard(false);
        }

        // ���Ԋu�ɕ��ׂ��J�[�h���擾
        fieldCards = sortFieldCards(cards, width, height, speed);

        // �t�B�[���h�J�[�h��I���ł���悤�ɂ���
        foreach (var item in fieldCards)
        {
            // �I���\�ȏ��
            item.PlayerNo = 0;
            // �f�b�L����폜
            cards.Remove(item);
        }
    }

    // �J�[�h�I��
    void setSelectCard(CardController card = null)
    {
        Vector3 pos;
        // ���������I�����
        if (selectCard)
        {
            pos = selectCard.transform.position;
            selectCard.transform.position = new Vector3(pos.x, 0, pos.z);
            selectCard = null;
        }

        // �J�[�h��񂪂Ȃ���ΏI��
        if (!card) return;

        // �I�񂾏�Ԃɂ���
        selectCard = card;
        pos = selectCard.transform.position;
        pos.y += 0.02f;
        selectCard.transform.position = pos;
    }

    // �J�[�h����
    void matchCards()
    {
        // ���C�𓊎�
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        // �q�b�g�����I�u�W�F�N�g����CardController���擾
        CardController card = hit.collider.gameObject.GetComponent<CardController>();
        // �J�[�h����Ȃ��Ȃ�I��
        if (!card) return;

        // �J�[�h���m�̃C���f�b�N�X�̋���
        int dist = 0;
        if (selectCard)
        {
            dist = (int)Vector2Int.Distance(selectCard.IndexPosition, card.IndexPosition);
        }

        // 2���ڑI��
        if (1 == dist && selectCard.No == card.No)
        {
            // �t�B�[���h�̃C���f�b�N�X���X�V
            fieldCards[selectCard.Index] = null;
            fieldCards[card.Index] = null;

            // �������J�[�h���\��
            selectCard.gameObject.SetActive(false);
            card.gameObject.SetActive(false);

            // �J�[�h���߂�
            for (int i = 0; i < fieldCards.Count; i++)
            {
                if (fieldCards[i]) continue;

                // �J�[�h��T��
                for (int j = i + 1; j < fieldCards.Count; j++)
                {
                    if (!fieldCards[j]) continue;
                    fieldCards[i] = fieldCards[j];
                    fieldCards[j] = null;
                    break;
                }
            }

            // �I������
            setSelectCard();

            // �t�B�[���h�J�[�h����ג���
            sortFieldCards(fieldCards, width, height, FieldResetTime / 2);

            // �Q�[���I���`�F�b�N
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
        // �J�[�h�I��
        else if (0 == card.PlayerNo)
        {
            setSelectCard(card);
        }
        // �f�b�L����t�B�[���h��1���o��
        else
        {
            // �t�B�[���h�փJ�[�h���U
            for (int i = 0; i < fieldCards.Count; i++)
            {
                if (fieldCards[i]) continue;
                fieldCards[i] = cards[0];
                fieldCards[i].PlayerNo = 0;
                cards.RemoveAt(0);
                break;
            }

            // �I������
            setSelectCard();
            // �J�[�h����ג���
            sortFieldCards(fieldCards, width, height, FieldResetTime);
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

    // �V���b�t���{�^��
    public void OnClickShuffle()
    {
        // �A�j���[�V������S�ďI���i�A�ő΍�j
        if (null != DOTween.PlayingTweens())
        {
            foreach (var item in DOTween.PlayingTweens())
            {
                item.Kill();
            }
        }

        // �t�B�[���h�̃J�[�h���f�b�L��
        foreach (var item in fieldCards)
        {
            if (!item) continue;
            cards.Add(item);
        }
        fieldCards.Clear();

        // �f�b�L���V���b�t��
        cardsDirector.ShuffleCards(cards);
        // �t�B�[���h���Z�b�g
        resetField(FieldResetTime / 2);
    }

    // �V�[���ēǂݍ���
    public void OnClickRestart()
    {
        SceneManager.LoadScene("CoupleScene");
    }
}

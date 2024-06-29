using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MemorySceneDirector : MonoBehaviour
{
    //���ʃJ�[�h�Ǘ��N���X
    [SerializeField] CardsDirector cardsDirector;
    //�^�C�}�[
    [SerializeField] Text textTimer;
    //�Q�[���Ŏg���J�[�h
    List<CardController> cards;
    //�c���������ׂ邩
    int width = 5;
    int height = 4;
    //�I�񂾃J�[�h
    List<CardController> selectCards;
    int selectCountMax = 2;
    //�Q�[���I���t���O
    bool isGameEnd;
    //�o�ߎ���
    float gameTimer;
    //�O��̕b��
    int oldSecond;
    void Start()
    {
        //�V���b�t�����ꂽ�J�[�h���擾
        cards = cardsDirector.GetMemoryCards();

        //�J�[�h�S�̂�^�񒆂ɂ��炷���߂̃I�t�Z�b�g
        Vector2 offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f);

        //�J�[�h����������Ȃ��Ƃ��A�G���[��\������
        if (cards.Count < width * height)
        {
            Debug.LogError("�J�[�h������܂���");
        }

        //�J�[�h����ׂ�
        for (int i = 0; i < width * height; i++)
        {
            //�\���ʒu
            float x = (i % width - offset.x) * CardController.Width;
            float y = (i / width - offset.y) * CardController.Height;

            //�ꏊ�Ɗp�x
            cards[i].transform.position = new Vector3(x, 0, y);
            cards[i].FlipCard(false);
        }

        //�e��t���O������
        selectCards = new List<CardController>();
        oldSecond = -1;
    }

    // Update is called once per frame
    void Update()
    {
        //�Q�[���I���Ȃ珈�������Ȃ�
        if (isGameEnd) return;

        //�o�ߎ��Ԃ𑫂�
        gameTimer += Time.deltaTime;

        //�^�C�}�[�\���X�V
        textTimer.text = getTimerText(gameTimer);

        //�}�E�X�������ꂽ�Ƃ�
        if (Input.GetMouseButtonDown(0))
        {
            //�R��ڂ̃^�b�v
            if (!canOpen()) return;
            //Ray���΂��ē����蔻����Ƃ�
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //�q�b�g�����Q�[���I�u�W�F�N�g����CardController���擾
                CardController card = hit.collider.gameObject.GetComponent<CardController>();
                //�J�[�h����Ȃ��������߂������J�[�h�Ȃ�I��
                if (!card || selectCards.Contains(card)) return;
                //�J�[�h�I�[�v��
                card.FlipCard();
                //�I�������J�[�h��ۑ�
                selectCards.Add(card);
            }
        }
    }

    //�^�C�}�[�\�����擾����
    string getTimerText(float timer)
    {
        //�b�����擾
        int sec = (int)timer % 60;
        //�O��ƕb���������Ȃ�O��̕\���l��Ԃ�
        string ret = textTimer.text;
        //�O��ƕb���ɈႢ�����邩
        if (oldSecond != sec)
        {
            //�������v�Z
            int min = (int)timer / 60;
            //min��sec��00�̂悤�ȕ�����ɂ���i�O���߁A�[���p�f�B���O�j
            string pmin = string.Format("{0:D2}", min);
            string psec = string.Format("{0:D2}", sec);

            ret = pmin + ":" + psec;
            oldSecond = sec;
        }

        return ret;
    }

    //�����P���߂���邩�ǂ���
    bool canOpen()
    {
        //�Q�������Ă��Ȃ��ꍇ�́A�J�[�h���߂��邱�Ƃ��ł���
        if (selectCards.Count < selectCountMax) return true;

        //�Q�������Ă���ꍇ�́A�I�������J�[�h�������������ǂ����`�F�b�N
        bool equal = true;
        foreach (var item in selectCards)
        {
            //�I�������J�[�h�𗠕Ԃ��ɂ���
            item.FlipCard(false);

            //�����������ǂ������`�F�b�N
            if (item.No != selectCards[0].No)
            {
                equal = false;
            }
        }

        //�S������������������J�[�h������
        if (equal)
        {
            //�߂������J�[�h���\��
            foreach (var item in selectCards)
            {
                item.gameObject.SetActive(false);
            }

            //�S���̃J�[�h����\���ɂȂ��Ă�����Q�[���I��
            isGameEnd = true;
            foreach (var item in cards)
            {
                if (item.gameObject.activeSelf)
                {
                    isGameEnd = false;
                    break;
                }
            }

            //�Q�[���N���A�b����\��
            if (isGameEnd)
            {
                textTimer.text = "�N���A!!" + getTimerText(gameTimer);
            }

        }

        //�I�������J�[�h���N���A
        selectCards.Clear();

        return false;
    }
}

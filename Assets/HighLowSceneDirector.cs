using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighLowSceneDirector : MonoBehaviour
{
    //���ʃJ�[�h�Ǘ��N���X
    [SerializeField] CardsDirector cardsDirector;
    //UI
    [SerializeField] GameObject buttonHigh;
    [SerializeField] GameObject buttonLow;
    [SerializeField] Text textInfo;
    //�Q�[���Ŏg���J�[�h
    List<CardController> cards;
    //���݂̃C���f�b�N�X
    int cardIndex;
    //������
    int winCount;
    //�E�F�C�g����
    const float NextWaitTimer = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        //�V���b�t�����ꂽ�J�[�h���擾
        cards = cardsDirector.GetHighLowCards();
        //�����ʒu�ƌ�����ݒ�
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.position = new Vector3(0, 0, 0.15f);
            cards[i].FlipCard(false);
        }
        //�Q���z��
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

    //�Q�[���{�^���̕\��/��\��
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

    //�Q�[������
    void checkHighLow(bool high)
    {
        //�E�̃J�[�h���I�[�v��
        cards[cardIndex + 1].GetComponent<CardController>().FlipCard();
        //�f�t�H���g�̕�����
        string result = "LOSE... : ";
        //���ƉE�̃J�[�h�ԍ�
        int lno = cards[cardIndex].GetComponent<CardController>().No;
        int rno = cards[cardIndex + 1].GetComponent<CardController>().No;

        //��������
        if (lno == rno)
        {
            result = "NO GAME!!";
        }
        //HIGH��I��
        else if (high)
        {
            if (lno < rno)
            {
                winCount++;
                result = "WIN!! : ";
                GetComponent<AudioSource>().Play();
            }
        }
        //LOW��I��
        else
        {
            if (lno > rno)
            {
                winCount++;
                result = "WIN!! : ";
                GetComponent<AudioSource>().Play();
            }
        }

        //�C���t�H�X�V
        textInfo.text = result + winCount;

        //���̃Q�[��
        StartCoroutine(nextCards());
    }

    //���̃Q�[��
    IEnumerator nextCards()
    {
        //�w��b���҂�
        yield return new WaitForSeconds(NextWaitTimer);

        //�O��̃J�[�h��ЂÂ���
        cards[cardIndex].gameObject.SetActive(false);
        cards[cardIndex + 1].gameObject.SetActive(false);

        //���̃J�[�h�̈ꖇ��
        cardIndex += 2;

        //�J�[�h������Ȃ�
        if (cards.Count - 1 <= cardIndex)
        {
            textInfo.text = "�I��!! " + winCount;
        }
        //���̃Q�[��
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

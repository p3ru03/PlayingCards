using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BlackJackSceneDirector : MonoBehaviour
{
    //���ʃJ�[�h�Ǘ��N���X
    [SerializeField] CardsDirector cardsDirector;
    //UI
    [SerializeField] GameObject buttonHit;
    [SerializeField] GameObject buttonStay;
    [SerializeField] Text textPlayerInfo;
    [SerializeField] Text textDealerInfo;
    //�Q�[���Ŏg���J�[�h
    List<CardController> cards;
    //��D
    List<CardController> playerHand;
    List<CardController> dealerHand;
    //�R�D�̃C���f�b�N�X
    int cardIndex;
    //�E�F�C�g����
    const float NextWaitTime = 1;

    AudioSource audioPlayer;
    [SerializeField] AudioClip win;
    [SerializeField] AudioClip lose;


    // Start is called before the first frame update
    void Start()
    {
        //�V���b�t�������J�[�h���擾
        cards = cardsDirector.GetShuffleCards();
        //�����ʒu
        foreach (var item in cards)
        {
            item.transform.position = new Vector3(100, 0, 0);
            item.FlipCard(false);
        }

        //��D�̏�����
        playerHand = new List<CardController>();
        dealerHand = new List<CardController>();

        cardIndex = 0;

        //�f�B�[���[�̎�D��ǉ�
        CardController card;

        card = hitCard(dealerHand);
        card = hitCard(dealerHand);
        card.FlipCard();

        //�v���C���[�̎�D��ǉ�
        hitCard(playerHand).FlipCard();
        hitCard(playerHand).FlipCard();

        //�e�L�X�g���X�V
        textPlayerInfo.text = "" + getScore(playerHand);

        audioPlayer = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    //�q�b�g
    CardController hitCard(List<CardController> hand)
    {
        float x = -0.1f;
        float z = -0.05f;

        //�f�B�[���[�̏����ʒu
        if (dealerHand == hand)
        {
            z = 0.1f;
        }
        //�J�[�h������Ȃ�E�ɕ��ׂ�
        if (0 < hand.Count)
        {
            x = hand[hand.Count - 1].transform.position.x;
            z = hand[hand.Count - 1].transform.position.z;
        }
        //�R�D�̃C���f�b�N�X����J�[�h���擾
        CardController card = cards[cardIndex];
        card.transform.position = new Vector3(x + CardController.Width, 0, z);
        hand.Add(card);
        cardIndex++;

        return card;
    }

    //��D���J�E���g
    int getScore(List<CardController> hand)
    {
        int score = 0;
        List<CardController> ace = new List<CardController>();

        //��D���v�Z
        foreach (var item in hand)
        {
            int no = item.No;

            if (1 == no)
            {
                ace.Add(item);
            }
            //J Q K �̌v�Z
            else if (10 < no)
            {
                no = 10;
            }

            score += no;
        }

        //A��11�ɂ���
        foreach (var item in ace)
        {
            if ((score + 10) < 22)
            {
                score += 10;
            }
        }

        return score;
    }
    //�v���C���[�q�b�g�{�^��
    public void OnClickHit()
    {
        //�J�[�h���ꖇ����
        CardController card = hitCard(playerHand);
        card.FlipCard();
        //�e�L�X�g�X�V
        int score = getScore(playerHand);
        textPlayerInfo.text = "" + score;
        //21�𒴂��Ă��Ȃ���
        if (21 < score)
        {
            textPlayerInfo.text = "�o�[�X�g!!�@�s�k...";

            buttonHit.gameObject.SetActive(false);
            buttonStay.gameObject.SetActive(false);

            audioPlayer.PlayOneShot(lose);
        }
    }

    //�v���C���[�X�e�C�{�^��
    public void OnClickStay()
    {
        //�{�^���������Ȃ��悤�ɔ�\��
        buttonHit.gameObject.SetActive(false);
        buttonStay.gameObject.SetActive(false);
        //�������Ă���P���ڂ��I�[�v��
        dealerHand[0].FlipCard();
        //�e�L�X�g�X�V
        int score = getScore(dealerHand);
        textDealerInfo.text = "" + score;

        //�f�B�[���[���ꖇ����
        StartCoroutine(dealerHit());
    }

    //�f�B�[���[�̔�
    IEnumerator dealerHit()
    {
        //�w��b���҂�
        yield return new WaitForSeconds(NextWaitTime);
        //���݂̃J�E���g
        int score = getScore(dealerHand);
        //17�ȉ��Ȃ狭���I�Ɉ���
        if (18 > score)
        {
            CardController card = hitCard(dealerHand);
            card.FlipCard();

            textDealerInfo.text = "" + getScore(dealerHand);
        }
        //���s�`�F�b�N
        score = getScore(dealerHand);

        //�o�[�X�g���Ă���v���C���[�̏���
        if (21 < score)
        {
            textDealerInfo.text += " �o�[�X�g";
            textPlayerInfo.text = "����!!";
            audioPlayer.PlayOneShot(win);
        }
        //18�ȏ�Ȃ�f�B�[���[�X�e�C�ŏ��s�`�F�b�N
        else if (17 < score)
        {
            string textplayer = "����!!";
            if (getScore(playerHand) < getScore(dealerHand))
            {
                textplayer = "�s�k...";
            }
            else if (getScore(playerHand) == getScore(dealerHand))
            {
                textplayer = "��������!!";
            }
            textPlayerInfo.text = textplayer;

            if (textplayer.Contains("����"))
            {
                audioPlayer.PlayOneShot(win);
            }
            else if (textplayer.Contains("�s�k"))
            {
                audioPlayer.PlayOneShot(lose);
            }
        }
        else
        {
            StartCoroutine(dealerHit());
        }
    }

    //���X�^�[�g
    public void OnClickRestart()
    {
        SceneManager.LoadScene("BlackJackScene");
    }
}

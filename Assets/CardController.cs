using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�}�[�N
public enum SuitType
{
    Spade,
    Club,
    Diamond,
    Heart,
    Joker
}

public class CardController : MonoBehaviour
{
    //�J�[�h�T�C�Y
    public const float Width = 0.06f;
    public const float Height = 0.09f;

    //���̃J�[�h�̃}�[�N
    public SuitType Suit;
    //�ԍ�
    public int No;
    //�ǂ̃v���C���[�̃J�[�h��
    public int PlayerNo;
    //�_�o�����J�b�v���ŕ��ׂ��Ƃ��̓����ԍ�
    public int Index;
    //��D�����l3D
    public Vector3 HandPosition;
    //��D�����lx,y
    public Vector2Int IndexPosition;
    //�J���[
    public Color SuitColor;
    //�\�ʂ���ɂȂ��Ă��邩
    public bool isFrontUp;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //�J�[�h���߂���i�����Ȃ��ŕ\���ɂ���j
    public void FlipCard(bool frontup = true)
    {
        float anglez = 0;
        if (!frontup)
        {
            anglez = 180;
        }

        isFrontUp = frontup;
        transform.eulerAngles = new Vector3(0, 0, anglez);
    }
}

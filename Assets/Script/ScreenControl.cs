using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenControl : MonoBehaviour
{

    private void Awake()
    {

        Camera cam = GetComponent<Camera>();

        //ī�޶� ������Ʈ viewport rect
        Rect rt = cam.rect;

        // ���� ���θ�� 16 9
        float scale_height =  ( (float)Screen.width / Screen.height ) / ((float) 16 / 9) ; // ���� ����
        float scale_width = 1f / scale_height ;

        if(scale_height < 1)
        {
            rt.height = scale_height ;
            rt.y = (1f - scale_height) / 2f;

        }
        else
        {
            rt.width = scale_width ;
            rt.x = (1f - scale_width) / 2f;
        }

        cam.rect = rt ;
    }


}

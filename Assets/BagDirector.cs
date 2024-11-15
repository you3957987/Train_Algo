using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.IO;
using System;
using static UnityEngine.Rendering.DebugUI;


public class BagDirector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    GameDirector game_director;// ���ͽ�Ʈ�� �� ���� ����ϴ� Ŭ����

    public Sprite[] bag_img;
    public GameObject[] node;
    public int bagCapacity = 13; // ���� �� �賶 �뷮
    public int current_bag_count = 0;
    public int current_score_count = 0;

    int[,] container = new int[100,100];

    List<int> mugaeList = new List<int>();  // kg ������ ������ ����Ʈ == �迭
    List<int> valueList = new List<int>();  // value ������ ������ ����Ʈ == �迭

    void Start()
    {
        game_director = GetComponent<GameDirector>(); // �ſ� �߿�!!!
    }

    // Update is called once per frame
    void Update()
    {
        //GameDirector.GetCircle(); // ����� �ڵ�
        if (Input.GetKeyDown(KeyCode.A)) // A Ű �Է½� ��� �ڽ� Ȱ��ȭ �� ���� �ڽ� ��ġ
        {
            // �� ���� ���� ������Ʈ(��� Box)�� Ȱ��ȭ
            ActivateAndSetRandomSprites();
            //Debug.Log('a');
        }
        if (Input.GetKeyDown(KeyCode.S)) // S Ű ���� ������ ���� ����Ʈ �迭�� ���
        {

            Check_Circle_Bag();
        }
        if (Input.GetKeyDown(KeyCode.D)) // D Ű �Է½� ���� �˰��� ����
        {

            Knampsack(container, mugaeList, valueList, current_bag_count);
        }

    }

    void Check_Circle_Bag()
    {
        int n = game_director.GetCircle();  // ���� ���õ� Circle�� �ε����� ������
        Debug.Log("���� ���� �� " + n);

        current_bag_count += node[n].transform.childCount; // ���� �賶 ���� �� ����

        foreach (Transform child in node[n].transform)
        {
            if (child.name.StartsWith("Box"))  // �̸��� "Box"�� �����ϴ� ���� ������Ʈ�鸸
            {
                Bag bag = child.GetComponent<Bag>();  // Bag Ŭ���� ��������

                if (bag != null)  // Bag ������Ʈ�� �����ϴ��� Ȯ��
                {
                    mugaeList.Add(bag.kg);
                    valueList.Add(bag.value);

                    Debug.Log($"Box - kg: {bag.kg}, value: {bag.value}");
                }
            }
        }

    }



    void Knampsack(int[,] container, List<int> mugaeList, List<int> valueList, int n) // n�� ���� ���� ��忡 ���ٽ� ����� �߰� ����
    {
        for(int i = 0; i < n; i++)
        {
            container[i, 0] = 0;
        }
        for(int i = 0; i <= bagCapacity; i++) // '<=' �߿�!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            container[0,i] = 0; 
        }

        for (int i = 1; i <= n; i++) // 1�� ���Ǻ��� n�� ���Ǳ���
        {
            for (int w = 1; w <= bagCapacity; w++) // 1���� bagCapacity���� ��� �뷮�� ���� ���
            {
                if (mugaeList[i - 1] >= w) // ���� ������ �賶�� ���� �� �ִ��� Ȯ��
                {
                    container[i, w] = container[i - 1, w];
                }
                else
                {
                    container[i, w] = Math.Max(container[i - 1, w], container[i - 1, w - mugaeList[i - 1]] + valueList[i - 1]);
                }
            }
        }

        // � ������ ��Ҵ��� ���� == �� �ڵ�� �� �� ����;;
        int remainingCapacity = bagCapacity;
        List<int> selectedItems = new List<int>(); // ���õ� ���� �ε����� ������ ����Ʈ
        int totalValue = 0; // ��ġ ������ ������ ����

        List<int> temp_mugaeList = new List<int>();  // kg ������ ������ ����Ʈ == �迭
        List<int> temp_valueList = new List<int>();  // value ������ ������ ����Ʈ == �迭

        Debug.Log(mugaeList.Count);
   
        // ������ ���Ǻ��� ������
        for (int i = n; i > 0; i--)
        {
            if (container[i, remainingCapacity] != container[i - 1, remainingCapacity]) // ���� ������ �������
            {
                selectedItems.Add(i - 1); // 0-based �ε����̹Ƿ� i - 1

                temp_mugaeList.Add(mugaeList[i - 1]);
                temp_valueList.Add(valueList[i - 1]);

                totalValue += valueList[i - 1]; // ���õ� ������ ��ġ�� ���տ� ���ϱ�

                remainingCapacity -= mugaeList[i - 1]; // �� ������ ���Ը�ŭ �뷮���� ����
            }
        }

  
        // ���õ� ���ǵ� ���
        Debug.Log("���õ� ���ǵ�:");
        foreach (int item in selectedItems)
        {
            Debug.Log($"���� {item} (����: {mugaeList[item]}, ��ġ: {valueList[item]})");
            mugaeList.Add(mugaeList[item]);
            valueList.Add(mugaeList[item]);
        }

        Debug.Log($"�賶�� ��� ���� ����: {selectedItems.Count}");
        Debug.Log($"�賶�� ��� ���ǵ��� �� ��ġ: {totalValue}");
        current_bag_count = selectedItems.Count;
        current_score_count = totalValue;

        mugaeList.Clear();
        valueList.Clear();
        mugaeList.AddRange(temp_mugaeList);  // �� ����Ʈ�� ���� �߰�
        valueList.AddRange(temp_valueList);  // �� ����Ʈ�� ���� �߰�

        Debug.Log(mugaeList.Count);

    }

    void ActivateAndSetRandomSprites() // �� ������ ���� �����ϰ� ����
    {
        foreach (GameObject circle in node)
        {
            // �� ���� ��� ���� Box ������Ʈ�� Ȱ��ȭ�ϰ� ��������Ʈ ����
            foreach (Transform child in circle.transform)
            {
                if (child.name.StartsWith("Box")) // �̸��� "Box"�� �����ϴ� ���� ������Ʈ��
                {
                    child.gameObject.SetActive(true); // Box�� Ȱ��ȭ

                    Bag bag = child.GetComponent<Bag>(); // �ڽ��� Bag Ŭ���� ��������

                    SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();

                    int random_num = UnityEngine.Random.Range(0, bag_img.Length);// 0 ����

                    spriteRenderer.sprite = bag_img[random_num];

                    if(random_num == 0)
                    {
                        bag.kg = 1;
                    }
                    else if(random_num == 1)
                    {
                        bag.kg = 3;
                    }
                    else if(random_num == 2)
                    {
                        bag.kg = 5;
                    }

                    random_num = UnityEngine.Random.Range( 1, 20);// ���� ��ġ ���ϱ�
                    bag.value = random_num;

                }
            }
        }
    }
}

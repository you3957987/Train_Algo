using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.IO;
using System;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Concurrent;


public class BagDirector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    GameDirector game_director;// ���ͽ�Ʈ�� �� ���� ����ϴ� Ŭ����

    public Sprite[] bag_img;
    public GameObject[] node;
    public int bagCapacity = 13; // ���� �� �賶 �뷮
    public int current_bag_count = 0;
    public int current_bag_score = 0; //  ���� �賶�� �ִ� ����

    int[,] container = new int[100,100];

    List<int> mugaeList = new List<int>();  // kg ������ ������ ����Ʈ == �迭
    List<int> valueList = new List<int>();  // value ������ ������ ����Ʈ == �迭

    //UI ����
    public TMP_Text bag_weight_ui;
    public Image[] current_bag_what;
    public TMP_Text score_ui;
    int current_score = 0;

    public TMP_FontAsset font; // ���� ��ġ ��Ʈ

    void Start()
    {
        game_director = GetComponent<GameDirector>(); // �ſ� �߿�!!!
        Set_Bag_weight(current_bag_score);
        score_ui.text = current_score.ToString();
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
        if (Input.GetKeyDown(KeyCode.F)) // D Ű �Է½� ���� �˰��� ����
        {
            SetBag_SetScore();
        }
   
    }
    public void SortAndUpdateBagUI_UP() // ���� ����
    {

        List<int> sortedItems = new List<int>();

        for (int i = 0; i < valueList.Count; i++)
        {
            sortedItems.Add(i);
        }

        //quick_sort(valueList, sortedItems, 0, valueList.Count - 1); // ���� ����
        sortedItems.Sort((a, b) => valueList[a].CompareTo(valueList[b]));  // ���� �������� ����


        UpdateBagUI(sortedItems, mugaeList, valueList);
    }

    public void SortAndUpdateBagUI_DOWN() // ���� ����
    {

        List<int> sortedItems = new List<int>();

        for (int i = 0; i < valueList.Count; i++)
        {
            sortedItems.Add(i);
        }

        quick_sort(valueList, sortedItems, 0, valueList.Count - 1); // ���� ����
        //sortedItems.Sort((a, b) => valueList[b].CompareTo(valueList[a]));  //  ���� ����


        UpdateBagUI(sortedItems, mugaeList, valueList);
    }
 
    void quick_sort(List<int> valueList, List<int> list, int left, int right)
    {
        if (left < right)
        {
            // Partition�� ���� pivot�� �ùٸ� ��ġ�� ��ġ�ǵ��� ��
            int q = Partition(valueList, list, left, right);

            // �ǹ� �������� ���ҵ� �κе��� ��������� ����
            quick_sort(valueList, list, left, q - 1);
            quick_sort(valueList, list, q + 1, right);
        }
    }

    int Partition(List<int> valueList, List<int> list, int left, int right)
    {
        int pivotIndex = left;  // �ǹ� �ε��� (���� ����)
        int pivotValue = valueList[list[left]];  // �ǹ��� ��
        int low = left + 1;  // low�� left + 1���� ����
        int high = right;

        while (true)
        {
            // low�� valueList[pivotValue]���� ���� �� ã��
            while (low <= right && valueList[list[low]] >= pivotValue)
            {
                low++;
            }

            // high�� valueList[pivotValue]���� ū �� ã��
            while (high >= left + 1 && valueList[list[high]] < pivotValue)
            {
                high--;
            }

            // low�� high���� ������ �� ���� ��ȯ
            if (low < high)
            {
                int temp = list[low];
                list[low] = list[high];
                list[high] = temp;
            }
            else
            {
                break;
            }
        }

        // �ǹ��� �ùٸ� ��ġ�� �̵�
        int tempPivot = list[left];
        list[left] = list[high];
        list[high] = tempPivot;

        return high; // �ǹ��� ���������� ��ġ�� �ε��� ��ȯ
    }


    void SetBag_SetScore() // ���濡 �ִ� �� ������ ġȯ �� ���� �ʱ�ȭ
    {
        current_score += current_bag_score;
        current_bag_score = 0;

        Set_Bag_weight(current_bag_score);

        mugaeList.Clear();
        valueList.Clear(); // ���� �ʱ�ȭ
        current_bag_count = 0;


        score_ui.text = current_score.ToString();

        for (int i = 0; i < current_bag_what.Length; i++)
        {
            current_bag_what[i].gameObject.SetActive(false); // ��� ���� ��Ȱ��ȭ
        }


    }

    void Set_Bag_weight(int score_temp)
    {
        bag_weight_ui.text = score_temp.ToString();
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
                    child.gameObject.SetActive(false);  // �ڽ��� ��Ȱ��ȭ
                }
            }
        }
    }


    // ���� ��ȹ �˰��� ���!!!!!!!!!!!
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
        UpdateBagUI(selectedItems, mugaeList, valueList); // UI ������Ʈ ȣ��
        foreach (int item in selectedItems)
        {
            Debug.Log($"���� {item} (����: {mugaeList[item]}, ��ġ: {valueList[item]})");
            mugaeList.Add(mugaeList[item]);
            valueList.Add(mugaeList[item]);
        }

        Debug.Log($"�賶�� ��� ���� ����: {selectedItems.Count}");
        Debug.Log($"�賶�� ��� ���ǵ��� �� ��ġ: {totalValue}");
        current_bag_count = selectedItems.Count;
        current_bag_score = totalValue;
        Set_Bag_weight(current_bag_score);

        mugaeList.Clear();
        valueList.Clear();
        mugaeList.AddRange(temp_mugaeList);  // �� ����Ʈ�� ���� �߰�
        valueList.AddRange(temp_valueList);  // �� ����Ʈ�� ���� �߰�

        Debug.Log(mugaeList.Count);

    }
    private void UpdateBagUI(List<int> selectedItems, List<int> mugaeList, List<int> valueList)
    {
        // ��� �賶 UI ���� �ʱ�ȭ
        for (int i = 0; i < current_bag_what.Length; i++)
        {
            current_bag_what[i].gameObject.SetActive(false); // ��� ���� ��Ȱ��ȭ
        }

        int bagSlotIndex = 0;
        int temp = 0;

        // ���õ� ���� ������ UI�� ������Ʈ
        foreach (int item in selectedItems)
        {
            if (bagSlotIndex < current_bag_what.Length)
            {
                // ���� ���� Ȱ��ȭ
                current_bag_what[bagSlotIndex].gameObject.SetActive(true);

                if (mugaeList[item] == 1)
                    temp = 0;
                else if (mugaeList[item] == 3)
                    temp = 1;
                else if (mugaeList[item] == 5)
                    temp = 2;
                else
                {
                    Debug.LogWarning($"�߸��� ���԰�: {mugaeList[item]}");
                    continue; // �߸��� ���Դ� ����
                }

                current_bag_what[bagSlotIndex].sprite = bag_img[temp];

                // �ؽ�Ʈ ������Ʈ (��ġ ǥ��)
                TextMeshProUGUI valueText = current_bag_what[bagSlotIndex].GetComponentInChildren<TextMeshProUGUI>();
                if (valueText != null)
                {
                    valueText.text = valueList[item].ToString();
                }
                else
                {
                    Debug.LogWarning($"�賶 ���� {bagSlotIndex}�� �ؽ�Ʈ ������Ʈ�� �����ϴ�.");
                }

                bagSlotIndex++; // ���� �������� �̵�
            }
            else
            {
                Debug.LogWarning("�賶 ������ �����մϴ�. �߰����� ������ ǥ�õ��� �ʽ��ϴ�.");
                break;
            }
        }
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

                    RemoveExistingValueText(child);

                    CreateValueText(child, bag.value);

                }
            }
        }
    }

    void CreateValueText(Transform boxTransform, int value)
    {
        // �ؽ�Ʈ ������Ʈ ����
        GameObject textObj = new GameObject("ValueText");
        textObj.transform.SetParent(boxTransform); // �θ�� Box�� ����

        // TextMeshPro ������Ʈ �߰�
        TextMeshPro textMeshPro = textObj.AddComponent<TextMeshPro>();
        textMeshPro.text = value.ToString(); // �ؽ�Ʈ ������ ��ġ ������ ����
        textMeshPro.font = font; // ������ ��Ʈ ���
        textMeshPro.fontSize = 3; // �ؽ�Ʈ ũ��
        textMeshPro.color = Color.magenta; // �ؽ�Ʈ ����

        // �ؽ�Ʈ ���� ���� (��� ����)
        textMeshPro.alignment = TextAlignmentOptions.Center;

        // RectTransform ����
        RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, 0.5f, 0); // �ڽ� �߽ɿ��� ��¦ �Ʒ��� ��ġ
        rectTransform.localScale = Vector3.one; // ũ�� ���� ����
        rectTransform.sizeDelta = new Vector2(5, 1); // �ؽ�Ʈ �ڽ� ũ�� ����
    }
    void RemoveExistingValueText(Transform boxTransform)
    {
        // "ValueText"��� �̸��� ���� �ڽ� ������Ʈ�� ã�� ����
        foreach (Transform child in boxTransform)
        {
            if (child.name == "ValueText")
            {
                Destroy(child.gameObject); // �ؽ�Ʈ ������Ʈ ����
            }
        }
    }

}

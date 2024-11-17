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

    GameDirector game_director;// 다익스트라 및 기차 운용하는 클래스

    public Sprite[] bag_img;
    public GameObject[] node;
    public int bagCapacity = 13; // 게임 내 배낭 용량
    public int current_bag_count = 0;
    public int current_bag_score = 0; //  현재 배낭에 있는 점수

    int[,] container = new int[100,100];

    List<int> mugaeList = new List<int>();  // kg 값들을 저장할 리스트 == 배열
    List<int> valueList = new List<int>();  // value 값들을 저장할 리스트 == 배열

    //UI 관련
    public TMP_Text bag_weight_ui;
    public Image[] current_bag_what;
    public TMP_Text score_ui;
    int current_score = 0;

    public TMP_FontAsset font; // 물건 가치 폰트

    void Start()
    {
        game_director = GetComponent<GameDirector>(); // 매우 중요!!!
        Set_Bag_weight(current_bag_score);
        score_ui.text = current_score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        //GameDirector.GetCircle(); // 실험용 코드
        if (Input.GetKeyDown(KeyCode.A)) // A 키 입력시 모든 박스 활성화 및 랜덤 박스 배치
        {
            // 각 원의 하위 오브젝트(모든 Box)를 활성화
            ActivateAndSetRandomSprites();
            //Debug.Log('a');
        }
        if (Input.GetKeyDown(KeyCode.S)) // S 키 현재 원에서 물건 리스트 배열에 담기
        {
            Check_Circle_Bag();
        }
        if (Input.GetKeyDown(KeyCode.D)) // D 키 입력시 동적 알고리즘 실행
        {
            Knampsack(container, mugaeList, valueList, current_bag_count);
        }
        if (Input.GetKeyDown(KeyCode.F)) // D 키 입력시 동적 알고리즘 실행
        {
            SetBag_SetScore();
        }
   
    }
    public void SortAndUpdateBagUI_UP() // 오름 차순
    {

        List<int> sortedItems = new List<int>();

        for (int i = 0; i < valueList.Count; i++)
        {
            sortedItems.Add(i);
        }

        //quick_sort(valueList, sortedItems, 0, valueList.Count - 1); // 내림 차순
        sortedItems.Sort((a, b) => valueList[a].CompareTo(valueList[b]));  // 오름 차순으로 정렬


        UpdateBagUI(sortedItems, mugaeList, valueList);
    }

    public void SortAndUpdateBagUI_DOWN() // 내림 차순
    {

        List<int> sortedItems = new List<int>();

        for (int i = 0; i < valueList.Count; i++)
        {
            sortedItems.Add(i);
        }

        quick_sort(valueList, sortedItems, 0, valueList.Count - 1); // 내림 차순
        //sortedItems.Sort((a, b) => valueList[b].CompareTo(valueList[a]));  //  오름 차순


        UpdateBagUI(sortedItems, mugaeList, valueList);
    }
 
    void quick_sort(List<int> valueList, List<int> list, int left, int right)
    {
        if (left < right)
        {
            // Partition을 통해 pivot이 올바른 위치로 배치되도록 함
            int q = Partition(valueList, list, left, right);

            // 피벗 기준으로 분할된 부분들을 재귀적으로 정렬
            quick_sort(valueList, list, left, q - 1);
            quick_sort(valueList, list, q + 1, right);
        }
    }

    int Partition(List<int> valueList, List<int> list, int left, int right)
    {
        int pivotIndex = left;  // 피벗 인덱스 (가장 왼쪽)
        int pivotValue = valueList[list[left]];  // 피벗의 값
        int low = left + 1;  // low는 left + 1에서 시작
        int high = right;

        while (true)
        {
            // low는 valueList[pivotValue]보다 작은 값 찾기
            while (low <= right && valueList[list[low]] >= pivotValue)
            {
                low++;
            }

            // high는 valueList[pivotValue]보다 큰 값 찾기
            while (high >= left + 1 && valueList[list[high]] < pivotValue)
            {
                high--;
            }

            // low가 high보다 작으면 두 값을 교환
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

        // 피벗을 올바른 위치로 이동
        int tempPivot = list[left];
        list[left] = list[high];
        list[high] = tempPivot;

        return high; // 피벗이 최종적으로 위치한 인덱스 반환
    }


    void SetBag_SetScore() // 가방에 있는 거 점수로 치환 및 가방 초기화
    {
        current_score += current_bag_score;
        current_bag_score = 0;

        Set_Bag_weight(current_bag_score);

        mugaeList.Clear();
        valueList.Clear(); // 가방 초기화
        current_bag_count = 0;


        score_ui.text = current_score.ToString();

        for (int i = 0; i < current_bag_what.Length; i++)
        {
            current_bag_what[i].gameObject.SetActive(false); // 모든 슬롯 비활성화
        }


    }

    void Set_Bag_weight(int score_temp)
    {
        bag_weight_ui.text = score_temp.ToString();
    }

    void Check_Circle_Bag()
    {
        int n = game_director.GetCircle();  // 현재 선택된 Circle의 인덱스를 가져옴
        Debug.Log("물건 담을 원 " + n);

        current_bag_count += node[n].transform.childCount; // 현재 배낭 물건 수 증가

        foreach (Transform child in node[n].transform)
        {
            if (child.name.StartsWith("Box"))  // 이름이 "Box"로 시작하는 하위 오브젝트들만
            {
                Bag bag = child.GetComponent<Bag>();  // Bag 클래스 가져오기

                if (bag != null)  // Bag 컴포넌트가 존재하는지 확인
                {
                    mugaeList.Add(bag.kg);
                    valueList.Add(bag.value);

                    Debug.Log($"Box - kg: {bag.kg}, value: {bag.value}");
                    child.gameObject.SetActive(false);  // 박스를 비활성화
                }
            }
        }
    }


    // 동적 계획 알고리즘 사용!!!!!!!!!!!
    void Knampsack(int[,] container, List<int> mugaeList, List<int> valueList, int n) // n은 물건 갯수 노드에 접근시 생기는 추가 물건
    {
        for(int i = 0; i < n; i++)
        {
            container[i, 0] = 0;
        }
        for(int i = 0; i <= bagCapacity; i++) // '<=' 중요!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            container[0,i] = 0; 
        }

        for (int i = 1; i <= n; i++) // 1번 물건부터 n번 물건까지
        {
            for (int w = 1; w <= bagCapacity; w++) // 1부터 bagCapacity까지 모든 용량에 대해 계산
            {
                if (mugaeList[i - 1] >= w) // 현재 물건을 배낭에 넣을 수 있는지 확인
                {
                    container[i, w] = container[i - 1, w];
                }
                else
                {
                    container[i, w] = Math.Max(container[i - 1, w], container[i - 1, w - mugaeList[i - 1]] + valueList[i - 1]);
                }
            }
        }

        // 어떤 물건을 담았는지 추적 == 이 코드는 잘 모름 ㅎㅎ;;
        int remainingCapacity = bagCapacity;
        List<int> selectedItems = new List<int>(); // 선택된 물건 인덱스를 저장할 리스트
        int totalValue = 0; // 가치 총합을 저장할 변수

        List<int> temp_mugaeList = new List<int>();  // kg 값들을 저장할 리스트 == 배열
        List<int> temp_valueList = new List<int>();  // value 값들을 저장할 리스트 == 배열

        Debug.Log(mugaeList.Count);
   
        // 마지막 물건부터 역추적
        for (int i = n; i > 0; i--)
        {
            if (container[i, remainingCapacity] != container[i - 1, remainingCapacity]) // 현재 물건을 담았으면
            {
                selectedItems.Add(i - 1); // 0-based 인덱스이므로 i - 1

                temp_mugaeList.Add(mugaeList[i - 1]);
                temp_valueList.Add(valueList[i - 1]);

                totalValue += valueList[i - 1]; // 선택된 물건의 가치를 총합에 더하기

                remainingCapacity -= mugaeList[i - 1]; // 그 물건의 무게만큼 용량에서 빼기
            }
        }

  
        // 선택된 물건들 출력
        Debug.Log("선택된 물건들:");
        UpdateBagUI(selectedItems, mugaeList, valueList); // UI 업데이트 호출
        foreach (int item in selectedItems)
        {
            Debug.Log($"물건 {item} (무게: {mugaeList[item]}, 가치: {valueList[item]})");
            mugaeList.Add(mugaeList[item]);
            valueList.Add(mugaeList[item]);
        }

        Debug.Log($"배낭에 담긴 물건 갯수: {selectedItems.Count}");
        Debug.Log($"배낭에 담긴 물건들의 총 가치: {totalValue}");
        current_bag_count = selectedItems.Count;
        current_bag_score = totalValue;
        Set_Bag_weight(current_bag_score);

        mugaeList.Clear();
        valueList.Clear();
        mugaeList.AddRange(temp_mugaeList);  // 새 리스트에 값을 추가
        valueList.AddRange(temp_valueList);  // 새 리스트에 값을 추가

        Debug.Log(mugaeList.Count);

    }
    private void UpdateBagUI(List<int> selectedItems, List<int> mugaeList, List<int> valueList)
    {
        // 모든 배낭 UI 슬롯 초기화
        for (int i = 0; i < current_bag_what.Length; i++)
        {
            current_bag_what[i].gameObject.SetActive(false); // 모든 슬롯 비활성화
        }

        int bagSlotIndex = 0;
        int temp = 0;

        // 선택된 물건 정보를 UI에 업데이트
        foreach (int item in selectedItems)
        {
            if (bagSlotIndex < current_bag_what.Length)
            {
                // 현재 슬롯 활성화
                current_bag_what[bagSlotIndex].gameObject.SetActive(true);

                if (mugaeList[item] == 1)
                    temp = 0;
                else if (mugaeList[item] == 3)
                    temp = 1;
                else if (mugaeList[item] == 5)
                    temp = 2;
                else
                {
                    Debug.LogWarning($"잘못된 무게값: {mugaeList[item]}");
                    continue; // 잘못된 무게는 무시
                }

                current_bag_what[bagSlotIndex].sprite = bag_img[temp];

                // 텍스트 업데이트 (가치 표시)
                TextMeshProUGUI valueText = current_bag_what[bagSlotIndex].GetComponentInChildren<TextMeshProUGUI>();
                if (valueText != null)
                {
                    valueText.text = valueList[item].ToString();
                }
                else
                {
                    Debug.LogWarning($"배낭 슬롯 {bagSlotIndex}에 텍스트 컴포넌트가 없습니다.");
                }

                bagSlotIndex++; // 다음 슬롯으로 이동
            }
            else
            {
                Debug.LogWarning("배낭 슬롯이 부족합니다. 추가적인 물건은 표시되지 않습니다.");
                break;
            }
        }
    }

    void ActivateAndSetRandomSprites() // 각 정점별 무게 랜덤하게 생성
    {
        foreach (GameObject circle in node)
        {
            // 각 원의 모든 하위 Box 오브젝트를 활성화하고 스프라이트 설정
            foreach (Transform child in circle.transform)
            {
                if (child.name.StartsWith("Box")) // 이름이 "Box"로 시작하는 하위 오브젝트만
                {
                    child.gameObject.SetActive(true); // Box를 활성화

                    Bag bag = child.GetComponent<Bag>(); // 박스의 Bag 클래스 가져오기

                    SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();

                    int random_num = UnityEngine.Random.Range(0, bag_img.Length);// 0 부터

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

                    random_num = UnityEngine.Random.Range( 1, 20);// 물건 가치 정하기
                    bag.value = random_num;

                    RemoveExistingValueText(child);

                    CreateValueText(child, bag.value);

                }
            }
        }
    }

    void CreateValueText(Transform boxTransform, int value)
    {
        // 텍스트 오브젝트 생성
        GameObject textObj = new GameObject("ValueText");
        textObj.transform.SetParent(boxTransform); // 부모는 Box로 설정

        // TextMeshPro 컴포넌트 추가
        TextMeshPro textMeshPro = textObj.AddComponent<TextMeshPro>();
        textMeshPro.text = value.ToString(); // 텍스트 내용을 가치 값으로 설정
        textMeshPro.font = font; // 지정된 폰트 사용
        textMeshPro.fontSize = 3; // 텍스트 크기
        textMeshPro.color = Color.magenta; // 텍스트 색상

        // 텍스트 정렬 설정 (가운데 정렬)
        textMeshPro.alignment = TextAlignmentOptions.Center;

        // RectTransform 조정
        RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, 0.5f, 0); // 박스 중심에서 살짝 아래로 배치
        rectTransform.localScale = Vector3.one; // 크기 비율 유지
        rectTransform.sizeDelta = new Vector2(5, 1); // 텍스트 박스 크기 설정
    }
    void RemoveExistingValueText(Transform boxTransform)
    {
        // "ValueText"라는 이름을 가진 자식 오브젝트를 찾고 삭제
        foreach (Transform child in boxTransform)
        {
            if (child.name == "ValueText")
            {
                Destroy(child.gameObject); // 텍스트 오브젝트 삭제
            }
        }
    }

}

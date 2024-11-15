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

    GameDirector game_director;// 다익스트라 및 기차 운용하는 클래스

    public Sprite[] bag_img;
    public GameObject[] node;
    public int bagCapacity = 13; // 게임 내 배낭 용량
    public int current_bag_count = 0;
    public int current_score_count = 0;

    int[,] container = new int[100,100];

    List<int> mugaeList = new List<int>();  // kg 값들을 저장할 리스트 == 배열
    List<int> valueList = new List<int>();  // value 값들을 저장할 리스트 == 배열

    void Start()
    {
        game_director = GetComponent<GameDirector>(); // 매우 중요!!!
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
                }
            }
        }

    }



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
        foreach (int item in selectedItems)
        {
            Debug.Log($"물건 {item} (무게: {mugaeList[item]}, 가치: {valueList[item]})");
            mugaeList.Add(mugaeList[item]);
            valueList.Add(mugaeList[item]);
        }

        Debug.Log($"배낭에 담긴 물건 갯수: {selectedItems.Count}");
        Debug.Log($"배낭에 담긴 물건들의 총 가치: {totalValue}");
        current_bag_count = selectedItems.Count;
        current_score_count = totalValue;

        mugaeList.Clear();
        valueList.Clear();
        mugaeList.AddRange(temp_mugaeList);  // 새 리스트에 값을 추가
        valueList.AddRange(temp_valueList);  // 새 리스트에 값을 추가

        Debug.Log(mugaeList.Count);

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

                }
            }
        }
    }
}

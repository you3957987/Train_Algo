using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.IO;


public class GameDirector : MonoBehaviour
{
    // 1. 노드 및 기차 관련 변수
    public GameObject[] circles; // 노드 저장
    public GameObject train; // 움직이는 기차


    // 2. 선 관련 변수
    public GameObject linePrefab; // 라인 렌더러 프리팹
    private GameObject createSelectedCircle = null; // 선 생성시 첫 번째 노드 저장
    private GameObject deleteSelectedCircle = null; // 선 삭제시 첫 번째 노드 저장
    private HashSet<(int, int)> edges = new HashSet<(int, int)>(); // 생성된 간선 목록 (노드 쌍)
    private List<GameObject> createdLines = new List<GameObject>(); // 생성된 선(GameObject) 목록
    private Dictionary<(int, int), int> edgeWeights = new Dictionary<(int, int), int>(); // 간선의 가중치 저장 (노드 쌍 -> 가중치)

    // 3. 기차 이동 관련 변수
    public bool isTrainMoving = false; // 기차가 움직이는지 여부
    private Vector3 targetPosition; // 기차의 이동 목표 위치
    private float trainMoveStartTime; // 기차 이동 시작 시간 (추후 애니메이션이나 이동 계산용)

    // 4. 마우스 클릭 및 오류 방지 관련 변수
    public int create_check = 0; // 선 생성 시 마우스 클릭 오류 방지용
    public int delete_check = 0; // 선 삭제 시 마우스 클릭 오류 방지용

    // 5. 폰트 및 색깔 저장
    public TMP_FontAsset font; // 가중치 폰트
    private Color originalColor; // 기존 색 저장용 변수

    // 6. 기차의 현재 위치 및 목적지 노드 관련 변수
    GameObject closestCircle; // 기차의 현재 위치 노드 (가장 가까운 노드)
    GameObject targetCircle; // 기차의 목적지 노드 (도달하려는 노드)


    // 기본 베이스 코드-------------------------------------------------------------------

    // 알고리즘 코드------
    GraphType g = new GraphType(10);// 노드 추가시!!
    int node_num;


    int[] distance = new int[100]; // 시작점으로부터 최단 경로 거리
    bool[] found = new bool[100]; // 방문한 정점 표시
    public const int INF = 100000000;  // INF 값 정의
    int line_weight; // g 에 넣을 가중치 중간 저장용 변수

    int w_s = 1; //간선 가중치 최소
    int w_e = 20; // 간선 가중치 최대

    int[] saveRoute; // 경로 추적용
    int[] vertex; // 노드 저장

    int[] dk_path = new int[100]; // 다익스트라로 출발-> 도착 까지 경로 저장
    public bool is_dk = false;
    int currentPathIndex = 1; // 기차 이동시 몇번 이동 했는지
    int path_num; // 기차가 몇번 이동 해야하는지 ( 다익스트라 )

    bool dk_can; // 다익스트라 알고리즘이 시행 불가능할 경우, 최단 경로로 이동하는 함수 실행 불가능하게

    float vibrationAmount = 0.2f;
    float vibrationSpeed = 20f;    // 진동 속도
    Vector3 startPosition;
    bool vib = false;

    // UI 관련 코드
    public Image[] uiImages; // UI에 있는 Image 컴포넌트 3개를 드래그하여 연결
    public Sprite[] nodeSprites; // 사용할 이미지 스프라이트 배열 (0부터 7까지 노드 이미지)
    public int one_ui, two_ui, three_ui; // 1 번인 다음 목적지. 2,3, 번은 대기
    public TMP_Text max_line; // 이동 가능한 간선 최대치 UI

    public int current_max_line_weight; // 현재 이동 가능한 간선 가중치
    public int max_line_weight = 100; // 경유지 통과시 초기화 되는 최대 간선 가중치

    public Button link_button;
    public Sprite[] link_button_img;
    public int link_selector = 0;

    public Image[] line_light;
    public Sprite[] line_ight_img;
    public int chance_line_delete = 3;

    // 배낭 문제 알고리즘 코드
    BagDirector BagDirector;

    public TMP_Text line_one, line_two, line_three;
    public int next_line; // 다음 간선 생성시 가중치
    bool click_create; // 클릭으로 간선 선택시


    // 게임 시작시 관련

    public Image start_img;
    public Sprite[] start_icon;
    TimerDirector timerDirector;
    BagDirector bagDirector;

    void Start()
    {
        //g.weight[0, 2] = 8; // 값 변하는지 실험용
        Application.targetFrameRate = 60; // 60 프레임 고정
        timerDirector = GetComponent<TimerDirector>();
        bagDirector = GetComponent<BagDirector>();

        node_num = nodeSprites.Length; // 노드 갯수 하드 코딩 안하게
        g.PrintGraph(g); // 디버깅
        DistanceSet(g); // g 초기화


        current_max_line_weight = max_line_weight;
        max_line.text = current_max_line_weight.ToString(); // 이동 가능한 최대 간선 가중치 수

        Set_line_light(chance_line_delete); // 디폴트 값은 3

        StartCoroutine(ShowStartImages());

        //SetRandomImages(0); // 시작지인 0 은 랜덤값에서 제외
        //SetRandomLine(); // 다음 간선 유아이 생성
    }
    void Update()
    {
        DetectMouseClick();

        if (Input.GetKeyDown(KeyCode.Space) && !isTrainMoving)
        {
            Debug.Log("train_start");
            CheckCurrentCircleConnections(); //스페이스바 누르면 기차가 무한히 움직임
        }
        if( is_dk) // 프레임 마다 이동
        {
            Dk_Move(); 
        }
        if (isTrainMoving) // 프레임 마다 이동
        {
            MoveTrainToTarget(); 
        }

        if (Input.GetKeyDown(KeyCode.M))  // M 키를 눌렀을 때 그래프 확인
        {
            g.PrintGraph(g);
        }
        if (Input.GetKeyDown(KeyCode.N))  // N 키를 눌렀을 때 그래프 확인
        {
            PrintDistance(g);
        }


        if (Input.GetKeyDown(KeyCode.Q))  // Q 버튼을 클릭하면 이미지 교체
        {
            SwapImages();
        }
        if (Input.GetKeyDown(KeyCode.W))  // W 키를 눌렀을 때 모든 간선 삭제
        {
            DeleteAllEdges();
        }
        if (Input.GetKeyDown(KeyCode.E))  // E 키를 누르면 랜덤 간선 생성
        {
            GenerateRandomEdges();
        }
        if (Input.GetKeyDown(KeyCode.R) && !isTrainMoving && !is_dk)  // 최단거리까지 이동
        {
            dk_can = true;
            int n = GetCircle(); // 위치한 원 숫자 받기 함수
            Shortest_path(g, n);
            if (dk_can == false)
            {
                if(vib == false)
                {
                    startPosition = train.transform.position; // 원래 위치 저장
                    StartCoroutine(VibrateSpider());
                }
                return; // 다익스트라 실행 안되는 경우 멈추기
            }


            TracePath(n, one_ui); // 출발지는 0, 도착지는 1번 
            Dk_Move();
        }
        if (Input.GetKeyDown(KeyCode.T))  // T 다읻스트라 알고리즘 실행
        {
            int n = GetCircle(); // 위치한 원 숫자 받기 함수
            Shortest_path(g, n);
        }
        if (Input.GetKeyDown(KeyCode.Y))  // Y 키는 출발지에서 각 노드로의 최단 경로
        {
            int n = GetCircle(); // 위치한 원 숫자 받기 함수
            TraceAllPaths(n);
        }
        if (Input.GetKeyDown(KeyCode.U))  // U 키를 눌렀을 때 도착지까지의 경로만 확인
        {
            int n = GetCircle(); // 위치한 원 숫자 받기 함수
            TracePath(n, one_ui); // 출발지는 현재 위치, 도착지는 1번

        }
        if (Input.GetKeyDown(KeyCode.I))  // P 키를 눌렀을 때 기차가 있는 원 위치 확인
        {
            SwapLine();
        }
        if (Input.GetKeyDown(KeyCode.P))  // P 키를 눌렀을 때 기차가 있는 원 위치 확인
        {
            int n = GetCircle(); // 위치한 원 숫자 받기 함수
            Debug.Log(n);  // 추출한 숫자 출력
        }

    }

    void Set_line_light( int n ) // 받은 숫자 만큼만 불빛 활성화 n = 3, 2, 1, 0 만 들어옴
    {
        for(int i = 0; i < 3; i++) // 불은 총 3개. 0, 1, 2
        {
            if( i < n )
            {
                line_light[i].sprite = line_ight_img[0];  // 불좀 켜줄래?
            }
            else
            {
                line_light[i].sprite = line_ight_img[1];  // 불좀 꺼줄래?
            }
        }

    }

    public void Press_Link_Button()
    {
        SpriteState spritestate = link_button.spriteState;

        if (link_selector == 0) // 링크 상태에서
        {

            link_selector = 1;
            link_button.image.sprite = link_button_img[2];
            spritestate.pressedSprite = link_button_img[3];

        }
        else // 언링크 상태에서
        {
            link_selector = 0;
            link_button.image.sprite = link_button_img[0];
            spritestate.pressedSprite = link_button_img[1];
        }

        link_button.spriteState = spritestate; // 변경된 SpriteState 버튼에 다시 설정
    }

    public void Press_Run_Button()
    {
        if (!isTrainMoving && !is_dk && timerDirector.game_start)  // 최단거리까지 이동. 게임 사직시 버튼 작동
        {
            dk_can = true;
            int n = GetCircle(); // 위치한 원 숫자 받기 함수
            Shortest_path(g, n);
            if (dk_can == false)
            {
                if (vib == false)
                {
                    startPosition = train.transform.position; // 원래 위치 저장
                    StartCoroutine(VibrateSpider());
                }
                return; // 다익스트라 실행 안되는 경우 멈추기
            }


            TracePath(n, one_ui); // 출발지는 0, 도착지는 1번 
            Dk_Move();
        }
    }

    IEnumerator ShowStartImages() // 게임 시작!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {

        foreach (Sprite icon in start_icon)
        {
            start_img.sprite = icon;  // 현재 스프라이트 설정
            yield return new WaitForSeconds(1f); // 1초 대기

        }

        start_img.enabled = false; // 이미지 숨기기
       
        SetRandomImages(0); // 시작지인 0 은 랜덤값에서 제외
        SetRandomLine(); // 다음 간선 유아이 생성
        timerDirector.game_start = true;
        bagDirector.ActivateAndSetRandomSprites();
        GenerateRandomEdges();
        GenerateRandomEdges();
    }

    IEnumerator VibrateSpider()
    {
        float elapsedTime = 0f;
        vib = true;

        while (elapsedTime < 0.6f) // 진동을 1초 동안만 진행
        {
            // Sin 함수로 상하 진동
            float offset = Mathf.Sin(elapsedTime * vibrationSpeed) * vibrationAmount;
            train.transform.position = startPosition + new Vector3(0, offset, 0); // Y축으로 진동

            elapsedTime += Time.deltaTime; // 시간 증가
            yield return null;
        }

        train.transform.position = startPosition; // 진동 후 원위치로
        vib = false;
    }

    void SetRandomImages(int n)
    {

        // 0부터 7까지의 노드 중 랜덤하게 3개 선택, n은 제외
        List<int> randomNodes = new List<int>();
        while (randomNodes.Count < 3)
        {
            int randomNum = Random.Range(0, nodeSprites.Length);
            if (randomNum != n && !randomNodes.Contains(randomNum)) // n과 중복된 값 제외
            {
                randomNodes.Add(randomNum);
            }
        }
        one_ui = randomNodes[0];
        two_ui = randomNodes[1];
        three_ui = randomNodes[2];

        // 선택된 노드에 따라 이미지 변경
        for (int i = 0; i < uiImages.Length; i++)
        {
            uiImages[i].sprite = nodeSprites[randomNodes[i]]; // 각 Image에 스프라이트 할당
        }
    }

    void SwapImages()
    {
        // 첫 번째 이미지 값은 두 번째로, 두 번째 이미지 값은 세 번째로 변경
        int temp = one_ui;
        one_ui = two_ui;
        two_ui = three_ui;

        // 세 번째 이미지는 두 번째 이미지 값과 겹치지 않는 랜덤 값으로 변경
        int randomNum;
        do
        {
            randomNum = Random.Range(0, nodeSprites.Length);  // 0부터 7까지 랜덤값
        } while (randomNum == temp || randomNum == one_ui || randomNum == two_ui);  // 두 번째 이미지 값과 겹치지 않게

        three_ui = randomNum;

        // 변경된 이미지 값으로 UI 업데이트
        uiImages[0].sprite = nodeSprites[one_ui];
        //uiImages[1].sprite = nodeSprites[two_ui];
        //uiImages[2].sprite = nodeSprites[three_ui]; // 이거 가는 노드는 하나만 보이게
    }

    void SetRandomLine()
    {
        line_one.text = Random.Range(w_s, w_e + 1).ToString();  // w_s 부터 w_e까지 랜덤 값 (w_e 포함)
        line_two.text = Random.Range(w_s, w_e + 1).ToString();
        line_three.text = Random.Range(w_s, w_e + 1).ToString();
        next_line = int.Parse(line_one.text);
    }

    void SwapLine()
    {
        line_one.text = line_two.text;
        line_two.text = line_three.text;
        line_three.text = Random.Range(w_s, w_e + 1).ToString();
        next_line = int.Parse(line_one.text);
    }

    void DeleteAllEdges()
    {
        // 모든 정점 쌍에 대해 DeleteEdge 호출
        for (int i = 0; i < circles.Length; i++)
        {
            for (int j = i + 1; j < circles.Length; j++)
            {
                // 두 정점 사이 간선을 삭제
                DeleteEdge(circles[i], circles[j]);
            }
        }
        g.SetGraph(node_num);
    }
    void GenerateRandomEdges()
    {

        List<(int, int)> possibleEdges = new List<(int, int)>();

        // 가능한 모든 간선 쌍 생성 (중복 방지)
        for (int i = 0; i < circles.Length; i++)
        {
            for (int j = i + 1; j < circles.Length; j++)
            {
                possibleEdges.Add((i, j));
            }
        }

        // 정점 - 10개의 간선을 랜덤하게 선택하여 생성
        int edgeCount = circles.Length/2;
        for (int k = 0; k < edgeCount; k++)
        {
            int randomIndex = Random.Range(0, possibleEdges.Count);
            (int startIdx, int endIdx) = possibleEdges[randomIndex];
            possibleEdges.RemoveAt(randomIndex);

            CreateEdge(circles[startIdx], circles[endIdx]);
        }
    }

    // 다익스트라로 길 찾는 코드--------------------------------------------------------------


    int Choose(int[] distance, int n, bool[] found)
    {
        int i, min, minpos;

        min = INF;
        minpos = -1;

        for (i = 0; i < n; i++)
        {
            if (distance[i] < min && !found[i])
            {
                min = distance[i];
                minpos = i;
            }
        }
        return minpos;
    }

    void Shortest_path(GraphType g, int start)
    {
        int i, u, w;
        saveRoute = new int[g.n];
        vertex = new int[g.n];

        for (i = 0; i < g.n; i++)
        {
            vertex[i] = i;
        } // 경로 배열들 초기화

        for (i = 0; i < g.n; i++)
        {
            distance[i] = g.weight[start, i];
            found[i] = false; // 0 = FALSE ,, TRUE = 1
            saveRoute[i] = vertex[start];
        }

        found[start] = true;
        distance[start] = 0;
        saveRoute[start] = vertex[start];

        for (i = 0; i < g.n - 1; i++)
        {
            Debug.Log("다익스트라~");
            u = Choose(distance, g.n, found);
            //Debug.Log("선택된 노드 : " + u);
            if (u == -1)
            {
                Debug.Log("다익스트라를 위한 간선 연결 부족");
                dk_can = false;
                return;
            }
            found[u] = true;

            for (w = 0; w < g.n; w++)
            {
                if (!found[w])
                {
                    if (distance[u] + g.weight[u, w] < distance[w])
                    {
                        distance[w] = distance[u] + g.weight[u, w];
                        saveRoute[w] = vertex[u];
                    }
                }
            }
        }
    }

    void TraceAllPaths(int start) // 모든 경로 확인
    {
        // 모든 노드에 대해 경로 추적
        for (int i = 0; i < g.n; i++)
        {
            string route = "";
            Debug.Log("시작 꼭지점 " + vertex[start] + "부터 꼭지점 " + vertex[i] + "까지의 경로");

            int index = i;

            // 경로 추적 (start에서 i까지)
            Stack<int> path = new Stack<int>(); // Stack을 이용하여 경로 뒤집기

            // 경로 추적
            while (index != start)
            {
                path.Push(vertex[index]); // 현재 노드를 스택에 추가
                index = stringToInt(saveRoute[index]);  // 결정적인 역할을 한 꼭지점으로 이동
            }

            // start 노드도 포함시키기
            path.Push(vertex[start]);

            // 경로 출력
            while (path.Count > 0)
            {
                route += " " + path.Pop();
            }

            Debug.Log(route);
        }
    }

    void TracePath(int start, int end) // 특정 경로 확인
    {
        string route = "";
        Debug.Log("시작 꼭지점 " + vertex[start] + "부터 꼭지점 " + vertex[end] + "까지의 경로");

        int index = end;
        path_num = 0;

        // 경로 추적 (end에서 start까지)
        Stack<int> path = new Stack<int>(); // Stack을 이용하여 경로 뒤집기

        // 경로 추적
        while (index != start)
        {
            path.Push(vertex[index]); // 현재 노드를 스택에 추가
            index = stringToInt(saveRoute[index]);  // 결정적인 역할을 한 꼭지점으로 이동
        }

        // start 노드도 포함시키기
        path.Push(vertex[start]);
        int i = 0;

        // 경로 출력
        while (path.Count > 0)
        {
            dk_path[i] = path.Pop();  // 경로에서 노드를 꺼내서 배열에 저장
            route += " " + dk_path[i]; // 디버깅용 경로 출력
            i++;
            path_num++;
        }

        Debug.Log(route);
    }



    // 노드를 int로 변환하는 함수 (필요한 경우에만 사용)
    int stringToInt(int vertex)
    {
        return vertex; // 현재는 vertex가 이미 정수형이므로 그냥 반환
    }

    void PrintDistance(GraphType g) // 거리값 확인 함수
    {
        string distanceString = "";  // 거리 값을 저장할 변수

        // distance 배열의 값들을 한 줄로 합침
        for (int i = 0; i < g.n; i++)  // 노드 개수만큼
        {
            distanceString += distance[i].ToString() + "\t";  // 각 값을 탭으로 구분하여 추가
        }

        // 한 줄로 합쳐진 거리 출력
        Debug.Log(distanceString);
    }
    void DistanceSet(GraphType g)
    {
        for (int i = 0; i < g.n; i++)  // 0부터 7까지
        {
            distance[i] = 0;
        }
    }

    // 아래는 기차 베이스 코드-----------------------------------------------------------------------

    void CheckCurrentCircleConnections()
    {
        
        closestCircle = GetClosestCircle(train.transform.position);

        if (closestCircle != null)
        {
            int currentIndex = System.Array.IndexOf(circles, closestCircle);
            Debug.Log("현재 위치한 원 = " + closestCircle.name);

            targetCircle = null;
            int minWeight = int.MaxValue;

            foreach (var edge in edges)
            {
                if (edge.Item1 == currentIndex || edge.Item2 == currentIndex)
                {
                    int connectedIndex = (edge.Item1 == currentIndex) ? edge.Item2 : edge.Item1;
                    int weight = edgeWeights[(edge.Item1, edge.Item2)];

                    Debug.Log("연결된 원 = " + circles[connectedIndex].name + ", 가중치 = " + weight);

                    if (weight < minWeight)
                    {
                        minWeight = weight;
                        targetCircle = circles[connectedIndex];
                    }
                }
            }

            if (targetCircle != null)
            {
                Debug.Log("가장 낮은 가중치로 이동할 원 = " + targetCircle.name + ", 가중치 = " + minWeight);
                targetPosition = targetCircle.transform.position;
                isTrainMoving = true;
                trainMoveStartTime = Time.time;

            }
            else
            {
                Debug.Log("line_off");
            }
        }
    }

    public int GetCircle()
    {
        closestCircle = GetClosestCircle(train.transform.position);  // closestCircle은 GameObject 타입
        string circleName = closestCircle.name;  // GameObject의 이름을 가져옴
        string circleNumberString = circleName.Substring(6);  // "Circle" 이후의 숫자 부분만 추출
        int n = int.Parse(circleNumberString);  // 숫자 문자열을 int로 변환

        return n;
    }

    void MoveTrainToTarget()
    {

        // 일정한 속도로 이동 (속도 = 거리 / 시간)

        Vector3 targetPosition2D = new Vector3(targetCircle.transform.position.x,
                                                   targetCircle.transform.position.y,
                                                   -5f);

        // 기차의 현재 위치를 2D로 변환 (x, y 값만 사용)
        Vector3 trainPosition2D = new Vector3(train.transform.position.x, train.transform.position.y, -5f);

        train.transform.position = Vector3.MoveTowards(trainPosition2D, targetPosition2D, 0.1f);


        if (trainPosition2D == targetPosition2D )
        {
            // 지정한 시간이 지나면 순간이동

            isTrainMoving = false;  // 이동 완료 후 isTrainMoving을 false로 설정
            closestCircle = null;
            targetCircle = null;

            StartCoroutine(RestoreTrainMovement());
        }
    }

    IEnumerator RestoreTrainMovement()
    {
        // 1초 대기
        yield return new WaitForSeconds(1f);

        // 1초 후에 기차가 다시 이동할 수 있도록 설정
        Debug.Log('a');
        CheckCurrentCircleConnections();
        isTrainMoving = true;
    }

    void Dk_Move()
    {
        int edgeWeight = 0;
        int toNode, fromNode = 0;

        is_dk = true;
        // 경로가 남아 있을 때
        if (currentPathIndex < path_num) //
        {
            // 현재 목표 지점 (dk_path[currentPathIndex]는 목표로 가야 할 circle 번호)
            Vector3 targetPosition2D = new Vector3(circles[dk_path[currentPathIndex]].transform.position.x,
                                                   circles[dk_path[currentPathIndex]].transform.position.y,
                                                   -5f);

            // 기차의 현재 위치를 2D로 변환 (x, y 값만 사용)
            Vector3 trainPosition2D = new Vector3(train.transform.position.x, train.transform.position.y, -5f);

            train.transform.position = Vector3.MoveTowards(trainPosition2D, targetPosition2D, 0.1f);

            // 앞으로 이동할 간선의 가중치 디버깅 출력
            if (currentPathIndex > 0)
            {
                fromNode = dk_path[currentPathIndex - 1]; // 출발 노드
                toNode = dk_path[currentPathIndex];       // 도착 노드
                edgeWeight = g.weight[fromNode, toNode];  // 간선의 가중치
            }

            // 목표 지점에 도달했는지 확인 (x, y 기준으로만 거리 확인)
            if (trainPosition2D == targetPosition2D )  // 목표 위치에 충분히 가까워졌을 때
            {
                //Debug.Log(currentPathIndex);
                // 목표 지점에 도달하면 currentPathIndex를 증가시켜 다음 경로로 이동
                currentPathIndex++;
                bagDirector.Check_Circle_Bag();
                bagDirector.Loot_KanmpSack();

                int n = GetCircle();  // 현재 선택된 Circle의 인덱스를 가져옴
                if ( bagDirector.mid_node_one != n && bagDirector.mid_node_two != n ) // 경유지 통과 아닌 경우에만 감소
                {
                    current_max_line_weight -= edgeWeight;
                    max_line.text = current_max_line_weight.ToString();
                }

                // 경로 끝까지 갔으면 이동을 종료
                if (currentPathIndex >= path_num)
                {
                    train.transform.position = new Vector3(targetPosition2D.x, targetPosition2D.y, -5f); // 최종 도착 시 z 값 고정
                    isTrainMoving = false;  // 기차 이동 완료
                    is_dk = false;
                    closestCircle = null;
                    targetCircle = null;
                    currentPathIndex = 1;
                    SwapImages();
                    bagDirector.ActivateAndSetRandomSprites();
                    bagDirector.SetBag_SetScore();
                    DeleteAllEdges();
                    GenerateRandomEdges();
                    GenerateRandomEdges();
                    chance_line_delete = 3;
                    Set_line_light(chance_line_delete);
                }
            }
        }
    }


    public GameObject GetClosestCircle(Vector3 position)
    {
        GameObject closestCircle = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject circle in circles)
        {
            float distance = Vector3.Distance(position, circle.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCircle = circle;
            }
        }
        return closestCircle;
    }

    void DetectMouseClick()
    {
        if (Input.GetMouseButtonDown(0) && delete_check != 1 && link_selector == 0) // 링크 셀렉터가 0인 상태
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            GameObject clickedCircle = GetClickedCircle(mousePosition);

            if (clickedCircle != null)
            {
                int clickedIndex = System.Array.IndexOf(circles, clickedCircle);

                if (createSelectedCircle == clickedCircle)// 자기 자신노드에서 자기 자신으로 간선 추가 방지
                {
                    create_check = 0;
                    ChangeCircleColor(createSelectedCircle, originalColor);
                    createSelectedCircle = null;
                    return;
                }

                if (createSelectedCircle == null) // 첫번째 노드 클릭
                {
                    Debug.Log(clickedCircle + " 추가 1"); // 1 원 확인
                    createSelectedCircle = clickedCircle;
                    originalColor = clickedCircle.GetComponent<SpriteRenderer>().color;
                    ChangeCircleColor(clickedCircle, Color.green);
                    create_check = 1;
                }
                else // 간선 연결한 두번째 노드 클릭시
                {
                    Debug.Log(clickedCircle + " 추가 2"); // 2 원 확인

                    click_create = true;

                    CreateEdge(createSelectedCircle, clickedCircle); // 간선 추가 함수

                    click_create = false;   

                    ChangeCircleColor(createSelectedCircle, originalColor); // 색 복원

                    createSelectedCircle = null;
                    create_check = 0;
                }
            }
        }
        else if (Input.GetMouseButtonDown(0) && create_check != 1 && link_selector == 1 && chance_line_delete != 0)// 마우스 왼클릭 + 이전에 선 생성중 아님 + 링크 버튼이 언링크 상태 + 삭제 기화 0 x이면
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            GameObject clickedCircle = GetClickedCircle(mousePosition);

            if (clickedCircle != null)
            {
                int clickedIndex = System.Array.IndexOf(circles, clickedCircle);

                if (deleteSelectedCircle == null)
                {
                    deleteSelectedCircle = clickedCircle;
                    originalColor = clickedCircle.GetComponent<SpriteRenderer>().color;
                    ChangeCircleColor(clickedCircle, Color.red);
                    delete_check = 1;
                }
                else
                {
                    // 간선이 기차의 이동 경로에 포함되면 삭제할 수 없게 막음
                    if (IsTrainOnEdge(deleteSelectedCircle, clickedCircle))
                    {
                        Debug.Log("기차가 이 간선을 이동 중이므로 삭제할 수 없습니다.");
                        ChangeCircleColor(deleteSelectedCircle, originalColor);
                        deleteSelectedCircle = null;
                        delete_check = 0;
                        return;  // 간선이 기차의 경로에 포함되면 삭제 불가
                    }

                    int firstIndex = int.Parse(deleteSelectedCircle.name.Replace("Circle", ""));
                    int secondIndex = int.Parse(clickedCircle.name.Replace("Circle", ""));

                    g.weight[firstIndex, secondIndex] = INF;
                    g.weight[secondIndex, firstIndex] = INF;

                    DeleteEdge(deleteSelectedCircle, clickedCircle);
                    ChangeCircleColor(deleteSelectedCircle, originalColor);
                    deleteSelectedCircle = null;
                    delete_check = 0;
                    Set_line_light(--chance_line_delete); // 선 삭제 기회 1 감소후 UI 업데이트
                }
            }
        }
    }

    bool IsTrainOnEdge(GameObject start, GameObject end)
    {
        if (start == closestCircle && end == targetCircle)
        {
            return true;
        }
        else if (start == targetCircle && end == closestCircle)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    GameObject GetClickedCircle(Vector3 mousePosition)
    {
        foreach (GameObject circle in circles)
        {
            if (Vector3.Distance(circle.transform.position, mousePosition) < 0.8f)
            {
                return circle;
            }
        }
        return null;
    }

    void CreateEdge(GameObject start, GameObject end)
    {
        int first = Mathf.Min(System.Array.IndexOf(circles, start), System.Array.IndexOf(circles, end));
        int second = Mathf.Max(System.Array.IndexOf(circles, start), System.Array.IndexOf(circles, end));

        if (!edges.Contains((first, second)))
        {
            GameObject lineObject = Instantiate(linePrefab);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            int weight;

            lineRenderer.SetPosition(0, start.transform.position);
            lineRenderer.SetPosition(1, end.transform.position);

            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material.color = Color.white;
            lineRenderer.sortingLayerName = "Default";  // 사용하려는 레이어 이름 설정
            lineRenderer.sortingOrder = 1; // 숫자가 높을수록 앞으로 렌더링

            edges.Add((first, second));
            createdLines.Add(lineObject);

            if(click_create)
            {
                weight = next_line;
                SwapLine();
            }
            else
            {
                weight = Random.Range(w_s, w_e);
            }

            edgeWeights[(first, second)] = weight;
            DisplayEdgeWeight(lineObject, weight);

            line_weight = weight; // 클릭 함수에서 g 에 가중치 넣기 위해 전역으로 저장
       
            // 간선의 양방향 가중치 업데이트
            g.weight[first, second] = line_weight;
            g.weight[second, first] = line_weight;
        }
    }

    void DeleteEdge(GameObject start, GameObject end)
    {
        int first = Mathf.Min(System.Array.IndexOf(circles, start), System.Array.IndexOf(circles, end));
        int second = Mathf.Max(System.Array.IndexOf(circles, start), System.Array.IndexOf(circles, end));

        if (edges.Contains((first, second)))
        {
            edges.Remove((first, second));

            foreach (var line in createdLines)
            {
                LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                if (AreLinesEqual(lineRenderer, start, end))
                {
                    Destroy(line);
                    createdLines.Remove(line);
                    break;
                }
            }
        }
    }

    bool AreLinesEqual(LineRenderer lineRenderer, GameObject start, GameObject end)
    {
        return (Vector3.Distance(lineRenderer.GetPosition(0), start.transform.position) < 0.1f &&
                Vector3.Distance(lineRenderer.GetPosition(1), end.transform.position) < 0.1f) ||
               (Vector3.Distance(lineRenderer.GetPosition(1), start.transform.position) < 0.1f &&
                Vector3.Distance(lineRenderer.GetPosition(0), end.transform.position) < 0.1f);
    }

    void ChangeCircleColor(GameObject circle, Color color)
    {
        circle.GetComponent<SpriteRenderer>().color = color;
    }

    void DisplayEdgeWeight(GameObject lineObject, int weight)
    {
        GameObject weightTextObject = new GameObject("WeightText");
        weightTextObject.transform.SetParent(lineObject.transform);

        Vector3 midPoint = (lineObject.GetComponent<LineRenderer>().GetPosition(0) + lineObject.GetComponent<LineRenderer>().GetPosition(1)) / 2;
        midPoint.z = -2f;

        weightTextObject.transform.position = midPoint;

        TextMeshPro textMesh = weightTextObject.AddComponent<TextMeshPro>();
        textMesh.text = weight.ToString();
        textMesh.fontSize = 8f;
        textMesh.color = Color.red;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.rectTransform.sizeDelta = new Vector2(2f, 1f);

        Renderer renderer = weightTextObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Default";  // 설정하려는 Sorting Layer 이름
            renderer.sortingOrder = 2;  // Order in Layer 설정 (숫자가 클수록 앞으로 렌더링됨)
        }

    }

    bool IsTrainCurrentlyOnCircle(GameObject circle)
    {
        // 기차가 이동 중인 간선의 두 점과 기차의 현재 위치, 목표 위치가 비교됩니다.
        if (train.transform.position == circle.transform.position || targetPosition == circle.transform.position)
        {
            return true;
        }

        return false;
    }

}

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.IO;


public class GameDirector : MonoBehaviour
{
    // 1. ��� �� ���� ���� ����
    public GameObject[] circles; // ��� ����
    public GameObject train; // �����̴� ����


    // 2. �� ���� ����
    public GameObject linePrefab; // ���� ������ ������
    private GameObject createSelectedCircle = null; // �� ������ ù ��° ��� ����
    private GameObject deleteSelectedCircle = null; // �� ������ ù ��° ��� ����
    private HashSet<(int, int)> edges = new HashSet<(int, int)>(); // ������ ���� ��� (��� ��)
    private List<GameObject> createdLines = new List<GameObject>(); // ������ ��(GameObject) ���
    private Dictionary<(int, int), int> edgeWeights = new Dictionary<(int, int), int>(); // ������ ����ġ ���� (��� �� -> ����ġ)

    // 3. ���� �̵� ���� ����
    public bool isTrainMoving = false; // ������ �����̴��� ����
    private Vector3 targetPosition; // ������ �̵� ��ǥ ��ġ
    private float trainMoveStartTime; // ���� �̵� ���� �ð� (���� �ִϸ��̼��̳� �̵� ����)

    // 4. ���콺 Ŭ�� �� ���� ���� ���� ����
    public int create_check = 0; // �� ���� �� ���콺 Ŭ�� ���� ������
    public int delete_check = 0; // �� ���� �� ���콺 Ŭ�� ���� ������

    // 5. ��Ʈ �� ���� ����
    public TMP_FontAsset font; // ����ġ ��Ʈ
    private Color originalColor; // ���� �� ����� ����

    // 6. ������ ���� ��ġ �� ������ ��� ���� ����
    GameObject closestCircle; // ������ ���� ��ġ ��� (���� ����� ���)
    GameObject targetCircle; // ������ ������ ��� (�����Ϸ��� ���)


    // �⺻ ���̽� �ڵ�-------------------------------------------------------------------

    // �˰��� �ڵ�------
    GraphType g = new GraphType(10);// ��� �߰���!!
    int node_num;


    int[] distance = new int[100]; // ���������κ��� �ִ� ��� �Ÿ�
    bool[] found = new bool[100]; // �湮�� ���� ǥ��
    public const int INF = 100000000;  // INF �� ����
    int line_weight; // g �� ���� ����ġ �߰� ����� ����

    int w_s = 1; //���� ����ġ �ּ�
    int w_e = 20; // ���� ����ġ �ִ�

    int[] saveRoute; // ��� ������
    int[] vertex; // ��� ����

    int[] dk_path = new int[100]; // ���ͽ�Ʈ��� ���-> ���� ���� ��� ����
    public bool is_dk = false;
    int currentPathIndex = 1; // ���� �̵��� ��� �̵� �ߴ���
    int path_num; // ������ ��� �̵� �ؾ��ϴ��� ( ���ͽ�Ʈ�� )

    bool dk_can; // ���ͽ�Ʈ�� �˰����� ���� �Ұ����� ���, �ִ� ��η� �̵��ϴ� �Լ� ���� �Ұ����ϰ�

    float vibrationAmount = 0.2f;
    float vibrationSpeed = 20f;    // ���� �ӵ�
    Vector3 startPosition;
    bool vib = false;

    // UI ���� �ڵ�
    public Image[] uiImages; // UI�� �ִ� Image ������Ʈ 3���� �巡���Ͽ� ����
    public Sprite[] nodeSprites; // ����� �̹��� ��������Ʈ �迭 (0���� 7���� ��� �̹���)
    public int one_ui, two_ui, three_ui; // 1 ���� ���� ������. 2,3, ���� ���
    public TMP_Text max_line; // �̵� ������ ���� �ִ�ġ UI

    public int current_max_line_weight; // ���� �̵� ������ ���� ����ġ
    public int max_line_weight = 100; // ������ ����� �ʱ�ȭ �Ǵ� �ִ� ���� ����ġ

    public Button link_button;
    public Sprite[] link_button_img;
    public int link_selector = 0;

    public Image[] line_light;
    public Sprite[] line_ight_img;
    public int chance_line_delete = 3;

    // �賶 ���� �˰��� �ڵ�
    BagDirector BagDirector;

    public TMP_Text line_one, line_two, line_three;
    public int next_line; // ���� ���� ������ ����ġ
    bool click_create; // Ŭ������ ���� ���ý�


    // ���� ���۽� ����

    public Image start_img;
    public Sprite[] start_icon;
    TimerDirector timerDirector;
    BagDirector bagDirector;

    void Start()
    {
        //g.weight[0, 2] = 8; // �� ���ϴ��� �����
        Application.targetFrameRate = 60; // 60 ������ ����
        timerDirector = GetComponent<TimerDirector>();
        bagDirector = GetComponent<BagDirector>();

        node_num = nodeSprites.Length; // ��� ���� �ϵ� �ڵ� ���ϰ�
        g.PrintGraph(g); // �����
        DistanceSet(g); // g �ʱ�ȭ


        current_max_line_weight = max_line_weight;
        max_line.text = current_max_line_weight.ToString(); // �̵� ������ �ִ� ���� ����ġ ��

        Set_line_light(chance_line_delete); // ����Ʈ ���� 3

        StartCoroutine(ShowStartImages());

        //SetRandomImages(0); // �������� 0 �� ���������� ����
        //SetRandomLine(); // ���� ���� ������ ����
    }
    void Update()
    {
        DetectMouseClick();

        if (Input.GetKeyDown(KeyCode.Space) && !isTrainMoving)
        {
            Debug.Log("train_start");
            CheckCurrentCircleConnections(); //�����̽��� ������ ������ ������ ������
        }
        if( is_dk) // ������ ���� �̵�
        {
            Dk_Move(); 
        }
        if (isTrainMoving) // ������ ���� �̵�
        {
            MoveTrainToTarget(); 
        }

        if (Input.GetKeyDown(KeyCode.M))  // M Ű�� ������ �� �׷��� Ȯ��
        {
            g.PrintGraph(g);
        }
        if (Input.GetKeyDown(KeyCode.N))  // N Ű�� ������ �� �׷��� Ȯ��
        {
            PrintDistance(g);
        }


        if (Input.GetKeyDown(KeyCode.Q))  // Q ��ư�� Ŭ���ϸ� �̹��� ��ü
        {
            SwapImages();
        }
        if (Input.GetKeyDown(KeyCode.W))  // W Ű�� ������ �� ��� ���� ����
        {
            DeleteAllEdges();
        }
        if (Input.GetKeyDown(KeyCode.E))  // E Ű�� ������ ���� ���� ����
        {
            GenerateRandomEdges();
        }
        if (Input.GetKeyDown(KeyCode.R) && !isTrainMoving && !is_dk)  // �ִܰŸ����� �̵�
        {
            dk_can = true;
            int n = GetCircle(); // ��ġ�� �� ���� �ޱ� �Լ�
            Shortest_path(g, n);
            if (dk_can == false)
            {
                if(vib == false)
                {
                    startPosition = train.transform.position; // ���� ��ġ ����
                    StartCoroutine(VibrateSpider());
                }
                return; // ���ͽ�Ʈ�� ���� �ȵǴ� ��� ���߱�
            }


            TracePath(n, one_ui); // ������� 0, �������� 1�� 
            Dk_Move();
        }
        if (Input.GetKeyDown(KeyCode.T))  // T �ٟ޽�Ʈ�� �˰��� ����
        {
            int n = GetCircle(); // ��ġ�� �� ���� �ޱ� �Լ�
            Shortest_path(g, n);
        }
        if (Input.GetKeyDown(KeyCode.Y))  // Y Ű�� ��������� �� ������ �ִ� ���
        {
            int n = GetCircle(); // ��ġ�� �� ���� �ޱ� �Լ�
            TraceAllPaths(n);
        }
        if (Input.GetKeyDown(KeyCode.U))  // U Ű�� ������ �� ������������ ��θ� Ȯ��
        {
            int n = GetCircle(); // ��ġ�� �� ���� �ޱ� �Լ�
            TracePath(n, one_ui); // ������� ���� ��ġ, �������� 1��

        }
        if (Input.GetKeyDown(KeyCode.I))  // P Ű�� ������ �� ������ �ִ� �� ��ġ Ȯ��
        {
            SwapLine();
        }
        if (Input.GetKeyDown(KeyCode.P))  // P Ű�� ������ �� ������ �ִ� �� ��ġ Ȯ��
        {
            int n = GetCircle(); // ��ġ�� �� ���� �ޱ� �Լ�
            Debug.Log(n);  // ������ ���� ���
        }

    }

    void Set_line_light( int n ) // ���� ���� ��ŭ�� �Һ� Ȱ��ȭ n = 3, 2, 1, 0 �� ����
    {
        for(int i = 0; i < 3; i++) // ���� �� 3��. 0, 1, 2
        {
            if( i < n )
            {
                line_light[i].sprite = line_ight_img[0];  // ���� ���ٷ�?
            }
            else
            {
                line_light[i].sprite = line_ight_img[1];  // ���� ���ٷ�?
            }
        }

    }

    public void Press_Link_Button()
    {
        SpriteState spritestate = link_button.spriteState;

        if (link_selector == 0) // ��ũ ���¿���
        {

            link_selector = 1;
            link_button.image.sprite = link_button_img[2];
            spritestate.pressedSprite = link_button_img[3];

        }
        else // ��ũ ���¿���
        {
            link_selector = 0;
            link_button.image.sprite = link_button_img[0];
            spritestate.pressedSprite = link_button_img[1];
        }

        link_button.spriteState = spritestate; // ����� SpriteState ��ư�� �ٽ� ����
    }

    public void Press_Run_Button()
    {
        if (!isTrainMoving && !is_dk && timerDirector.game_start)  // �ִܰŸ����� �̵�. ���� ������ ��ư �۵�
        {
            dk_can = true;
            int n = GetCircle(); // ��ġ�� �� ���� �ޱ� �Լ�
            Shortest_path(g, n);
            if (dk_can == false)
            {
                if (vib == false)
                {
                    startPosition = train.transform.position; // ���� ��ġ ����
                    StartCoroutine(VibrateSpider());
                }
                return; // ���ͽ�Ʈ�� ���� �ȵǴ� ��� ���߱�
            }


            TracePath(n, one_ui); // ������� 0, �������� 1�� 
            Dk_Move();
        }
    }

    IEnumerator ShowStartImages() // ���� ����!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {

        foreach (Sprite icon in start_icon)
        {
            start_img.sprite = icon;  // ���� ��������Ʈ ����
            yield return new WaitForSeconds(1f); // 1�� ���

        }

        start_img.enabled = false; // �̹��� �����
       
        SetRandomImages(0); // �������� 0 �� ���������� ����
        SetRandomLine(); // ���� ���� ������ ����
        timerDirector.game_start = true;
        bagDirector.ActivateAndSetRandomSprites();
        GenerateRandomEdges();
        GenerateRandomEdges();
    }

    IEnumerator VibrateSpider()
    {
        float elapsedTime = 0f;
        vib = true;

        while (elapsedTime < 0.6f) // ������ 1�� ���ȸ� ����
        {
            // Sin �Լ��� ���� ����
            float offset = Mathf.Sin(elapsedTime * vibrationSpeed) * vibrationAmount;
            train.transform.position = startPosition + new Vector3(0, offset, 0); // Y������ ����

            elapsedTime += Time.deltaTime; // �ð� ����
            yield return null;
        }

        train.transform.position = startPosition; // ���� �� ����ġ��
        vib = false;
    }

    void SetRandomImages(int n)
    {

        // 0���� 7������ ��� �� �����ϰ� 3�� ����, n�� ����
        List<int> randomNodes = new List<int>();
        while (randomNodes.Count < 3)
        {
            int randomNum = Random.Range(0, nodeSprites.Length);
            if (randomNum != n && !randomNodes.Contains(randomNum)) // n�� �ߺ��� �� ����
            {
                randomNodes.Add(randomNum);
            }
        }
        one_ui = randomNodes[0];
        two_ui = randomNodes[1];
        three_ui = randomNodes[2];

        // ���õ� ��忡 ���� �̹��� ����
        for (int i = 0; i < uiImages.Length; i++)
        {
            uiImages[i].sprite = nodeSprites[randomNodes[i]]; // �� Image�� ��������Ʈ �Ҵ�
        }
    }

    void SwapImages()
    {
        // ù ��° �̹��� ���� �� ��°��, �� ��° �̹��� ���� �� ��°�� ����
        int temp = one_ui;
        one_ui = two_ui;
        two_ui = three_ui;

        // �� ��° �̹����� �� ��° �̹��� ���� ��ġ�� �ʴ� ���� ������ ����
        int randomNum;
        do
        {
            randomNum = Random.Range(0, nodeSprites.Length);  // 0���� 7���� ������
        } while (randomNum == temp || randomNum == one_ui || randomNum == two_ui);  // �� ��° �̹��� ���� ��ġ�� �ʰ�

        three_ui = randomNum;

        // ����� �̹��� ������ UI ������Ʈ
        uiImages[0].sprite = nodeSprites[one_ui];
        //uiImages[1].sprite = nodeSprites[two_ui];
        //uiImages[2].sprite = nodeSprites[three_ui]; // �̰� ���� ���� �ϳ��� ���̰�
    }

    void SetRandomLine()
    {
        line_one.text = Random.Range(w_s, w_e + 1).ToString();  // w_s ���� w_e���� ���� �� (w_e ����)
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
        // ��� ���� �ֿ� ���� DeleteEdge ȣ��
        for (int i = 0; i < circles.Length; i++)
        {
            for (int j = i + 1; j < circles.Length; j++)
            {
                // �� ���� ���� ������ ����
                DeleteEdge(circles[i], circles[j]);
            }
        }
        g.SetGraph(node_num);
    }
    void GenerateRandomEdges()
    {

        List<(int, int)> possibleEdges = new List<(int, int)>();

        // ������ ��� ���� �� ���� (�ߺ� ����)
        for (int i = 0; i < circles.Length; i++)
        {
            for (int j = i + 1; j < circles.Length; j++)
            {
                possibleEdges.Add((i, j));
            }
        }

        // ���� - 10���� ������ �����ϰ� �����Ͽ� ����
        int edgeCount = circles.Length/2;
        for (int k = 0; k < edgeCount; k++)
        {
            int randomIndex = Random.Range(0, possibleEdges.Count);
            (int startIdx, int endIdx) = possibleEdges[randomIndex];
            possibleEdges.RemoveAt(randomIndex);

            CreateEdge(circles[startIdx], circles[endIdx]);
        }
    }

    // ���ͽ�Ʈ��� �� ã�� �ڵ�--------------------------------------------------------------


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
        } // ��� �迭�� �ʱ�ȭ

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
            Debug.Log("���ͽ�Ʈ��~");
            u = Choose(distance, g.n, found);
            //Debug.Log("���õ� ��� : " + u);
            if (u == -1)
            {
                Debug.Log("���ͽ�Ʈ�� ���� ���� ���� ����");
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

    void TraceAllPaths(int start) // ��� ��� Ȯ��
    {
        // ��� ��忡 ���� ��� ����
        for (int i = 0; i < g.n; i++)
        {
            string route = "";
            Debug.Log("���� ������ " + vertex[start] + "���� ������ " + vertex[i] + "������ ���");

            int index = i;

            // ��� ���� (start���� i����)
            Stack<int> path = new Stack<int>(); // Stack�� �̿��Ͽ� ��� ������

            // ��� ����
            while (index != start)
            {
                path.Push(vertex[index]); // ���� ��带 ���ÿ� �߰�
                index = stringToInt(saveRoute[index]);  // �������� ������ �� ���������� �̵�
            }

            // start ��嵵 ���Խ�Ű��
            path.Push(vertex[start]);

            // ��� ���
            while (path.Count > 0)
            {
                route += " " + path.Pop();
            }

            Debug.Log(route);
        }
    }

    void TracePath(int start, int end) // Ư�� ��� Ȯ��
    {
        string route = "";
        Debug.Log("���� ������ " + vertex[start] + "���� ������ " + vertex[end] + "������ ���");

        int index = end;
        path_num = 0;

        // ��� ���� (end���� start����)
        Stack<int> path = new Stack<int>(); // Stack�� �̿��Ͽ� ��� ������

        // ��� ����
        while (index != start)
        {
            path.Push(vertex[index]); // ���� ��带 ���ÿ� �߰�
            index = stringToInt(saveRoute[index]);  // �������� ������ �� ���������� �̵�
        }

        // start ��嵵 ���Խ�Ű��
        path.Push(vertex[start]);
        int i = 0;

        // ��� ���
        while (path.Count > 0)
        {
            dk_path[i] = path.Pop();  // ��ο��� ��带 ������ �迭�� ����
            route += " " + dk_path[i]; // ������ ��� ���
            i++;
            path_num++;
        }

        Debug.Log(route);
    }



    // ��带 int�� ��ȯ�ϴ� �Լ� (�ʿ��� ��쿡�� ���)
    int stringToInt(int vertex)
    {
        return vertex; // ����� vertex�� �̹� �������̹Ƿ� �׳� ��ȯ
    }

    void PrintDistance(GraphType g) // �Ÿ��� Ȯ�� �Լ�
    {
        string distanceString = "";  // �Ÿ� ���� ������ ����

        // distance �迭�� ������ �� �ٷ� ��ħ
        for (int i = 0; i < g.n; i++)  // ��� ������ŭ
        {
            distanceString += distance[i].ToString() + "\t";  // �� ���� ������ �����Ͽ� �߰�
        }

        // �� �ٷ� ������ �Ÿ� ���
        Debug.Log(distanceString);
    }
    void DistanceSet(GraphType g)
    {
        for (int i = 0; i < g.n; i++)  // 0���� 7����
        {
            distance[i] = 0;
        }
    }

    // �Ʒ��� ���� ���̽� �ڵ�-----------------------------------------------------------------------

    void CheckCurrentCircleConnections()
    {
        
        closestCircle = GetClosestCircle(train.transform.position);

        if (closestCircle != null)
        {
            int currentIndex = System.Array.IndexOf(circles, closestCircle);
            Debug.Log("���� ��ġ�� �� = " + closestCircle.name);

            targetCircle = null;
            int minWeight = int.MaxValue;

            foreach (var edge in edges)
            {
                if (edge.Item1 == currentIndex || edge.Item2 == currentIndex)
                {
                    int connectedIndex = (edge.Item1 == currentIndex) ? edge.Item2 : edge.Item1;
                    int weight = edgeWeights[(edge.Item1, edge.Item2)];

                    Debug.Log("����� �� = " + circles[connectedIndex].name + ", ����ġ = " + weight);

                    if (weight < minWeight)
                    {
                        minWeight = weight;
                        targetCircle = circles[connectedIndex];
                    }
                }
            }

            if (targetCircle != null)
            {
                Debug.Log("���� ���� ����ġ�� �̵��� �� = " + targetCircle.name + ", ����ġ = " + minWeight);
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
        closestCircle = GetClosestCircle(train.transform.position);  // closestCircle�� GameObject Ÿ��
        string circleName = closestCircle.name;  // GameObject�� �̸��� ������
        string circleNumberString = circleName.Substring(6);  // "Circle" ������ ���� �κи� ����
        int n = int.Parse(circleNumberString);  // ���� ���ڿ��� int�� ��ȯ

        return n;
    }

    void MoveTrainToTarget()
    {

        // ������ �ӵ��� �̵� (�ӵ� = �Ÿ� / �ð�)

        Vector3 targetPosition2D = new Vector3(targetCircle.transform.position.x,
                                                   targetCircle.transform.position.y,
                                                   -5f);

        // ������ ���� ��ġ�� 2D�� ��ȯ (x, y ���� ���)
        Vector3 trainPosition2D = new Vector3(train.transform.position.x, train.transform.position.y, -5f);

        train.transform.position = Vector3.MoveTowards(trainPosition2D, targetPosition2D, 0.1f);


        if (trainPosition2D == targetPosition2D )
        {
            // ������ �ð��� ������ �����̵�

            isTrainMoving = false;  // �̵� �Ϸ� �� isTrainMoving�� false�� ����
            closestCircle = null;
            targetCircle = null;

            StartCoroutine(RestoreTrainMovement());
        }
    }

    IEnumerator RestoreTrainMovement()
    {
        // 1�� ���
        yield return new WaitForSeconds(1f);

        // 1�� �Ŀ� ������ �ٽ� �̵��� �� �ֵ��� ����
        Debug.Log('a');
        CheckCurrentCircleConnections();
        isTrainMoving = true;
    }

    void Dk_Move()
    {
        int edgeWeight = 0;
        int toNode, fromNode = 0;

        is_dk = true;
        // ��ΰ� ���� ���� ��
        if (currentPathIndex < path_num) //
        {
            // ���� ��ǥ ���� (dk_path[currentPathIndex]�� ��ǥ�� ���� �� circle ��ȣ)
            Vector3 targetPosition2D = new Vector3(circles[dk_path[currentPathIndex]].transform.position.x,
                                                   circles[dk_path[currentPathIndex]].transform.position.y,
                                                   -5f);

            // ������ ���� ��ġ�� 2D�� ��ȯ (x, y ���� ���)
            Vector3 trainPosition2D = new Vector3(train.transform.position.x, train.transform.position.y, -5f);

            train.transform.position = Vector3.MoveTowards(trainPosition2D, targetPosition2D, 0.1f);

            // ������ �̵��� ������ ����ġ ����� ���
            if (currentPathIndex > 0)
            {
                fromNode = dk_path[currentPathIndex - 1]; // ��� ���
                toNode = dk_path[currentPathIndex];       // ���� ���
                edgeWeight = g.weight[fromNode, toNode];  // ������ ����ġ
            }

            // ��ǥ ������ �����ߴ��� Ȯ�� (x, y �������θ� �Ÿ� Ȯ��)
            if (trainPosition2D == targetPosition2D )  // ��ǥ ��ġ�� ����� ��������� ��
            {
                //Debug.Log(currentPathIndex);
                // ��ǥ ������ �����ϸ� currentPathIndex�� �������� ���� ��η� �̵�
                currentPathIndex++;
                bagDirector.Check_Circle_Bag();
                bagDirector.Loot_KanmpSack();

                int n = GetCircle();  // ���� ���õ� Circle�� �ε����� ������
                if ( bagDirector.mid_node_one != n && bagDirector.mid_node_two != n ) // ������ ��� �ƴ� ��쿡�� ����
                {
                    current_max_line_weight -= edgeWeight;
                    max_line.text = current_max_line_weight.ToString();
                }

                // ��� ������ ������ �̵��� ����
                if (currentPathIndex >= path_num)
                {
                    train.transform.position = new Vector3(targetPosition2D.x, targetPosition2D.y, -5f); // ���� ���� �� z �� ����
                    isTrainMoving = false;  // ���� �̵� �Ϸ�
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
        if (Input.GetMouseButtonDown(0) && delete_check != 1 && link_selector == 0) // ��ũ �����Ͱ� 0�� ����
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            GameObject clickedCircle = GetClickedCircle(mousePosition);

            if (clickedCircle != null)
            {
                int clickedIndex = System.Array.IndexOf(circles, clickedCircle);

                if (createSelectedCircle == clickedCircle)// �ڱ� �ڽų�忡�� �ڱ� �ڽ����� ���� �߰� ����
                {
                    create_check = 0;
                    ChangeCircleColor(createSelectedCircle, originalColor);
                    createSelectedCircle = null;
                    return;
                }

                if (createSelectedCircle == null) // ù��° ��� Ŭ��
                {
                    Debug.Log(clickedCircle + " �߰� 1"); // 1 �� Ȯ��
                    createSelectedCircle = clickedCircle;
                    originalColor = clickedCircle.GetComponent<SpriteRenderer>().color;
                    ChangeCircleColor(clickedCircle, Color.green);
                    create_check = 1;
                }
                else // ���� ������ �ι�° ��� Ŭ����
                {
                    Debug.Log(clickedCircle + " �߰� 2"); // 2 �� Ȯ��

                    click_create = true;

                    CreateEdge(createSelectedCircle, clickedCircle); // ���� �߰� �Լ�

                    click_create = false;   

                    ChangeCircleColor(createSelectedCircle, originalColor); // �� ����

                    createSelectedCircle = null;
                    create_check = 0;
                }
            }
        }
        else if (Input.GetMouseButtonDown(0) && create_check != 1 && link_selector == 1 && chance_line_delete != 0)// ���콺 ��Ŭ�� + ������ �� ������ �ƴ� + ��ũ ��ư�� ��ũ ���� + ���� ��ȭ 0 x�̸�
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
                    // ������ ������ �̵� ��ο� ���ԵǸ� ������ �� ���� ����
                    if (IsTrainOnEdge(deleteSelectedCircle, clickedCircle))
                    {
                        Debug.Log("������ �� ������ �̵� ���̹Ƿ� ������ �� �����ϴ�.");
                        ChangeCircleColor(deleteSelectedCircle, originalColor);
                        deleteSelectedCircle = null;
                        delete_check = 0;
                        return;  // ������ ������ ��ο� ���ԵǸ� ���� �Ұ�
                    }

                    int firstIndex = int.Parse(deleteSelectedCircle.name.Replace("Circle", ""));
                    int secondIndex = int.Parse(clickedCircle.name.Replace("Circle", ""));

                    g.weight[firstIndex, secondIndex] = INF;
                    g.weight[secondIndex, firstIndex] = INF;

                    DeleteEdge(deleteSelectedCircle, clickedCircle);
                    ChangeCircleColor(deleteSelectedCircle, originalColor);
                    deleteSelectedCircle = null;
                    delete_check = 0;
                    Set_line_light(--chance_line_delete); // �� ���� ��ȸ 1 ������ UI ������Ʈ
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
            lineRenderer.sortingLayerName = "Default";  // ����Ϸ��� ���̾� �̸� ����
            lineRenderer.sortingOrder = 1; // ���ڰ� �������� ������ ������

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

            line_weight = weight; // Ŭ�� �Լ����� g �� ����ġ �ֱ� ���� �������� ����
       
            // ������ ����� ����ġ ������Ʈ
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
            renderer.sortingLayerName = "Default";  // �����Ϸ��� Sorting Layer �̸�
            renderer.sortingOrder = 2;  // Order in Layer ���� (���ڰ� Ŭ���� ������ ��������)
        }

    }

    bool IsTrainCurrentlyOnCircle(GameObject circle)
    {
        // ������ �̵� ���� ������ �� ���� ������ ���� ��ġ, ��ǥ ��ġ�� �񱳵˴ϴ�.
        if (train.transform.position == circle.transform.position || targetPosition == circle.transform.position)
        {
            return true;
        }

        return false;
    }

}

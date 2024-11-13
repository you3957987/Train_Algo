using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;


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
    public GameObject closestCircle; // ������ ���� ��ġ ��� (���� ����� ���)
    public GameObject targetCircle; // ������ ������ ��� (�����Ϸ��� ���)


    // �⺻ ���̽� �ڵ�-------------------------------------------------------------------

    // �˰��� �ڵ�------

    int[] distance = new int[100]; // ���������κ��� �ִ� ��� �Ÿ�
    bool[] found = new bool[100]; // �湮�� ���� ǥ��
    public const int INF = 100000000;  // INF �� ����
    int line_weight; // g �� ���� ����ġ �߰� ����� ����

    GraphType g = new GraphType(8);// ���⼭ ��� ���� ��ȭ�� ����
    int w_s = 1; //���� ����ġ �ּ�
    int w_e = 20; // ���� ����ġ �ִ�

    int[] saveRoute; // ��� ������
    int[] vertex; // ��� ����

    int[] dk_path = new int[100]; // ���ͽ�Ʈ��� ���-> ���� ���� ��� ����
    public bool is_dk = false;
    int currentPathIndex = 1;
   
    void Start()
    {
        //g.weight[0, 2] = 8; // �� ���ϴ��� �����
        g.PrintGraph(g);
        DistanceSet(g);
    }

    void Update()
    {
        DetectMouseClick();

        if (Input.GetKeyDown(KeyCode.Space) && !isTrainMoving)
        {
            Debug.Log("train_start");
            CheckCurrentCircleConnections(); //�����̽��� ������ ������ ������ ������
        }

        if (Input.GetKeyDown(KeyCode.H) && !isTrainMoving && !is_dk )
        {
            TracePath(7, 0); // ������� 7, �������� 0 ���� ���� �� ����
            Dk_Move();
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

        if (Input.GetKeyDown(KeyCode.K))  // k �ٟ޽�Ʈ�� �˰��� ����
        {
            Shortest_path(g, 7); // ������� 7
        }
        if (Input.GetKeyDown(KeyCode.T))  // T Ű�� ��������� �� ������ �ִ� ���
        {
            TraceAllPaths(7); // �������� 7
        }
        if (Input.GetKeyDown(KeyCode.Y))  // Y Ű�� ������ �� ������������ ��θ� Ȯ��
        {
            TracePath(7,0); // ������� 7, �������� 0 ���� ���� �� ����

        }
        if (Input.GetKeyDown(KeyCode.P))  // P Ű�� ������ �� ������ �ִ� �� ��ġ Ȯ��
        {
            int n = GetCircle(); // ��ġ�� �� ���� �ޱ� �Լ�
            Debug.Log(n);  // ������ ���� ���
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
            Debug.Log("���õ� ��� : " + u);
            if (u == -1)
            {
                Debug.Log("���ͽ�Ʈ�� ���� ���� ���� ����");
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

    int GetCircle()
    {
        closestCircle = GetClosestCircle(train.transform.position);  // closestCircle�� GameObject Ÿ��
        string circleName = closestCircle.name;  // GameObject�� �̸��� ������
        string circleNumberString = circleName.Substring(6);  // "Circle" ������ ���� �κи� ����
        int n = int.Parse(circleNumberString);  // ���� ���ڿ��� int�� ��ȯ

        return n;
    }

    void MoveTrainToTarget()
    {
        float duration = 4f; // �̵��� �ð� (��)
        float timeElapsed = Time.time - trainMoveStartTime;
        float speedMultiplier = 4f; // �� ���� �����Ͽ� �̵� �ӵ��� ������ų �� �ֽ��ϴ�.

        // �̵��� �Ÿ� (��ǥ������ �Ÿ�)
        float distance = Vector3.Distance(train.transform.position, targetPosition);

        // ������ �ӵ��� �̵� (�ӵ� = �Ÿ� / �ð�)
        if (timeElapsed < duration)
        {
            // �̵��� �Ÿ� ������ ����Ͽ� ���� �ӵ��� �̵�
            float step = distance / duration * Time.deltaTime;

            // ������ �ӵ��� �̵� (�ӵ� ���� ����)
            Vector3 newPosition = Vector3.MoveTowards(train.transform.position, targetPosition, step * speedMultiplier);

            // z ���� �׻� -5�� ����
            newPosition.z = -5f;

            train.transform.position = newPosition;
        }
        else
        {
            // ������ �ð��� ������ �����̵�
            train.transform.position = new Vector3(targetPosition.x, targetPosition.y, -5f);
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
        CheckCurrentCircleConnections();
        isTrainMoving = true;
    }

    void Dk_Move()
    {
        // duration�� ��θ��� �̵��� �ð����� ����
        float duration = 3f; // �� ��� ���� �̵� �ð� (��)
        float timeElapsed = Time.time - trainMoveStartTime;
        is_dk = true;
        float speedMultiplier = 3f;

        // ��ΰ� ���� ���� ��
        if (currentPathIndex < dk_path.Length)
        {
            // ���� ��ǥ ���� (dk_path[currentPathIndex]�� ��ǥ�� ���� �� circle ��ȣ)
            Vector2 targetPosition2D = new Vector2(circles[dk_path[currentPathIndex]].transform.position.x,
                                                   circles[dk_path[currentPathIndex]].transform.position.y);

            // ������ ���� ��ġ�� 2D�� ��ȯ (x, y ���� ���)
            Vector2 trainPosition2D = new Vector2(train.transform.position.x, train.transform.position.y);

            // ��ǥ ��ġ������ �Ÿ� (x�� y ��ǥ�� �������� ���)
            float distance = Vector2.Distance(trainPosition2D, targetPosition2D);

            // �̵��� �Ÿ� ������ ����Ͽ� ���� �ӵ��� �̵�
            float step = distance / duration * Time.deltaTime;

            // �ӵ� ���� ���� ���� �� 2D ��ġ ������Ʈ
            Vector2 newPosition2D = Vector2.MoveTowards(trainPosition2D, targetPosition2D, step * speedMultiplier);

            // �� ��ġ�� z ���� -5�� �����Ͽ� �̵� ����
            train.transform.position = new Vector3(newPosition2D.x, newPosition2D.y, -5f);

            // ��ǥ ������ �����ߴ��� Ȯ�� (x, y �������θ� �Ÿ� Ȯ��)
            if (distance < 0.1f)  // ��ǥ ��ġ�� ����� ��������� ��
            {
                // ��ǥ ������ �����ϸ� currentPathIndex�� �������� ���� ��η� �̵�
                currentPathIndex++;

                // �̵��� ��� ������ �� �ֵ��� timeElapsed�� �ʱ�ȭ (���� �ð� ����� ��� ����� �ʿ� ����)
                trainMoveStartTime = Time.time;

                // ��� ������ ������ �̵��� ����
                if (currentPathIndex >= dk_path.Length)
                {
                    train.transform.position = new Vector3(targetPosition2D.x, targetPosition2D.y, -5f); // ���� ���� �� z �� ����
                    isTrainMoving = false;  // ���� �̵� �Ϸ�
                    is_dk = false;
                    closestCircle = null;
                    targetCircle = null;
                    currentPathIndex = 1;
                }
            }
        }
    }


    GameObject GetClosestCircle(Vector3 position)
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
        if (Input.GetMouseButtonDown(0) && delete_check != 1)
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
                    CreateEdge(createSelectedCircle, clickedCircle); // ���� �߰� �Լ�
                    ChangeCircleColor(createSelectedCircle, originalColor); // �� ����

                    int firstIndex = int.Parse(createSelectedCircle.name.Replace("Circle", ""));
                    int secondIndex = int.Parse(clickedCircle.name.Replace("Circle", ""));

                    // ������ ����� ����ġ ������Ʈ
                    g.weight[firstIndex, secondIndex] = line_weight;
                    g.weight[secondIndex, firstIndex] = line_weight;

                    createSelectedCircle = null;
                    create_check = 0;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && create_check != 1)
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
            if (Vector3.Distance(circle.transform.position, mousePosition) < 0.5f)
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

            lineRenderer.SetPosition(0, start.transform.position);
            lineRenderer.SetPosition(1, end.transform.position);

            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material.color = Color.black;

            edges.Add((first, second));
            createdLines.Add(lineObject);
            int weight = Random.Range(w_s, w_e);
            edgeWeights[(first, second)] = weight;
            DisplayEdgeWeight(lineObject, weight);

            line_weight = weight; // Ŭ�� �Լ����� g �� ����ġ �ֱ� ���� �������� ����
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
        midPoint.z = -9f;

        weightTextObject.transform.position = midPoint;

        TextMeshPro textMesh = weightTextObject.AddComponent<TextMeshPro>();
        textMesh.text = weight.ToString();
        textMesh.fontSize = 10f;
        textMesh.color = Color.red;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.rectTransform.sizeDelta = new Vector2(2f, 1f);
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
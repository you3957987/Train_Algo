using UnityEngine;

public class GraphType
{
    public int n { get; set; } // 노드의 개수
    public int[,] weight { get; set; } // 가중치 배열
    public const int INF = 100000000;  // INF 값 정의

    public GraphType(int size)
    {
        n = size;
        weight = new int[100, 100]; // 100x100 크기로 초기화
        SetGraph(n);
    }

    public void  SetGraph(int n) // g 값 초기화 하는 코드
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                {
                    weight[i, j] = INF;  // 자기 자신이 아닌 것들은 INF로 설정
                }
                else
                {
                    weight[i, j] = 0;  // 자기 자신과의 거리는 0
                }
            }
        }
    }

    public void PrintGraph(GraphType g) // 그래프 상태 디버깅하는 코드
    {
        int n = g.n;  // GraphType에서 n값을 가져옴
        int i, j;

        for (i = 0; i < n; i++)
        {
            string rowState = "";  // 각 행에 대한 상태를 저장할 변수

            for (j = 0; j < n; j++)
            {
                string value = g.weight[i, j] == GraphType.INF ? "INF" : g.weight[i, j].ToString();
                rowState += value + "\t";  // 각 요소를 탭으로 구분하여 연결
            }

            // 각 행의 상태를 한 줄씩 출력
            Debug.Log("Row " + i + ": " + rowState);  // 각 행을 바로 출력
        }
    }
}

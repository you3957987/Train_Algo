using UnityEngine;

public class GraphType
{
    public int n { get; set; } // ����� ����
    public int[,] weight { get; set; } // ����ġ �迭
    public const int INF = 100000000;  // INF �� ����

    public GraphType(int size)
    {
        n = size;
        weight = new int[100, 100]; // 100x100 ũ��� �ʱ�ȭ
        SetGraph(n);
    }

    public void  SetGraph(int n) // g �� �ʱ�ȭ �ϴ� �ڵ�
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                {
                    weight[i, j] = INF;  // �ڱ� �ڽ��� �ƴ� �͵��� INF�� ����
                }
                else
                {
                    weight[i, j] = 0;  // �ڱ� �ڽŰ��� �Ÿ��� 0
                }
            }
        }
    }

    public void PrintGraph(GraphType g) // �׷��� ���� ������ϴ� �ڵ�
    {
        int n = g.n;  // GraphType���� n���� ������
        int i, j;

        for (i = 0; i < n; i++)
        {
            string rowState = "";  // �� �࿡ ���� ���¸� ������ ����

            for (j = 0; j < n; j++)
            {
                string value = g.weight[i, j] == GraphType.INF ? "INF" : g.weight[i, j].ToString();
                rowState += value + "\t";  // �� ��Ҹ� ������ �����Ͽ� ����
            }

            // �� ���� ���¸� �� �پ� ���
            Debug.Log("Row " + i + ": " + rowState);  // �� ���� �ٷ� ���
        }
    }
}

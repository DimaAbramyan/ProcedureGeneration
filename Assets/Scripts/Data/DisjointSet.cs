using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DisjointSet
{
    private int[] parent;
    private int[] rank;

    public DisjointSet(int size)
    {
        parent = new int[size];
        rank = new int[size];

        for (int i = 0; i < size; i++)
        {
            parent[i] = i;
            rank[i] = 0;
        }
    }

    public int Find(int x)
    {
        if (parent[x] != x)
            parent[x] = Find(parent[x]);

        return parent[x];
    }

    public void Union(int a, int b)
    {
        int rootA = Find(a);
        int rootB = Find(b);

        if (rootA == rootB)
            return;

        if (rank[rootA] < rank[rootB])
        {
            parent[rootA] = rootB;
        }
        else if (rank[rootA] > rank[rootB])
        {
            parent[rootB] = rootA;
        }
        else
        {
            parent[rootB] = rootA;
            rank[rootA]++;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph {
    public int N;
    public List<List<bool>> adjMat;
    public int maxClique;

    public Graph(int _N, float edgeProb) {
        N = _N;
        // Create an N x N matrix initialized to 0.
        adjMat = new List<List<bool>>();
        for (int i = 0; i < N; i++) {
            List<bool> row = new List<bool>();
            for (int j = 0; j < N; j++) {
                row.Add(false);
            }
            adjMat.Add(row);
        }
        GenRandomGraph(edgeProb);
        maxClique = MaxClique();
    }

    public void GenRandomGraph(float edgeProb) {
        for (int i = 0; i < N; i++) {
            for (int j = i + 1; j < N; j++) {
                if (Random.value < edgeProb) {
                    adjMat[i][j] = true;
                    adjMat[j][i] = true;
                }
            }
        }
    }

    public bool CheckClique(List<int> nodes) {
        for (int i = 0; i < nodes.Count; i++) {
            for (int j = i + 1; j < nodes.Count; j++) {
                int u = nodes[i];
                int v = nodes[j];
                if (!adjMat[u][v]) {
                    return false;
                }
            }
        }
        return true;
    }

    public int MaxClique() {
        int maxSize = 0;
        // Iterate through all subsets of N nodes, represented by 
        // an integer bitmask from [0, 2^N). Note that this assumes
        // there are less than 31 nodes (otherwise the bitmask overflows).
        for (int mask = 0; mask < (1 << N); mask++) {
            List<int> nodes = new List<int>();
            for (int i = 0; i < N; i++) {
                if ((mask & (1 << i)) != 0) {
                    nodes.Add(i);
                }
            }
            if (CheckClique(nodes)) {
                maxSize = Mathf.Max(maxSize, nodes.Count);
            }
        }
        return maxSize;
    }
}

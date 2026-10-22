/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph {
    // Number of nodes
    public int N;
    // Adjacency matrix
    public bool[,] adjMat;
    // Size of maximum clique (stored so it doesn't have to be recomputed)
    public int maxClique;
    // Size of minimum vertex cover
    public int minVertexCover;

    public Graph(int _N, float edgeProb, bool hamPath) {
        N = _N;
        // Create an N x N matrix initialized to 0.
        adjMat = new bool[N, N];
        GenRandomGraph(edgeProb);
        if (hamPath) {
            GenHamiltonianPath();
        }
        maxClique = MaxClique();
        minVertexCover = MinVertexCover();
    }

    // Generates a random Hamiltonian path in the graph.
    public void GenHamiltonianPath() {
        // Generate random permutation for the Hamiltonian path
        int[] p = new int[N];
        for (int i = 0; i < N; i++) {
            p[i] = i;
        }
        System.Random rng = new System.Random();
        for (int i = N - 1; i > 0; i--) {
            int j = rng.Next(i + 1); // Random integer in [0, i]
            (p[i], p[j]) = (p[j], p[i]);
        }

        // Add edges along path to graph
        for (int i = 1; i < N; i++) {
            adjMat[p[i - 1], p[i]] = true;
            adjMat[p[i], p[i - 1]] = true;
        }
    }

    // Generate a random graph where between any two nodes,
    // an edge is generated with probabality edgeProb.
    public void GenRandomGraph(float edgeProb) {
        for (int i = 0; i < N; i++) {
            for (int j = i + 1; j < N; j++) {
                if (Random.value < edgeProb) {
                    adjMat[i, j] = true;
                    adjMat[j, i] = true;
                }
            }
        }
    }

    public bool CheckHamiltonianPath(List<int> nodes) {
        if (nodes.Count != N) {
            return false;
        }
        bool[] seen = new bool[N];
        for (int i = 0; i < nodes.Count; i++) {
            if (seen[i]) return false; // Cannot repeat nodes
            seen[i] = true;

            if (i > 0) {
                if (!adjMat[nodes[i - 1], nodes[i]]) {
                    return false; // Not a path
                }
            }
        }
        return true;
    }

    // Checks if a list of nodes is a vertex cover.
    public bool VertexCover(List<int> nodes) {
        bool[] node_set = new bool[N];
        for (int i = 0; i < nodes.Count; i++) {
            if (nodes[i] < 0 || nodes[i] >= N) {
                return false; // Node ID out of bounds
            }
            node_set[nodes[i]] = true;
        }

        for (int i = 0; i < N; i++) {
            for (int j = i + 1; j < N; j++) {
                if (adjMat[i, j] && !node_set[i] && !node_set[j]) {
                    return false;
                }
            }
        }
        return true;
    }

    // Checks if a list of nodes is a clique.
    public bool CheckClique(List<int> nodes) {
        for (int i = 0; i < nodes.Count; i++) {
            for (int j = i + 1; j < nodes.Count; j++) {
                int u = nodes[i];
                int v = nodes[j];
                if (u < 0 || u >= N || v < 0 || v >= N) {
                    return false; // Node ID out of bounds
                }
                // If any two nodes are not connected, not a clique
                if (!adjMat[u, v]) {
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

    public int MinVertexCover() {
        int minSize = N;
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
            if (VertexCover(nodes)) {
                minSize = Mathf.Min(minSize, nodes.Count);
            }
        }
        return minSize;
    }
}

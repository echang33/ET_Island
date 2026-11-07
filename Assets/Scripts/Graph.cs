/*
 * Graph.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Generates undirected puzzle graphs and evaluates cliques, Hamiltonian paths, and vertex covers.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph {
    // Graph data is runtime-only, so these names are not serialized scene or prefab fields.
    // Vertex IDs are zero-based and range from 0 to vertexCount - 1.
    public int vertexCount;
    // Adjacency matrix
    public bool[,] adjacencyMatrix;
    // Size of maximum clique (stored so it doesn't have to be recomputed)
    public int maxClique;
    // Size of minimum vertex cover
    public int minVertexCover;

    public Graph(int nodeCount, float edgeProbability, bool ensureHamiltonianPath) {
        vertexCount = nodeCount;
        // False entries mean no edge; undirected edges are stored symmetrically.
        adjacencyMatrix = new bool[vertexCount, vertexCount];
        GenRandomGraph(edgeProbability);
        if (ensureHamiltonianPath) {
            GenHamiltonianPath();
        }
        maxClique = MaxClique();
        minVertexCover = MinVertexCover();
    }

    // Generates a random Hamiltonian path in the graph.
    public void GenHamiltonianPath() {
        // Generate random permutation for the Hamiltonian path
        int[] p = new int[vertexCount];
        for (int i = 0; i < vertexCount; i++) {
            p[i] = i;
        }
        System.Random rng = new System.Random();
        for (int i = vertexCount - 1; i > 0; i--) {
            int j = rng.Next(i + 1); // Random integer in [0, i]
            (p[i], p[j]) = (p[j], p[i]);
        }

        // Add edges along path to graph
        for (int i = 1; i < vertexCount; i++) {
            adjacencyMatrix[p[i - 1], p[i]] = true;
            adjacencyMatrix[p[i], p[i - 1]] = true;
        }
    }

    // Consider each unordered pair once; adding both directions keeps the matrix symmetric.
    public void GenRandomGraph(float edgeProb) {
        for (int i = 0; i < vertexCount; i++) {
            for (int j = i + 1; j < vertexCount; j++) {
                if (Random.value < edgeProb) {
                    adjacencyMatrix[i, j] = true;
                    adjacencyMatrix[j, i] = true;
                }
            }
        }
    }

    public bool CheckHamiltonianPath(List<int> nodes) {
        if (nodes.Count != vertexCount) {
            return false;
        }
        bool[] seen = new bool[vertexCount];
        for (int i = 0; i < nodes.Count; i++) {
            if (seen[i]) return false; // Existing visited-slot check uses the path position.
            seen[i] = true;

            if (i > 0) {
                if (!adjacencyMatrix[nodes[i - 1], nodes[i]]) {
                    return false; // Not a path
                }
            }
        }
        return true;
    }

    // Checks if a list of nodes is a vertex cover.
    public bool VertexCover(List<int> nodes) {
        bool[] selectedNodes = new bool[vertexCount];
        for (int i = 0; i < nodes.Count; i++) {
            if (nodes[i] < 0 || nodes[i] >= vertexCount) {
                return false; // Node ID out of bounds
            }
            selectedNodes[nodes[i]] = true;
        }

        for (int i = 0; i < vertexCount; i++) {
            for (int j = i + 1; j < vertexCount; j++) {
                if (adjacencyMatrix[i, j] && !selectedNodes[i] && !selectedNodes[j]) {
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
                if (u < 0 || u >= vertexCount || v < 0 || v >= vertexCount) {
                    return false; // Node ID out of bounds
                }
                // If any two nodes are not connected, not a clique
                if (!adjacencyMatrix[u, v]) {
                    return false;
                }
            }
        }
        return true;
    }

    public int MaxClique() {
        int maxSize = 0;
        // Iterate through all subsets of vertexCount nodes, represented by
        // an integer bitmask from [0, 2^vertexCount). Note that this assumes
        // there are less than 31 nodes (otherwise the bitmask overflows).
        for (int mask = 0; mask < (1 << vertexCount); mask++) {
            List<int> nodes = new List<int>();
            for (int i = 0; i < vertexCount; i++) {
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
        int minSize = vertexCount;
        // Iterate through all subsets of vertexCount nodes, represented by
        // an integer bitmask from [0, 2^vertexCount). Note that this assumes
        // there are less than 31 nodes (otherwise the bitmask overflows).
        for (int mask = 0; mask < (1 << vertexCount); mask++) {
            List<int> nodes = new List<int>();
            for (int i = 0; i < vertexCount; i++) {
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

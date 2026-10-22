/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraphUI : MonoBehaviour {
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private GameObject vertexPrefab;
    [SerializeField] private GameObject edgePrefab;

    [SerializeField] private int numNodes = 8;

    // Graph nodes are drawn around an ellipse
    [SerializeField] private float ellipseMajorAxis = 300f;
    [SerializeField] private float ellipseMinorAxis = 200f;

    // Width of displayed edge
    [SerializeField] private float edgeWidth = 6f;
    // Probability of an edge being present (for random graph generation)
    [SerializeField] private float edgeDensity = 0.7f;

    [SerializeField] private RectTransform edgeContainer;
    [SerializeField] private RectTransform vertexContainer;

    // Answer box for user to type in their answer
    [SerializeField] private TMP_InputField answerBox;

    // graph stores the graph data structure
    private Graph graph;
    // vertices and edges store the rendered UI GameObjects
    private List<GameObject> vertices, edges;

    // The type of puzzle (0: max clique, 1: hamiltonian path, 2: min vertex cover)
    private int puzzleType;
    private string[] questions = {
        "Find a maximum clique in the graph. List the vertex numbers of the clique (comma separated). If there are multiple maximum cliques, you may enter any.",
        "Find a Hamiltonian path in the graph. List the vertex numbers of the path in order (comma separated). If there are multiple Hamiltonian paths, you may enter any.",
        "Find a minimum vertex cover in the graph. List the vertex numbers of the vertex cover (comma separated). If there are minimum vertex covers, you may enter any."
    };

    // Store a reference to the PuzzleActivator object that activated
    // this puzzle.
    private PuzzleActivator activator;

    void Start() {
        // Register function to be called when answer is submitted
        answerBox.onSubmit.AddListener(CheckAnswer);

        vertices = new List<GameObject>();
        edges = new List<GameObject>();

        System.Random rng = new System.Random();
        puzzleType = rng.Next(3); // Generate puzzle type randomly between [0, 2]
        questionText.text = questions[puzzleType]; // Set question text based on puzzle type
        graph = new Graph(numNodes, edgeDensity, puzzleType == 1);
        DrawGraph();
    }

    public void Initialize(PuzzleActivator _activator) {
        activator = _activator;
    }

    private void DrawGraph() {
        // Draw vertices
        for (int i = 0; i < graph.N; i++) {
            CreateVertex(i);
        }
        // Draw edges
        for (int i = 0; i < graph.N; i++) {
            for (int j = i + 1; j < graph.N; j++) {
                if (graph.adjMat[i, j] == true) {
                    CreateEdge(i, j);
                }
            }
        }  
    }

    private void CreateVertex(int id) {
        GameObject vertex = Instantiate(vertexPrefab, vertexContainer);
        RectTransform rect = vertex.GetComponent<RectTransform>();

        // Evenly distribute vertices around the ellipse
        float angle = 2f * Mathf.PI * id / graph.N;

        float x = ellipseMajorAxis * Mathf.Cos(angle);
        float y = ellipseMinorAxis * Mathf.Sin(angle);

        rect.anchoredPosition = new Vector2(x, y);

        // Set the vertex label to contain integer ID
        VertexUI vertexUI = vertex.GetComponent<VertexUI>();
        vertexUI.SetLabel(id);

        // Store created vertex
        vertices.Add(vertex);
    }

    private void CreateEdge(int from, int to) {
        // Get the screen positions of the two vertices the edge connects
        RectTransform fromVertex = vertices[from].GetComponent<RectTransform>();
        RectTransform toVertex = vertices[to].GetComponent<RectTransform>();
        Vector2 fromPos = fromVertex.anchoredPosition;
        Vector2 toPos = toVertex.anchoredPosition;

        // Instantiate edge object based on prefab
        GameObject edge = Instantiate(edgePrefab, edgeContainer);
        RectTransform rect = edge.GetComponent<RectTransform>();
        Vector2 direction = toPos - fromPos;

        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotate and scale edge (which is just a square image) so that it is
        // drawn between two vertices
        rect.anchoredPosition = (fromPos + toPos) / 2f;
        rect.sizeDelta = new Vector2(length, edgeWidth);
        rect.rotation = Quaternion.Euler(0f, 0f, angle);

        // Store created edge
        edges.Add(edge);
    }

    private void CheckAnswer(string input) {
        // Parse user's answer
        string[] parts = input.Split(',');
        List<int> nodes = new List<int>();
        foreach (string part in parts) {
            if (!int.TryParse(part.Trim(), out int node)) {
                Debug.Log("Invalid answer. Please enter comma separated integers.");
                return;
            }

            // Make sure the vertex ID is valid
            if (node < 0 || node >= graph.N) {
                Debug.Log("Invalid vertex ID: " + node);
                return;
            }
            nodes.Add(node);
        }

        Debug.Log(string.Join(", ", nodes));

        if (puzzleType == 0) {
            // Check whether the submitted vertices form a max clique
            if (!graph.CheckClique(nodes)) {
                Debug.Log("Incorrect. The vertices do not form a clique.");
                activator.FailPuzzle();
            } else if (nodes.Count < graph.maxClique) {
                Debug.Log("Incorrect. There is a larger clique.");
                activator.FailPuzzle();
            } else {
                Debug.Log("Correct.");
                activator.ClosePuzzle();
            }
        } else if (puzzleType == 1) {
            // Check whether the submitted vertices form a Hamiltonian path
            if (!graph.CheckHamiltonianPath(nodes)) {
                Debug.Log("Incorrect. The vertices do not correspond to a Hamiltonian path.");
                activator.FailPuzzle();
            } else {
                Debug.Log("Correct.");
                activator.ClosePuzzle();
            }
        } else {
            // Check whether the submitted vertices form a min vertex cover
            if (!graph.VertexCover(nodes)) {
                Debug.Log("Incorrect. The vertices do not form a vertex cover.");
                activator.FailPuzzle();
            } else if (nodes.Count > graph.minVertexCover) {
                Debug.Log("Incorrect. There is a smaller vertex cover.");
                activator.FailPuzzle();
            } else {
                Debug.Log("Correct.");
                activator.ClosePuzzle();
            }
        }
    }
}

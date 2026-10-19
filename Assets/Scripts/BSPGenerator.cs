using System;
using System.Collections.Generic;
using UnityEngine;

public class BSPGenerator : MonoBehaviour {
    [Header("Bounds")]
    public Vector3 boundsMin = Vector3.zero;
    public Vector3 boundsMax = new Vector3(100f, 50f, 100f);

    [Header("Minimum Region Dimensions")]
    [Min(0.01f)] public float minWidth = 10f;
    [Min(0.01f)] public float minHeight = 5f;
    [Min(0.01f)] public float minDepth = 10f;

    [Header("Adjacency")]
    [Min(0f)] public float adjacencyEpsilon = 0.001f;
    [Min(0f)] public float minFaceOverlap = 3f;

    [Header("Walls and Openings")]
    [Min(0f)] public float shrinkageBorder = 0;
    [Min(0.01f)] public float wallThickness = 0.2f;
    public Vector2 openingSize = new Vector2(2f, 2f);
    public Material wallMaterial;

    [Header("Randomness")]
    public int seed = 42;
    public float removePercent = 0.4f;

    [Serializable]
    public struct Region {
        public Vector3 Min;
        public Vector3 Max;

        public Region(Vector3 min, Vector3 max) {
            Min = min;
            Max = max;
        }

        public Vector3 Size => Max - Min;
        public Vector3 Center => (Min + Max) * 0.5f;
    }

    [Serializable]
    public struct Edge {
        public int RegionA;
        public int RegionB;

        // 0 = X-normal face, 1 = Y-normal face, 2 = Z-normal face.
        public int Axis;
        public float PlaneCoordinate;

        // Only the two coordinates lying in the face are relevant.
        public Vector3 OverlapMin;
        public Vector3 OverlapMax;
        public Vector3 OpeningMin;
        public Vector3 OpeningMax;

        public Edge(int a, int b, int axis, float plane, Vector3 overlapMin, 
                Vector3 overlapMax, Vector3 openingMin, Vector3 openingMax) {
            RegionA = a;
            RegionB = b;
            Axis = axis;
            PlaneCoordinate = plane;
            OverlapMin = overlapMin;
            OverlapMax = overlapMax;
            OpeningMin = openingMin;
            OpeningMax = openingMax;
        }
    }

    private enum SplitAxis { X, Y, Z }

    private int N;
    private List<Region> leafRegions = new List<Region>();
    private List<Edge> edges = new List<Edge>();
    // adjList[i] stores a list of (j, e), indicating edges[e] goes from i -> j
    private List<List<(int, int)>> adjList = new List<List<(int, int)>>();
    // Stores whether each region was removed during removal phase
    private List<bool> removed;

    private System.Random rng;
    private Transform wallRoot;

    private void Awake() {
        Generate();
    }

    public void Generate() {
        rng = new System.Random(seed);
        SplitRecursively(new Region(boundsMin, boundsMax));
        N = leafRegions.Count;

        removed = new List<bool>();
        for (int i = 0; i < N; i++) {
            removed.Add(false);
        }
        BuildAdjacencyGraph();
        RemoveRegions();
        GenerateWalls();
    }

    private void DFS(int cur, bool[] vis) {
        if (vis[cur] || removed[cur]) {
            return;
        }
        vis[cur] = true;
        for (int i = 0; i < adjList[cur].Count; i++) {
            int to = adjList[cur][i].Item1;
            DFS(to, vis);
        }
    }

    private bool IsConnected() {
        bool[] vis = new bool[N];
        for (int i = 0; i < N; i++) {
            if (!vis[i]) {
                DFS(i, vis);
                break;
            }
        }
        for (int i = 0; i < N; i++) {
            if (!removed[i] && !vis[i]) {
                return false;
            }
        }
        return true;
    }

    private void RemoveRegions() {
        // Generate random permutation for order to attempt removals
        int[] p = new int[N];
        for (int i = 0; i < N; i++) {
            p[i] = i;
        }
        for (int i = N - 1; i > 0; i--) {
            int j = rng.Next(i + 1); // Random integer in [0, i]
            (p[i], p[j]) = (p[j], p[i]);
        }

        int removedCnt = 0;
        for (int i = 0; i < N; i++) {
            if (removedCnt > removePercent * N) {
                break;
            }
            // Attempt removal, then check if it disconnects graph
            removed[p[i]] = true;
            if (!IsConnected()) {
                // Don't remove if it disconnects
                removed[p[i]] = false;
            } else {
                removedCnt++;
            }
        }
        print(removedCnt);
        CompressGraph();
    }

    private void CompressGraph() {
        // Map old vertex IDs to new vertex IDs. -1 means the vertex was removed.
        int[] vertexMap = new int[N];
        int newN = 0;
        for (int i = 0; i < N; i++) {
            vertexMap[i] = (removed[i] ? -1 : newN++);
        }

        // Map old edge IDs to new edge IDs. -1 means the edge was removed.
        int[] edgeMap = new int[edges.Count];
        int newEdgeCount = 0;
        for (int i = 0; i < edges.Count; i++) {
            Edge e = edges[i];
            if (vertexMap[e.RegionA] == -1 || vertexMap[e.RegionB] == -1) {
                edgeMap[i] = -1;
            } else {
                edgeMap[i] = newEdgeCount++;
            }
        }

        // Rebuild regions using the new vertex IDs.
        List<Region> newRegions = new List<Region>(newN);
        for (int i = 0; i < N; i++) {
            if (vertexMap[i] != -1) {
                newRegions.Add(leafRegions[i]);
            }
        }

        // Rebuild edges, updating endpoint IDs.
        List<Edge> newEdges = new List<Edge>(newEdgeCount);
        for (int i = 0; i < edges.Count; i++) {
            if (edgeMap[i] == -1) {
                continue;
            }
            Edge e = edges[i];
            e.RegionA = vertexMap[e.RegionA];
            e.RegionB = vertexMap[e.RegionB];
            newEdges.Add(e);
        }

        // Rebuild adjacency lists, updating both vertex and edge IDs.
        List<List<(int, int)>> newAdjList = new List<List<(int, int)>>(newN);
        for (int i = 0; i < newN; i++) {
            newAdjList.Add(new List<(int, int)>());
        }
        for (int oldU = 0; oldU < N; oldU++) {
            int newU = vertexMap[oldU];
            if (newU == -1) {
                continue;
            }
            foreach ((int oldV, int oldE) in adjList[oldU]) {
                int newV = vertexMap[oldV];
                int newE = edgeMap[oldE];
                if (newV != -1 && newE != -1) {
                    newAdjList[newU].Add((newV, newE));
                }
            }
        }

        // Replace the old graph.
        N = newN;
        leafRegions = newRegions;
        edges = newEdges;
        adjList = newAdjList;

        // All remaining vertices are now retained.
        removed = new List<bool>(N);
        for (int i = 0; i < N; i++) {
            removed.Add(false);
        }
    }

    // BSP generation
    private void SplitRecursively(Region region) {
        List<SplitAxis> available = new List<SplitAxis>();
        Vector3 size = region.Size;

        if (size.x >= 2f * minWidth) available.Add(SplitAxis.X);
        if (size.y >= 2f * minHeight) available.Add(SplitAxis.Y);
        if (size.z >= 2f * minDepth) available.Add(SplitAxis.Z);

        if (available.Count == 0) {
            leafRegions.Add(region);
            return;
        }

        Split(region, available[rng.Next(available.Count)],
            out Region first, out Region second);

        SplitRecursively(first);
        SplitRecursively(second);
    }

    private void Split(Region region, SplitAxis axis,
        out Region first, out Region second) {
        int i = (int)axis;
        float minLength = i == 0 ? minWidth : i == 1 ? minHeight : minDepth;

        Vector3 min = region.Min;
        Vector3 max = region.Max;
        float splitMin = min[i] + minLength;
        float splitMax = max[i] - minLength;
        float splitPosition = splitMin + (float) rng.NextDouble() * (splitMax - splitMin);

        Vector3 firstMax = max;
        Vector3 secondMin = min;
        firstMax[i] = splitPosition;
        secondMin[i] = splitPosition;

        first = new Region(min, firstMax);
        second = new Region(secondMin, max);
    }

    // Returns the two axes lying in a face, in ascending order.
    private static void GetFaceAxes(int axis, out int u, out int v) {
        u = axis == 0 ? 1 : 0;
        v = axis == 2 ? 1 : 2;
    }

    private static float MinOn(Region r, int axis) => r.Min[axis];
    private static float MaxOn(Region r, int axis) => r.Max[axis];

    private static bool ApproximatelyEqual(float a, float b, float epsilon) {
        return Mathf.Abs(a - b) <= epsilon;
    }

    private void BuildAdjacencyGraph() {
        for (int i = 0; i < N; i++) {
            adjList.Add(new List<(int, int)>());
        }
        for (int i = 0; i < leafRegions.Count; i++) {
            for (int j = i + 1; j < leafRegions.Count; j++) {
                if (TryCreateEdge(i, j, out Edge edge)) {
                    adjList[i].Add((j, edges.Count));
                    adjList[j].Add((i, edges.Count));
                    edges.Add(edge);
                }
            }
        }
    }

    // Tries to create an edge between regions a and b. An edge can be created if the two regions
    // share a face and the overlapping region on that face is larger than a fixed minimum.
    private bool TryCreateEdge(int indexA, int indexB, out Edge edge) {
        Region a = leafRegions[indexA];
        Region b = leafRegions[indexB];
        edge = default;

        for (int axis = 0; axis < 3; axis++) {
            // To share a face, the max of one region must match with the min of the other region
            // on some axis.
            bool aBeforeB = ApproximatelyEqual(a.Max[axis], b.Min[axis], adjacencyEpsilon);
            bool bBeforeA = ApproximatelyEqual(b.Max[axis], a.Min[axis], adjacencyEpsilon);

            if (!aBeforeB && !bBeforeA) continue;

            float plane = aBeforeB ? (a.Max[axis] + b.Min[axis]) * 0.5f : (b.Max[axis] + a.Min[axis]) * 0.5f;

            GetFaceAxes(axis, out int u, out int v);

            // Need to account for shrinkage border and wall thickness to get interior face regions.
            // Note that Region min/max are BSP cutting planes.    
            float uMin = Mathf.Max(a.Min[u], b.Min[u]) + shrinkageBorder + wallThickness;
            float uMax = Mathf.Min(a.Max[u], b.Max[u]) - shrinkageBorder - wallThickness;
            float vMin = Mathf.Max(a.Min[v], b.Min[v]) + shrinkageBorder + wallThickness;
            float vMax = Mathf.Min(a.Max[v], b.Max[v]) - shrinkageBorder - wallThickness;

            if (uMax - uMin < minFaceOverlap || vMax - vMin < minFaceOverlap) {
                continue;
            }

            Vector3 overlapMin = Vector3.zero;
            Vector3 overlapMax = Vector3.zero;
            overlapMin[u] = uMin;
            overlapMin[v] = vMin;
            overlapMax[u] = uMax;
            overlapMax[v] = vMax;

            // Clamp requested opening dimensions to the overlap.
            float holeU = Mathf.Min(Mathf.Max(0f, openingSize.x), uMax - uMin);
            float holeV = Mathf.Min(Mathf.Max(0f, openingSize.y), vMax - vMin);

            Vector3 openingMin = Vector3.zero;
            Vector3 openingMax = Vector3.zero;
            if (axis == 0) {
                // If face is a wall (and not a ceiling / floor), align opening to be as low as possible.
                openingMin[u] = uMin;
                openingMin[v] = (vMin + vMax - holeV) * 0.5f;
            } else if (axis == 2) {
                // If face is a wall (and not a ceiling / floor), align opening to be as low as possible.
                openingMin[u] = (uMin + uMax - holeU) * 0.5f;
                openingMin[v] = uMin + shrinkageBorder + wallThickness * 0.5f;
            } else {
                openingMin[u] = (uMin + uMax - holeU) * 0.5f;
                openingMin[v] = (vMin + vMax - holeV) * 0.5f;
            }
            openingMax[u] = openingMin[u] + holeU;
            openingMax[v] = openingMin[v] + holeV;

            edge = new Edge(indexA, indexB, axis, plane, overlapMin, overlapMax, openingMin, openingMax);
            return true;
        }

        return false;
    }

    // Wall GameObjects
    private void GenerateWalls() {
        GameObject root = new GameObject("Generated Walls");
        wallRoot = root.transform;
        wallRoot.SetParent(transform, false);

        for (int i = 0; i < leafRegions.Count; i++) {
            Region region = leafRegions[i];
            // The min and max for each region represents the exterior volume for the region (including wall thickness).
            Vector3 min = region.Min + Vector3.one * shrinkageBorder;
            Vector3 max = region.Max - Vector3.one * shrinkageBorder;
            if (min.x >= max.x || min.y >= max.y || min.z >= max.z) {
                continue;
            }

            for (int axis = 0; axis < 3; axis++) {
                DrawRoomFace(i, axis, min[axis] + wallThickness * 0.5f, min, max);
                DrawRoomFace(i, axis, max[axis] - wallThickness * 0.5f, min, max);
            }
        }
    }

    private void DrawRoomFace(int regionIndex, int axis, float plane, Vector3 roomMin, Vector3 roomMax) {
        GetFaceAxes(axis, out int u, out int v);

        float uMin = roomMin[u], uMax = roomMax[u];
        float vMin = roomMin[v], vMax = roomMax[v];

        Region region = leafRegions[regionIndex];

        // Find all edges that connect to this face of this region.
        List<Edge> faceEdges = new List<Edge>();
        for (int i = 0; i < adjList[regionIndex].Count; i++) {
            Edge edge = edges[adjList[regionIndex][i].Item2];
            if (edge.Axis != axis) continue;

            // The plane is drawn at an inset of shrinkageBorder and half of the wall thickness, whereas
            // the edge plane is the original divider in the BSP. So the difference needs to account for these.
            float diff = shrinkageBorder + wallThickness * 0.5f + adjacencyEpsilon;
            if (ApproximatelyEqual(edge.PlaneCoordinate, plane, diff)) {
                faceEdges.Add(edge);
            }
        }

        // No neighboring room on this face: draw a solid wall.
        if (faceEdges.Count == 0) {
            CreateWallPanel(axis, plane, uMin, uMax, vMin, vMax);
            return;
        }

        // Clip all openings to this room's actual wall face.
        List<(float uMin, float uMax, float vMin, float vMax)> holes = new List<(float, float, float, float)>();
        foreach (Edge edge in faceEdges) {
            float hu0 = Mathf.Max(uMin, edge.OpeningMin[u]);
            float hu1 = Mathf.Min(uMax, edge.OpeningMax[u]);
            float hv0 = Mathf.Max(vMin, edge.OpeningMin[v]);
            float hv1 = Mathf.Min(vMax, edge.OpeningMax[v]);
            holes.Add((hu0, hu1, hv0, hv1));
        }

        // Partition the face at every opening's U boundaries.
        List<float> uCuts = new List<float> { uMin, uMax };
        foreach (var hole in holes) {
            uCuts.Add(hole.uMin);
            uCuts.Add(hole.uMax);
        }
        uCuts.Sort();

        for (int i = 0; i < uCuts.Count - 1; i++) {
            float left = uCuts[i];
            float right = uCuts[i + 1];
            if (right - left <= adjacencyEpsilon) continue;

            float midU = (left + right) * 0.5f;

            // Find every opening crossing this strip.
            List<(float min, float max)> intervals = new List<(float, float)>();
            foreach (var hole in holes) {
                if (midU > hole.uMin && midU < hole.uMax) {
                    intervals.Add((hole.vMin, hole.vMax));
                }
            }

            intervals.Sort((a, b) => a.min.CompareTo(b.min));

            // Merge overlapping openings and draw the solid gaps.
            float cursor = vMin;

            foreach (var interval in intervals) {
                float holeStart = Mathf.Clamp(interval.min, vMin, vMax);
                float holeEnd = Mathf.Clamp(interval.max, vMin, vMax);

                if (holeStart > cursor) {
                    CreateWallPanel(axis, plane, left, right, cursor, holeStart);
                }
                cursor = Mathf.Max(cursor, holeEnd);
            }

            if (cursor < vMax) {
                CreateWallPanel(axis, plane, left, right, cursor, vMax);
            }
        }
    }

    private void CreateWallPanel(int axis, float plane, float uMin, float uMax, float vMin, float vMax) {
        if (uMax <= uMin || vMax <= vMin) return;

        GetFaceAxes(axis, out int u, out int v);

        Vector3 center = Vector3.zero;
        Vector3 size = Vector3.zero;

        center[axis] = plane;
        center[u] = (uMin + uMax) * 0.5f;
        center[v] = (vMin + vMax) * 0.5f;

        size[axis] = wallThickness;
        size[u] = uMax - uMin;
        size[v] = vMax - vMin;

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(wallRoot, false);
        wall.transform.localPosition = center;
        wall.transform.localRotation = Quaternion.identity;
        wall.transform.localScale = size;

        if (wallMaterial != null &&
            wall.TryGetComponent<Renderer>(out Renderer renderer)) {
            renderer.sharedMaterial = wallMaterial;
        }
    }
}

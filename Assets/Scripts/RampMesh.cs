using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RampMesh : MonoBehaviour {
    public float width = 2f;
    public float length = 3f;
    public float height = 1.5f;

    public float rotationY = 0f;
    public Material material;

    [ContextMenu("Generate Ramp")]
    public void Generate() {
        Mesh mesh = new Mesh();
        mesh.name = "Ramp";

        // Ramp rises along -Z axis.
        Vector3[] vertices = {
            new Vector3(-width / 2, 0, -length / 2), // 0: near left
            new Vector3(width / 2, 0, -length / 2),  // 1: near right
            new Vector3(-width / 2, 0, length / 2), // 2: far left
            new Vector3(width / 2, 0, length / 2), // 3: far right
            new Vector3(-width / 2, height, length / 2), // 4: far top left
            new Vector3(width / 2, height, length / 2) // 5: far top right
        };

        int[] triangles = {
            // Bottom
            0, 1, 2,
            1, 3, 2,

            // Left side
            0, 2, 4,

            // Right side
            1, 5, 3,

            // Sloped top
            0, 4, 1,
            1, 4, 5,

            // Far end
            2, 3, 4,
            3, 5, 4
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (material != null) {
            renderer.sharedMaterial = material;
        }
        transform.localRotation = Quaternion.Euler(0, rotationY, 0);
    }
}

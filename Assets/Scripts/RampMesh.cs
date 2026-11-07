/*
 * RampMesh.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Builds a wedge-shaped ramp mesh with face-specific vertices, texture coordinates, and configurable rotation.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RampMesh : MonoBehaviour {
    [Header("Ramp Dimensions")]
    [Tooltip("Local X width. The ramp extends along local Z and rises toward positive Z.")]
    public float width = 2f;
    public float length = 3f;
    public float height = 1.5f;
    [Header("Appearance")]
    [Tooltip("World units per texture repeat. Must be nonzero because UV coordinates are divided by it.")]
    public float textureScale = 1f;

    [Tooltip("Local yaw in degrees, applied when Generate is called.")]
    public float rotationY = 0f;
    public Material material;

    public void Generate() {
        Mesh mesh = new Mesh();
        mesh.name = "Ramp";

        // The high edge is at +Z. Separate vertices per face give the wedge sharp normals.
        Vector3[] vertices = {
            // Bottom
            new Vector3(-width / 2, 0, -length / 2), // 0
            new Vector3(width / 2, 0, -length / 2),  // 1
            new Vector3(-width / 2, 0, length / 2),  // 2
            new Vector3(width / 2, 0, length / 2),   // 3

            // Left side
            new Vector3(-width / 2, 0, -length / 2), // 4
            new Vector3(-width / 2, 0, length / 2),  // 5
            new Vector3(-width / 2, height, length / 2), // 6

            // Right side
            new Vector3(width / 2, 0, -length / 2),  // 7
            new Vector3(width / 2, height, length / 2),  // 8
            new Vector3(width / 2, 0, length / 2),  // 9

            // Sloped top
            new Vector3(-width / 2, 0, -length / 2), // 10
            new Vector3(-width / 2, height, length / 2), // 11
            new Vector3(width / 2, 0, -length / 2),  // 12
            new Vector3(width / 2, height, length / 2),  // 13

            // Far end
            new Vector3(-width / 2, 0, length / 2),  // 14
            new Vector3(width / 2, 0, length / 2),   // 15
            new Vector3(-width / 2, height, length / 2), // 16
            new Vector3(width / 2, height, length / 2)   // 17
        };

        int[] triangles = {
            // Bottom
            0, 1, 2,
            1, 3, 2,

            // Left side
            4, 5, 6,

            // Right side
            7, 8, 9,

            // Sloped top
            10, 11, 12,
            12, 11, 13,

            // Far end
            14, 15, 16,
            15, 17, 16
        };

        float slopeLength = Mathf.Sqrt(length * length + height * height);
        Vector2[] uvs = {
            // Bottom: width x length
            new Vector2(0, 0),
            new Vector2(width, 0),
            new Vector2(0, length),
            new Vector2(width, length),

            // Left side: length x height
            new Vector2(0, 0),
            new Vector2(length, 0),
            new Vector2(length, height),

            // Right side: length x height
            new Vector2(0, 0),
            new Vector2(length, height),
            new Vector2(length, 0),

            // Sloped top: width x sloped length
            new Vector2(0, 0),
            new Vector2(0, slopeLength),
            new Vector2(width, 0),
            new Vector2(width, slopeLength),

            // Far end: width x height
            new Vector2(0, 0),
            new Vector2(width, 0),
            new Vector2(0, height),
            new Vector2(width, height)
        };
        for (int i = 0; i < uvs.Length; i++) {
            uvs[i] = uvs[i] / textureScale;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

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

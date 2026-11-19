using UnityEngine;

public class Ladder : MonoBehaviour {    
    [Header("Dimensions")]
    public float height = 4f;
    public float width = 1.0f;
    public float rungSpacing = 0.5f;

    [Header("Appearance")]
    public float railRadius = 0.08f;
    public float rungRadius = 0.06f;
    public Material ladderMaterial;

    [Header("Trigger")]
    public float triggerDepth = 1f;

    [Header("Orientation")]
    public float rotationY = 0f;

    private BoxCollider boxCollider;

    public float Bottom => transform.position.y;
    public float Top => boxCollider.bounds.max.y;
    public float CenterX => transform.position.x;
    public float CenterZ => transform.position.z;

    private void OnTriggerEnter(Collider other) {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null) {
            player.curLadder = this;
        }
    }

    private void OnTriggerExit(Collider other) {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null) {
            player.curLadder = null;
        }
    }

    public void GenerateLadder() {
        GameObject geometry = new GameObject("GeneratedGeometry");
        geometry.transform.SetParent(transform, false);

        // Generate vertical rails.
        CreateCylinder(
            geometry.transform,
            "LeftRail",
            new Vector3(-width / 2f, height / 2f, 0f),
            railRadius * 2f,
            height
        );

        CreateCylinder(
            geometry.transform,
            "RightRail",
            new Vector3(width / 2f, height / 2f, 0f),
            railRadius * 2f,
            height
        );

        // Generate horizontal rungs.
        if (rungSpacing > 0f) {
            int rungCount = Mathf.FloorToInt((height - 0.5f) / rungSpacing);

            for (int i = 0; i <= rungCount; i++) {
                float y = i * rungSpacing;

                CreateCylinder(
                    geometry.transform,
                    $"Rung_{i}",
                    new Vector3(0f, y, 0f),
                    rungRadius * 2f,
                    width,
                    Quaternion.Euler(0f, 0f, 90f)
                );
            }
        }

        // Generate trigger collider.
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = true;
        boxCollider.center = new Vector3(0f, height / 2f, 0f);
        boxCollider.size = new Vector3(
            width + 1f,
            height,
            triggerDepth
        );
        transform.localRotation = Quaternion.Euler(0, rotationY, 0);
    }

    private void CreateCylinder(
        Transform parent,
        string objectName,
        Vector3 localPosition,
        float diameter,
        float length,
        Quaternion rotation = default
    ) {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        cylinder.name = objectName;
        cylinder.transform.SetParent(parent, false);
        cylinder.transform.localPosition = localPosition;
        cylinder.transform.localRotation = rotation;

        // Unity's default cylinder has height 2 and diameter 1.
        cylinder.transform.localScale = new Vector3(
            diameter,
            length / 2f,
            diameter
        );

        // Remove the primitive's automatically generated collider.
        DestroyGenerated(cylinder.GetComponent<Collider>());

        if (ladderMaterial != null) {
            cylinder.GetComponent<MeshRenderer>().sharedMaterial = ladderMaterial;
        }
    }

    private void DestroyGenerated(Object obj) {
        if (obj == null) return;

        if (Application.isPlaying) {
            Destroy(obj);
        } else {
            DestroyImmediate(obj);
        }
    }
}

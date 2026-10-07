using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VertexUI : MonoBehaviour {
    private TMP_Text label;

    void Awake() {
        label = GetComponentInChildren<TMP_Text>();
    }

    public void SetLabel(int id) {
        label.text = id.ToString();
    }
}

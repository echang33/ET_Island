/*
 * PuzzleActivator.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Opens a graph puzzle on player entry, pauses gameplay, and applies success or failure to turrets.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleActivator : MonoBehaviour {
    [Header("Puzzle UI")]
    [SerializeField] private GameObject graphPuzzlePrefab;
    [Header("Affected Turrets")]
    [Tooltip("Turrets enraged by failure. Leave empty to affect all EnemyFSM turrets in the scene.")]
    [SerializeField] private EnemyFSM[] turrets;
    private GameObject activePuzzle;
    private GameObject player;
    private GraphUI puzzleUI;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) {
            return;
        }
        player = other.gameObject;

        // If puzzle is already ongoing, then don't do anything
        if (activePuzzle != null) {
            return;
        }

        // Instantiate puzzle UI
        activePuzzle = Instantiate(graphPuzzlePrefab);
        puzzleUI = activePuzzle.GetComponentInChildren<GraphUI>();
        puzzleUI.Initialize(this);

        // Pause scaled simulation, but keep the UI responsive. PlayerInput is disabled separately.
        Time.timeScale = 0f;
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        playerInput.enabled = false;

        // Unlock cursor so user can type in UI answer box
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePuzzle() {
        if (activePuzzle != null) {
            Destroy(activePuzzle);
            activePuzzle = null;
        }

        // Restore gameplay
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    public void SucceedPuzzle() {
        EnemyFSM[] affectedTurrets = turrets;
        if (affectedTurrets == null || affectedTurrets.Length == 0) {
            affectedTurrets = FindObjectsByType<EnemyFSM>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        // Deactivate all turrets
        foreach (EnemyFSM turret in affectedTurrets) {
            if (turret != null) {
                turret.Deactivate();
            }
        }

        // Remove bullets already in flight so deactivated turrets cannot cause a delayed hit.
        ProjectileMovement[] projectiles = FindObjectsByType<ProjectileMovement>(FindObjectsSortMode.None);
        foreach (ProjectileMovement projectile in projectiles) {
            Destroy(projectile.gameObject);
        }
        ClosePuzzle();
    }

    public void FailPuzzle() {
        EnemyFSM[] affectedTurrets = turrets;
        if (affectedTurrets == null || affectedTurrets.Length == 0) {
            affectedTurrets = FindObjectsByType<EnemyFSM>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        foreach (EnemyFSM turret in affectedTurrets) {
            if (turret != null) {
                turret.ActivateSuperMode();
            }
        }

        ClosePuzzle();
    }
}

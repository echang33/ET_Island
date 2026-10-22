/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleActivator : MonoBehaviour {
    [SerializeField] private GameObject graphPuzzlePrefab;
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

        // Freeze the 3D game play
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

        // Delete all live bullets (gives player immunity)
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

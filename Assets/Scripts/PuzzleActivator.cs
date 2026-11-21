/*
 * PuzzleActivator.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Opens a graph puzzle on player entry, pauses gameplay, and applies success or failure to turrets.
 */

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
    private bool waitingForPlayerExit;
    private Collider[] playerColliders;
    private Collider[] activationColliders;
    private PlayerMovement playerMovement;
    private Life playerLife;
    private float previousTimeScale;
    private bool inputWasEnabled;

    private void OnTriggerEnter(Collider other) {
        PlayerMovement enteringPlayer = other.GetComponentInParent<PlayerMovement>();
        if (enteringPlayer == null || !enteringPlayer.CompareTag("Player")) {
            return;
        }
        // Closing a puzzle does not rearm the trigger. The player must leave
        // completely before entering again, including any extra player colliders.
        if (activePuzzle != null || waitingForPlayerExit) {
            return;
        }
        Life enteringLife = enteringPlayer.GetComponent<Life>();
        PlayerInput playerInput = enteringPlayer.GetComponent<PlayerInput>();
        // Damage callbacks can run before Life.Update processes the death.
        if ((enteringLife != null && enteringLife.IsDead) || !enteringPlayer.isActiveAndEnabled
            || playerInput == null || !playerInput.isActiveAndEnabled || Time.timeScale <= 0f) {
            return;
        }
        player = enteringPlayer.gameObject;
        playerMovement = enteringPlayer;
        playerLife = enteringLife;
        playerColliders = player.GetComponentsInChildren<Collider>(true);
        activationColliders = GetComponentsInChildren<Collider>(true);
        waitingForPlayerExit = true;
        previousTimeScale = Time.timeScale;
        inputWasEnabled = playerInput.enabled;

        // Instantiate puzzle UI
        activePuzzle = Instantiate(graphPuzzlePrefab);
        puzzleUI = activePuzzle.GetComponentInChildren<GraphUI>();
        puzzleUI.Initialize(this);

        // Pause scaled simulation, but keep the UI responsive. PlayerInput is disabled separately.
        Time.timeScale = 0f;
        playerInput.enabled = false;

        // Unlock cursor so user can type in UI answer box
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void FixedUpdate() {
        if (!waitingForPlayerExit || activePuzzle != null || playerMovement == null
            || !playerMovement.isActiveAndEnabled || (playerLife != null && playerLife.IsDead)) {
            return;
        }

        // Kinematic changes during death can produce exit/enter callbacks even
        // without leaving. Rearm only after a live player's body is really outside.
        bool hasActivePlayerCollider = false;
        foreach (Collider body in playerColliders) {
            if (body == null || !body.enabled || !body.gameObject.activeInHierarchy) continue;
            hasActivePlayerCollider = true;
            foreach (Collider trigger in activationColliders) {
                if (trigger == null || !trigger.enabled || !trigger.isTrigger
                    || !trigger.gameObject.activeInHierarchy
                    || trigger.GetComponentInParent<PuzzleActivator>() != this) continue;
                if (body.bounds.Intersects(trigger.bounds)
                    && Physics.ComputePenetration(body, body.transform.position, body.transform.rotation,
                        trigger, trigger.transform.position, trigger.transform.rotation, out _, out _)) {
                    return;
                }
            }
        }
        if (hasActivePlayerCollider) waitingForPlayerExit = false;
    }

    public void CancelForPlayerDeath(Transform deadPlayer) {
        if (player != null && deadPlayer != null && player == deadPlayer.gameObject) {
            ClosePuzzle();
        }
    }

    public void ClosePuzzle() {
        if (activePuzzle == null) return;
        activePuzzle.SetActive(false);
        Destroy(activePuzzle);
        activePuzzle = null;

        // Restore gameplay
        Time.timeScale = previousTimeScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        playerInput.enabled = inputWasEnabled;
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

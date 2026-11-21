
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleActivator : MonoBehaviour {
    [Header("Puzzle UI")]
    [SerializeField] private GameObject graphPuzzlePrefab;

    [Header("Affected Turrets")]
    [Tooltip("Turrets enraged by failure. Leave empty to affect all EnemyFSM turrets in the scene.")]
    [SerializeField] private EnemyFSM[] turrets;

    private enum State {
        Ready,
        Active,
        WaitingForExit
    }

    private State state = State.Ready;

    private GameObject activePuzzle;
    private PlayerMovement playerMovement;
    private Life playerLife;
    private PlayerInput playerInput;

    private Collider[] playerColliders;
    private Collider[] activationColliders;

    private float previousTimeScale;
    private bool inputWasEnabled;

    // After puzzle is solved, it cannot be activated again.
    public bool solved;

    // Reference to BSPGenerator and the region this activator belongs to.
    private BSPGenerator bspGen;
    private int region;

    public void InitBSP(BSPGenerator gen, int regionInd) {
        bspGen = gen;
        region = regionInd;
    }

    public bool IsReady() {
        return state == State.Ready;
    }

    private void OnTriggerEnter(Collider other) {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null && state == State.Ready && !solved) {
            player.curPuzzle = this;
        }
    }

    private void OnTriggerExit(Collider other) {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null && player.curPuzzle == this) {
            player.curPuzzle = null;
        }
    }

    public void ActivatePuzzle(PlayerMovement player) {
        if (solved || state != State.Ready) {
            return;
        }

        PlayerLifeSetup(player);

        // Instantiate puzzle UI.
        activePuzzle = Instantiate(graphPuzzlePrefab);
        GraphUI puzzleUI = activePuzzle.GetComponentInChildren<GraphUI>();
        puzzleUI.Initialize(this);

        // Pause gameplay and disable player input.
        Time.timeScale = 0f;
        playerInput.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        state = State.Active;
    }

    private void PlayerLifeSetup(PlayerMovement player) {
        playerMovement = player;
        playerLife = player.GetComponent<Life>();
        playerInput = player.GetComponent<PlayerInput>();

        // Damage callbacks can run before Life.Update processes death.
        if ((playerLife != null && playerLife.IsDead)
            || !player.isActiveAndEnabled
            || playerInput == null || !playerInput.isActiveAndEnabled
            || Time.timeScale <= 0f) {
            playerMovement = null;
            return;
        }

        playerColliders = player.GetComponentsInChildren<Collider>(true);
        activationColliders = GetComponentsInChildren<Collider>(true);

        previousTimeScale = Time.timeScale;
        inputWasEnabled = playerInput.enabled;
    }

    private void FixedUpdate() {
        if (state != State.WaitingForExit)
            return;

        if (playerMovement == null) {
            state = State.Ready;
            return;
        }

        if (IsPlayerInsideTrigger())
            return;

        state = State.Ready;
    }

    private bool IsPlayerInsideTrigger() {
        bool hasActiveCollider = false;

        foreach (Collider body in playerColliders) {
            if (body == null || !body.enabled || !body.gameObject.activeInHierarchy)
                continue;

            hasActiveCollider = true;

            foreach (Collider trigger in activationColliders) {
                if (trigger == null || !trigger.enabled || !trigger.isTrigger
                    || !trigger.gameObject.activeInHierarchy
                    || trigger.GetComponentInParent<PuzzleActivator>() != this)
                    continue;

                if (body.bounds.Intersects(trigger.bounds)
                    && Physics.ComputePenetration(
                        body, body.transform.position, body.transform.rotation,
                        trigger, trigger.transform.position, trigger.transform.rotation,
                        out _, out _)) {
                    return true;
                }
            }
        }

        // No active player colliders means the player is no longer occupying the trigger.
        return false;
    }

    public void CancelForPlayerDeath(Transform deadPlayer) {
        if (state == State.Active
            && playerMovement != null
            && deadPlayer == playerMovement.transform) {
            ClosePuzzle();
        }
    }

    public void ClosePuzzle() {
        if (state != State.Active)
            return;

        Destroy(activePuzzle);
        activePuzzle = null;

        // Restore gameplay.
        Time.timeScale = previousTimeScale;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerInput.enabled = inputWasEnabled;
        state = State.WaitingForExit;
    }

    private EnemyFSM[] GetAffectedTurrets() {
        if (turrets != null && turrets.Length > 0)
            return turrets;

        return FindObjectsByType<EnemyFSM>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
    }

    public void SucceedPuzzle() {
        if (bspGen != null)
            bspGen.ClearRoom(region);

        // Deactivate all affected turrets.
        foreach (EnemyFSM turret in GetAffectedTurrets()) {
            if (turret != null)
                turret.Deactivate();
        }

        // Remove projectiles already in flight.
        ProjectileMovement[] projectiles =
            FindObjectsByType<ProjectileMovement>(FindObjectsSortMode.None);

        foreach (ProjectileMovement projectile in projectiles) {
            if (projectile != null)
                Destroy(projectile.gameObject);
        }

        solved = true;
        ClosePuzzle();
    }

    public void FailPuzzle() {
        foreach (EnemyFSM turret in GetAffectedTurrets()) {
            if (turret != null)
                turret.ActivateSuperMode();
        }

        ClosePuzzle();
    }
}

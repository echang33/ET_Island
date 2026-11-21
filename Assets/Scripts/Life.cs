
/*
 * Life.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Tracks health, triggers death effects,
 * hides player on death, and allows respawning.
 */

using UnityEngine;
using UnityEngine.Events;

public class Life : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Remaining health. Reaching zero triggers death.")]
    public float amount;

    [Header("Death Events and Effects")]
    public UnityEvent onDeath;

    [Tooltip("Particle effect spawned independently on death.")]
    public GameObject deathEffectPrefab;

    public AudioSource deathEffect;

    [Header("Player Visibility")]
    [Tooltip("Child GameObject containing the visible player model.")]
    public GameObject playerModel;

    private bool isDead = false;
    public bool IsDead => isDead || amount <= 0f;

    void Update()
    {
        if (!isDead && amount <= 0)
        {
            // Prevent death from triggering repeatedly
            isDead = true;

            // Spawn independent death particles
            if (deathEffectPrefab != null)
            {
                Instantiate(
                    deathEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );
            }

            // Play death sound independently
            if (deathEffect != null && deathEffect.clip != null)
            {
                GameObject soundObject = new GameObject("Death Sound");
                soundObject.transform.position = transform.position;
                AudioSource sound = soundObject.AddComponent<AudioSource>();
                sound.playOnAwake = false;
                sound.clip = deathEffect.clip;
                sound.outputAudioMixerGroup = deathEffect.outputAudioMixerGroup;
                sound.volume = deathEffect.volume;
                sound.spatialBlend = 1f;
                sound.Play();
                Destroy(soundObject, sound.clip.length);
            }

            // Hide player model
            if (playerModel != null)
            {
                playerModel.SetActive(false);
            }

            // Notify GameManager
            onDeath?.Invoke();

            // Do not destroy the player
        }
    }

    // Called by GameManager when respawning
    public void Revive(float restoredHealth)
    {
        // Restore health
        amount = restoredHealth;

        // Allow future deaths
        isDead = false;

        // Make player visible again
        if (playerModel != null)
        {
            playerModel.SetActive(true);
        }
    }
}

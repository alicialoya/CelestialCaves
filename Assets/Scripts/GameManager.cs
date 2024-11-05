using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public string spawnTo;

    public Transform playerSpawnPoint;

    void Start() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this);
            SceneManager.sceneLoaded  += OnSceneLoad;
        } else {
            Destroy(gameObject);
        }
    }

// Reloads scene when player diess
    void OnSceneLoad(Scene scene, LoadSceneMode mode) {
    // Reset player position
        if (!string.IsNullOrEmpty(spawnTo)) {
            // Locate the specified spawn point
            SpawnPoint target = Object.FindObjectsOfType<SpawnPoint>()
                .FirstOrDefault(x => x.spawnId == spawnTo);
            if (target != null) {
                PlayerController.instance.transform.position = target.transform.position;
            } else {
                Debug.LogWarning($"SpawnPoint with spawnId '{spawnTo}' not found in the scene.");
            }
            spawnTo = null;  // Clear spawnTo after use
        }
        // Attach camera to player
        foreach (CinemachineVirtualCamera cam in Object.FindObjectsOfType<CinemachineVirtualCamera>()) {
            cam.Follow = PlayerController.instance.transform;
        }

        // Reset player's power-ups for the new level
        if (PlayerController.instance != null) {
            PlayerController.instance.ResetPowerUps();
        }

        // Reset health when a new level is loaded
        PlayerController.instance.ResetHealth();

        RespawnPlayer();

    }

// Restart the level (called when player dies)
    public void RestartLevel() {
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);

        // Reset player health to full
        PlayerController.instance.ResetHealth();
        PlayerController.instance.ResetPowerUps();
    }

public void RespawnPlayer() {
    // Check if the PlayerController instance exists
    if (PlayerController.instance == null) {
        Debug.LogWarning("PlayerController instance is missing.");
        return; 
    }

    // Find the spawn point
     GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");

    if (spawnPoint != null) {
        PlayerController.instance.transform.position = spawnPoint.transform.position;
        PlayerController.instance.ResetHealth(); // Reset health after respawn
    } else {
        Debug.LogWarning("SpawnPoint not found in the scene.");
    }
}

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }
}

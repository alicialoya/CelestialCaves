using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour {
    
    public AudioClip clip;
// Handle power up collider, set super jump to true
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            other.GetComponent<PlayerController>().superJumpUnlocked = true;
            AudioSource.PlayClipAtPoint(clip, transform.position);
            Destroy(gameObject);
        }
    }
}

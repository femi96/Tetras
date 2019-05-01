using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudio : MonoBehaviour {

  [Header("Audio")]
  public AudioSource bgMusic;

  void Start() {
    bgMusic.volume = 0.1f;
    bgMusic.Play();
  }

  void Update() {
    if (Input.GetKeyDown(KeyCode.Minus)) {
      bgMusic.volume -= bgMusic.volume * 0.1f;
    }

    if (Input.GetKeyDown(KeyCode.Equals)) {
      bgMusic.volume += bgMusic.volume * 0.1111f;
    }
  }
}

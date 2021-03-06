﻿using System.Collections;
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
      VolumeDown();
    }

    if (Input.GetKeyDown(KeyCode.Equals)) {
      VolumeUp();
    }

    if (Input.GetKeyDown(KeyCode.M)) {
      ToggleMute();
    }
  }

  public void VolumeDown() {
    bgMusic.volume -= bgMusic.volume * 0.1f;
  }
  public void VolumeUp() {
    bgMusic.volume += bgMusic.volume * 0.111111f;
  }
  public void ToggleMute() {
    bgMusic.mute = !bgMusic.mute;
  }
}

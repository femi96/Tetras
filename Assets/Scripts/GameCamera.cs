using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour {

  public Game game;

  private Vector3 targetPos;

  private float x = 0.0f; // Current camera angles
  private float y = 30.0f;

  private float distance = 7f;  // Current distance from wand

  private float xSpeed = 9.0f;    // Angular change rate
  private float ySpeed = 12.0f;
  private float yMinLimit = 5f;   // Angle bounds
  private float yMaxLimit = 80f;

  private float distanceSpeed = 2f; // Distance change rate
  private float distanceMin = 3f;     // Distance bounds
  private float distanceMax = 10f;

  void Start() {

    // Camera positioning
    Vector3 angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;

    targetPos = Vector3.up * game.gridHeight / 5f;
    distanceMax = Mathf.Max(Mathf.Max(game.gridSizeX, game.gridSizeZ), game.gridHeight);
    distanceMin = Mathf.Min(Mathf.Min(game.gridSizeX, game.gridSizeZ), game.gridHeight);
    distance = distanceMax;
  }

  void Update() {

    // Lock cursor to screen on input
    if (CanMove()) {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    } else {
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }
  }

  void LateUpdate() {

    // Don't move camera if in menu
    if (!CanMove()) return;

    targetPos = (Vector3.up * game.gridHeight * 0.2f) + (game.currentCoords[0].y * Vector3.up * 0.1f);

    // Update camera position based on mouse movement
    x += Input.GetAxis("Mouse X") * xSpeed;
    y -= Input.GetAxis("Mouse Y") * ySpeed;
    x = (x + 360f) % 360f;
    y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

    Quaternion rotation = Quaternion.Euler(y, x, 0);

    distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * distanceSpeed, distanceMin, distanceMax);

    Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
    Vector3 position = rotation * negDistance;

    transform.rotation = rotation;
    transform.position = position + targetPos;
  }

  public bool CanMove() {
    return game.inGame;
  }
}

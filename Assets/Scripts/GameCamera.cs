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

    // Initialize colors
    colorStart = game.GetNextColor();
    colorEnd = game.GetNextColor();

    // Camera positioning
    Vector3 angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;

    targetPos = Vector3.up * game.gridHeight * 0.3f;
    distanceMax = 1.5f * Mathf.Max(Mathf.Max(game.gridSizeX, game.gridSizeZ), game.gridHeight);
    distanceMin = 0.8f * Mathf.Min(Mathf.Min(game.gridSizeX, game.gridSizeZ), game.gridHeight);
    distance = distanceMax;
  }

  void Update() {

    UpdateColor();

    // Lock cursor to screen on input
    if (CanPlayerMove()) {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    } else {
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }
  }

  private Color colorStart = new Color(1f, 1f, 1f);
  private Color colorEnd = new Color(0f, 0f, 0f);
  private float colorTime = 0f;

  private void UpdateColor() {

    float cRate = game.GetColorRate();
    colorTime += cRate * Time.deltaTime;
    float a = Mathf.Clamp(colorTime, 0.0f, 1.0f);
    float b = 1.0f - a;

    Camera.main.backgroundColor = b * colorStart + a * colorEnd;

    if (colorTime > 1.0f) {
      colorTime -= 1.0f;
      colorStart = colorEnd;
      colorEnd = game.GetNextColor();
    }
  }

  void LateUpdate() {

    // Don't move camera if in menu
    if (CanRotateMove()) {

      x += Time.deltaTime * xSpeed;
      x = (x + 360f) % 360f;
      Quaternion rotation = Quaternion.Euler(y, x, 0);
      Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
      Vector3 position = rotation * negDistance;

      transform.rotation = rotation;
      transform.position = position + targetPos;

    }

    // Update camera position based on mouse movement
    if (CanPlayerMove()) {
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
  }

  public bool CanPlayerMove() {
    return game.inGame;
  }

  public bool CanRotateMove() {
    return game.moveCamInMenu && !game.inGame;
  }
}

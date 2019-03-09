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

    targetPos = Vector3.up * game.gridHeight * 0.3f;
    distanceMax = 1.5f * Mathf.Max(Mathf.Max(game.gridSizeX, game.gridSizeZ), game.gridHeight);
    distanceMin = 0.8f * Mathf.Min(Mathf.Min(game.gridSizeX, game.gridSizeZ), game.gridHeight);
    distance = distanceMax;
  }

  private float cr = 95;
  private float cg = 205;
  private float cb = 205;
  private float cLow = 95;
  private float cHigh = 205;
  private float rgbChange = 0;
  public float cRate = 1f;

  void Update() {

    // Color
    if (rgbChange == 1) {
      cr += cRate * Time.deltaTime;

      if (cr >= cHigh) {
        cr = cHigh;
        rgbChange = 2;
      }
    }

    if (rgbChange == 0) {
      cg -= cRate * Time.deltaTime;

      if (cg <= cLow) {
        cg = cLow;
        rgbChange = 1;
      }
    }

    if (rgbChange == 3) {
      cg += cRate * Time.deltaTime;

      if (cg >= cHigh) {
        cg = cHigh;
        rgbChange = 4;
      }
    }

    if (rgbChange == 2) {
      cb -= cRate * Time.deltaTime;

      if (cb <= cLow) {
        cb = cLow;
        rgbChange = 3;
      }
    }

    if (rgbChange == 5) {
      cb += cRate * Time.deltaTime;

      if (cb >= cHigh) {
        cb = cHigh;
        rgbChange = 0;
      }
    }

    if (rgbChange == 4) {
      cr -= cRate * Time.deltaTime;

      if (cr <= cLow) {
        cr = cLow;
        rgbChange = 5;
      }
    }

    Camera.main.backgroundColor = new Color(cr / 256f, cg / 256f, cb / 256f);

    // Lock cursor to screen on input
    if (CanMove() && !game.moveCamInMenu) {
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
    return game.inGame || game.moveCamInMenu;
  }
}

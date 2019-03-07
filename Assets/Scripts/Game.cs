using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TetraType {
  I,
  O,
  L,
  T,
  N,
  TL,
  TR,
  A,
}

public class Game : MonoBehaviour {

  public int gridSizeX = 4;
  public int gridSizeZ = 4;
  public int gridHeight = 20;
  private Vector3 gridOffset;

  public float fallSpeed = 1;
  public float level = 0;
  private float fallTime = 0;

  public bool inGame = false;

  private int score = 0;
  private int highScore = 0;

  public GameObject cube;
  public Material baseMat;
  public Material[] mats;

  public Vector3Int[] currentCoords;
  private GameObject[] currentBlocks;

  private GameObject[, ,] gridBlocks;

  private List<TetraType> tetraCubeQueue;

  [Header("UI")]
  public GameObject resetButton;
  public Text scoreText;
  public Text highScoreText;

  void Start() {
    highScoreText.text = "" + highScore;
    scoreText.text = "" + score;
  }

  void Update() {
    if (inGame) {
      CheckInput();
      UpdateFall();
    }
  }

  public void StartGame() {
    foreach (Transform child in transform) {
      Destroy(child.gameObject);
    }

    gridBlocks = new GameObject[gridSizeX, gridHeight, gridSizeZ];
    currentCoords = new Vector3Int[4];
    currentBlocks = new GameObject[4];
    gridOffset = -0.5f * new Vector3(gridSizeX, 0, gridSizeZ) + new Vector3(0.5f, 0, 0.5f);
    tetraCubeQueue = new List<TetraType>();


    for (int i = 0; i < gridSizeX; i++) {
      for (int j = 0; j < gridSizeZ; j++) {
        GameObject go = Instantiate(cube, transform);
        go.transform.position = new Vector3(i, -1, j) + gridOffset;
        go.GetComponent<Renderer>().material = baseMat;
      }
    }

    NewTetraCube();
    score = 0;
    inGame = true;

    resetButton.SetActive(false);
  }

  private void EndGame() {
    inGame = false;

    resetButton.SetActive(true);
  }

  private void LockTetraCube() {
    for (int i = 0; i < currentCoords.Length; i++) {
      if (currentBlocks[i] != null) {
        Vector3Int coords = currentCoords[i];
        gridBlocks[coords.x, coords.y, coords.z] = currentBlocks[i];
        currentBlocks[i] = null;
      }
    }
  }

  private void NewTetraCube() {
    if (tetraCubeQueue.Count == 0) {
      // Populate
      tetraCubeQueue = new List<TetraType>() {
        TetraType.I,
        TetraType.O,
        TetraType.L,
        TetraType.T,
        TetraType.N,
        TetraType.TL,
        TetraType.TR,
        TetraType.A,
      };

      for (int i = 0; i < tetraCubeQueue.Count; i++) {
        TetraType temp = tetraCubeQueue[i];
        int randomIndex = UnityEngine.Random.Range(i, tetraCubeQueue.Count);
        tetraCubeQueue[i] = tetraCubeQueue[randomIndex];
        tetraCubeQueue[randomIndex] = temp;
      }
    }

    TetraType type = tetraCubeQueue[0];
    tetraCubeQueue.RemoveAt(0);
    int l = gridHeight - 1;
    Material m = mats[(int)type];
    int cx = gridSizeX / 2;
    int cz = gridSizeZ / 2;

    switch (type) {
    case TetraType.I:
      currentCoords[2] = new Vector3Int(cx - 2, l, cz);
      currentCoords[1] = new Vector3Int(cx - 1, l, cz);
      currentCoords[0] = new Vector3Int(cx + 0, l, cz);
      currentCoords[3] = new Vector3Int(cx + 1, l, cz);
      break;

    case TetraType.O:
      currentCoords[0] = new Vector3Int(cx + 0, l, cz + 0);
      currentCoords[1] = new Vector3Int(cx + 0, l, cz - 1);
      currentCoords[2] = new Vector3Int(cx - 1, l, cz + 0);
      currentCoords[3] = new Vector3Int(cx - 1, l, cz - 1);
      break;

    case TetraType.L:
      currentCoords[1] = new Vector3Int(cx - 1, l, cz + 0);
      currentCoords[0] = new Vector3Int(cx + 0, l, cz + 0);
      currentCoords[2] = new Vector3Int(cx + 1, l, cz + 0);
      currentCoords[3] = new Vector3Int(cx - 1, l, cz - 1);
      break;

    case TetraType.T:
      currentCoords[1] = new Vector3Int(cx - 1, l, cz + 0);
      currentCoords[0] = new Vector3Int(cx + 0, l, cz + 0);
      currentCoords[2] = new Vector3Int(cx + 1, l, cz + 0);
      currentCoords[3] = new Vector3Int(cx + 0, l, cz - 1);
      break;

    case TetraType.N:
      currentCoords[1] = new Vector3Int(cx - 1, l, cz - 1);
      currentCoords[0] = new Vector3Int(cx + 0, l, cz + 0);
      currentCoords[2] = new Vector3Int(cx + 1, l, cz + 0);
      currentCoords[3] = new Vector3Int(cx + 0, l, cz - 1);
      break;

    case TetraType.TL:
      currentCoords[1] = new Vector3Int(cx - 1, l, cz + 0);
      currentCoords[0] = new Vector3Int(cx + 0, l, cz + 0);
      currentCoords[2] = new Vector3Int(cx + 0, l, cz - 1);
      currentCoords[3] = new Vector3Int(cx + 0, l - 1, cz - 1);
      break;

    case TetraType.TR:
      currentCoords[1] = new Vector3Int(cx - 1, l, cz + 0);
      currentCoords[0] = new Vector3Int(cx + 0, l, cz + 0);
      currentCoords[2] = new Vector3Int(cx + 0, l, cz - 1);
      currentCoords[3] = new Vector3Int(cx - 1, l - 1, cz + 0);
      break;

    case TetraType.A:
      currentCoords[1] = new Vector3Int(cx - 1, l, cz + 0);
      currentCoords[0] = new Vector3Int(cx + 0, l, cz + 0);
      currentCoords[2] = new Vector3Int(cx + 0, l, cz - 1);
      currentCoords[3] = new Vector3Int(cx + 0, l - 1, cz + 0);
      break;

    default:
      throw new Exception("Missing Type in NewTetraCube");
    }

    foreach (Vector3Int coords in currentCoords) {
      if (gridBlocks[coords.x, coords.y, coords.z] != null) {
        EndGame();
        return;
      }
    }

    for (int i = 0; i < currentCoords.Length; i++) {
      currentBlocks[i] = Instantiate(cube, transform);
      currentBlocks[i].transform.position = currentCoords[i] + gridOffset;
      currentBlocks[i].GetComponent<Renderer>().material = m;
    }
  }

  private void CheckInput() {
    // TODO MVP: Vary control based oncamera direction
    // TODO MVP: Add preview and slam
    // TODO MVP: Hold piece

    Vector3 camDirection = Camera.main.transform.forward;
    camDirection.y = 0;
    camDirection = camDirection.normalized;

    int fx = 0;
    int fz = 0;

    if (Mathf.Abs(camDirection.x) > Mathf.Abs(camDirection.z))
      fx = 1;
    else
      fz = 1;

    if (camDirection.x < 0)
      fx *= -1;

    if (camDirection.z < 0)
      fz *= -1;

    if (Input.GetKeyDown(KeyCode.D)) {
      MoveBlock(fz, 0, -fx);
    } else if (Input.GetKeyDown(KeyCode.A)) {
      MoveBlock(-fz, 0, fx);
    } else if (Input.GetKeyDown(KeyCode.W)) {
      MoveBlock(fx, 0, fz);
    } else if (Input.GetKeyDown(KeyCode.S)) {
      MoveBlock(-fx, 0, -fz);
    } else if (Input.GetKeyDown(KeyCode.Q)) {
      RotateBlock(fz, 0, -fx);
    } else if (Input.GetKeyDown(KeyCode.E)) {
      RotateBlock(-fx, 0, -fz);
    } else if (Input.GetKeyDown(KeyCode.R)) {
      RotateBlock(0, 1, 0);
    }
  }

  private void UpdateFall() {
    fallTime += Time.deltaTime * fallSpeed;

    if (fallTime > 1.0f) {
      fallTime -= 1.0f;
      bool fall = MoveBlock(0, -1, 0);

      if (!fall) {
        LockTetraCube();
        NewTetraCube();
        ClearLines();
      }
    }
  }

  private void ClearLines() {
    // TODO: Add effects
    // Remove cubes
    List<int> layers = new List<int>();

    for (int l = 0; l < gridHeight; l++) {
      bool hasEmpty = false;

      for (int i = 0; i < gridSizeX; i++) {
        for (int j = 0; j < gridSizeZ; j++) {
          hasEmpty = hasEmpty || gridBlocks[i, l, j] == null;
        }
      }

      if (!hasEmpty) {
        layers.Add(l);

        for (int i = 0; i < gridSizeX; i++) {
          for (int j = 0; j < gridSizeZ; j++) {
            Destroy(gridBlocks[i, l, j]);
            gridBlocks[i, l, j] = null;
          }
        }
      }
    }

    // Lower lines
    foreach (int layer in layers) {
      for (int l = layer; l < gridHeight; l++) {
        for (int i = 0; i < gridSizeX; i++) {
          for (int j = 0; j < gridSizeZ; j++) {
            if (l + 1 < gridHeight) {
              gridBlocks[i, l, j] = gridBlocks[i, l + 1, j];

              if (gridBlocks[i, l, j] != null)
                gridBlocks[i, l, j].transform.position = new Vector3(i, l, j) + gridOffset;
            } else {
              gridBlocks[i, l, j] = null;
            }
          }
        }
      }
    }

    // Score
    // TODO: Add comboing
    if (layers.Count > 0) {
      int bonus = 100;

      for (int l = 1; l < layers.Count; l++) {
        bonus *= 3;
      }

      score += bonus;

      if (highScore < score)
        highScore = score;
    }

    highScoreText.text = "" + highScore;
    scoreText.text = "" + score;
  }

  private bool MoveBlock(int x, int y, int z) {
    // Check new position
    foreach (Vector3Int coords in currentCoords) {
      int nx = coords.x + x;
      int ny = coords.y + y;
      int nz = coords.z + z;
      bool collision = false;
      collision = collision || nx >= gridSizeX || nx < 0;
      collision = collision || ny >= gridHeight || ny < 0;
      collision = collision || nz >= gridSizeZ || nz < 0;
      collision = collision || gridBlocks[nx, ny, nz] != null;

      if (collision)
        return false;
    }

    // Move to new position
    for (int i = 0; i < currentCoords.Length; i++) {
      currentCoords[i] += new Vector3Int(x, y, z);
      currentBlocks[i].transform.position = currentCoords[i] + gridOffset;;
    }

    return true;
  }

  private bool RotateBlock(int x, int y, int z) {

    // Find center
    Vector3Int center = currentCoords[0];

    // Rotate current position to new position
    Vector3Int[] newCoords = new Vector3Int[4];

    for (int i = 0; i < currentCoords.Length; i++) {
      Vector3 rel = currentCoords[i] - center;
      Vector3 newPos = center + Quaternion.Euler(90 * x, 90 * y, 90 * z) * rel;
      newCoords[i] = new Vector3Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y), Mathf.RoundToInt(newPos.z));
    }

    // Check new position for collision
    foreach (Vector3Int coords in newCoords) {
      int nx = coords.x;
      int ny = coords.y;
      int nz = coords.z;
      bool collision = false;
      collision = collision || nx >= gridSizeX || nx < 0;
      collision = collision || ny >= gridHeight || ny < 0;
      collision = collision || nz >= gridSizeZ || nz < 0;
      collision = collision || gridBlocks[nx, ny, nz] != null;

      if (collision)
        return false;
    }

    // Move to new position
    for (int i = 0; i < currentCoords.Length; i++) {
      currentCoords[i] = newCoords[i];
      currentBlocks[i].transform.position = currentCoords[i] + gridOffset;
    }

    return true;
  }
}

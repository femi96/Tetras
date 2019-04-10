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
  None,
}

public class Game : MonoBehaviour {

  [Header("Game Options")]
  public int gridSizeX = 4;
  public int gridSizeZ = 4;
  public int gridHeight = 20;
  private Vector3 gridOffset;


  [Header("Progression")]
  private int level = 0;
  private int exp = 0;
  private float fallTime = 0;
  private int combo = 0;

  private int score = 0;
  private int highScore = 0;


  [Header("References")]
  public bool inGame = false;

  public GameObject cube;
  public GameObject previewCube;
  public Material baseMat;
  public Material[] mats;

  public Vector3Int[] currentCoords;
  private GameObject[] currentBlocks;

  public Vector3Int[] previewCoords;
  private GameObject[] previewBlocks;

  private GameObject[, ,] gridBlocks;

  private List<TetraType> tetraCubeQueue;
  private TetraType heldType = TetraType.None;
  private TetraType currentType = TetraType.None;
  private bool hasHeld;

  public bool moveCamInMenu;


  [Header("UI")]
  public GameObject menuUI;
  public GameObject gameUI;
  public Text scoreText;
  public Text highScoreText;
  public Text heldText;
  public Text cameraMoveText;
  public Text levelText;
  public GameObject comboUI;
  public Text comboText;
  public GameObject optionsUI;
  public GameObject[] optionsMenusUI;

  public Text gridTextX;
  public Text gridTextY;
  public Text gridTextZ;

  void Start() {
    highScoreText.text = "" + highScore;
    scoreText.text = "" + score;
    moveCamInMenu = true;
    ToggleCamInMenu();
    comboUI.SetActive(false);
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
    previewCoords = new Vector3Int[4];
    previewBlocks = new GameObject[4];
    gridOffset = -0.5f * new Vector3(gridSizeX, 0, gridSizeZ) + new Vector3(0.5f, 0, 0.5f);
    tetraCubeQueue = new List<TetraType>();
    heldType = TetraType.None;


    for (int i = 0; i < gridSizeX; i++) {
      for (int j = 0; j < gridSizeZ; j++) {
        GameObject go = Instantiate(cube, transform);
        go.transform.position = new Vector3(i, -1, j) + gridOffset;
        go.GetComponent<Renderer>().material = baseMat;
      }
    }

    NewTetraCube(false);
    score = 0;
    inGame = true;

    menuUI.SetActive(false);
    gameUI.SetActive(true);

    // Progression
    level = 0;
    exp = -16;
    combo = 0;
    UpdateLevelUI();
    comboUI.SetActive(false);
  }

  private void EndGame() {
    inGame = false;

    menuUI.SetActive(true);
    gameUI.SetActive(false);
    comboUI.SetActive(false);
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

  private void NewTetraCube(bool fromHeld) {
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

    // if from held tetraCube
    if (fromHeld) {
      if (hasHeld)
        return;

      TetraType temp = currentType;
      currentType = heldType;
      heldType = temp;

      foreach (GameObject block in currentBlocks) {
        Destroy(block);
      }

      hasHeld = true;
    }

    heldText.text = heldType.ToString();

    // Pop from queue
    if (!fromHeld || currentType == TetraType.None) {
      currentType = tetraCubeQueue[0];
      tetraCubeQueue.RemoveAt(0);
      hasHeld = false;
    }

    int l = gridHeight - 1;
    Material m = mats[(int)currentType];
    int cx = gridSizeX / 2;
    int cz = gridSizeZ / 2;

    switch (currentType) {
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

    currentBlocks[0].GetComponent<Renderer>().material.SetColor("_Color", new Color(0.25f, 0.25f, 0.25f));
    UpdatePreview();
  }

  private void CheckInput() {
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
    } else if (Input.GetKeyDown(KeyCode.Space)) {
      SlamBlock();
    } else if (Input.GetKeyDown(KeyCode.C)) {
      NewTetraCube(true);
    }
  }

  private void UpdateFall() {
    float fastFall = 1.0f;
    float fallSpeed = 1.0f + 0.05f * level;

    if (Input.GetKey(KeyCode.F))
      fastFall = Mathf.Max(12.0f / fallSpeed, 1.0f);

    fallTime += Time.deltaTime * fallSpeed * fastFall;

    if (fallTime > 1.0f) {
      fallTime -= 1.0f;
      bool fall = MoveBlock(0, -1, 0);

      if (!fall) {
        LockTetraCube();
        NewTetraCube(false);
        ClearLines();
      }
    }
  }

  private void ClearLines() {
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
    layers.Reverse();

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
    if (layers.Count > 0) {
      combo += 1;
      int bonus = 100 * combo * gridSizeX * gridSizeZ / 16;

      for (int l = 1; l < layers.Count; l++) {
        bonus *= 3;
      }

      bonus = bonus + (bonus * level / 2);

      score += bonus;
      exp += layers.Count * gridSizeX * gridSizeZ;
      int expCap = 16 + Mathf.RoundToInt(level / 3.125f);

      while (exp >= expCap && level < 50) {
        exp -= expCap;
        level += 1;
        UpdateLevelUI();
        expCap = 16 + Mathf.RoundToInt(level / 3.125f);
      }

      if (highScore < score)
        highScore = score;
    } else {
      combo = 0;
    }

    highScoreText.text = "" + highScore;
    scoreText.text = "" + score;
    comboText.text = "" + combo;
    comboUI.SetActive(combo > 1);
  }

  private void SlamBlock() {
    bool falling = true;

    while (falling) {
      falling = MoveBlock(0, -1, 0);
    }

    fallTime = 1.0f;
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

    UpdatePreview();
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

    UpdatePreview();
    return true;
  }

  private void UpdatePreview() {

    for (int i = 0; i < previewCoords.Length; i++) {
      previewCoords[i] = currentCoords[i];
    }

    bool collision = false;

    while (!collision) {
      foreach (Vector3Int coords in previewCoords) {
        int ny = coords.y - 1;
        collision = collision || ny >= gridHeight || ny < 0;
        collision = collision || gridBlocks[coords.x, ny, coords.z] != null;
      }

      if (!collision) {
        for (int i = 0; i < previewCoords.Length; i++) {
          previewCoords[i] += new Vector3Int(0, -1, 0);
        }
      }
    }

    for (int i = 0; i < previewBlocks.Length; i++) {
      if (previewBlocks[i] == null)
        previewBlocks[i] = Instantiate(previewCube, transform);

      previewBlocks[i].transform.position = previewCoords[i] + gridOffset;
    }
  }

  private void UpdateLevelUI() {

    levelText.text = "" + level;
  }

  public void OpenTwitter() {
    Application.OpenURL("https://twitter.com/imef96");
  }

  public void QuitGame() {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
    Application.OpenURL(webplayerQuitURL);
#else
    Application.Quit();
#endif
  }

  public void ToggleCamInMenu() {
    moveCamInMenu = !moveCamInMenu;

    if (moveCamInMenu) {
      cameraMoveText.text = "Disable Menu Camera";
    } else {
      cameraMoveText.text = "Enable Menu Camera";
    }
  }

  public void ToggleOptionsUI() {

    // Options
    gridTextX.text = "" + gridSizeX;
    gridTextZ.text = "" + gridSizeZ;
    gridTextY.text = "" + gridHeight;

    optionsUI.SetActive(!optionsUI.activeSelf);
    menuUI.SetActive(!optionsUI.activeSelf);

    for (int i = 0; i < optionsMenusUI.Length; i++) {
      optionsMenusUI[i].SetActive(0 == i);
    }
  }

  public void ChangeOptionsMenu(int dir) {
    int activeIndex = 0;

    for (int i = 0; i < optionsMenusUI.Length; i++) {
      if (optionsMenusUI[i].activeSelf)
        activeIndex = i;
    }

    activeIndex += dir;

    if (activeIndex < 0)
      activeIndex += optionsMenusUI.Length;

    if (activeIndex >= optionsMenusUI.Length)
      activeIndex -= optionsMenusUI.Length;

    for (int i = 0; i < optionsMenusUI.Length; i++) {
      optionsMenusUI[i].SetActive(activeIndex == i);
    }
  }

  public void ChangeGridSize(int mode) {
    // I think this is the worst function I have ever written
    int delta = (mode % 2) - (1 - (mode % 2));

    if (mode / 2 == 0) {
      gridSizeX = Mathf.Max(gridSizeX + delta, 4);
      gridTextX.text = "" + gridSizeX;
    }

    if (mode / 2 == 1) {
      gridSizeZ = Mathf.Max(gridSizeZ + delta, 4);
      gridTextZ.text = "" + gridSizeZ;
    }

    if (mode / 2 == 2) {
      gridHeight = Mathf.Max(gridHeight + delta, 8);
      gridTextY.text = "" + gridHeight;
    }
  }
}

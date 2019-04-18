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
  private float fallSpeedMultiplier = 1f;
  private float fallSpeedCap = 12f;

  [Header("Style Options")]
  public Color[] colorOpts;
  public Texture[] textureOpts;

  private int colorPreview = 4;
  private int colorBase = 1;
  private int[] colors = new int[8] { 8, 12, 13, 17, 14, 10, 16, 7 };
  private int textureBase = 0;
  private int texturePreview = 1;
  private int[] textures = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

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
  public Material matBase;
  public Material matPreview;
  public Material matNormal;
  private Material[] mats = new Material[8];

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

  public Text optGFSMText;
  public Text optGFSCText;

  public Image[] tetraColorUI;
  public Image[] tetraTextureUI;

  void Start() {
    highScoreText.text = "" + highScore;
    scoreText.text = "" + score;
    moveCamInMenu = true;
    ToggleCamInMenu();
    comboUI.SetActive(false);

    for (int i = 0; i < 8; i++) {
      mats[i] = new Material(matNormal);
    }
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

    UpdateMaterial();

    for (int i = 0; i < gridSizeX; i++) {
      for (int j = 0; j < gridSizeZ; j++) {
        GameObject go = Instantiate(cube, transform);
        go.transform.position = new Vector3(i, -1, j) + gridOffset;
        go.GetComponent<Renderer>().material = matBase;
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

    Color pivotColor = 0.8f * m.color;
    currentBlocks[0].GetComponent<Renderer>().material.SetColor("_Color", pivotColor);
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
    float fallSpeed = 1.0f + 0.05f * level;
    fallSpeed = fallSpeed * fallSpeedMultiplier;

    if (Input.GetKey(KeyCode.F)) {
      if (fallSpeed < fallSpeedCap)
        fallSpeed = fallSpeedCap;
    }

    if (fallSpeed > fallSpeedCap)
      fallSpeed = fallSpeedCap;

    fallTime += Time.deltaTime * fallSpeed;

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

  private void UpdateMaterial() {
    matBase.SetTexture("_MainTex", textureOpts[textureBase]);
    matBase.SetColor("_Color", colorOpts[colorBase]);
    matPreview.SetTexture("_MainTex", textureOpts[texturePreview]);
    Color prevColor = colorOpts[colorPreview];
    prevColor.a = 0.5f;
    matPreview.SetColor("_Color", prevColor);

    for (int i = 0; i < 8; i++) {
      mats[i].SetTexture("_MainTex", textureOpts[textures[i]]);
      mats[i].SetColor("_Color", colorOpts[colors[i]]);
    }
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
      if (previewBlocks[i] == null) {
        previewBlocks[i] = Instantiate(previewCube, transform);
        previewBlocks[i].GetComponent<Renderer>().material = matPreview;
      }

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

    optGFSMText.text = "" + fallSpeedMultiplier;
    optGFSCText.text = "" + fallSpeedCap;

    optionsUI.SetActive(!optionsUI.activeSelf);
    menuUI.SetActive(!optionsUI.activeSelf);

    for (int i = 0; i < optionsMenusUI.Length; i++) {
      optionsMenusUI[i].SetActive(0 == i);
    }

    UpdateStyleImages();
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
      gridSizeX = 4;
      gridHeight = Mathf.Max(gridHeight + delta, 8);
      gridTextY.text = "" + gridHeight;
    }
  }

  public void ChangeFallSpeed(int mode) {
    int delta = (mode % 2) - (1 - (mode % 2));

    // Multiplier
    if (mode / 2 == 0) {
      fallSpeedMultiplier += delta * 0.05f;
      fallSpeedMultiplier = Mathf.Max(fallSpeedMultiplier, 0);
      optGFSMText.text = (Mathf.RoundToInt(100 * fallSpeedMultiplier) / 100f).ToString("N2");
    }

    // Cap
    if (mode / 2 == 1) {
      fallSpeedCap += delta * 0.2f;
      fallSpeedCap = Mathf.Max(fallSpeedCap, 0);
      optGFSCText.text = (Mathf.RoundToInt(10 * fallSpeedCap) / 10f).ToString("N1");
    }
  }

  public void ResetGameOptions() {
    gridSizeX = 4;
    gridSizeZ = 4;
    gridHeight = 20;
    fallSpeedMultiplier = 1f;
    fallSpeedCap = 12f;

    gridTextX.text = "" + gridSizeX;
    gridTextZ.text = "" + gridSizeZ;
    gridTextY.text = "" + gridHeight;
    optGFSMText.text = (Mathf.RoundToInt(100 * fallSpeedMultiplier) / 100f).ToString("N2");
    optGFSCText.text = (Mathf.RoundToInt(10 * fallSpeedCap) / 10f).ToString("N1");
  }

  public void ChangeTetraColor(int mode) {
    int delta = (mode % 2) - (1 - (mode % 2));
    delta = -delta;

    // Base
    if (mode / 2 == 0) {
      colorBase = (colorBase + delta) % colorOpts.Length;

      if (colorBase < 0)
        colorBase += colorOpts.Length;
    }

    // Preview
    if (mode / 2 == 1) {
      colorPreview = (colorPreview + delta) % colorOpts.Length;

      if (colorPreview < 0)
        colorPreview += colorOpts.Length;
    }

    // Tetras
    if (mode / 2 >= 2) {
      int ind = (mode / 2) - 2;
      colors[ind] = (colors[ind] + delta) % colorOpts.Length;

      if (colors[ind] < 0)
        colors[ind] += colorOpts.Length;
    }

    UpdateStyleImages();
  }

  public void ChangeTetraTexture(int mode) {
    int delta = (mode % 2) - (1 - (mode % 2));
    delta = -delta;

    // Base
    if (mode / 2 == 0) {
      textureBase = (textureBase + delta) % textureOpts.Length;

      if (textureBase < 0)
        textureBase += textureOpts.Length;
    }

    // Preview
    if (mode / 2 == 1) {
      texturePreview = (texturePreview + delta) % textureOpts.Length;

      if (texturePreview < 0)
        texturePreview += textureOpts.Length;
    }

    // Tetras
    if (mode / 2 >= 2) {
      int ind = (mode / 2) - 2;
      textures[ind] = (textures[ind] + delta) % textureOpts.Length;

      if (textures[ind] < 0)
        textures[ind] += textureOpts.Length;
    }

    UpdateStyleImages();
  }

  public void UpdateStyleImages() {

    tetraColorUI[0].color = colorOpts[colorBase];
    tetraColorUI[1].color = colorOpts[colorPreview];

    for (int i = 0; i < colors.Length; i++) {
      tetraColorUI[i + 2].color = colorOpts[colors[i]];
    }

    Vector2 pivot = new Vector2(0.5f, 0.5f);
    tetraColorUI[0].sprite = Sprite.Create((Texture2D)textureOpts[textureBase], new Rect(0, 0, textureOpts[textureBase].width, textureOpts[textureBase].height), pivot);
    tetraTextureUI[0].sprite = Sprite.Create((Texture2D)textureOpts[textureBase], new Rect(0, 0, textureOpts[textureBase].width, textureOpts[textureBase].height), pivot);
    tetraColorUI[1].sprite = Sprite.Create((Texture2D)textureOpts[texturePreview], new Rect(0, 0, textureOpts[texturePreview].width, textureOpts[texturePreview].height), pivot);
    tetraTextureUI[1].sprite = Sprite.Create((Texture2D)textureOpts[texturePreview], new Rect(0, 0, textureOpts[texturePreview].width, textureOpts[texturePreview].height), pivot);

    for (int i = 0; i < textures.Length; i++) {
      tetraColorUI[i + 2].sprite = Sprite.Create((Texture2D)textureOpts[textures[i]], new Rect(0, 0, textureOpts[textures[i]].width, textureOpts[textures[i]].height), pivot);
      tetraTextureUI[i + 2].sprite = Sprite.Create((Texture2D)textureOpts[textures[i]], new Rect(0, 0, textureOpts[textures[i]].width, textureOpts[textures[i]].height), pivot);
    }
  }
}
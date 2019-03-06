using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

  public int gridSizeX = 4;
  public int gridSizeZ = 4;
  public int gridHeight = 20;
  private Vector3 gridOffset;

  public float fallSpeed = 1;
  public float level = 0;
  private float fallTime = 0;

  public GameObject cube;

  private Vector3Int[] currentCoords;
  private GameObject[] currentBlocks;

  private GameObject[, ,] gridBlocks;

  void Start() {
    gridBlocks = new GameObject[gridSizeX, gridHeight, gridSizeZ];
    currentCoords = new Vector3Int[4];
    currentBlocks = new GameObject[4];
    gridOffset = -0.5f * new Vector3(gridSizeX, 0, gridSizeZ) + new Vector3(0.5f, 0, 0.5f);


    for (int i = 0; i < gridSizeX; i++) {
      for (int j = 0; j < gridSizeZ; j++) {
        GameObject go = Instantiate(cube, transform);
        go.transform.position = new Vector3(i, -1, j) + gridOffset;
      }
    }

    NewTetraCube();
  }

  void Update() {
    CheckInput();
    UpdateFall();
  }

  private void NewTetraCube() {

    // TODO: If blocked, end game
    // TODO: Random-8 tetracube queue

    for (int i = 0; i < currentCoords.Length; i++) {
      if (currentBlocks[i] != null) {
        Vector3Int coords = currentCoords[i];
        gridBlocks[coords.x, coords.y, coords.z] = currentBlocks[i];
      }

      currentCoords[i] = new Vector3Int(i, gridHeight - 1, 0);
      currentBlocks[i] = Instantiate(cube, transform);
      currentBlocks[i].transform.position = currentCoords[i] + gridOffset;
    }
  }

  private void CheckInput() {
    // TODO: Rotate inputs
    // TODO: Camera controls
    // TODO: Vary control based oncamera direction
    if (Input.GetKeyDown(KeyCode.A)) {
      MoveBlock(1, 0, 0);
    } else if (Input.GetKeyDown(KeyCode.D)) {
      MoveBlock(-1, 0, 0);
    } else if (Input.GetKeyDown(KeyCode.W)) {
      MoveBlock(0, 0, 1);
    } else if (Input.GetKeyDown(KeyCode.S)) {
      MoveBlock(0, 0, -1);
    }
  }

  private void UpdateFall() {
    fallTime += Time.deltaTime;

    if (fallTime > 1.0f) {
      fallTime -= 1.0f;
      bool fall = MoveBlock(0, -1, 0);

      if (!fall) {
        NewTetraCube();
        // TODO: clear line
      }
    }
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
}

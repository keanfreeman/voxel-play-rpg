using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelPlay;

public class Orchestrator : MonoBehaviour
{
    public GameObject vpEnvironment;
    public GameObject vpController;
    public GameObject playerPrefab;
    public GameObject opossumPrefab;

    private GameObject playerInstance;

    public Dictionary<GameObject, Vector3Int> creatures
        = new Dictionary<GameObject, Vector3Int>();

    // Start is called before the first frame update
    void Start()
    {
        InitCreatures();
        SpriteMovement spriteMovement = vpController.GetComponent<SpriteMovement>();
        spriteMovement.spriteContainer = playerInstance;
        vpController.GetComponent<SpriteMovement>().enabled = true;

        vpController.GetComponent<VoxelPlayPlayer>().enabled = true;

        vpController.GetComponent<VoxelPlayFirstPersonController>().enabled = true;
    }

    private void InitCreatures() {
        Vector3Int playerStartPosition = new Vector3Int(523, 50, 246);
        GameObject playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        this.creatures[playerInstance] = playerStartPosition;
        this.playerInstance = playerInstance;

        Vector3Int opossumStartPosition = new Vector3Int(521, 50, 246);
        GameObject opossumInstance = Instantiate(opossumPrefab, opossumStartPosition, Quaternion.identity);
        this.creatures[opossumInstance] = opossumStartPosition;
    }
}

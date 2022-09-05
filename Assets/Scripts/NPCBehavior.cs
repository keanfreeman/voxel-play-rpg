using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// moves around randomly
public class NPCBehavior : MonoBehaviour
{
    private const float NPC_MIN_IDLE_TIME = 1;
    private const float NPC_MAX_IDLE_TIME = 5;

    System.Random rng = new System.Random();

    public Vector3Int spritePosition = new Vector3Int(521, 50, 246);
    private float lastMoveTime = 0;

    void Update()
    {
        if (Time.time - lastMoveTime < NPC_MIN_IDLE_TIME) {
            return;
        }
        lastMoveTime = Time.time;

        Vector3Int newPosition = spritePosition + new Vector3Int(rng.Next(-1, 2), 0, rng.Next(-1, 2));
        MoveSprite(newPosition);
    }

    public void MoveSprite(Vector3Int position) {
        spritePosition = position;
        transform.position = spritePosition;
    }
}

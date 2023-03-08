using InstantiatedEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] public PlayerCharacter playerCharacter;
    public List<PlayerCharacter> partyMembers;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        partyMembers = new List<PlayerCharacter>();
    }
}

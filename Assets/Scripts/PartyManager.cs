using InstantiatedEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] public PlayerCharacter playerCharacter;
    public List<Traveller> partyMembers { get; private set; }

    void Awake() {
        partyMembers = new List<Traveller>();
    }
}

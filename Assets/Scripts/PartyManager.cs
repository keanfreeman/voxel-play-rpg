using InstantiatedEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager
{
    public PlayerCharacter playerCharacter;
    public List<PlayerCharacter> partyMembers;

    public PartyManager(PlayerCharacter playerCharacter) {
        this.playerCharacter = playerCharacter;
        partyMembers = new List<PlayerCharacter>();
    }
}

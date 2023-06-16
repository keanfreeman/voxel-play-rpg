using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioController audioController;
    [SerializeField] PartyManager partyManager;

    public Dictionary<string, AudioClip> nameToSong { get; private set; } = new();
    private Dictionary<EntityDefinition.MusicCube,
        HashSet<EntityDefinition.PlayerCharacter>> playersInCubes = new();
    private string defaultSongName;
    private InterruptedSong interruptedSong;

    private void Awake() {
        defaultSongName = audioController.lakeTheme.name;

        AudioClip[] songs = Resources.LoadAll<AudioClip>("audio/music/");
        foreach (AudioClip audioClip in songs) {
            nameToSong[audioClip.name] = audioClip;
        }
    }

    public async UniTask StartMusic() {
        // give players / music boxes chance to collide
        await UniTask.Delay(TimeSpan.FromMilliseconds(200));
        if (audioController.currSong == null && playersInCubes.Count == 0) {
            audioController.PlaySong(nameToSong[defaultSongName]);
        }
    }

    public void PlayCombatMusic(AudioClip song) {
        interruptedSong = new(audioController.audioSource.time, audioController.currSong);
        audioController.PlaySong(song);
    }

    public void EndCombatMusic() {
        audioController.PlaySong(interruptedSong.song);
        audioController.audioSource.time = interruptedSong.songTime;
        interruptedSong = null;
    }

    public void OnPlayerEnteredCube(EntityDefinition.MusicCube musicCube,
            EntityDefinition.PlayerCharacter pc) {
        if (!playersInCubes.ContainsKey(musicCube)) playersInCubes[musicCube] = new();
        playersInCubes[musicCube].Add(pc);
        if (playersInCubes[musicCube].Count < partyManager.partyMembers.Count) {
            return;
        }

        PlayNextSong();
    }

    public void OnPlayerExitCube(EntityDefinition.MusicCube musicCube,
            EntityDefinition.PlayerCharacter pc) {
        if (playersInCubes.ContainsKey(musicCube)) {
            playersInCubes[musicCube].Remove(pc);
            if (playersInCubes[musicCube].Count == 0) {
                playersInCubes.Remove(musicCube);
            }
        }

        PlayNextSong();
    }

    private void PlayNextSong() {
        if (interruptedSong != null) {
            return;
        }

        string nextSongName = GetNextPrioritySong();
        AudioClip song = nameToSong.GetValueOrDefault(nextSongName, null);
        if (audioController.currSong == song || audioController.nextSong == song) {
            return;
        }
        audioController.PlaySong(song);
    }

    private List<EntityDefinition.MusicCube> GetMusicCubesAllPlayersIn() {
        List<EntityDefinition.MusicCube> fullCubes = new();
        foreach (EntityDefinition.MusicCube musicCube in playersInCubes.Keys) {
            if (playersInCubes[musicCube].Count < partyManager.partyMembers.Count) {
                continue;
            }
            fullCubes.Add(musicCube);
        }

        fullCubes.Sort((x, y) => x.cubePriority.CompareTo(y.cubePriority));
        return fullCubes;
    }

    private string GetNextPrioritySong() {
        List<EntityDefinition.MusicCube> fullCubes = GetMusicCubesAllPlayersIn();
        return fullCubes.Count == 0 ? defaultSongName : fullCubes[0].audioClipName;
    }
}

public class InterruptedSong {
    public float songTime;
    public AudioClip song;

    public InterruptedSong(float songTime, AudioClip song) {
        this.songTime = songTime;
        this.song = song;
    }
}
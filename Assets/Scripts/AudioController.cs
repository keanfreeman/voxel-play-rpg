using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {
    [SerializeField] public AudioSource audioSource;

    [SerializeField] public AudioClip sashasTheme;
    [SerializeField] public AudioClip lakeTheme;
    [SerializeField] public AudioClip snowpointTheme;
    [SerializeField] public AudioClip eternaForestTheme;
    [SerializeField] public AudioClip rachelsTheme;
    [SerializeField] public AudioClip linsTheme;
    [SerializeField] List<SongDetails> songTimes;

    Dictionary<AudioClip, SongDetails> songMapping = new();

    public AudioClip currSong { get; private set; } = null;
    public AudioClip nextSong { get; set; } = null;

    private void Awake() {
        songMapping[sashasTheme] = songTimes[0];
        songMapping[lakeTheme] = songTimes[1];
        songMapping[snowpointTheme] = songTimes[2];
        songMapping[eternaForestTheme] = songTimes[3];
        songMapping[rachelsTheme] = songTimes[4];
        songMapping[linsTheme] = songTimes[5];
    }

    // for debugging music loops
    //private void Update() {
    //    if (Input.GetKeyUp(KeyCode.Keypad0)) {
    //        Debug.Log(audioSource.time);
    //    }
    //    if (Input.GetKeyUp(KeyCode.Keypad1)) {
    //        audioSource.time += 10;
    //    }
    //    if (Input.GetKeyUp(KeyCode.Keypad2)) {
    //        audioSource.time = songMapping[currSong].end - 5;
    //    }
    //}

    public void PlaySong(AudioClip song) {
        if (currSong == null) {
            PlaySongLoop(song);
            return;
        }
        nextSong = song;
    }

    private async UniTask PlaySongLoop(AudioClip song) {
        currSong = song;
        SongDetails songDetails = songMapping[currSong];
        audioSource.clip = songDetails.song;
        audioSource.Play();

        while (true) {
            await UniTask.NextFrame();

            if (nextSong != null) {
                await FadeOut();
                currSong = nextSong;
                nextSong = null;
                songDetails = songMapping[currSong];
                audioSource.clip = currSong;
                audioSource.Play();
            }

            if (audioSource.time >= songDetails.end) {
                audioSource.time = songDetails.start;
                Debug.Log("looped music");
            }
        }
    }

    private async UniTask FadeOut() {
        const float FADE_TIME = 2;

        float startTime = Time.time;
        float endTime = Time.time + FADE_TIME;
        float currTime = startTime;
        while (currTime < endTime) {
            float fractionDone =  (currTime - startTime) / (FADE_TIME);
            audioSource.volume = 1 - fractionDone;

            await UniTask.NextFrame();
            currTime = Time.time;
        }
        audioSource.volume = 0;
        audioSource.Stop();
        audioSource.volume = 1;
    }
}

[Serializable]
public class SongDetails {
    public AudioClip song;
    public float start;
    public float end;

    public SongDetails(AudioClip song, float start, float end) {
        this.song = song;
        this.start = start;
        this.end = end;
    }
}

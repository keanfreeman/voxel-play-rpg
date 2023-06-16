using GameMechanics;
using Orders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace EntityDefinition {
    [Serializable]
    public class MusicCube : IntangibleEntity {
        public string audioClipName;
        // lower is prioritized
        public int cubePriority;
        // bottom left
        public Vector3Int cubeStart;
        // top right
        public Vector3Int cubeEnd;

        [JsonConstructor]
        public MusicCube(string audioClipName, int cubePriority, Vector3Int cubeStart, Vector3Int cubeEnd)
                : base(cubeStart, "MusicCube") {
            this.audioClipName = audioClipName;
            this.cubePriority = cubePriority;
            this.cubeStart = cubeStart;
            this.cubeEnd = cubeEnd;
        }

        public Vector3 GetCubeXYZScale() {
            Vector3Int diff = cubeEnd - cubeStart;
            return Vector3.one + diff;
        }
    }
}

using NonVoxel;
using UnityEngine;

namespace Instantiated {
    public class MusicCube : IntangibleEntity {
        private MusicManager musicManager;

        public void Init(EntityDefinition.MusicCube cubeInfo, MusicManager musicManager) {
            this.entity = cubeInfo;
            this.musicManager = musicManager;
            this.transform.localScale = cubeInfo.GetCubeXYZScale();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.TryGetComponent<PlayerCharacter>(out var pc)) {
                musicManager.OnPlayerEnteredCube(GetCubeInfo(), pc.GetEntity());
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.gameObject.TryGetComponent<PlayerCharacter>(out var pc)) {
                musicManager.OnPlayerExitCube(GetCubeInfo(), pc.GetEntity());
            }
        }

        private EntityDefinition.MusicCube GetCubeInfo() {
            return (EntityDefinition.MusicCube)GetEntity();
        }
    }
}

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using VoxelPlay;

namespace VoxelPlayDemos {
	public class FoeController : MonoBehaviour {

		NavMeshAgent agent;
		VoxelPlayEnvironment env;

		void Start () {
			agent = gameObject.AddComponent<NavMeshAgent> ();
		}


		// Update is called once per frame
		void Update () {
			if (env == null) {
                env = VoxelPlayEnvironment.GetSceneInstance(gameObject.scene.buildIndex);
			}
			if (Random.value > 0.99f) {
				agent.SetDestination (env.cameraMain.transform.position);
			}
		}
	}

}

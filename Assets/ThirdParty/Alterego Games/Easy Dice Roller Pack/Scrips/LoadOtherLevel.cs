using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadOtherLevel : MonoBehaviour {
    // does not currently work as these scenes are not in the build index
    public void OnLoadLevel() {
        if(SceneManager.GetActiveScene().buildIndex == 0){
            SceneManager.LoadScene("DiceExample2");
        }else{
            SceneManager.LoadScene("DiceExample1");
        }
    }
}

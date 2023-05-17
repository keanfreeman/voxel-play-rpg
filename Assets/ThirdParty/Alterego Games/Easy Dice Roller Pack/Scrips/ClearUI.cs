using UnityEngine;
using System.Collections;

public class ClearUI : MonoBehaviour {
	public AddAmountInUI[] ScriptsToReset;

	public void OnClearAll(){
		for(int i = 0; i < ScriptsToReset.Length; i++){
			ScriptsToReset[i].OnResetAmountUI();
		}
	}
}

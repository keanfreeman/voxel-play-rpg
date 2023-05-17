using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AddAmountInUI : MonoBehaviour {
	private Text thisText;

	public void OnResetAmountUI(){
		thisText.text = (0).ToString();
	}

	public void OnAddDice(int diceType){
		thisText.text = (int.Parse(thisText.text) + 1).ToString();
	}
	void Start(){
		thisText = GetComponent<Text>();
	}
}

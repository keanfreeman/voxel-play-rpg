using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum DiceTypes{D4,D6,D8,D10,D12,D20}

public class Table : MonoBehaviour {
    [SerializeField] Transform throwPosition;

    [Header("Dice")]
    // all prefabs of the dice
    public GameObject D4Prefab;					//4 sided die prefab
    public GameObject D6Prefab;					//6 sided die prefab
    public GameObject D8Prefab;					//8 sided die prefab
    public GameObject D10Prefab;				//10 sided die prefab
    public GameObject D12Prefab;				//12 sided die prefab
    public GameObject D20Prefab;				//20 sided die prefab

    [Header("Table settings")]
    public GameObject TableSurface;			//gameobject that detects if a die had landed on it (needs a collider)
    public Text ResultCounter;					//textfield where the results are shown
    public float SpinPower;				//Amount of random spin
    [Range(0,300)]
    public float ForcePower = 300;				//Amount of force whith which the dice are thrown
    [Range(0,300)]
    public float ForceRandomDifference = 50;	//Random factor of the force when thrown
    [Range(-1,1)]
    public float ThrowDirectionX;				//Direction in which the dice are thrown in the x direction
    [Range(-1,1)]
    public float ThrowDirectionY;				//Direction in which the dice are thrown in the Y direction
    [Range(-1,1)]
    public float ThrowDirectionZ;				//Direction in which the dice are thrown in the z direction
    [HideInInspector]
    public List<GameObject> DiceToThrow = new List<GameObject>();	//List of dice to throw (hidden)
    [HideInInspector]
    public List<Dice> DiceThrown = new List<Dice>();				//list of dice being thrown (hidden)
    [HideInInspector]
    public List<GameObject> DiceToThrowNotSpawnByTable = new List<GameObject>(); //list of dice to thow already in scene (hidden)

    public List<int> ThrowResults = new List<int>();

    //private DiceTypes diceType;
    private float spawnDistance;
    private int countX = 0;
    private int countY = 0;
    private int countZ = 0;
    private bool checkForResult = false;
    private int count = 0;

    //-----------AddDice()-------------
    // add a set amount dice of a certain type to the 'DiceToThrow' list
    //-------------------------------
    public void AddDiceD4(int _amount){
            for(int i = 0; i < _amount; i++){
                DiceToThrow.Add(D4Prefab);
            }
        spawnDistance = Mathf.Floor(Mathf.Pow(DiceToThrow.Count, 1f / 3f));	
    }
    public void AddDiceD6(int _amount){
        for(int i = 0; i < _amount; i++){
            DiceToThrow.Add(D6Prefab);
        }
        spawnDistance = Mathf.Floor(Mathf.Pow(DiceToThrow.Count, 1f / 3f));	
    }
    public void AddDiceD8(int _amount){
        for(int i = 0; i < _amount; i++){
            DiceToThrow.Add(D8Prefab);
        }
        spawnDistance = Mathf.Floor(Mathf.Pow(DiceToThrow.Count, 1f / 3f));	
    }
    public void AddDiceD10(int _amount){
        for(int i = 0; i < _amount; i++){
            DiceToThrow.Add(D10Prefab);
        }
        spawnDistance = Mathf.Floor(Mathf.Pow(DiceToThrow.Count, 1f / 3f));	
    }
    public void AddDiceD12(int _amount){
        for(int i = 0; i < _amount; i++){
            DiceToThrow.Add(D12Prefab);
        }
        spawnDistance = Mathf.Floor(Mathf.Pow(DiceToThrow.Count, 1f / 3f));	
    }
    public void AddDiceD20(int _amount){
        for(int i = 0; i < _amount; i++){
            DiceToThrow.Add(D20Prefab);
        }
        spawnDistance = Mathf.Floor(Mathf.Pow(DiceToThrow.Count, 1f / 3f));	
    }

    
    //-----------ThrowDice()-------------
    // Send message to all dice that they need to be thrown
    //-------------------------------
    public void OnThrowDice(){
        if(checkForResult == false){
            ClearResultUI();
            ClearResultList();
            countX = 0;
            countY = 0;
            countZ = 0;

            CleanUpOldDice();
            SpawnDice();
            for(int i = 0; i < DiceThrown.Count; i++){
                if(DiceThrown[i].ImAwake == false){
                    DiceThrown[i].SetSpinPower(SpinPower);
                    DiceThrown[i].ForcePower = ForcePower;
                    DiceThrown[i].ForceRandomDifference = ForceRandomDifference;
                    DiceThrown[i].ThrowDirectionX = ThrowDirectionX;
                    DiceThrown[i].ThrowDirectionY = ThrowDirectionZ;
                    DiceThrown[i].ThrowDirectionZ = ThrowDirectionZ;
                }
                DiceThrown[i].OnThrow();
            }
            //start checking for result
            checkForResult = true;
        }
    }

    //-----------ClearDiceList()-------------
    // Clear the 'DiceThrown' and the 'DiceToThrow' list
    //-------------------------------
    public void OnClearDiceList(){
        ClearResultUI();
        CleanUpOldDice();
        DiceToThrow.Clear();
        DiceThrown.Clear();
        for(int i = 0; i < DiceToThrowNotSpawnByTable.Count; i++){
            DiceToThrowNotSpawnByTable[i].GetComponent<Dice>().Initialise();
        }
    }

    //-----------ResetDice()-------------
    // Clear the 'DiceThrown' and the 'DiceToThrow' list
    //-------------------------------
    public void OnResetDice(){
        ClearResultUI();
        CleanUpOldDice();
    }

    //-----------SpawnDice()-------------
    // instantiate all the dice from the 'DiceToThrow' list
    // add all dice from the 'DiceToThrow' list to the 'DiceThrown' list
    //clear the 'DiceToThrow' list
    //-------------------------------
    public void SpawnDice(){
        if(DiceToThrow != null || DiceToThrow.Count > 0){
            for(int i = 0; i < DiceToThrow.Count; i++){
                if(DiceToThrow[i].GetComponent<Dice>().ImAwake == false){
                    GameObject newDie = Instantiate(DiceToThrow[i]);
                    // keanfree - added random rotation to ensure randomness, since throw rotation isn't working.
                    newDie.transform.Rotate(new Vector3(Random.Range(0, 360), Random.Range(0, 360), 
                        Random.Range(0, 360)));
                    DiceThrown.Add(newDie.GetComponent<Dice>());
                    newDie.GetComponent<Dice>().OnReadyToThrow(this);
                    //newDie.transform.parent = this.transform;
                    SpawnLocation(i,newDie);
                }else{
                    DiceThrown.Add(DiceToThrow[i].GetComponent<Dice>());
                    DiceToThrow[i].GetComponent<Dice>().OnReadyToThrowNotSpawnedByTable();
                }
            }
        }else{
            Debug.LogWarning("Do dice to be thrown were chosen");
        }
    }

    //-----------SpawnLocation()-------------
    // set the spawnlocation for all the dice
    //-------------------------------
    private void SpawnLocation(int currentDieNumer, GameObject currentDie){
        int distanceFromCenter = Mathf.CeilToInt(currentDieNumer / 8f);
        float degreesAroundCenter = currentDieNumer == 0 ? 0 : 360 * (((currentDieNumer - 1) % 8) / 8f);
        Vector3 startPoint = throwPosition.position;
        startPoint += distanceFromCenter * Vector3.right;
        Vector3 finalPosition = Quaternion.Euler(0, degreesAroundCenter, 0)
            * (startPoint - throwPosition.position) + throwPosition.position;

        currentDie.transform.position = finalPosition;
        currentDie.transform.GetComponent<Dice>().OnRethrowLocation(currentDie.transform.localPosition);
        if(countX + 1 < (int)spawnDistance){
             countX++;
        }else {
            countX = 0;
            countY ++;
        }

        if(countY >= (int)spawnDistance){
            countY = 0;
            countZ ++;
        }
    }
    
    //-----------CleanUpOldDice()-------------
    // Destroy all dice in the 'DiceThrown' list
    // and reset the dice allready in the scene
    //-------------------------------
    public void CleanUpOldDice(){
        for(int i = 0; i < DiceThrown.Count; i++){
            DiceThrown[i].CleanUp();
        }
        DiceThrown.Clear();
    }

    //-----------ThrowResult()-------------
    // Check if all dice results are in
    //-------------------------------
    public bool ThrowResult(){
        bool done = true;
        for(int i = 0; i < DiceThrown.Count; i++){
            if(DiceThrown[i].InfoSend == false){
                done = false;
            }
        }
        if (done){
            checkForResult = false;
            ResultsEnd();
        }

        return done;
    }
    //-----------UpdateResults()-------------
    // Update result visually in UI
    //-------------------------------
    public void UpdateResults(DiceTypes diceType, int result){
        ThrowResults.Add(result);
        if(ResultCounter != null){
            count++;
            ResultCounter.text += diceType.ToString() + " Rolled: " + result + "\n";
            RectTransform _rectObject = ResultCounter.GetComponent<RectTransform> ();
            _rectObject.sizeDelta = new Vector2 (_rectObject.sizeDelta.x, 50.05f +(15 * count));
        }else{
            //Debug.LogWarning("There's no UI text object attached to the tablescipt, results will not be shown");
        }
    }
    //
    private void ClearResultUI(){
        if(ResultCounter != null){
            count = 0;
            ResultCounter.text = "";
            RectTransform _rectObject = ResultCounter.GetComponent<RectTransform> ();
            _rectObject.sizeDelta = new Vector2 (_rectObject.sizeDelta.x, 50.05f +(15 * count));
        }else{
            //Debug.LogWarning("There's no UI text object attached to the tablescipt, results will not be cleared");
        }
    }
    //
    private void ClearResultList(){
        ThrowResults.Clear();
    }
    //
    public void ResultsEnd(){
        //Debug.Log("All DICE RESULTS ARE IN!");
        if(ResultCounter != null){
            count++;
            ResultCounter.text += "All DICE RESULTS ARE IN!";
            RectTransform _rectObject = ResultCounter.GetComponent<RectTransform> ();
            _rectObject.sizeDelta = new Vector2 (_rectObject.sizeDelta.x, 50.05f +(15 * count));
        }else{
            //Debug.LogWarning("There's no UI text object attached to the tablescipt, results will not be shown");
        }
    }
    public void AddDieToList(GameObject _die){
        DiceToThrow.Add(_die);
        DiceToThrowNotSpawnByTable.Add(_die);
    }
    public void AddDieToListNotSpawnByTable(GameObject _die){
        bool AlreadyInList = false;
        for(int i = 0; i < DiceToThrowNotSpawnByTable.Count; i++){
            if(DiceToThrowNotSpawnByTable[i] == _die){
                AlreadyInList = true;
            }
        }
        if(AlreadyInList == false){
            DiceToThrowNotSpawnByTable.Add(_die);
        }
        DiceToThrow.Add(_die);
    }

    public void Update(){
        if(checkForResult){
            ThrowResult();
        }
    }
}

using UnityEngine;
using System.Collections;

public class Dice : MonoBehaviour {
	[SerializeField] public MeshRenderer dieRenderer;

	public DiceTypes MyDiceType;				//What type of Die is it
	public float MeasureDistance = 0.04f;       //Size of the sensor checking if floor-surface is hit

	[Range(0,300)]
	public float ForcePower = 300;				//Amount of force whith which the Die is thrown
	[Range(0,300)]
	public float ForceRandomDifference = 50;	//Random factor of the force when thrown
	[Range(-1,1)]
	public float ThrowDirectionX;				//Direction in which the die is thrown in the x direction
	[Range(-1,1)]
	public float ThrowDirectionY;				//Direction in which the die is thrown in the Y direction
	[Range(-1,1)]
	public float ThrowDirectionZ;				//Direction in which the die is thrown in the z direction
	[HideInInspector]
	public bool InfoSend = false;				//Bool to check if info is send (hidden)
	[HideInInspector]
	public bool SpawnedByTable = false;			//Bool to check if the die is spawned by ther table (hidden)
	[HideInInspector]
	public bool ImAwake = false;				//Bool to check if the die is awake and ready to use (hidden)
	public GameObject[] Sides;					//array of object to locate each side of the die
	private int RethrowLimit = 2;				//Max amount of rethrows before auto calculate outcome

	private Rigidbody myRigidBody;				
	public int result = -1;
	private bool moving = true;
	private Table myTableSurface;
	private bool onFloor = false;
	private Vector3 throwLocation;
	private bool oneTimeWarning = true;
	private int currentRethrowns = 0;

	private float spinPower;				//Amount of random spin

	//-----------Start()-------------
	void Start(){
		throwLocation = this.transform.localPosition;
		Initialise();
	}
	//-----------Reset()-------------
	//Set up die to be used directly from scene
	//or to be spaned by the table
	//------------------------------
	public void Initialise(){
		ImAwake = true;
		if(SpawnedByTable == false){
			this.transform.localPosition = throwLocation;
			//locate table
			if(this.transform.parent != null){
				if(this.transform.parent.GetComponent<Table>() != null){
					currentRethrowns = 0;
					this.transform.parent.GetComponent<Table>().AddDieToListNotSpawnByTable(this.gameObject);
					myTableSurface = this.transform.parent.GetComponent<Table>();
				}else{
					Debug.LogWarning("Make sure the Die is a Child of a 'Table'");
				}
			}else{
				Debug.LogWarning("Make sure the Die is a Child of a 'Table'");
			}

			OnReadyToThrowNotSpawnedByTable();
		}
	}

	public void SetSpinPower(float spinPower) {
		this.spinPower = spinPower;
	}

	//-----------OnReadyToThrow()-------------
	//Position the Die in the air and give it a random spin
	//---------------------------------------
	public void OnReadyToThrow(Table _tableSurface){
		SpawnedByTable = true;
		myRigidBody = GetComponent<Rigidbody>();
		myRigidBody.useGravity = false;
		myTableSurface = _tableSurface;
		myRigidBody.AddTorque(Random.Range(-spinPower,spinPower),Random.Range(-spinPower,spinPower),Random.Range(-spinPower,spinPower));
	}
	//-----------OnReadyToThrowNotSpawnedByTable()-------------
	//Position the Die that are already in the scene in the air and give it a random spin
	//--------------------------------------------------------
	public void OnReadyToThrowNotSpawnedByTable(){
		InfoSend = false;
		moving = false;
		myRigidBody = GetComponent<Rigidbody>();
		myRigidBody.freezeRotation = false;
		myRigidBody.isKinematic = true;
		myRigidBody.isKinematic = false;
		myRigidBody.useGravity = false;
		myRigidBody.AddTorque(Random.Range(-spinPower,spinPower),Random.Range(-spinPower,spinPower),Random.Range(-spinPower,spinPower));

	}
	//-----------OnThrow()-------------
	//Throw the Die
	//---------------------------------
	public void OnThrow(){
		this.transform.localPosition = throwLocation;
		myRigidBody.freezeRotation = false;
		myRigidBody.useGravity = true;
		myRigidBody.isKinematic = true;
		myRigidBody.isKinematic = false;
		Vector3 newForce = new Vector3(ThrowDirectionX * (ForcePower+Random.Range(-ForceRandomDifference,ForceRandomDifference)), ThrowDirectionY * (ForcePower+Random.Range(-ForceRandomDifference,ForceRandomDifference)),ThrowDirectionZ * (ForcePower+Random.Range(-ForceRandomDifference,ForceRandomDifference)));
		myRigidBody.AddForce(newForce);
        moving = true;
	}
	//-----------OnRethrowLocation()-------------
	//Store location where the die should be rethrown form
	//------------------------------------------
	public void OnRethrowLocation(Vector3 _throwLocation){
		throwLocation = _throwLocation;
	}

	//-----------OnReThrow()-------------
	//If the Die does not land correctly rethrow it
	//-----------------------------------
	public void OnReThrow(){
		if(currentRethrowns < RethrowLimit){
			currentRethrowns += 1;
			moving = true;
			InfoSend = false;
			this.transform.localPosition = throwLocation;
			myRigidBody.AddTorque(Random.Range(-spinPower,spinPower),Random.Range(-spinPower,spinPower),Random.Range(-spinPower,spinPower));
			OnThrow();
		}else{
			Debug.Log("Die needed to be rethrown to many times, something is wrong with the scene, sending random valid number");
			result = Sides[Random.Range(0,Sides.Length)].GetComponent<DieSide>().MyValue;
			myTableSurface.UpdateResults(GetComponent<Dice>().MyDiceType, result);
			myRigidBody.freezeRotation = true;
			moving = false;
			InfoSend = true;
			currentRethrowns = 0;
		}
	}

	public void Update(){
		if (myTableSurface == null){ 
			if(oneTimeWarning){
				Debug.LogWarning("Use the Table prefab to instantiate a dice, or make the table the parent of the dices");
				oneTimeWarning = false;
			}
			return;
		}

		//Check if Die is not fallen past the TableSurface
		if(myTableSurface.TableSurface.transform.position.y > this.transform.position.y && moving){
			Debug.Log("a "+ MyDiceType +" has fallen past the table");
			OnReThrow();
		}

		//wait untill the dice stopped moving
		if (myRigidBody.IsSleeping() && moving){



			onFloor = false;
			result = -1;
			for(int i = 0; i < Sides.Length; i++){
				Collider[] hitColliders = Physics.OverlapSphere(Sides[i].transform.position, MeasureDistance);
				int j = 0;
				while (j < hitColliders.Length) {
					// todo - fix so that the side closest to the floor is picked,
					// it doesn't have to actually hit the floor
					if (hitColliders[j].transform.gameObject == myTableSurface.TableSurface){
						onFloor = true;
						result = Sides[i].GetComponent<DieSide>().MyValue;
					}
					j++;
				}
			}

			//result
			if(result != -1
				// keanfree - no longer required to land on the floor
				&& onFloor
				){
				//Debug.Log(result);
				myTableSurface.UpdateResults(GetComponent<Dice>().MyDiceType, result);
				myRigidBody.freezeRotation = true;
				moving = false;
				InfoSend = true;
			}else if(onFloor == false){
				Debug.Log("Didn't land on floor, need to reroll");
				OnReThrow();
			}else{
				Debug.LogWarning("The die landed on a side that's not set correctly");
				OnReThrow();
			}
		}
	}

	public void CleanUp(){
		if(SpawnedByTable){
			DestroyImmediate(this.gameObject);
		}else{
			Initialise();
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		for(int i = 0; i < Sides.Length; i++){
			Gizmos.DrawSphere(Sides[i].transform.position,MeasureDistance);
		}


	}
}

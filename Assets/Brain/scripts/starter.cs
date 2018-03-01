using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class starter : MonoBehaviour {
	public KeyCode starterKey = KeyCode.Keypad5;
	public string scene2LoadName = "Playground";
	public Dropdown sceneNameDropdown;

	public string throwSequencesLocation = @"LogFiles/";
	public string throwSequenceCoreName = "throwSequence";
	public Dropdown throwSeqDropdown;

	public InputField subjetIDin;
	public InputField runNumin;
	GameObject loadMsg;

	// Use this for initialization
	void Start(){
		loadMsg = GameObject.Find("GUI/loadOverlay");
		loadMsg.SetActive(false);
		starterData.sequenceFilePath = Application.dataPath + "/" + throwSequencesLocation;
		DirectoryInfo d = new DirectoryInfo(starterData.sequenceFilePath);
		FileInfo[] Files = d.GetFiles(throwSequenceCoreName+"*.csv"); //Getting Text files

		throwSeqDropdown.options.Clear();
		Debug.Log("Files found: ");
		foreach(FileInfo file in Files )
		{
		  	Debug.Log(file.Name);
			Dropdown.OptionData newOption = new Dropdown.OptionData(file.Name);
			throwSeqDropdown.options.Add(newOption);
		}
		throwSeqDropdown.RefreshShownValue();
	}
	
	// Update is called once per frame
	void Update(){
		scene2LoadName = sceneNameDropdown.options[sceneNameDropdown.value].text;

		if(Input.GetKeyDown(starterKey) || Input.GetKeyDown(KeyCode.Alpha5)){
			StartGame(true);
		}
		
		if(Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6)){
			StartGame(false);
		}
		
		if(Input.GetKey(KeyCode.Q))
			Application.Quit();		
	}

	public void StartGame(bool trueGame){
		//enabling or not the pseudoGame
		starterData.pseudoGame = !trueGame;
		
		loadMsg.SetActive(true);
		
		starterData.throwSequence = throwSeqDropdown.options[throwSeqDropdown.value].text;

		try{
			starterData.SubjectID = int.Parse(subjetIDin.text);
		}
		catch{
			starterData.SubjectID = 0;
		}

		try{
			starterData.runNumber = int.Parse(runNumin.text);
		}
		catch{
			starterData.runNumber = 0;
		}

		Debug.Log("SubjectID: "+subjetIDin.text);
		Debug.Log("Run number ID: "+runNumin.text);
		Debug.Log("data SubjectID: "+starterData.SubjectID.ToString());
		Debug.Log("data Run number ID: "+starterData.runNumber.ToString());

		SceneManager.LoadScene(scene2LoadName);
	}
}

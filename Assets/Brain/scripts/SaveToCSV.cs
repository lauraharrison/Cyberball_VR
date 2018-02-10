using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class SaveToCSV : MonoBehaviour {
    public string subjectID;
    public string runNumber;
    
    //public GameObject player;
    public Transform headTransform;
    string[] columnHeaders;
    float timer;
    string path = @"LogFiles/";
	string filePath;

	starterData starterData;

	void Awake(){
		starterData = GetComponent<starterData>();
		subjectID = starterData.SubjectID.ToString();
		runNumber = starterData.runNumber.ToString();
		path = starterData.sequenceFilePath;
	}

    // Use this for initialization
    void Start () {
        timer = 0;
        if(subjectID == "" || runNumber == "")
        {
            Debug.LogError("Assign Subject ID and Run Number in the CSVLogFile GameObject");
        }
        //if(player == null)
        //{
        //    player = GameObject.FindWithTag("Player");
        //    if(player == null)
        //    {
        //        Debug.LogError("Assign Player Gameobject in the CSVLogFile GameObject");
        //    }
        //}
        if(headTransform == null)
        {
            Debug.LogError("Assign headTransform in the CSVLogFile GameObject");
        }
        filePath = path + "SubjectID_" + subjectID + "_" + "RunNumber_" + runNumber +".csv";

		//check if file exists before writing anything
		CheckIfExistingFile();

        columnHeaders = new string[] {
           "Time (seconds)",
           "Throw",
           "Gaze Time to other player",
           "Player Head Direction",
           //"\"Player Position (x,y)\""
        };        

        // Currently overrides any existing file
		// if (!File.Exists(filePath))
       // {
            // Create a file to write to.
            string createColumnHeaders = "";
            bool commaFlag = false;
            for(int i = 0; i < columnHeaders.Length; i++)
            {
                if (commaFlag)
                {
                    createColumnHeaders += ",";
                }
                else
                {
                    commaFlag = true;
                }
                createColumnHeaders += columnHeaders[i];
            }
            createColumnHeaders += Environment.NewLine;
			File.WriteAllText(filePath, createColumnHeaders);
       // }

        // This text is always added, making the file longer over time
        // if it is not deleted.
       // string appendText = "This is extra text" + Environment.NewLine;
		// File.AppendAllText(filePath, appendText);

        // Open the file to read from.
		//string readText = File.ReadAllText(filePath);
       // Console.WriteLine(readText);
    }
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;

    }

    public void WriteToFile(string ballToss, float gazeTime = 0)
    {
        string row = timer.ToString();

		row += ", " + ballToss;
		
		if(gazeTime > 0){
			row += ", gazeTime: "+gazeTime.ToString();
		}
		else{
			row += ", no gaze";
		}		
		
        //gaze direction
        row += ",";
        row += "\"("+headTransform.forward.x+"," + headTransform.forward.y+ "," + headTransform.forward.z + ")\"";
        //player position(x,y)
        //row += ",";
        //row += "\"(" + player.transform.position.x + "," + player.transform.position.y + ")\"";
        row += Environment.NewLine;
        Debug.Log("Writing: " + row);
		File.AppendAllText(filePath, row);
    }

	void CheckIfExistingFile(){
		string timeStamp = System.DateTime.Now.ToShortDateString().Replace ("/", "_");
		timeStamp += "_"+ System.DateTime.Now.ToShortTimeString().Replace(":","_").Replace(" ","_");
		filePath = path + "SubjectID_" + subjectID + "_" + "RunNumber_" + runNumber + "_"+timeStamp + ".csv";
		Debug.Log("<color=blue>logFile on: "+filePath+"</color>");
	}
}

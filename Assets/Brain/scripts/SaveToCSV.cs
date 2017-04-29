using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
public class SaveToCSV : MonoBehaviour {
    public string subjectID;
    public string runNumber;
    public int numEvents;
    public GameObject player;
    public GameObject camera;
    string[] columnHeaders;
    float timer;
    string path = @"LogFiles/";
    // Use this for initialization
    void Start () {
        timer = 0;
        if(subjectID == "" || runNumber == "")
        {
            Debug.LogError("Assign Subject ID and Run Number in the CSVLogFile GameObject");
        }
        if(player == null)
        {
            player = GameObject.FindWithTag("Player");
            if(player == null)
            {
                Debug.LogError("Assign Player Gameobject in the CSVLogFile GameObject");
            }
        }
        if(camera == null)
        {
            Debug.LogError("Assign Camera Gameobject in the CSVLogFile GameObject");
        }
        path += "SubjectID_" + subjectID + "_" + "RunNumber_" + runNumber +".csv";
        columnHeaders = new string[] {
           "Time (seconds)",
           "Player throws ball to Remy",
           "Player throws ball to Stefani",
           "Stefani throws ball to Player",
           "Stefani throws ball to Remy",
           "Remy throws ball to Player",
           "Remy throws ball to Stefani",
           "Stefani glances at Remy",
           "Remy glances at Stefani",
           "Gaze Time",
           "Gaze Direction",
           "\"Player Position (x,y)\""
        };
        //not including player position and gaze direction since those are appended at the end
        numEvents = columnHeaders.Length-3;

        // Currently overrides any existing file
       // if (!File.Exists(path))
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
            File.WriteAllText(path, createColumnHeaders);
       // }

        // This text is always added, making the file longer over time
        // if it is not deleted.
       // string appendText = "This is extra text" + Environment.NewLine;
       // File.AppendAllText(path, appendText);

        // Open the file to read from.
        //string readText = File.ReadAllText(path);
       // Console.WriteLine(readText);
    }
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;

    }

    public void WriteToFile(bool[] eventArr, float gazeTime = 0)
    {
        string row = timer.ToString();

        for (int i = 0; i < eventArr.Length; i++)
        {
            row += ",";
            //gaze time
            if (i == eventArr.Length-1)
            {
                if (eventArr[i])
                {
                    row += gazeTime.ToString();
                }
                else
                {
                    row += "N/A";
                }
            }
            //True,False for rest of events
            else
            {
                row += eventArr[i].ToString();
            }

        }
        //gaze direction
        row += ",";
        row += "\"("+camera.transform.forward.x+"," + camera.transform.forward.y+ "," + camera.transform.forward.z + ")\"";
        //player position(x,y)
        row += ",";
        row += "\"(" + player.transform.position.x + "," + player.transform.position.y + ")\"";
        row += Environment.NewLine;
        Debug.Log("Writing: " + row);
        File.AppendAllText(path, row);
    }
}

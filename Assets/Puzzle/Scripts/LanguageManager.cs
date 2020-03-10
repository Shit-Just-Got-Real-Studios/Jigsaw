using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class LanguageManager : MonoBehaviour {


    public TextAsset languageSheetCSV;
	string lang;

    string[] langStrings;
    int langIndex = 2;
    string[] stringList;
    public string difficultyString;
    public string timerString;

	public Text play;
	public Text adv;
	public Text areYouSure;
	public Text tutorialAlert;
	public Text cancel;
	public Text sort;
	public Text complete;
	public Text connectInternet1;
	public Text connectInternet2;
	public Text download;
	public Text easy;
	public Text epic;
	public Text guide;
	public Text hard;
	public Text med;
	public Text preview;
	public Text regroup;
	public Text start;
	public Text yes;
    public Text backToMenu;
    public Text pauseToMenu;



    void Start () {
        if (languageSheetCSV == null)
        {
            Debug.LogError("There is no CSV sheet for languages assigned in the controller inspector.");
        }

        lang = Application.systemLanguage.ToString().ToUpper();

        ReadCSVFile();
        SetLanguage();
    }

    void ReadCSVFile()
    {
        string[] txtLines = languageSheetCSV.text.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        string[] languageLine = txtLines[0].Split(new string[] { "\t" }, StringSplitOptions.None);

        for (int i = 0; i < languageLine.Length; i++)
        {
            if (languageLine[i].ToUpper().Trim() == lang)
            {
                langIndex = i;
                break;
            }
        }

        stringList = new string[txtLines.Length];

        for (int i = 0; i < txtLines.Length; i++)
        {
            stringList[i] = txtLines[i].Split(new string[] { "\t" }, StringSplitOptions.None)[langIndex].Trim();
            if (stringList[i].StartsWith("\""))
            {
                stringList[i] = stringList[i].Substring(1, stringList[i].Length - 2);
            }
            if (stringList[i].EndsWith("\""))
            {
                stringList[i] = stringList[i].Substring(0, stringList[i].Length - 1);
            }
        }
    }


	void SetLanguage(){
		play.text = stringList[1];
        areYouSure.text = stringList[2];
        tutorialAlert.text = stringList[3];
        cancel.text = stringList[4];
        sort.text = stringList[5];
        complete.text = stringList[6];
        connectInternet1.text = stringList[7];
        connectInternet2.text = stringList[7];
        download.text = stringList[8];
        easy.text = stringList[9];
        med.text = stringList[10];
        adv.text = stringList[11];
        hard.text = stringList[12];
        epic.text = stringList[13];
        guide.text = stringList[14];
        preview.text = stringList[15];
        regroup.text = stringList[16];
        start.text = stringList[17];
        yes.text = stringList[18];
        backToMenu.text = stringList[19];
        pauseToMenu.text = stringList[19];
        timerString = stringList[20];
        difficultyString = stringList[21];
    }

}

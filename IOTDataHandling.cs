using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[SerializeField]
[System.Serializable]
public class SensorData
{
    public int row;
    public float SensorValue;
    public String time;
}
[SerializeField]
[System.Serializable]
public class SensorDataList
{
    public string SensorName;
    public int Rows;
    public SensorData[] sensorDataList;
}
[SerializeField]
[System.Serializable]
public class SensorsList
{
    public string SensorsProjectName;
    public string Graph_X_Lable;
    public string Graph_Y_Lable;
    public int Rows;
    public SensorDataList[] sensorsList;
}

public class IOTDataHandling : MonoBehaviour
{

    public SensorsList Sensors;
    //string JASON_url = "http://seganapps.com/RemoteControl/IOT/HomeMarzdaran/SensorValuesReporter/SensorValues_Buffer.json";
    string JASON_url = "file://C:/Users/Hamed/AppData/LocalLow/DefaultCompany/IOT Sample/Sensors_Report.json";
    public string JSON_Address;

    public bool FirstJSONCreation = false;
    public bool PrintJSONFromWeb = false;
    public bool DataAchieved = false;
    int countertoAlert = 0;
    int countertoAlert_criteria = 5;
    public string Report;
    WaitForSeconds wts;


    void Start()
    {
        wts = new WaitForSeconds(1);
        //if (FirstJSONCreation)
        //    CreateFirstJASON();
        //else
        //    GetData();
    }

    public void GetData()
    {
        if (!FirstJSONCreation)
            StartCoroutine(LoadDataFromWeb(JASON_url));
        else
            CreateFirstJASON();

    }
    IEnumerator LoadDataFromWeb(string path)
    {
        JSON_Address = path;
        WWW www = new WWW(path);
        while (www == null)
        {
            countertoAlert++;
            yield return wts;
            if (countertoAlert > countertoAlert_criteria)
            {
                Report = "seconds to check: " + countertoAlert + "  /  ERROR to Connet to Web : " + www.error;
                Debug.Log(Report);
            }
        }
        yield return www;
        if (www.error == null)
        {
            //Debug.Log(www.text);
            ConvertTo_InternalClass(www.text, PrintJSONFromWeb);
        }
        else
        {
            Report = "ERROR to Connet to Web : " + www.error;
            Debug.Log(Report);
        }
    }
    public void ConvertTo_InternalClass(string content, bool printJSONFromWeb)
    {

        if (content != null && content != "")
        {
            string contents = content;
            SensorsList data = null;
            try
            {
                data = JsonUtility.FromJson<SensorsList>(contents);
            }
            catch (System.FormatException error)
            {
                Report = "ERROR to Connet to Web : " + error;
                Debug.Log(Report);
            }
            Sensors = data;
            Debug.Log("Data Achieved!");
            if (printJSONFromWeb)
            {
                string contentsfromWeb = JsonUtility.ToJson(Sensors, true);
                System.IO.File.WriteAllText(Application.persistentDataPath + "/TempReportJSON fromWeb.json", contentsfromWeb);
                Debug.Log("TempReportJSON fromWeb.json Saved");
            }
            DataAchieved = true;
            Report = "";
        }
        else
        {
            Report = "Achived Data from web is empty !";
            Debug.Log(Report);
        }
    }
    public void CreateFirstJASON()
    {
        int TotalList = 2;
        int SubtotlaList = 3;

        Sensors = new SensorsList();
        Sensors.SensorsProjectName = "Proejct Name ...";
        Sensors.Rows = TotalList;
        Sensors.sensorsList = new SensorDataList[Sensors.Rows];
        for (int i = 0; i < Sensors.Rows; i++)
        {
            Sensors.sensorsList[i] = new SensorDataList();
            Sensors.sensorsList[i].SensorName = "Sensor List Number = " + i.ToString();
            Sensors.sensorsList[i].Rows = SubtotlaList;
            Sensors.sensorsList[i].sensorDataList = new SensorData[Sensors.sensorsList[i].Rows];
            for (int j = 0; j < Sensors.sensorsList[i].Rows; j++)
            {
                Sensors.sensorsList[i].sensorDataList[j] = new SensorData();
                Sensors.sensorsList[i].sensorDataList[j].row = j;
                Sensors.sensorsList[i].sensorDataList[j].SensorValue = UnityEngine.Random.Range(-10, 40);
                Sensors.sensorsList[i].sensorDataList[j].time = DateTime.Now.ToString();
            }
        }
        string contents = JsonUtility.ToJson(Sensors, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/Sensors_Report.json", contents);
        Debug.Log("Sensors_Report.json Saved");
    }
}

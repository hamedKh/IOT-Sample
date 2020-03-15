using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

public enum MinutesValues
{
    in_a_Day = 24 * 60,
    in_a_Month_part1 = 24 * 60 * 31,
    in_a_Month_part2 = 24 * 60 * 30,
    in_a_Year = 24 * 60 * 365
}

public class JSON_to_Graph : MonoBehaviour
{

    public bool PlotDrawing;
    public IOTDataHandling IOTDataHandling;
    public GraphWindow Plotter;
    public int ReloadDataInSec = 5;
    float time;

    int TotalSensors;
    int LastSensorRows;
    public PersianCalendar pc = new PersianCalendar();

    void Start()
    {
        drawTemp();
    }
    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time >= ReloadDataInSec)
        {
            drawTemp();
            time = 0;
        }
    }
    public void drawTemp()
    {
        Plotter.Log.text = "Connecting ";
        IOTDataHandling.GetData();
        if (PlotDrawing)
        {
            StartCoroutine(dataAchievementCheck());

        }
    }

    IEnumerator dataAchievementCheck()
    {
        string dotting = "";
        while (!IOTDataHandling.DataAchieved)
        {
            dotting += ".";
            Plotter.Log.text = "Connecting " + dotting + "\n" + IOTDataHandling.Report;
            yield return null;
        }


        TotalSensors = IOTDataHandling.Sensors.sensorsList.Length;
        LastSensorRows = IOTDataHandling.Sensors.sensorsList[TotalSensors - 1].sensorDataList.Length;
        Plotter.GraphTilte.text = IOTDataHandling.Sensors.SensorsProjectName;
        //Plotter.Legends = new UnityEngine.UI.Text[1];
        //Plotter.Legends[0].text =
        Plotter.Log.text =
            "Data achived from : " + "\n" + IOTDataHandling.JSON_Address + "\n" +
            "LastData recieved at: " + IOTDataHandling.Sensors.sensorsList[TotalSensors - 1].sensorDataList[LastSensorRows - 1].time.ToString();

        StartCoroutine(DrawTempCurve());
    }

    IEnumerator DrawTempCurve()
    {
        Debug.Log("Start Drawing");

        Curve[] curves = new Curve[TotalSensors];

        curves = new Curve[TotalSensors];

        for (int i = 0; i < TotalSensors; i++)
        {
            int sensor_i_Rows = IOTDataHandling.Sensors.sensorsList[i].sensorDataList.Length;

            curves[i] = new Curve();
            curves[i].Name = IOTDataHandling.Sensors.sensorsList[i].SensorName;
            curves[i].X_List = new List<float>();
            curves[i].Y_List = new List<float>();
            curves[i].curveColor = Plotter.Colors[i];
            curves[i].CurveThickness = 1f;
            for (int j = 0; j < sensor_i_Rows; j++)
            {
                DateTime RelatedTime = System.DateTime.Parse(IOTDataHandling.Sensors.sensorsList[i].sensorDataList[j].time);

                // Convert time to minutes: 

                int TotalMinutesIn_aYear = RelatedTime.Hour * 60 + RelatedTime.Minute + (pc.GetDayOfYear(RelatedTime) - 1) * (24 * 60);
                string PersianDate = pc.GetYear(RelatedTime).ToString() + "/" + pc.GetMonth(RelatedTime) + "/" + pc.GetDayOfMonth(RelatedTime);
                //Debug.Log(i + "   " + j + "   " + RelatedTime + "    " + pc.GetYear(RelatedTime) + "   " + PersianDate + "   " + TotalMinutesIn_aYear);
                //curves[i].X_List.Add(IOTDataHandling.Sensors.sensorsList[i].sensorDataList[j].row);
                curves[i].X_List.Add(TotalMinutesIn_aYear);
                curves[i].Y_List.Add(IOTDataHandling.Sensors.sensorsList[i].sensorDataList[j].SensorValue);
                Plotter.GraphDrawn = false;

                //Debug.Log("Total Sensors Number : " + TotalSensors + "   / Sensor No. " + i + "  - value no. " + IOTDataHandling.Sensors.sensorsList[i].sensorDataList[j].row + " is: " + IOTDataHandling.Sensors.sensorsList[i].sensorDataList[j].SensorValue);
            }
        }
        Plotter.RefereshDiagram_andDrawByList(curves,IOTDataHandling.Sensors.Graph_X_Lable, IOTDataHandling.Sensors.Graph_Y_Lable);

        Debug.Log("End Drawing");
        IOTDataHandling.DataAchieved = false;

        while (!Plotter.GraphDrawn)
        {
            Plotter.Log.text = "Graph is Drawing";
            yield return null;
        }


        FitPlotter();
    }
    public void FitPlotter()
    {
        if (Plotter.FitDiagramByAchivedData)
        {
            string Min_x_IndexValue_s = "";
            string Min_x_IndexSensor_s = "";
            string Max_x_IndexValue_s = "";
            string Max_x_IndexSensor_s = "";

            string Min_y_IndexValue_s = "";
            string Min_y_IndexSensor_s = "";
            string Max_y_IndexValue_s = "";
            string Max_y_IndexSensor_s = "";

            //int[][] IndexMatrix = null;

            Plotter.scale_X = 1;
            Plotter.scale_Y = 1;
            Plotter.Origin = Vector3.zero;
            Plotter.RefreshDiagram();

            int TotalSensor = IOTDataHandling.Sensors.sensorsList.Length;
            float Min_x = TotalTimeInMinutes(TotalSensor - 1, IOTDataHandling.Sensors.sensorsList[TotalSensor - 1].sensorDataList.Length - 1);
            float Max_x = TotalTimeInMinutes(TotalSensor - 1, IOTDataHandling.Sensors.sensorsList[TotalSensor - 1].sensorDataList.Length - 1);
            float Min_y = IOTDataHandling.Sensors.sensorsList[TotalSensor - 1].sensorDataList[IOTDataHandling.Sensors.sensorsList[TotalSensor - 1].sensorDataList.Length - 1].SensorValue;
            float Max_y = IOTDataHandling.Sensors.sensorsList[TotalSensor - 1].sensorDataList[IOTDataHandling.Sensors.sensorsList[TotalSensor - 1].sensorDataList.Length - 1].SensorValue;

            for (int sensorNo = 0; sensorNo < TotalSensor; sensorNo++)
            {
                for (int SensorRow = 0; SensorRow < IOTDataHandling.Sensors.sensorsList[sensorNo].sensorDataList.Length; SensorRow++)
                {
                    if (Min_y >= IOTDataHandling.Sensors.sensorsList[sensorNo].sensorDataList[SensorRow].SensorValue)
                    {
                        Min_y = IOTDataHandling.Sensors.sensorsList[sensorNo].sensorDataList[SensorRow].SensorValue;
                        Min_y_IndexValue_s = SensorRow.ToString();
                        Min_y_IndexSensor_s = sensorNo.ToString();
                    }
                    if (Max_y <= IOTDataHandling.Sensors.sensorsList[sensorNo].sensorDataList[SensorRow].SensorValue)
                    {
                        Max_y = IOTDataHandling.Sensors.sensorsList[sensorNo].sensorDataList[SensorRow].SensorValue;
                        Max_y_IndexValue_s = SensorRow.ToString();
                        Max_y_IndexSensor_s = sensorNo.ToString();
                    }

                    if (Min_x >= TotalTimeInMinutes(sensorNo, SensorRow))
                    {
                        Min_x = TotalTimeInMinutes(sensorNo, SensorRow);
                        Min_x_IndexValue_s = SensorRow.ToString();
                        Min_x_IndexSensor_s = sensorNo.ToString();
                    }
                    if (Max_x <= TotalTimeInMinutes(sensorNo, SensorRow))
                    {
                        Max_x = TotalTimeInMinutes(sensorNo, SensorRow);
                        Max_x_IndexValue_s = SensorRow.ToString();
                        Max_x_IndexSensor_s = sensorNo.ToString();
                    }
                }
            }
            //Debug.Log("Min_x: " + Min_x + "  /  Max_x: " + Max_x + "  /  Min_y: " + Min_y + "  /  Max_y: " + Max_y);


            //for (int ii = 0; ii < FloatJSON.jSONReport.TempList.Length; ii++)
            //{
            //    if (float.Parse(FloatJSON.jSONReport.TempList[ii].temp) <= Min_y)
            //    {
            //        Min_y = float.Parse(FloatJSON.jSONReport.TempList[ii].temp);
            //        Min_y_Index_s = ii.ToString();
            //        //Debug.Log("Min_y_Index= " + ii);
            //    }


            //    if (float.Parse(FloatJSON.jSONReport.TempList[ii].temp) >= Max_y)
            //    {
            //        Max_y = float.Parse(FloatJSON.jSONReport.TempList[ii].temp);
            //        Max_y_Index_s = ii.ToString();
            //        //Debug.Log("Max_y_Index= " + ii);
            //    }
            //}

            int Min_x_IndexValue = int.Parse(Min_x_IndexValue_s);
            int Min_x_IndexSensor = int.Parse(Min_x_IndexSensor_s);
            int Max_x_IndexValue = int.Parse(Max_x_IndexValue_s);
            int Max_x_IndexSensor = int.Parse(Max_x_IndexSensor_s);

            int Min_y_IndexValue = int.Parse(Min_y_IndexValue_s);
            int Min_y_IndexSensor = int.Parse(Min_y_IndexSensor_s);
            int Max_y_IndexValue = int.Parse(Max_y_IndexValue_s);
            int Max_y_IndexSensor = int.Parse(Max_y_IndexSensor_s);


            //Debug.Log("Min_x_IndexValue: " + Min_x_IndexValue + "  /  Min_x_IndexSensor: " + Min_x_IndexSensor + "  /  Max_x_IndexValue: " + Max_x_IndexValue + "  /  Max_x_IndexSensor: " + Max_x_IndexSensor);
            //Debug.Log("Min_y_IndexValue: " + Min_y_IndexValue + "  /  Min_y_IndexSensor: " + Min_y_IndexSensor + "  /  Max_y_IndexValue: " + Max_y_IndexValue + "  /  Max_y_IndexSensor: " + Max_y_IndexSensor);


            float PlotterWindow_Length = Plotter.GraphWindowFrame.rect.size.x;
            float PlotterWindow_Hegith = Plotter.GraphWindowFrame.rect.size.y;

            bool b = false;
            float Margin_Up = Plotter.Margin_Up;
            float Margin_Down = Plotter.Margin_Down;
            float Margin_Left = Plotter.Margin_Left;
            float Margin_Right = Plotter.Margin_Right;

            float Strech_X = PlotterWindow_Length / (((Max_x + Margin_Right) - (Min_x - Margin_Left)) * Plotter.minorGrid_X * Plotter.globalScale);
            float Strech_Y = PlotterWindow_Hegith / (((Max_y + Margin_Up) - (b ? 0 : (Min_y - Margin_Down))) * Plotter.minorGrid_Y * Plotter.globalScale);

            Plotter.scale_X = ((int)Strech_X == 0 ? 1 : (int)Strech_X);
            Plotter.scale_Y = ((int)Strech_Y == 0 ? 1 : (int)Strech_Y);

            //Strech_X = Mathf.Ceil(Strech_X * 500) / 500;

            //Plotter.scale_X = Strech_X;
            //Plotter.scale_Y = Strech_Y;
            Plotter.RefreshDiagram();

            float Movement_X = -(Plotter.DotsStartPos[Min_x_IndexSensor].Points[Min_x_IndexValue].x + PlotterWindow_Length / 2) + Margin_Left * Plotter.minorGrid_X * Plotter.globalScale * Strech_X;
            float Movement_Y = -((b ? 0 : (Plotter.DotsStartPos[Min_y_IndexSensor].Points[Min_y_IndexValue].y)) + PlotterWindow_Hegith / 2) + Margin_Down * Plotter.minorGrid_Y * Plotter.globalScale * Strech_Y;

            //Debug.Log(Plotter.DotsStartPos[Min_y_Index].y + "    " + PlotterWindow_Hegith / 2);

            Plotter.Origin = Vector3.right * Movement_X + Vector3.up * Movement_Y;
            Plotter.RefreshDiagram();

            Debug.Log("Fit Drawing");


        }
    }
    int TotalTimeInMinutes(int SensorNo, int valueRow)
    {
        DateTime RelatedTime = System.DateTime.Parse(IOTDataHandling.Sensors.sensorsList[SensorNo].sensorDataList[valueRow].time);
        int TotalMinutesIn_aYear = RelatedTime.Hour * 60 + RelatedTime.Minute + (pc.GetDayOfYear(RelatedTime) - 1) * (24 * 60);
        return TotalMinutesIn_aYear;
    }
}

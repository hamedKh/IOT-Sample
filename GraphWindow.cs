using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System;

[SerializeField]
[System.Serializable]
public class Curve
{
    public string Name;
    public Color curveColor;
    public float CurveThickness;
    public List<float> X_List;
    public List<float> Y_List;
}
[SerializeField]
[System.Serializable]
public class PointsOnDiagram
{
    public int CurveNumber;
    public Vector3[] Points;
}
public class GraphWindow : MonoBehaviour
{

    public bool FitDiagramByAchivedData;
    public bool XGridsFades;
    public bool YGridsFades;

    #region Declaration

    [Space(2)]
    [Header("Diagrma Elements")]

    public RectTransform GraphWindowFrame;
    public GameObject LineRendererNode;
    public Transform GridsParent;
    public Transform GridsLablesParent;
    public Transform PointsParent;
    public Transform CurvesParent;
    public Transform LegendsParent;
    public RectTransform V_Axis;
    public RectTransform H_Axis;
    public RectTransform Dot;
    public RectTransform V_AxisGrid;
    public RectTransform H_AxisGrid;
    public Text AxisGridLable;
    public Text AxisLable_X;
    public Text AxisLable_Y;
    public Text Legend;
    public Text Log;
    public Text GraphTilte;
    public Text[] Legends;
    public Vector3 origin; // realated to bottom left point

    [Space(10)]
    [Header("Diagrma Properties")]

    public Color[] Colors;
    public int MaxSteps = 70;
    public int globalScale = 10;
    public float scale_X = 1;
    public float scale_Y = 1;
    public int minorGrid_X = 3;
    public int majorGrid_X = 15;
    public int minorGrid_Y = 1;
    public int majorGrid_Y = 10;

    public float Margin_Up = 0;
    public float Margin_Down = 0;
    public float Margin_Left = 0;
    public float Margin_Right = 0;

    public int minorGridTextSize = 10;
    public int majorGridTextSize = 14;
    public float minorGridWidth = 1.5f;
    public float majorGridWidth = 3f;
    public Color minorGridColor = Color.white * 0.4f;
    public Color majorGridColor = Color.white * 0.6f;
    public float MainAxisWidth = 4f;
    public float delayinDrawing = 0.1f;
    WaitForSeconds waitForSec;

    #endregion

    public Curve[] curves;
    public PointsOnDiagram[] DotsStartPos;
    public bool GraphDrawn;
    MinutesValues minutesValues = new MinutesValues();

    public Vector3 Origin
    {
        get
        {
            return origin;
        }

        set
        {
            origin = value;
        }
    }

    private void Start()
    {
        Log.text = "Connecting";
        //RefereshDiagram_andDrawByFunction();
    }

    public void DrawGraph_byList(Curve[] curves , String X_Axis_Lable, String Y_Axis_Lable)
    {
        AxisLable_X.text = X_Axis_Lable;
        AxisLable_Y.text = Y_Axis_Lable;
        GraphWindowFrame = this.GetComponent<RectTransform>();
        waitForSec = new WaitForSeconds(delayinDrawing);
        float minorGrid_X_ = minorGrid_X * scale_X;
        float majorGrid_X_ = majorGrid_X * scale_X;
        float minorGrid_Y_ = minorGrid_Y * scale_Y;
        float majorGrid_Y_ = majorGrid_Y * scale_Y;
        Vector3 GraphWindowFrameSize_Half = new Vector3(GraphWindowFrame.rect.size.x / 2, GraphWindowFrame.rect.size.y / 2, 0);
        Vector3 offset = new Vector3(GraphWindowFrame.rect.size.x / 2, GraphWindowFrame.rect.size.y / 2, 0);
        offset -= Vector3.right * Origin.x + Vector3.up * Origin.y;
        H_Axis.localPosition -= Vector3.up * offset.y;
        V_Axis.localPosition -= Vector3.right * offset.x;
        H_Axis.sizeDelta = Vector2.right * GraphWindowFrame.rect.size.x + Vector2.up * MainAxisWidth;
        V_Axis.sizeDelta = Vector2.up * GraphWindowFrame.rect.size.y + Vector2.right * MainAxisWidth;
        DrawGridLines(GraphWindowFrame.rect.size.x, V_Axis.localPosition.x, minorGrid_X_ * globalScale, 1, V_AxisGrid, "Grid X", majorGrid_X_ * globalScale, offset.x, scale_X, Vector2.right, Vector2.up, GraphWindowFrame.rect.size.y, GraphWindowFrameSize_Half);
        DrawGridLines(GraphWindowFrame.rect.size.x, V_Axis.localPosition.x, minorGrid_X_ * globalScale, -1, V_AxisGrid, "Grid X", majorGrid_X_ * globalScale, offset.x, scale_X, Vector2.right, Vector2.up, GraphWindowFrame.rect.size.y, GraphWindowFrameSize_Half);
        DrawGridLines(GraphWindowFrame.rect.size.y, H_Axis.localPosition.y, minorGrid_Y_ * globalScale, 1, H_AxisGrid, "Grid Y", majorGrid_Y_ * globalScale, offset.y, scale_Y, Vector2.up, Vector2.right, GraphWindowFrame.rect.size.x, GraphWindowFrameSize_Half);
        DrawGridLines(GraphWindowFrame.rect.size.y, H_Axis.localPosition.y, minorGrid_Y_ * globalScale, -1, H_AxisGrid, "Grid Y", majorGrid_Y_ * globalScale, offset.y, scale_Y, Vector2.up, Vector2.right, GraphWindowFrame.rect.size.x, GraphWindowFrameSize_Half);

        int curveNumbers = curves.Length;
        UILineRenderer[] LineRenderers = new UILineRenderer[curveNumbers];
        DotsStartPos = new PointsOnDiagram[curveNumbers];
        for (int l = 0; l < curveNumbers; l++)
        {
            GameObject Legend_ = Instantiate(Legend.gameObject, LegendsParent);
            Legend_.tag = "grid";
            Legend_.GetComponent<Text>().text = curves[l].Name;
            Legend_.GetComponent<Text>().color = curves[l].curveColor;
            Legend_.GetComponent<RectTransform>().localPosition = Vector2.right* Legend.GetComponent<RectTransform>().localPosition.x + Vector2.up * (Legend.GetComponent<RectTransform>().localPosition.y - l * 15);

            GameObject lineRenderer = GameObject.Instantiate(LineRendererNode, CurvesParent);
            lineRenderer.tag = "grid";
            LineRenderers[l] = lineRenderer.GetComponent<UILineRenderer>();
            LineRenderers[l].Points = new Vector2[curves[l].X_List.Count];
            LineRenderers[l].color = curves[l].curveColor;
            LineRenderers[l].lineThickness = curves[l].CurveThickness;

            DotsStartPos[l] = null;
            DotsStartPos[l] = new PointsOnDiagram();
            DotsStartPos[l].Points = new Vector3[curves[l].X_List.Count];

            GraphDrawn = false;
            float X_Value; float Y_Value;

            for (int i = 0; i < curves[l].X_List.Count; i++)
            {

                // Main Function ____________________________________________________

                X_Value = scale_X * globalScale * curves[l].X_List[i];
                Y_Value = scale_Y * globalScale * curves[l].Y_List[i];
                //Debug.Log("curve " + l + " / point " + i + "  /  X: " + curves[l].X_List[i] + "  /  Y: " + curves[l].Y_List[i]);
                // __________________________________________________________________

                DotsStartPos[l].Points[i] = Vector3.right * X_Value + Vector3.up * Y_Value;
                DotsStartPos[l].Points[i] -= offset;
                GameObject p = Instantiate(Dot.gameObject, PointsParent);
                p.GetComponent<RectTransform>().localPosition = DotsStartPos[l].Points[i];
                LineRenderers[l].Points[i] = Vector2.right * DotsStartPos[l].Points[i].x + Vector2.up * DotsStartPos[l].Points[i].y;
            }
        }

        GraphDrawn = true;

        #region Grids Drawing


        V_AxisGrid.localPosition = Vector2.right * -10000;
        H_AxisGrid.localPosition = Vector2.up * -10000;
        #endregion
    }
    void DrawGridLines(float graphWindowrectSiz_xy, float mainAxislocalPos_xy, float minorGrid_xy, int sign, RectTransform MainAxisRect, String gridName, float majorGrid_xy, float offset_xy, float scale_xy, Vector2 dir1, Vector2 dir2, float graphWindowrectSiz_yx, Vector2 GraphWindowFrameSize_half)
    {
        for (int step = 0; step < (graphWindowrectSiz_xy / 2 - (sign) * mainAxislocalPos_xy); step = step + (int)minorGrid_xy)
        {
            float w = ((((float)step / (float)majorGrid_xy) == Mathf.Ceil((float)step / (float)majorGrid_xy)) ? majorGridWidth : minorGridWidth);
            Color c = ((((float)step / (float)majorGrid_xy) == Mathf.Ceil((float)step / (float)majorGrid_xy)) ? majorGridColor : minorGridColor);

            bool CheckOutsideGrid_minus = false;
            bool CheckOutsideGrid_plus = false;

            if (dir1 == Vector2.right)
            {
                CheckOutsideGrid_minus = (-dir1 * (offset_xy - sign * step)).x < -GraphWindowFrameSize_half.x;
                CheckOutsideGrid_plus = (-dir1 * (offset_xy - sign * step)).x > GraphWindowFrameSize_half.x;
                //Debug.Log(CheckOutsideGrid_minus + " / " + CheckOutsideGrid_plus);
            }
            if (dir1 == Vector2.up)
            {
                CheckOutsideGrid_minus = (-dir1 * (offset_xy - sign * step)).y < -GraphWindowFrameSize_half.y;
                CheckOutsideGrid_plus = (-dir1 * (offset_xy - sign * step)).y > GraphWindowFrameSize_half.y;
                //Debug.Log(CheckOutsideGrid_minus + " / " + CheckOutsideGrid_plus);
            }

            if (!(CheckOutsideGrid_minus || CheckOutsideGrid_plus))
            {
                GameObject gridLine = Instantiate(MainAxisRect.gameObject, GridsParent);
                gridLine.tag = "grid";
                gridLine.name = gridName + ": " + (sign * step).ToString();
                gridLine.GetComponent<RectTransform>().sizeDelta = dir1 * w + dir2 * graphWindowrectSiz_yx;
                gridLine.GetComponent<RectTransform>().localPosition = -dir1 * (offset_xy - sign * step);

                if (dir1 == Vector2.right && XGridsFades)
                    gridLine.GetComponent<Image>().color = c / 2;
                else
                if (dir1 == Vector2.right && !XGridsFades)
                    gridLine.GetComponent<Image>().color = c;
                else
                if (dir1 == Vector2.up && YGridsFades)
                    gridLine.GetComponent<Image>().color = c / 2;
                else
                if (dir1 == Vector2.up && !YGridsFades)
                    gridLine.GetComponent<Image>().color = c;


                GameObject textGrid = Instantiate(AxisGridLable.gameObject, GridsLablesParent);
                textGrid.tag = "grid";
                bool MajorGrid = (((float)step / (float)majorGrid_xy) == Mathf.Ceil((float)step / (float)majorGrid_xy));
                int textsize = 0;

                if (dir2 == Vector2.up)
                {
                    if (MajorGrid)
                    {
                        textGrid.GetComponent<RectTransform>().localPosition = -dir1 * (offset_xy - sign * step) - dir2 * ((graphWindowrectSiz_yx / 2 + 10) + 20);
                        float xAxis_Value = (sign * ((float)step / globalScale) / scale_xy);
                        if (xAxis_Value % (int)MinutesValues.in_a_Day == 0)
                        {
                            if (xAxis_Value <= 6 * (int)MinutesValues.in_a_Month_part1)  // In First 6 Month in HijriShamsi Calender
                            {
                                int day = (((int)xAxis_Value / (int)MinutesValues.in_a_Day) % 31) + 1;
                                int month = ((int)Mathf.Ceil((int)xAxis_Value / (int)MinutesValues.in_a_Month_part1)) + 1;
                                textGrid.GetComponent<Text>().text = day.ToString() + " / " + month.ToString();
                            }
                            else // In Second 6 Month in HijriShamsi Calender
                            {
                                int day = ((((int)xAxis_Value - (6 * (int)MinutesValues.in_a_Month_part1)) / (int)MinutesValues.in_a_Day) % 30) + 1;
                                int month = 6 + (int)Mathf.Ceil(((int)xAxis_Value - (6 * (int)MinutesValues.in_a_Month_part1)) / (int)MinutesValues.in_a_Month_part2) + 1;
                                textGrid.GetComponent<Text>().text = day.ToString() + " / " + month.ToString();
                            }
                        }
                        else
                        {
                            textGrid.GetComponent<Text>().text = (sign * ((float)step / globalScale) / scale_xy).ToString();
                        }
                        textGrid.GetComponent<Text>().fontSize = majorGridTextSize;
                    }
                    else
                    {
                        float xAxis_Value = (sign * ((float)step / globalScale) / scale_xy);
                        int residualMinutes = 0;
                        int H = 0;
                        int M = 0;
                        if (xAxis_Value <= 6 * (int)MinutesValues.in_a_Month_part1)  // In First 6 Month in HijriShamsi Calender
                        {
                            residualMinutes = (int)(xAxis_Value % (int)MinutesValues.in_a_Month_part1);
                            int intermediate = (residualMinutes % (int)MinutesValues.in_a_Day);
                            H = (int)Mathf.Ceil(intermediate / 60);
                            M = intermediate - H * 60;

                            textGrid.GetComponent<Text>().text = H.ToString() + " : " + M.ToString();
                        }
                        else // In Second 6 Month in HijriShamsi Calender
                        {
                            residualMinutes = (int)(xAxis_Value - ((int)MinutesValues.in_a_Month_part1 * 6) % (int)MinutesValues.in_a_Month_part2);
                            int intermediate = (residualMinutes % (int)MinutesValues.in_a_Day);
                            H = (int)Mathf.Ceil(intermediate / 60);
                            M = intermediate - H * 60;

                            textGrid.GetComponent<Text>().text = H.ToString() + " : " + M.ToString();
                        }
                        //Debug.Log((sign * ((float)step / globalScale) / scale_xy).ToString() + "      " + xAxis_Value + "      " + residualMinutes + "  " + H + ":" + M);
                        //textGrid.GetComponent<Text>().text = (sign * ((float)step / globalScale) / scale_xy).ToString();
                        textGrid.GetComponent<RectTransform>().localPosition = -dir1 * (offset_xy - sign * step) - dir2 * (graphWindowrectSiz_yx / 2 + 10);
                        textGrid.GetComponent<Text>().fontSize = minorGridTextSize;
                    }
                }
                else
                {
                    if (MajorGrid)
                    {
                        textGrid.GetComponent<Text>().fontSize = majorGridTextSize;
                    }
                    else
                    {
                        textGrid.GetComponent<Text>().fontSize = minorGridTextSize;
                    }
                    textGrid.GetComponent<Text>().text = (sign * ((float)step / globalScale) / scale_xy).ToString();
                    textGrid.GetComponent<RectTransform>().localPosition = -dir1 * (offset_xy - sign * step) - dir2 * (graphWindowrectSiz_yx / 2 + 10);
                }
            }
        }
    }
    public void DrawGraph_byFunction()
    {
        /*
                #region Drawing Setup
                GraphWindowFrame = this.GetComponent<RectTransform>();
                UIlineRenderer = this.GetComponent<UILineRenderer>();
                int minorGrid_X_ = minorGrid_X * scale_X;
                int majorGrid_X_ = majorGrid_X * scale_X;
                int minorGrid_Y_ = minorGrid_Y * scale_Y;
                int majorGrid_Y_ = majorGrid_Y * scale_Y;

                Vector3 DotsStartPos; float X_Value; float Y_Value;
                Vector3 offset = new Vector3(GraphWindowFrame.rect.size.x / 2, GraphWindowFrame.rect.size.y / 2, 0);
                offset -= Vector3.right * Origin.x + Vector3.up * Origin.y;
                H_Axis.localPosition -= Vector3.up * offset.y;
                V_Axis.localPosition -= Vector3.right * offset.x;
                H_Axis.sizeDelta = Vector2.right * GraphWindowFrame.rect.size.x + Vector2.up * MainAxisWidth;
                V_Axis.sizeDelta = Vector2.up * GraphWindowFrame.rect.size.y + Vector2.right * MainAxisWidth;
                UIlineRenderer.Points = new Vector2[MaxSteps];

                #endregion

                DrawGridLines(GraphWindowFrame.rect.size.x, V_Axis.localPosition.x, minorGrid_X_ * globalScale, 1, V_AxisGrid, "Grid X", majorGrid_X_ * globalScale, offset.x, scale_X, Vector2.right, Vector2.up, GraphWindowFrame.rect.size.y);
                DrawGridLines(GraphWindowFrame.rect.size.x, V_Axis.localPosition.x, minorGrid_X_ * globalScale, -1, V_AxisGrid, "Grid X", majorGrid_X_ * globalScale, offset.x, scale_X, Vector2.right, Vector2.up, GraphWindowFrame.rect.size.y);
                DrawGridLines(GraphWindowFrame.rect.size.y, H_Axis.localPosition.y, minorGrid_Y_ * globalScale, 1, H_AxisGrid, "Grid Y", majorGrid_Y_ * globalScale, offset.y, scale_Y, Vector2.up, Vector2.right, GraphWindowFrame.rect.size.x);
                DrawGridLines(GraphWindowFrame.rect.size.y, H_Axis.localPosition.y, minorGrid_Y_ * globalScale, -1, H_AxisGrid, "Grid Y", majorGrid_Y_ * globalScale, offset.y, scale_Y, Vector2.up, Vector2.right, GraphWindowFrame.rect.size.x);

                #region Main Curve Drawing

                for (int i = 0; i < MaxSteps; i++)
                {

                    // Main Function ____________________________________________________

                    X_Value = scale_X * globalScale * i;
                    Y_Value = scale_Y * globalScale * Mathf.Tan((float)i * (float)Math.PI / 180);//(Mathf.Sqrt(i) + Mathf.Tan(i * i));

                    // __________________________________________________________________

                    DotsStartPos = Vector3.right * X_Value + Vector3.up * Y_Value;
                    DotsStartPos -= offset;
                    GameObject p = Instantiate(Dot.gameObject, this.transform);
                    p.GetComponent<RectTransform>().localPosition = DotsStartPos;
                    UIlineRenderer.Points[i] = Vector2.right * DotsStartPos.x + Vector2.up * DotsStartPos.y;
                }

                #endregion

                #region Grids Drawing


                #region OldCodes
                //for (int gx = 0; gx < (GraphWindowFrame.rect.size.x / 2 - V_Axis.localPosition.x); gx += minorGrid_X)
                //{
                //    GameObject grid_x = Instantiate(V_AxisGrid.gameObject, this.transform);
                //    grid_x.tag = "grid";
                //    grid_x.name = "Grid X: " + gx.ToString();
                //    float w = ((((float)gx / (float)majorGrid_X) == Mathf.Ceil((float)gx / (float)majorGrid_X)) ? majorGridWidth : minorGridWidth); ;
                //    Color c = ((((float)gx / (float)majorGrid_X) == Mathf.Ceil((float)gx / (float)majorGrid_X)) ? majorGridColor : minorGridColor);
                //    grid_x.GetComponent<RectTransform>().sizeDelta = Vector2.right * w + Vector2.up * GraphWindowFrame.rect.size.y;
                //    grid_x.GetComponent<RectTransform>().localPosition = -Vector2.right * (offset.x - gx);
                //    grid_x.GetComponent<Image>().color = c;
                //}
                //for (int gx = 0; gx < (GraphWindowFrame.rect.size.x / 2 + V_Axis.localPosition.x); gx += minorGrid_X)
                //{
                //    GameObject grid_x = Instantiate(V_AxisGrid.gameObject, this.transform);
                //    grid_x.tag = "grid";
                //    grid_x.name = "Grid X: " + (-gx).ToString();
                //    float w = ((((float)gx / (float)majorGrid_X) == Mathf.Ceil((float)gx / (float)majorGrid_X)) ? majorGridWidth : minorGridWidth); ;
                //    Color c = ((((float)gx / (float)majorGrid_X) == Mathf.Ceil((float)gx / (float)majorGrid_X)) ? majorGridColor : minorGridColor);
                //    grid_x.GetComponent<RectTransform>().sizeDelta = Vector2.right * w + Vector2.up * GraphWindowFrame.rect.size.y;
                //    grid_x.GetComponent<RectTransform>().localPosition = -Vector2.right * (offset.x + gx);
                //    grid_x.GetComponent<Image>().color = c;
                //}

                //for (int gy = 0; gy < (GraphWindowFrame.rect.size.y / 2 - H_Axis.localPosition.y); gy += minorGrid_Y)
                //{
                //    GameObject grid_y = Instantiate(H_AxisGrid.gameObject, this.transform);
                //    grid_y.tag = "grid";
                //    grid_y.name = "Grid Y: " + gy.ToString();
                //    float w = ((((float)gy / (float)majorGrid_Y) == Mathf.Ceil((float)gy / (float)majorGrid_Y)) ? majorGridWidth : minorGridWidth);
                //    Color c = ((((float)gy / (float)majorGrid_X) == Mathf.Ceil((float)gy / (float)majorGrid_X)) ? majorGridColor : minorGridColor);
                //    grid_y.GetComponent<RectTransform>().sizeDelta = Vector2.up * w + Vector2.right * GraphWindowFrame.rect.size.x;
                //    grid_y.GetComponent<RectTransform>().localPosition = -Vector2.up * (offset.y - gy);
                //    grid_y.GetComponent<Image>().color = c;
                //}
                #endregion
                V_AxisGrid.localPosition = Vector2.right * -10000;
                H_AxisGrid.localPosition = Vector2.up * -10000;
                #endregion
                */
    }
    void CleanGraph()
    {
        RectTransform[] T = GetComponentsInChildren<RectTransform>();
        for (int r = 0; r < T.Length; r++)
        {
            if (T[r].gameObject.tag == "dot" || T[r].gameObject.tag == "grid")
            {
                Destroy(T[r].gameObject);
            }
        }
        H_Axis.localPosition = Vector3.zero;
        V_Axis.localPosition = Vector3.zero;
    }
    public void RefereshDiagram_andDrawByFunction()
    {
        CleanGraph();
        DrawGraph_byFunction();
    }
    public void RefereshDiagram_andDrawByList(Curve[] c, String X_Axis_Lable, String Y_Axis_Lable)
    {
        curves = c;
        CleanGraph();
        AxisLable_X.text = X_Axis_Lable;
        AxisLable_Y.text = Y_Axis_Lable;
        DrawGraph_byList(curves, X_Axis_Lable, Y_Axis_Lable);
    }
    public void RefreshDiagram()
    {
        CleanGraph();
        DrawGraph_byList(curves , AxisLable_X.text, AxisLable_Y.text);
    }
}

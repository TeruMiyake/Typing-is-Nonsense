using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XCharts;

public class ResultChartsSubManager : MonoBehaviour
{
    // ResultDetailSubManager からセット
    TrialData trialData;
    KeyBindDicts dicts;

    // グラフ
    public GameObject CharBasedCPSLineChartObject;
    public GameObject CharBasedSPCLineChartObject;
    public GameObject TimeBasedScatterChartObject;
    LineChart charBasedCPSLineChart;
    LineChart charBasedSPCLineChart;
    ScatterChart timeBasedScatterChart;

    // cps で表示するか、spc で表示するか
    bool isPreferringCPS = true;

    void Awake()
    {
        charBasedCPSLineChart = CharBasedCPSLineChartObject.GetComponent<LineChart>();
        charBasedSPCLineChart = CharBasedSPCLineChartObject.GetComponent<LineChart>();
        timeBasedScatterChart = TimeBasedScatterChartObject.GetComponent<ScatterChart>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// グラフの初期設定（シーン読み込み時に一度だけ実行）
    /// チャートの表示設定に加え、Serie(s) の作成まで行う
    /// </summary>
    public void InitializeCharts()
    {
        InitializeCharBasedCPSLineChart();
        InitializeCharBasedSPCLineChart();
        InitializeTimeBasedScatterChart();
    }
    void InitializeCharBasedCPSLineChart()
    {
        LineChart _chart = charBasedCPSLineChart;
        // 表示設定
        _chart.RemoveData();
        _chart.title.show = false;
        _chart.tooltip.show = true;
        _chart.legend.show = true;

        _chart.dataZoom.enable = true;
        _chart.dataZoom.minShowNum = 10;

        _chart.grid.left = 35;
        _chart.grid.right = 35;
        _chart.grid.top = 10;
        _chart.grid.bottom = 25;

        _chart.xAxes[0].type = Axis.AxisType.Category;
        _chart.xAxes[0].splitNumber = 10;
        _chart.xAxes[0].show = true;

        _chart.xAxes[1].show = false;

        _chart.yAxes[0].type = Axis.AxisType.Value;
        _chart.yAxes[0].show = true;

        _chart.yAxes[0].minMaxType = Axis.AxisMinMaxType.Default;

        _chart.yAxes[1].show = true;


        // Serie(s) 設定
        var percharSerie = charBasedCPSLineChart.AddSerie(SerieType.Line, "CPS (each char)");
        percharSerie.xAxisIndex = 0;
        percharSerie.yAxisIndex = 0;
        // これを true にしていると、データ数が多いときに表示が軽量化されてしまう
        percharSerie.large = false; 
        percharSerie.symbol.type = SerieSymbolType.Circle;
        percharSerie.symbol.size = 2.5f;
        percharSerie.symbol.selectedSize = 4.5f;
        percharSerie.lineType = LineType.Normal;
        percharSerie.lineStyle.width = 0.1f;

        percharSerie.symbol.forceShowLast = true;

        var avgSerie = charBasedCPSLineChart.AddSerie(SerieType.Line, "CPS (average)");
        avgSerie.xAxisIndex = 0;
        avgSerie.yAxisIndex = 0;
        // これを true にしていると、データ数が多いときに表示が軽量化されてしまう
        avgSerie.large = false;
        avgSerie.symbol.type = SerieSymbolType.Circle;
        avgSerie.symbol.size = 2f;
        avgSerie.symbol.selectedSize = 3f;
        avgSerie.lineType = LineType.Normal;
        avgSerie.lineStyle.width = 0.8f;

        //var missaccumulatedSerie = charBasedLineChart.AddSerie(SerieType.Line);
        //missaccumulatedSerie.xAxisIndex = 0;
        //missaccumulatedSerie.yAxisIndex = 1;
        //missaccumulatedSerie.symbol.type = SerieSymbolType.Triangle;
        //missaccumulatedSerie.symbol.size = 2.0f;
        //missaccumulatedSerie.symbol.selectedSize = 3.0f;
        //missaccumulatedSerie.lineStyle.type = LineStyle.Type.DashDotDot;
        //missaccumulatedSerie.lineStyle.width = 0.3f;
        //missaccumulatedSerie.lineStyle.opacity = 0.7f;
        //missaccumulatedSerie.markLine.show = false;
    }
    void InitializeCharBasedSPCLineChart()
    {
        LineChart _chart = charBasedSPCLineChart;
        // 表示設定
        _chart.RemoveData();
        _chart.title.show = false;
        _chart.tooltip.show = true;
        _chart.legend.show = true;

        _chart.dataZoom.enable = true;
        _chart.dataZoom.minShowNum = 10;

        _chart.grid.left = 35;
        _chart.grid.right = 35;
        _chart.grid.top = 10;
        _chart.grid.bottom = 25;

        _chart.xAxes[0].type = Axis.AxisType.Category;
        _chart.xAxes[0].splitNumber = 10;
        _chart.xAxes[0].show = true;

        _chart.xAxes[1].show = false;

        _chart.yAxes[0].type = Axis.AxisType.Value;
        _chart.yAxes[0].show = true;

        _chart.yAxes[0].minMaxType = Axis.AxisMinMaxType.Default;

        _chart.yAxes[1].show = true;


        // Serie(s) 設定
        var percharSerie = _chart.AddSerie(SerieType.Line, "SPC (each char)");
        percharSerie.xAxisIndex = 0;
        percharSerie.yAxisIndex = 0;
        // これを true にしていると、データ数が多いときに表示が軽量化されてしまう
        percharSerie.large = false;
        percharSerie.symbol.type = SerieSymbolType.Circle;
        percharSerie.symbol.size = 2.5f;
        percharSerie.symbol.selectedSize = 4.5f;
        percharSerie.lineType = LineType.Normal;
        percharSerie.lineStyle.width = 0.1f;

        percharSerie.symbol.forceShowLast = true;

        var avgSerie = _chart.AddSerie(SerieType.Line, "SPC (average)");
        avgSerie.xAxisIndex = 0;
        avgSerie.yAxisIndex = 0;
        // これを true にしていると、データ数が多いときに表示が軽量化されてしまう
        avgSerie.large = false;
        avgSerie.symbol.type = SerieSymbolType.Circle;
        avgSerie.symbol.size = 2f;
        avgSerie.symbol.selectedSize = 3f;
        avgSerie.lineType = LineType.Normal;
        avgSerie.lineStyle.width = 0.8f;

    }
    void InitializeTimeBasedScatterChart()
    {
        timeBasedScatterChart.RemoveData();
        timeBasedScatterChart.title.show = false;
        timeBasedScatterChart.tooltip.show = true;
        timeBasedScatterChart.legend.show = true;
        timeBasedScatterChart.dataZoom.enable = true;

        timeBasedScatterChart.xAxes[0].show = true;
        timeBasedScatterChart.xAxes[1].show = false;
        timeBasedScatterChart.yAxes[0].show = true;
        timeBasedScatterChart.yAxes[1].show = true;

        timeBasedScatterChart.xAxes[0].type = Axis.AxisType.Value;
        timeBasedScatterChart.xAxes[0].splitNumber = 10;
    }
    public void SetDataToCharBasedCPSLineChart()
    {
        var _chart = charBasedCPSLineChart;

        // Serie(s) は消さずにデータクリア
        _chart.ClearData();

        var percharSerieIndex = 0;
        var avgSerieIndex = 1;
        var missaccumulatedSerieIndex = 2;

        // 軸データ設定
        for (int i = 0; i < 360; i++)
        {
            _chart.AddXAxisData($"{dicts.ToChar_FromCharID(trialData.TaskCharIDs[i])}({i + 1})", 0);
        }
        // なんか境界でバグるので入れてみたが、意味があるかどうか
        _chart.AddXAxisData(" ", 0);

        for (int i = 0; i < 360; i++)
        {
            int charidx = i + 1;
            // per char
            MilliSecond mspc_i_ms = trialData.CorrectKeyTime[charidx] - trialData.CorrectKeyTime[charidx - 1];
            double cps_i_double = mspc_i_ms.ToCPS(1);
            _chart.AddData(percharSerieIndex, cps_i_double);

            // all time avg.
            MilliSecond ms_alltime_i_ms = trialData.CorrectKeyTime[charidx];
            double cps_alltime_double = ms_alltime_i_ms.ToCPS(charidx);
            _chart.AddData(avgSerieIndex, cps_alltime_double);
        }
    }
    public void SetDataToCharBasedSPCLineChart()
    {
        var _chart = charBasedSPCLineChart;

        // Serie(s) は消さずにデータクリア
        _chart.ClearData();

        var percharSerieIndex = 0;
        var avgSerieIndex = 1;
        var missaccumulatedSerieIndex = 2;

        // 軸データ設定
        for (int i = 0; i < 360; i++)
        {
            _chart.AddXAxisData($"{dicts.ToChar_FromCharID(trialData.TaskCharIDs[i])}({i + 1})", 0);
        }
        // なんか境界でバグるので入れてみたが、意味があるかどうか
        _chart.AddXAxisData(" ", 0);

        for (int i = 0; i < 360; i++)
        {
            int charidx = i + 1;
            // per char
            MilliSecond mspc_i_ms = trialData.CorrectKeyTime[charidx] - trialData.CorrectKeyTime[charidx - 1];
            double spc_i_double = mspc_i_ms.ToSPC(1);
            _chart.AddData(percharSerieIndex, spc_i_double);

            // all time avg.
            MilliSecond ms_alltime_i_ms = trialData.CorrectKeyTime[charidx];
            double spc_alltime_double = ms_alltime_i_ms.ToSPC(charidx);
            _chart.AddData(avgSerieIndex, spc_alltime_double);
        }

        //scatter.AddSerie(SerieType.Scatter);
        //Serie scatterserie = scatter.series.GetSerie(0);
        //Tooltip scatterTooltip = scatter.tooltip;
        //scatterTooltip.AddSerieDataIndex(0, 3);

        //scatterserie.symbol.size = 3;
        //scatterserie.symbol.selectedSize = 5;

        //for (int i = 0; i < trialData.TypedKeys; i++)
        //{
        //    float keytime = (float)(trialData.CorrectKeyTime[i + 1] - trialData.CorrectKeyTime[i]) / 1000;
        //    scatter.AddData(0, keytime);
        //}

        //Serie line2 = scatter.AddSerie(SerieType.Line);
        //List<double> data = new List<double>();
        //for (int i = 0; i < trialData.TypedKeys; i++)
        //{
        //    double keytime = (double)(trialData.CorrectKeyTime[i + 1] - trialData.CorrectKeyTime[i]) / 1000;
        //    data.Add(keytime);
        //}
        //line2.AddData(data);

    }
    public void SetDataToTimeBasedScatterChart()
    {
        // Serie(s) は消さずにデータクリア
        timeBasedScatterChart.ClearData();

    }
    public void ShowPreferredCharBasedLineChart()
    {
        HideBothCharBasedLineCharts();
        if (isPreferringCPS) ShowCharBasedCPSLineChart();
        else ShowCharBasedSPCLineChart();
    }
    public void HideBothCharBasedLineCharts()
    {
        HideCharBasedCPSLineChart();
        HideCharBasedSPCLineChart();
    }
    public void ShowCharBasedCPSLineChart()
    {
        CharBasedCPSLineChartObject.SetActive(true);
    }
    public void HideCharBasedCPSLineChart()
    {
        CharBasedCPSLineChartObject.SetActive(false);
    }
    public void ShowCharBasedSPCLineChart()
    {
        CharBasedSPCLineChartObject.SetActive(true);
    }
    public void HideCharBasedSPCLineChart()
    {
        CharBasedSPCLineChartObject.SetActive(false);
    }
    public void ShowTimeBasedScatterChart()
    {
        TimeBasedScatterChartObject.SetActive(true);
    }
    public void HideTimeBasedScatterChart()
    {
        TimeBasedScatterChartObject.SetActive(false);
    }
    /// <summary>
    /// CPS 表示にするかどうかを選択し、異なる CharBased チャートが表示されていれば切り替える
    /// </summary>
    /// <param name="prefers"></param>
    public void SetPreferringCPS(bool prefers)
    {
        isPreferringCPS = prefers;
        if (isPreferringCPS && CharBasedSPCLineChartObject.activeSelf)
        {
            ShowCharBasedCPSLineChart();
            HideCharBasedSPCLineChart();
        }
        if (!isPreferringCPS && CharBasedCPSLineChartObject.activeSelf)
        {
            ShowCharBasedSPCLineChart();
            HideCharBasedCPSLineChart();
        }
    }
    /// <summary>
    /// ResultDetailSubManager から呼び出すチャート起動メソッド
    /// trialData, dicts はここで生成
    /// </summary>
    /// <param name="_trialData"></param>
    public void ShowResultChart(TrialData _trialData)
    {
        trialData = _trialData;
        dicts = new KeyBindDicts(trialData.GetKeyBind());

        SetDataToCharBasedCPSLineChart();
        SetDataToCharBasedSPCLineChart();
        SetDataToTimeBasedScatterChart();
    
        HideTimeBasedScatterChart();
        ShowPreferredCharBasedLineChart();
    }
}

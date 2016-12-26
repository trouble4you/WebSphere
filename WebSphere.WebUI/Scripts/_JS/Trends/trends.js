var global_data = new Array();
var global_current_trends = new Array();
var global_current_trends_ids = new Array();
var global_auto_update = false;
var plot;
var placeholder;
var somePlot;
var last_data = null;
var y_from_zero = true;

var latestPosition = null;
var updateLegendTimeout = null;

var trends_colors = ['#006699', '#FFA500', '#00CCCC', '#990066', '#CC0033', '#000066', '#663333', '#009999', '#991199',
'#660000', '#006633', '#009933', '#990000', '#669900', '#666600', '#333333', '#000000', '#00FF7F', '#FF0000'];


function Print( ) {
 
var canvas = plot.getCanvas();
    var img = canvas.toDataURL("image/png");
    var popup = window.open();
    popup.document.write('<p style="text-align: center;">Тренд по объекту   ' + object_name + ' (' + object_type_str + '") в период   с  ' + GetDateLabel_Fromat2(start_date) + '  по  ' + GetDateLabel_Fromat2(end_date) + '  </p><p style="text-align: center;"><img style:    src="' + img + '"/></p>');
    popup.focus(); //required for IE
    popup.print();
    
}

function page_UpdateTrend(obj) {
    if (start_date != 0)
        document.getElementById("datetimepicker_start").value = GetDateLabel_Fromat2(start_date);

    if (end_date != 0)
        document.getElementById("datetimepicker_end").value = GetDateLabel_Fromat2(end_date);

    //var obj = JSON.parse(data);
    last_data = obj;

    document.getElementById("loading").style.display = 'none';

    start_date = obj.date_min_sec;
    end_date = obj.date_max_sec;

    var all_data = {};

    for (var i = 0; i < obj.trends.length; i++)
    {
        all_data[obj.trends[i].id] = [];
        var points = obj.trends[i].Points;
        for (var j = 0; j < points.length; j++) {
            all_data[obj.trends[i].id].push([points[j].dt - 21600, points[j].v]);
    }
}

global_current_trends_ids = [];
    global_data = [];
    for (key in all_data) {
        global_data.push({ label: signals_array[key], data: all_data[key] });
        global_current_trends_ids.push(key);
    }
    plot.setData(global_data);

    SetAxe_Y();

    var r_min = obj.date_min_sec ;
    var r_max = obj.date_max_sec ;

    var axes = plot.getAxes();
    axes.xaxis.options.min = r_min ;
    axes.xaxis.options.max = r_max ;
    axes.xaxis.options.ticks = [];
    //axes.xaxis.options.timeformat = "%y_%m_%d";

    for (var i = 0; i <= 10; i++) {
        var t1 = (r_max - r_min)/10;
        var t2 = t1 * i; 
        var r_v = r_min + (t1 * i);
        axes.xaxis.options.ticks.push([r_v , GetDateTotrend(r_v)]);
    } 
    plot.setupGrid();
    plot.draw();
    plot.clearSelection();

    //alert(plot.getOptions().colors);

    for (key in signals_array) {
        var el = document.getElementById("trend_color_" + key);
        if (el) el.style.backgroundColor = "transparent";
    }

    for (var i = 0; i < global_current_trends_ids.length; i++) {
        var signal_id = global_current_trends_ids[i];
        //document.getElementById("trend_color_" + signal_id).style.backgroundColor = plot.getOptions().colors[i];

        document.getElementById("trend_color_" + signal_id).style.backgroundColor = trends_colors[i % trends_colors.length];
    }
}

function no_UpdateTrend(obj) {
    document.getElementById("loading").style.display = 'none';

    ErrMessage("Не удалось загрузить тренды");
}

function SetAxe_Y() {

    if (y_from_zero) {
        var v_min = 0;
        var v_max = last_data.max;
        var axes_max = v_max + (v_max - v_min) * 0.15;
        var axes_min = v_min;

        var axes = plot.getAxes();
        axes.yaxis.options.min = axes_min;
        axes.yaxis.options.max = axes_max;

    }
    else {
        var v_min = last_data.min;
        var v_max = last_data.max;

        var axes_min = v_min - (v_max - v_min) * 0.15;
        var axes_max = v_max + (v_max - v_min) * 0.15;

        if (v_max == v_min && v_max > 0) {
            axes_min = v_max * (-1.15);
            axes_max = v_max * 1.15;
        }
        else if (v_max == v_min && v_max < 0) {
            axes_min = v_max * (1.15);
            axes_max = v_max * (-1.15);
        }
        else if (v_max == v_min && v_max == 0) {
            axes_min = -100;
            axes_max = 100;
        }

        var axes = plot.getAxes();
        axes.yaxis.options.min = axes_min;
        axes.yaxis.options.max = axes_max;
    }
}

function Switch_Y() {
    y_from_zero = !y_from_zero;
    SetAxe_Y();

    plot.setupGrid();
    plot.draw();
}


function UpdateTrends() {
    var loading = document.getElementById("loading");
    if (loading != null) loading.style.display = 'block';
    var reset_cach = "1";

    var auto_update_range = "0";

    if (global_auto_update) {
        auto_update_range = 5 * 60;
    }

 
    var _d = {
        "start_date": parseInt(start_date),
        "end_date": parseInt(end_date),
        "signal_id": GetStrCurrentTrends(),
        "reset_cach": reset_cach,
        "object_id": global_object_id,
        "auto_update": auto_update_range
    }; 
    $.ajax({ type: "POST", url: "/api/Trend/GetData", data: _d, async: true, success: page_UpdateTrend, Error: no_UpdateTrend }); 
}

function updateLegend() {
    updateLegendTimeout = null;
    var pos = latestPosition;
    var axes = plot.getAxes();
    if (pos.x < axes.xaxis.min || pos.x > axes.xaxis.max || pos.y < axes.yaxis.min || pos.y > axes.yaxis.max)
        return;

    var azaza = GetDateLabel_Fromat2(pos.x);
    var i,j; 
    var dataset = plot.getData();
    for (i = 0; i < dataset.length; i++) {
        var series = dataset[i];
        for (j = 0; j < series.data.length; ++j)
            if (series.data[j][0] > pos.x)
                break;

        var y;
        var p1 = series.data[j - 1];
        var p2 = series.data[j];

        if (p1 === undefined || p2 === undefined) {
            continue;
        }
        else {
            if (p1 == null)
                y = p2[1];
            else if (p2 == null)
                y = p1[1];
            else
                y = parseFloat(p1[1]) + parseFloat((p2[1] - p1[1]) * (pos.x - p1[0]) / (p2[0] - p1[0]));

            y = parseFloat(y);

            //current_values += ", ";
            //current_values += signals_array[global_current_trends_ids[i]] + ": " + y.toFixed(2);
            var id = global_current_trends_ids[i];
            document.getElementById("trend_value_" + id).value = y.toFixed(2);
        }
    }
    //alert(global_current_trends_ids);
    document.getElementById("y_value").value = azaza;
}

$(function () {
    if (start_date != 0)
        document.getElementById("datetimepicker_start").value = GetDateLabel_Fromat2(start_date);

    if (end_date != 0)
        document.getElementById("datetimepicker_end").value = GetDateLabel_Fromat2(end_date);
    UpdateTrends();
    jQuery('#datetimepicker_end').datetimepicker({ lang: 'ru', format: 'Y-m-d H:i' });
    jQuery('#datetimepicker_start').datetimepicker({ lang: 'ru', format: 'Y-m-d H:i' });




    var trend_1 = [];

    placeholder = $("#placeholder");

    placeholder.bind("plothover", function (event, pos, item) {
        latestPosition = pos;
        if (!updateLegendTimeout)
            updateLegendTimeout = setTimeout(updateLegend, 50);
    });

    placeholder.bind("plotselected", function (event, ranges) {
        start_date = parseInt(ranges.xaxis.from);
        end_date = parseInt(ranges.xaxis.to);
        global_auto_update = false;
        UpdateTrends();
    });

    $("input").change(function () {
        options.canvas = $(this).is(":checked");
        $.plot("#placeholder", data, options);
    });

    //var somePlot = $.plot("#placeholder", [{ data: global_data }], options);
    plot = $.plot(placeholder, [{ data: trend_1 }], {

        canvas: true,
        series: { lines: { show: true }, points: { show: false } },
       
        xaxis: {
            mode: "time" 
        },
        crosshair: { mode: "x" },
        grid: { hoverable: true, clickable: true, backgroundColor: { colors: ["#fff", "#fff"] } },
        yaxis: { min: 0, max: 0 },
        selection: { mode: "x" },
        colors: trends_colors, 
        legend: { position: "sw" }
    });

});

function AddTrendToPlot(id) {
    for (var i = 0; i < global_current_trends.length; i++)
        if (global_current_trends[i] == id)
            return;

    global_current_trends.push(id);
}

function RemoveTrendFromPlot(id) {
    for (var i = 0; i < global_current_trends.length; i++)
        if (global_current_trends[i] == id) {
            global_current_trends.splice(i, 1);
            return;
        }
}

function GetStrCurrentTrends() {
    var first = true;
    var r = "{";
    for (var i = 0; i < global_current_trends.length; i++) {
        if (!first) r += ",";
        r += global_current_trends[i];
        first = false;
    }
    r += "}";
    return r;
}

function CheckBoxClick(_signal_id) {
    var el = document.getElementById("trend_" + _signal_id);
    if (el.checked)
        AddTrendToPlot(_signal_id);
    else
        RemoveTrendFromPlot(_signal_id);

    UpdateTrends();
}
function AddClick(_signal_id) {
    AddTrendToPlot(_signal_id);
    UpdateTrends();
}
function DelClick(_signal_id, p) {
    var el = document.getElementById("del_trend_" + p);
    el.style.display = "none";
    var tr = document.getElementById("trend_" + p + "s").value = 0;
    var tr = document.getElementById("trend_value_" + _signal_id).value = "";
    RemoveTrendFromPlot(_signal_id);
    UpdateTrends();
}

function RangeMinus() {
    global_auto_update = false;
    var d = (end_date - start_date) * 0.25;
    start_date += d;
    end_date -= d;
    UpdateTrends();
}

function RangePlus() {
    global_auto_update = false;
    var d = (end_date - start_date) * 0.25;
    start_date -= d;
    end_date += d;
    UpdateTrends();
}

function RangeLeft() {
    global_auto_update = false;
    var d = (end_date - start_date) * 0.25;
    start_date -= d;
    end_date -= d;
    UpdateTrends();
}

function RangeRight() {
    global_auto_update = false;
    var d = (end_date - start_date) * 0.25;
    start_date += d;
    end_date += d;
    UpdateTrends();
}

function RangeCustom(s) {
    global_auto_update = false;
    var d = (end_date + start_date) / 2;
    start_date = d - s / 2;
    end_date = d + s / 2;
    UpdateTrends();
}



function PeriodRange() {
    global_auto_update = false;

    var sd = document.getElementById("datetimepicker_start").value;
    var ed = document.getElementById("datetimepicker_end").value;

    var d1 = GetDateFromDateTimePicker(sd);
    var d2 = GetDateFromDateTimePicker(ed);

    start_date = GetDatetimeFromDate(d1);
    end_date = GetDatetimeFromDate(d2);
    UpdateTrends();

}

function Last5Mins(s) {
    global_auto_update = true;
    UpdateTrends();
}

function Range5min() { RangeCustom(5 * 60); }
function Range30min() { RangeCustom(30 * 60); }
function Range60mins() { RangeCustom(60 * 60); }
function RangeDay() { RangeCustom(24 * 60 * 60); }
function RangeWeek() { RangeCustom(7 * 24 * 60 * 60); }
function RangeMonth() { RangeCustom(31 * 7 * 24 * 60 * 60); }


 
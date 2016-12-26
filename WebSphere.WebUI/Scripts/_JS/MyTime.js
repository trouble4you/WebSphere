function LeapYear(year) {
    return (year % 4 == 0) && ((year % 100 != 0) || (year % 400 == 0));
}

function BetweenYears(sy, fy) {
    var ret = 0;
    for (var i = sy; i < fy; i++) {
        ret += (LeapYear(i) ? 366 : 365);
    }
    return ret;
}

function SinceStartYear(year, month, day) {
    var ret = 0;
    var m = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    for (var i = 1; i < month; i++) {
        ret += m[i - 1];
    }
    if (month > 2 && LeapYear(year))
        ret += 1;
    ret += day - 1;
    return ret;
}

function GetBetweenDayCount(sd, sm, sy, fd, fm, fy) {
    var ret = 0;
    if (sy == fy) {
        ret = SinceStartYear(fy, fm, fd) - SinceStartYear(sy, sm, sd);
    }
    else if (fy > sy) {
        ret = BetweenYears(sy, fy) + SinceStartYear(fy, fm, fd) - SinceStartYear(sy, sm, sd);
    }
    return ret;
}

function GetDatetimeFromDate(dt)
{
    if (dt == null)
        return 0;
    var y = dt.getFullYear();
    var m = dt.getMonth() + 1;
    var d = dt.getDate();
    var h = dt.getHours();
    var mt = dt.getMinutes();
    var s = dt.getSeconds();
    var dc = GetBetweenDayCount(1, 1, 1970, d, m, y);
    return (dc * 86400) + (h * 3600) + (mt * 60) + s;
}

function GetDateLabel_UTC(sec)
{
    var d1 = new Date(Date.UTC(1970, 0, 1, 0, 0, 0));
    var d = new Date(d1.getTime() + sec * 1000);
    var r = "";
    if (d.getUTCDate() < 10) r += "0";
    r += d.getUTCDate() + ".";
    if (d.getUTCMonth() < 10) r += "0";
    r += (parseInt(d.getUTCMonth()) + 1) + ".";
    r += d.getUTCFullYear() + " ";
    if (d.getUTCHours() < 10) r += "0";
    r += d.getUTCHours() + ":";
    if (d.getUTCMinutes() < 10) r += "0";
    r += d.getUTCMinutes() + ":";
    if (d.getUTCSeconds() < 10) r += "0";
    r += d.getUTCSeconds();
    return r;
}

function GetDateLabel(sec)
{
    // Принимает количество секунд с начала века
    // Возвращает дату в текстовом виде: 1970-02-03 04:05:06
    var d1 = new Date(Date.UTC(1970, 0, 1, 0, 0, 0));
    var d = new Date(d1.getTime() + sec * 1000);
    var r = "";
    if (d.getDate() < 10) r += "0";
    r += d.getDate() + ".";
    if (d.getMonth() < 10) r += "0";
    r += (parseInt(d.getMonth()) + 1) + ".";
    r += d.getFullYear() + " ";
    if (d.getHours() < 10) r += "0";
    r += d.getHours() + ":";
    if (d.getMinutes() < 10) r += "0";
    r += d.getMinutes() + ":";
    if (d.getSeconds() < 10) r += "0";
    r += d.getSeconds();
    return r;
}

function GetDateLabel2(sec)
{ 
    // Возвращает дату в текстовом виде: 1970-02-03 04:05:06
    var d1 = new Date(Date.UTC(1970, 0, 1, 0, 0, 0));
    var d = new Date(d1.getTime() + sec * 1000);
    var r = "";
    r += d.getFullYear() + "-";
    if (d.getMonth() < 10) r += "0";
    r += (parseInt(d.getMonth()) + 1) + "-";
    if (d.getDate() < 10) r += "0";
    r += d.getDate() + "<br />";
    if (d.getHours() < 10) r += "0";
    r += d.getHours() + ":";
    if (d.getMinutes() < 10) r += "0";
    r += d.getMinutes() + ":";
    if (d.getSeconds() < 10) r += "0";
    r += d.getSeconds();
    return r;
}
function GetDateTotrend(sec)
{ 
    // Возвращает дату в текстовом виде: 1970-02-03 04:05:06
    var d1 = new Date(1970, 0, 1, 0, 0, 0);
    var d = new Date(d1.getTime() + sec * 1000);
    var r = "";
    r += d.getFullYear() + "-";
    if (d.getMonth() < 10) r += "0";
    r += (parseInt(d.getMonth()) + 1) + "-";
    if (d.getDate() < 10) r += "0";
    r += d.getDate() + "<br />";
    if (d.getHours() < 10) r += "0";
    r += d.getHours() + ":";
    if (d.getMinutes() < 10) r += "0";
    r += d.getMinutes() + ":";
    if (d.getSeconds() < 10) r += "0";
    r += d.getSeconds();
    return r;
}

function Get_DateStr(d)
{
    var r = "";
    r += d.getFullYear() + "-";
    if (d.getMonth() < 10) r += "0";
    r += (parseInt(d.getMonth()) + 1) + "-";
    if (d.getDate() < 10) r += "0";
    r += d.getDate() + " ";
    if (d.getHours() < 10) r += "0";
    r += d.getHours() + ":";
    if (d.getMinutes() < 10) r += "0";
    r += d.getMinutes() + ":";
    if (d.getSeconds() < 10) r += "0";
    r += d.getSeconds();
    return r;
}

function GetDateLabel_Fromat2(sec)
{
    // Принимает количество секунд 
    // Возвращает дату в текстовом виде: 1970-02-03 04:05:06  
    var d = new Date(1970, 0, 1, 0, 0, 0); 
    d.setSeconds(sec);
    var r = "";
    r += d.getFullYear() + "-";
    if (d.getMonth() < 9) r += "0";
    r += (parseInt(d.getMonth()) + 1) + "-";
    if (d.getDate() < 10) r += "0";
    r += d.getDate() + " ";
    if (d.getHours() < 10) r += "0";
    r += d.getHours() + ":";
    if (d.getMinutes() < 10) r += "0";
    r += d.getMinutes();
    return r;
}

function GetOnlyDateStrLabel_FromDate(d)
{
    var r = "";
    r += d.getFullYear() + "-";
    if (d.getMonth() < 10) r += "0";
    r += (parseInt(d.getMonth()) + 1) + "-";
    if (d.getDate() < 10) r += "0";
    r += d.getDate();
    return r;
}

function GetOnlyTimeStrLabel_FromDate(d)
{
    var r = "";
    if (d.getHours() < 10) r += "0";
    r += d.getHours() + ":";
    if (d.getMinutes() < 10) r += "0";
    r += d.getMinutes()+ ":";
    if (d.getSeconds() < 10) r += "0";
    r += d.getSeconds();
    return r;
}

function GetDateFromDateTimePicker(v)
{
    var v2 = v.replace(" ", "T");
    v2 += "Z";
    var d1 = new Date(v2);
    if (isNaN(d1.getTime()))
        return null;
    var offset = d1.getTimezoneOffset();
    var d2 = new Date(d1.getTime() + offset * 60 * 1000);
    return d2;
}

function Get_LocalDateStr_FromUTCDateStr(v)
{
    var v2 = v + "Z";
    var d1 = new Date(v2);
    if (isNaN(d1.getTime()))
        return "";
    var str = Get_DateStr(d1);
    return str;
}

function Get_Date_FromUTCDateStr(v)
{
    var v2 = v + "Z";
    var d1 = new Date(v2);
    return d1;
}
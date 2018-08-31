
function discreteChange(z, tagval, blink) {
    tagval = ParseToBool(tagval);
    var svg = document.getElementById('svg');
    var element = svg.contentDocument.getElementById(z);
    var list = element.getElementsByClassName("isValue");

    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList.add('ViewOff'); list[i].classList.remove('ViewOn');
    }
    var object;

    if (tagval == null) object = 'isDefault';
    else if (tagval) object = 'isTrue';
    else if (!tagval) object = 'isFalse';

    try {
        var el = element.getElementsByClassName(object)[0];
        el.classList.add('ViewOn'); el.classList.remove('ViewOff');
        if (blink) el_true.classList.add('DoVision');
    }
    catch (err) {
        console.log('error');
    }
}


function discreteColor(z, tagval, blink) {
    tagval = ParseToBool(tagval);
    var svg = document.getElementById('svg');
    var element = svg.contentDocument.getElementById(z);
    try {
         
        var list = element.getElementsByClassName("stateColor")[0];
        //list.classList = 'stateColor ';
        var z=element.getAttribute("" + tagval + "");
        if (tagval == null) list.classList.add(list.getAttribute("default"));
        else list.classList.add(z);
        if (true) list.classList.add('DoVision');
        else list.classList.remove('DoVision');
    }
    catch (err) {
        console.log('error');
    }
}

function analogChange(z, tagval, imit, al)
{
        if (al == null) al = 0; 
        var svg = document.getElementById('svg');
        var group = svg.contentDocument.getElementById(z);
        var states = ['unk','good', 'warn', 'bad'];  
        var color = group.getElementsByClassName("stateColor")[0]; 
        color.classList = 'stateColor ' + states[3]; 
        
        var text = group.getElementsByClassName("stateText")[0]; 
        text.innerHTML = tagval; 
        var imitation = group.getElementsByClassName("gImitation")[0]; 
        imitation.classList.remove('ViewOff');
        if (false)imitation.classList.add('ViewOn');  
}


function discreteChange_old(z, tagval, blink) {
    tagval = ParseToBool(tagval);
    var zd = '#' + z;
    
    var svg = document.getElementById('svg');
    var element = svg.contentDocument.getElementById(z);
    //var element =  svg.select(zd);
    try {
        // var el_true = element.select("#" + element.node.getElementsByClassName("isTrue")[0].id);
        //el_true.attr("display", tagval ? "block" : "none");

        var el_true = element.getElementsByClassName("isTrue")[0];
        //el_true.setAttribute("display", tagval ? "block" : "none");

        el_true.classList.add('ViewOn');
        el_true.classList.add('DoVision');

        // if (true) el_false.classList.add('DoClose');     
        /*  var list = element.node.getElementsByClassName("isTrue");
          for (var i = 0, len = list.length; i < len; i++) {
              list[i].classList.add('DoVision');  
          }
          */
    }
    catch (err) {
        console.log('error');
    }
    /*
        try {
            var el_false = element.select("#" + element.node.getElementsByClassName("isFalse")[0].id);
            el_false.attr("display", (tagval != null && !tagval) ? "block" : "none"); 
            
            // if (true) el_false.classList.add('DoClose'); 
            var list = element.node.getElementsByClassName("isFalse");
            for (var i = 0, len = list.length; i < len; i++) {
                list[i].classList.add('DoVision');
            } 
        }
        catch (err) { }
        try {
            var el_def = element.select("#" + element.node.getElementsByClassName("isDefault")[0].id);
            el_def.attr("display", (tagval == null) ? "block" : "none");
    
           
           // if (true) el_def.classList.add('DoClose');
        }
        catch (err) { }
    
        */
}


function Calculate(tagname) {
    var n2;
    var n1;
    if (tagname == "dFIT") {
        n1 = parseFloat(GM_OPC_VALS["fit01.RSG"]);
        n2 = parseFloat(GM_OPC_VALS["fit02.RPG"]);
        return !isNaN((n1 - n2)) ? (n1 - n2) : '---';
    }
    else
        if (tagname == "dFITn") {
            n1 = parseFloat(GM_OPC_VALS["fit01.nRSG"]);
            n2 = parseFloat(GM_OPC_VALS["fit02.nRPG"]);
            return !isNaN((n1 - n2)) ? (n1 - n2) : '---';
        }
        else
            if (tagname == "ProcFITn") {
                n1 = parseFloat(GM_OPC_VALS["fit01.nRSG"]);
                n2 = parseFloat(GM_OPC_VALS["fit02.nRPG"]);
                return !isNaN(parseInt((n1 - n2) / n1 * 100)) ? (parseInt((n1 - n2) / n1 * 100)) : '---';
            }
}
function SetTextpech(id, val) {

    var al = 1;
    if (val == 6 || val == 8 || val == 9 || val == 10) al = 2;
    else if (val == 1 || val == 2 || val == 3 || val == 4 || val == 5) al = 0;
    else al = 1;
    var svg = Snap('#svg');
    var group = svg.select('#' + id);
    var states = ['good', 'warn', 'bad'];
    var list = group.node.getElementsByClassName("stateColor");

    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[al];
    }

}
// --- ANIMATE ---

// тревоги
function animateEK_AlarmLevel(i, tagsname) {
    var id = '#' + i,
        hiAlarm = ParseToBool(GM_OPC_VALS[tagsname + "HiAlarmLevel"]),
        hiWork = ParseToBool(GM_OPC_VALS[tagsname + "HiWorkLevel"]),
        lowAlarm = ParseToBool(GM_OPC_VALS[tagsname + "LowAlarmLevel"]);
    showEK_AlarmLevel(id, hiAlarm, hiWork, lowAlarm);
}

// pump
function animateEK_Pump(i, tagsname) {
    var obj = '#' + i,
        on = ParseToBool(GM_OPC_VALS[tagsname + "Pump_On"]),
        power = ParseToBool(GM_OPC_VALS[tagsname + "Pump_Power"]),
        auto = ParseToBool(GM_OPC_VALS[tagsname + "AutoMan"]);

    showPumpState(obj, on);
    showPumpPowerState(obj, power);
    showPumpModeState(obj, auto);
}

// тревоги
function showEK_AlarmLevel(id, ha, hw, la) {
    var state = 0,
        svg = Snap('#svg'),
        element = svg.select(id),
        statesColor = ['good', 'bad', 'bad', 'bad'],
        statesText = ['Норма', 'ВАУ', 'ВРУ', 'НАУ'],
        list = '';

    ha ? state = 1 : '';
    hw ? state = 2 : '';
    la ? state = 3 : '';

    var list = element.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }

    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }
}

// --- SHOW ---
// pump (состояние, напряжение, режим)
function showPumpState(obj, x) { // состояние
    var state = 0,
        svg = Snap('#svg'),
        element = svg.select(obj),
        group = '',
        statesColor = ['unk', 'off', 'on'],
        statesText = ['Неопределено', 'Выключен', 'Включен'],
        list = '';

    if (x != null) {
        !x ? state = 1 : '';
        x ? state = 2 : '';
    }
    list = element.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }

    group = element.select("#" + element.node.getElementsByClassName("gState")[0].id);

    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }
}
function showPumpPowerState(obj, x) { // напряжение
    var state = 0,
        svg = Snap('#svg'),
        element = svg.select(obj),
        group = '',
        statesColor = ['unk', 'bad', 'good'],
        list = '';

    if (x != null) {
        !x ? state = 1 : '';
        x ? state = 2 : '';
    }
    group = element.select("#" + element.node.getElementsByClassName("gStatePower")[0].id);

    list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }
}
function showPumpModeState(obj, x) { // режим
    var state = 0,
        svg = Snap('#svg'),
        element = svg.select(obj),
        group = '',
        statesColor = ['unk', 'local', 'remote'],
        statesText = ['?', 'М', 'Д'],
        list = '';

    if (x != null) {
        !x ? state = 1 : '';
        x ? state = 2 : '';
    }

    group = element.select("#" + element.node.getElementsByClassName("gStateMode")[0].id);

    list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }

    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }
}
/****************************RK****************************************/
function animateRK(z, tagsname) {
    var zd = '#' + z;
    var alarm = ParseToBool(GM_OPC_VALS[tagsname + "_Fail"]);
    var position = ProcFl(GM_OPC_VALS[tagsname + "_Position"], 2);
    var power = ParseToBool(GM_OPC_VALS[tagsname + "_Ready"]);
    var controlremote = ParseToBool(GM_OPC_VALS[tagsname + "_Remote"]);
    var controllocal = ParseToBool(GM_OPC_VALS[tagsname + "_Local"]);
    showRKReadyState(zd, power, alarm);
    showRKRegState(zd, controlremote, controllocal);
    if (z == "RKPanel") showRKPositionState(zd, position);

    //showZDState(zd, alarm, isopen, isclosed, opening, closing);
}
function AnimateRKPanel(tagsname) {
    animateRK("KLPanel", tagsname);
}
function showRKReadyState(rk, power, alarm) {
    var state = 0;
    if (alarm == true) state = 1;
    else if (power == true) state = 2;
    var svg = Snap('#svg');
    var element = svg.select(rk);
    var group = element.select("#" + element.node.getElementsByClassName("gStateAlarm")[0].id);
    var statesColor = ['unk', 'bad', 'good'];
    var statesText = ['---', 'Не готов', 'Готов'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];

        list = group.node.getElementsByClassName("stateText");
        for (var i = 0, len = list.length; i < len; i++) {
            list[i].innerHTML = statesText[state];
        }

    }
}
function showRKPositionState(ek, position) {

    var state = 0;
    var svg = Snap('#svg');
    var element = svg.select(ek);
    var group = element.select("#" + element.node.getElementsByClassName("gPosition")[0].id);

    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = "Открыто на " + position + "%";
    }

}
function showRKRegState(ek, dregim, mregim) {

    var state = 0;
    if (dregim == mregim) state = 0;
    else if (mregim == true) state = 1;
    else if (dregim == true) state = 2;
    var svg = Snap('#svg');
    var element = svg.select(ek);
    var group = element.select("#" + element.node.getElementsByClassName("gStateMode")[0].id);

    var statesColor = ['unk', 'remote', 'local'];
    var statesText = ['?', 'М', 'Д'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}
function showRKPanel(st, dispname, tagname) {
    var svg = Snap('#svg');
    var el = svg.select("#RKPanel");
    var states = ['none', 'block'];
    if (st == 0) {
        el.attr("display", states[0]);
        ControlRKName = null;
    } else {
        el.attr("display", states[1]);
        if (arguments.length > 2) {
            var gname = el.select("#gName");
            gname.node.innerHTML = dispname;
            ControlRKName = tagname;
            AnimateRKPanel("rk1");
        }
    }
}
function cmdRKPosition() {
    var val = prompt();

    if (val != '') { OpcWrite("rk1_SetPosition", val + "0"); OkMessage("Команда обработана"); }
    else ErrMessage("Команда не обработана");
}

/****************************RK****************************************/
/****************************KL****************************************/
function animateKL(z, tagsname) {
    if (tagsname == "rk1") {
        var zd = '#' + z;
        var alarm = ParseToBool(GM_OPC_VALS[tagsname + "_Fail"]);
        //var position = ProcFl(GM_OPC_VALS[tagsname + "_Position"], 2);
        var power = ParseToBool(GM_OPC_VALS[tagsname + "_Ready"]);
        //var controlremote = ParseToBool(GM_OPC_VALS[tagsname + "_Remote"]);
        //var controllocal = ParseToBool(GM_OPC_VALS[tagsname + "_Local"]);
        // var alarm = true;
        var position = true;
        var power = true;
        var mode = 2;
        showKLState(zd, 0//GM_OPC_VALS[tagsname + ".State"]
            , GM_OPC_VALS[tagsname + "_Fail"]);
        if (ParseToBool(GM_OPC_VALS[tagsname + "_Remote"]) && ParseToBool(GM_OPC_VALS[tagsname + ".ModeAuto"]))
            mode = 1;
        else if (ParseToBool(GM_OPC_VALS[tagsname + "_Remote"]))
            mode = 0;
        else mode = 2;

        showRKReadyState(zd, power, alarm);
        //showRKRegState(zd, controlremote, controllocal); 

        showKLPIDState(ProcFl(GM_OPC_VALS[tagsname + "_Position"], 3),
            ProcFl(GM_OPC_VALS[tagsname + "_SetPosition"], 3) / 10,
            ProcFl(GM_OPC_VALS[tagsname + ".PID.Ustav"], 3),
            ProcFl(GM_OPC_VALS[tagsname + ".PID.Zona"], 3),
            ProcFl(GM_OPC_VALS[tagsname + ".PID.Kp"], 3),
            ProcFl(GM_OPC_VALS[tagsname + ".PID.Ip"], 3),
            ProcFl(GM_OPC_VALS[tagsname + ".PID.Dp"], 3));
        showKLRegState(zd, mode);
        //showZDState(zd, alarm, isopen, isclosed, opening, closing);
    } else {

        var zd = '#' + z;
        showKLState(zd, GM_OPC_VALS[tagsname + ".State"], GM_OPC_VALS[tagsname + ".Alarm"]);
        if (zd != "#KLPanel")
            showKLPosition(zd, ProcFl(GM_OPC_VALS[tagsname + ".Position"], 2));
        //showKLState(zd, "2", true);
        if (zd == "#KLPanel")
            //showKLPIDState(1, 2, 3, 4, 5, 6, 7);
            showKLPIDState(ProcFl(GM_OPC_VALS[tagsname + ".Position"], 3),
                ProcFl(GM_OPC_VALS[tagsname + ".SetPosition"], 3),
                ProcFl(GM_OPC_VALS[tagsname + ".PID.ActUstav"], 3),
                ProcFl(GM_OPC_VALS[tagsname + ".PID.ActZona"], 3),
                ProcFl(GM_OPC_VALS[tagsname + ".PID.ActKp"], 3),
                ProcFl(GM_OPC_VALS[tagsname + ".PID.ActIp"], 3),
                ProcFl(GM_OPC_VALS[tagsname + ".PID.ActDp"], 3));
        showKLRegState(zd, GM_OPC_VALS[tagsname + ".ControlState"]);
        //showKLRegState(zd, 2);

    }

}
function AnimateKLPanel(tagsname) {
    animateKL("KLPanel", tagsname);
}
function showKLPanel(st, dispname, tagname) {
    var svg = Snap('#svg');
    var el = svg.select("#KLPanel");
    var states = ['none', 'block'];
    if (st == 0) {
        el.attr("display", states[0]);
        KLPanelTagsName = null;
    } else {
        el.attr("display", states[1]);
        if (arguments.length > 2) {
            var gname = el.select("#gKLName");
            gname.node.innerHTML = dispname;
            KLPanelTagsName = tagname;
            AnimateKLPanel(tagname);
        }
    }
}
function cmdKLValve(cmd) {
    var tag, tag2, val;
    if (KLPanelTagsName == "rk1") {
        if (cmd == 8) { tag = KLPanelTagsName + ".ModeAuto"; val = true; }
        else if (cmd == 9) { tag = KLPanelTagsName + ".ModeAuto"; val = false; }
        else if (cmd == 101) { tag = KLPanelTagsName + ".PID.Ustav"; val = prompt(); }
        else if (cmd == 103) { tag = KLPanelTagsName + ".PID.Kp"; val = prompt(); }
        else if (cmd == 104) { tag = KLPanelTagsName + ".PID.Ip"; val = prompt(); }
        else if (cmd == 110) { tag = KLPanelTagsName + "_SetPosition"; val = prompt(); val = val + '0' }
        if (val != '' || val != null) { OpcWrite(tag, val); OkMessage("Команда обработана"); }
        else ErrMessage("Команда не обработана");
    } else {
        if (cmd == 0)
            tag = KLPanelTagsName + ".Control.Kvit";
        else if (cmd == 101) tag = KLPanelTagsName + ".PID.Ustav";
        else if (cmd == 1) tag = KLPanelTagsName + ".PID.SetUstav";
        else if (cmd == 102) tag = KLPanelTagsName + ".PID.Zona";
        else if (cmd == 2) tag = KLPanelTagsName + ".PID.SetZona";
        else if (cmd == 103) tag = KLPanelTagsName + ".PID.Kp";
        else if (cmd == 3) tag = KLPanelTagsName + ".PID.SetKp";
        else if (cmd == 104) tag = KLPanelTagsName + ".PID.Ip";
        else if (cmd == 4) tag = KLPanelTagsName + ".PID.SetIp";
        else if (cmd == 105) tag = KLPanelTagsName + ".PID.Dp";
        else if (cmd == 5) tag = KLPanelTagsName + ".PID.SetDp";
        else if (cmd == 7) tag = KLPanelTagsName + ".Position";
        else if (cmd == 8) tag = KLPanelTagsName + ".Control.ModeAuto";
        else if (cmd == 9) tag = KLPanelTagsName + ".Control.ModeManual";
        else if (cmd == 110) tag = KLPanelTagsName + ".SetPositionMan";
        else if (cmd == 10) tag = KLPanelTagsName + ".Control.SetManPosition";
        else if (cmd == 11) tag = KLPanelTagsName + ".Control.ModeManRem1";
        else if (cmd == 12) tag = KLPanelTagsName + ".Control.ModeManAdd1";
        if (cmd > 100) {
            val = prompt();
            if (val != null) {
                var valfl = parseFloat(val);
                if (valfl != null) {
                    if (OpcWrite(tag, val)) OkMessage("Команда обработана");
                    else ErrMessage("Команда не обработана");
                }
            } else ErrMessage("Команда не обработана");
        } else {
            if (OpcWrite(tag, true)) OkMessage("Команда обработана");
            else ErrMessage("Команда не обработана");
        }
    }
}
function showKLState(zd, state, alarm) {

    /*0 – Исходное состояние
    1 – Открывается
    2 – Открыт
    3 – Закрывается
    4 – Закрыт
    5 – Ошибка мониторинга обратной связи
    6 – Заблокирован
    */

    //F_alarm(zd, alarm);
    if (state == undefined || state == null) state = 7;
    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gState")[0].id);
    var states = ['unk', 'DoOpen', 'Open', 'DoClose', 'Close', 'bad', 'bad', 'unk'];
    var statesText = ['Исходное состояние ', 'Открывается ', 'Открыт', 'Закрывается ', 'Закрыт', 'Ошибка', 'Заблокирован', '?????'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}
function showKLRegState(zd, state) {

    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gStateMode")[0].id);

    var statesColor = ['remote', 'remote', 'local', 'unk'];
    var statesText = ['Р', 'А', 'М', '?'];
    if (state == null || 0 > state > 3)
        state = 3;
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}
function showKLPosition(zd, state) {

    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gPosition")[0].id);
    var list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = state;
    }

}
function showKLPIDState(pos, setPos, ust, zona, kp, ip, dp) {
    var svg = Snap('#svg');
    var element = svg.select('#Pos');
    var list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = pos;
    }
    element = svg.select('#SetPos');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = setPos;
    }
    element = svg.select('#Ust');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = ust;
    }
    element = svg.select('#Zona');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = zona;
    }
    element = svg.select('#Pk');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = kp;
    }
    element = svg.select('#Ik');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = ip;
    }
    element = svg.select('#Dk');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = dp;
    }

}

/****************************KL****************************************/
/****************************Heat****************************************/
function animateHeat(z, tagsname) {
    var zd = '#' + z;

    //if (zd == '#HeatPanel')
    showHeatRegState(zd, GM_OPC_VALS[tagsname + ".ControlState"]);
    showHeatState(zd, GM_OPC_VALS[tagsname + ".State"]);
    //showHeatRegState(zd, 1);
    //showHeatState(zd, 1);
}
function AnimateHeatPanel(tagsname) {
    animateHeat("HeatPanel", tagsname);
}
function showHeatPanel(st, dispname, tagname) {
    var svg = Snap('#svg');
    var el = svg.select("#HeatPanel");
    var states = ['none', 'block'];
    if (st == 0) {
        el.attr("display", states[0]);
        HeatPanel = null;
    } else {
        el.attr("display", states[1]);
        if (arguments.length > 2) {
            var gname = el.select("#gNameHeat");
            gname.node.innerHTML = dispname;
            HeatPanel = tagname;
            AnimateHeatPanel(tagname);
        }
    }
}
function cmdHeatValve(tag) {

    if (OpcWrite(HeatPanel + '.' + tag, true)) OkMessage("Команда обработана");
    else ErrMessage("Команда не обработана");
}
function showHeatState(zd, state) {

    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gState")[0].id);
    var states = ['Close', 'DoOpen', 'Open', 'DoClose', 'unk', 'unk'];
    var statesText = ['Остановлена ', 'Запускается ', 'Работает', 'Останавливается ', 'Ошибка', 'Заблокирована'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}
function showHeatRegState(zd, state) {

    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gStateMode")[0].id);

    var statesColor = ['unk', 'remote', 'local', 'unk'];
    var statesText = ['Р', 'А', 'М', '?'];
    if (state == null || 0 > state || state > 3)
        state = 3;
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}
/****************************Heat****************************************/
/****************************Wind****************************************/
function animateWind(z, tagsname) {
    var zd = '#' + z;
    //if (zd == '#WindPanel')
    showWindRegState(zd, GM_OPC_VALS[tagsname + ".ControlState"]);
    showWindState(zd, GM_OPC_VALS[tagsname + ".State"]);
    //showWindRegState(zd, 1);
    //showWindState(zd, 1);
}
function AnimateWindPanel(tagsname) {
    animateWind("WindPanel", tagsname);
}
function showWindPanel(st, tagname) {
    var svg = Snap('#svg');
    var el = svg.select("#WindPanel");
    var states = ['none', 'block'];
    if (st == 0) {
        el.attr("display", states[0]);
        ControlWind = null;
    } else {
        el.attr("display", states[1]);
        ControlWind = tagname;
        AnimateWindPanel(ControlWind);

    }
}
function cmdWindValve(tag) {
    if (OpcWrite(tag, true)) OkMessage("Команда обработана");
    else ErrMessage("Команда не обработана");
}
function showWindState(zd, state) {

    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gState")[0].id);
    var states = ['Close', 'DoOpen', 'Open', 'DoClose', 'unk', 'unk'];
    var statesText = ['Остановлена ', 'Запускается ', 'Работает', 'Останавливается ', 'Ошибка', 'Заблокирована'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}
function showWindRegState(zd, state) {

    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gStateMode")[0].id);

    var statesColor = ['unk', 'remote', 'local', 'unk'];
    var statesText = ['Р', 'А', 'М', '?'];
    if (state == null || state < 0 || state > 3)
        state = 3;
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}

/****************************Wind****************************************/
/****************************ZD****************************************/
function animateZD_samsyk(z, tagsname) {
    var zd = '#' + z;  
        var alarm = ParseToBool(GM_OPC_VALS[tagsname + ".Fault"]);
        var power = ParseToBool(GM_OPC_VALS[tagsname + ".Power"]);
        var isclosed = ParseToBool(GM_OPC_VALS[tagsname + ".IsClose"]);
        var closing = ParseToBool(GM_OPC_VALS[tagsname + ".Closing"]);
        var isopen = ParseToBool(GM_OPC_VALS[tagsname + ".IsOpen"]);
        var opening = ParseToBool(GM_OPC_VALS[tagsname + ".Opening"]);
        var controlremote = ParseToBool(GM_OPC_VALS[tagsname + ".Remote"]);
        var controllocal = ParseToBool(GM_OPC_VALS[tagsname + ".Local"]);
        var position = parseInt(GM_OPC_VALS[tagsname + ".Position"]);
        if (position == 0) { isopen = false; isclosed = true; }
        else if (position == 100) { isopen = true; isclosed = false; }
        //else alert(position);
        /*
        alarm = true;
        power = false;
        isclosed = false;
        closing = false;
        isopen = true;
        opening = false;
        controlremote = true;
        controllocal = false;*/
   
    showZDPowerState(zd, power);
    showZDRegState(zd, controlremote, controllocal, false);
    showZDState(zd, alarm, isopen, isclosed, opening, closing);
}

function animateZDState(z, tagsname) {
    var state = 'Состояние задвижки неизвестно';
    var zd = '#' + z;
    var isclosed = ParseToBool(GM_OPC_VALS[tagsname + ".IsClose"]);
    var closing = ParseToBool(GM_OPC_VALS[tagsname + ".Closing"]);
    var isopen = ParseToBool(GM_OPC_VALS[tagsname + ".IsOpen"]);
    var opening = ParseToBool(GM_OPC_VALS[tagsname + ".Opening"]);
    /*
    alarm = true;
    power = false;
    isclosed = false;
    closing = false;
    isopen = true;
    opening = false;
    controlremote = true;
    controllocal = false;*/
    var count = 0;
    if (isclosed) { state = "Задвижка закрыта"; count++; }
    if (closing) { state = "Задвижка закрывается"; count++; }
    if (isopen) { state = "Задвижка открыта"; count++; }
    if (opening) { state = "Задвижка открывается"; count++; }

    if (!count > 1)
        var state = 'Состояние задвижки неизвестно';

    SetText(z, state);

}


function animateZD(z, tagsname) {
    var zd = '#' + z;
    if (tagsname == "ksh1" || tagsname == "ksh2" || tagsname == "mgbb.ksh2" || tagsname == "mgbb.ksh3") {

        if (GM_OPC_VALS[tagsname + ".ControlState"] == 0) var controlremote = true;
        if (GM_OPC_VALS[tagsname + ".ControlState"] == 1) var controlauto = true;
        if (GM_OPC_VALS[tagsname + ".ControlState"] == 2) var controllocal = true;

        if (GM_OPC_VALS[tagsname + ".State"] == 4) var isclosed = true;
        else isclosed = false;
        if (GM_OPC_VALS[tagsname + ".State"] == 3) var closing = true;
        else closing = false;
        if (GM_OPC_VALS[tagsname + ".State"] == 2) var isopen = true;
        else isopen = false;
        if (GM_OPC_VALS[tagsname + ".State"] == 1) var opening = true;
        else opening = false;
        if (GM_OPC_VALS[tagsname + ".State"] == 5) var alarm = true;
        else alarm = false;
        var power = ParseToBool(GM_OPC_VALS[tagsname + ".Power"]);

    }
    else {
        var alarm = ParseToBool(GM_OPC_VALS[tagsname + ".Alarm"]);
        var power = ParseToBool(GM_OPC_VALS[tagsname + ".Power"]);
        var isclosed = ParseToBool(GM_OPC_VALS[tagsname + ".Closed"]);
        var closing = ParseToBool(GM_OPC_VALS[tagsname + ".Closing"]);
        var isopen = ParseToBool(GM_OPC_VALS[tagsname + ".Opened"]);
        var opening = ParseToBool(GM_OPC_VALS[tagsname + ".Opening"]);
        var controlremote = ParseToBool(GM_OPC_VALS[tagsname + ".ControlRemote"]);
        var controllocal = ParseToBool(GM_OPC_VALS[tagsname + ".ControlLocal"]);
        /*
        alarm = true;
        power = false;
        isclosed = false;
        closing = false;
        isopen = true;
        opening = false;
        controlremote = true;
        controllocal = false;*/
    }
    showZDPowerState(zd, power);
    showZDRegState(zd, controlremote, controllocal, controlauto);
    showZDState(zd, alarm, isopen, isclosed, opening, closing);
}
function AnimateZDPanel(tagsname) {
    animateZD("ZDPanel", tagsname);
}
function showZDPanel(st, dispname, tagname) {
    var svg = Snap('#svg');
    var el = svg.select("#ZDPanel");
    var states = ['none', 'block'];
    if (st == 0) {
        el.attr("display", states[0]);
        ZDPanelTagsName = null;
    } else {
        el.attr("display", states[1]);
        if (arguments.length > 2) {
            var gname = el.select("#gName");
            gname.node.innerHTML = dispname;
            ZDPanelTagsName = tagname;
            AnimateZDPanel(tagname);
        }
    }
}
function cmdZDValve(cmd) {
    var tag;
    if (cmd == 0)
        tag = ZDPanelTagsName + ".CloseZD";
    else if (cmd == 1)
        tag = ZDPanelTagsName + ".OpenZD";
    else if (cmd == 2)
        tag = ZDPanelTagsName + ".KvitZD";
    else
        tag = ZDPanelTagsName + "AutoMan";

    if (OpcWrite(tag, true)) OkMessage("Команда обработана");
    else ErrMessage("Команда не обработана");
}
function showZDState(zd, alarm, isopen, isclosed, opening, closing) {
    var alarmstate = 0;
    for (index = 2; index < arguments.length; ++index) {
        if (arguments[index]) alarmstate++;
    }

   F_alarm(zd, alarm);

    var state = 0;
    if (alarmstate > 1) state = 0;
    else if (isopen) state = 4;
    else if (isclosed) state = 3;
    else if (opening) state = 2;
    else if (closing) state = 1;

    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gState")[0].id);
    var states = ['unk', 'DoClose', 'DoOpen', 'Close', 'Open'];
    var statesText = ['Неопределено ', 'Закрывается ', 'Открывается', 'Закрыта ', 'Открыта'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}
function showZDRegState(zd, dregim, mregim, auto) {

    var state = 0;
    if (dregim == false && mregim == false) state = 0;
    if (dregim == true && mregim == true) state = 0;
    else if (mregim == true) state = 1;
    else if (dregim == true) state = 2;
    else if (auto == true) state = 3;
    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gStateMode")[0].id);

    var statesColor = ['unk', 'local', 'remote', 'remote'];
    var statesText = ['?', 'М', 'Д', 'A'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = statesText[state];
    }

}
function showZDPowerState(zd, power) {
    var state = 0;
    if (power == false) state = 1;
    else if (power == true) state = 2;
    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gStatePower")[0].id);
    var statesColor = ['unk', 'bad', 'good'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }

}

/****************************ZD****************************************/
////old funcs for types
/*
  
    function state(id, st) {

        var parent = getSVGById(id);

        var child1 = parent.childNodes[1];
        var child3 = parent.childNodes[3]; 
        if (st == 1) { 
            child1.setAttribute("fill", "green");
            child3.setAttribute("fill", "green");
        }
        else if (st == 2) {
            child1.setAttribute("fill", "yellow");
            child3.setAttribute("fill", "yellow");
        }
        else if (st == 3) {
            child1.animate("fill",1, mina("yellow","red"));
            child3.setAttribute("fill", "yellow");
        }
    }

    function opening(id) {

        var svg = Snap('#svg');
        var group = svg.select(id);
        var states = [{ fill: '#0f0' }, { fill: '#000' }];
        function animateOpen(el, i) {
            el.animate(states[i], 350, function () { animateOpen(el, ++i in states ? i : 0); });
        }
        var right = svg.select("#" + group.node.childNodes[0].id);
        var left = svg.select("#" + group.node.childNodes[1].id);
        animateOpen(right, 0);
        animateOpen(left, 0);
    }
    function closing(id) {

        var svg = Snap('#svg');
        var group = svg.select(id);
        var states = [{ fill: '#ff0' }, { fill: '#000' }];
        function animateOpen(el, i) {
            el.animate(states[i], 350, function () { animateOpen(el, ++i in states ? i : 0); });
        }
        var right = svg.select("#" + group.node.childNodes[0].id);
        var left = svg.select("#" + group.node.childNodes[1].id);
        animateOpen(right, 0);
        animateOpen(left, 0);
    }
    function alarm(id) {

        var svg = Snap('#svg');
        var group = svg.select(id);
        var states = [{ fill: '#f00' }, { fill: '#0f0' }];
        function animateOpen(el, i) {
            el.animate(states[i], 350, function () { animateOpen(el, ++i in states ? i : 0); });
        }
        var right = svg.select("#" + group.node.childNodes[0].id);
        var left = svg.select("#" + group.node.childNodes[1].id);
        animateOpen(right, 0);
        animateOpen(left, 0);
    }
    function update_textbox(svg,object) {
        var val = Fixed(parseFloat(object.LastValue),1);
        if (!isNaN(val))
            svg.textContent = val; //SetInnerTextSVG(object.Tag.TagName, val);
        else svg.textContent = DHS; //SetInnerTextSVG(object.Tag.TagName, DHS);
        svg.setAttribute("onmouseover", "top.OkMessage('" + object.Tag.TagName.toString() + "';'" + object.LastLogged.toString() + "')");
    }

    function update_image(svg, object) {
        var val = object.LastValue;
        if (val > 50) {
            svg.setAttribute("visibility", "hidden");
        } else {
            svg.setAttribute("visibility", "visible");
        }
    } 

    function update_gasState(svgElement, object) {
        var val = parseInt(parseFloat(object.LastValue) % 3);
        if (val == 0) {
            svgElement.setAttribute("fill", "#FFFF00");
        }
        else if (val == 1) {
            svgElement.setAttribute("fill", "#FF0000");
        }
        else {
            svgElement.setAttribute("fill", "#00FF00");
        }
    }

    function update_gasText(svgElement, object) {
        var val = parseInt(parseFloat(object.LastValue) % 3);
        if (val == 0) {
            svgElement.textContent = ">20";
        }
        else if (val == 1) {
            svgElement.textContent = ">50";
        }
        else {
            svgElement.textContent = "<20";
        }
    }
     
    function update_tempState(svgElement, object) {
        var val = parseInt(parseFloat(object.LastValue)%3);
        if (val == 0) {
            svgElement.setAttribute("fill", "#FFFF00"); 
        }
        else if (val == 1) {
            svgElement.setAttribute("fill", "#FF0000");
        }
        else {
            svgElement.setAttribute("fill", "#00FF00");
        }
    }

    function update_tempText(svgElement, object) {
        var val = Fixed(parseFloat(object.LastValue),2);
        svgElement.textContent = val; 
    }

    function update_pressState(svgElement, object) {
        var val = parseInt(parseFloat(object.LastValue) % 3);
        if (val == 0) {
            svgElement.setAttribute("fill", "#FFFF00");
        }
        else if (val == 1)
        { svgElement.setAttribute("fill", "#FF0000"); }
        else
        { svgElement.setAttribute("fill", "#00FF00"); }

    }

    function update_pressText(svgElement, object) {
        var val = Fixed(parseFloat(object.LastValue), 1);
        svgElement.textContent = val;
    }
*/

/****************************AddStructs****************************************/

var colorset_rg_grey = ['bad', 'good', 'unk'];
var colorset_rg_grey = ['off', 'good', 'unk'];

function PrepareTags() {

    for (var i = 0; i < tags.length; i++) {

        switch (tags[i].type) {
            // тревоги
            case "wind":

                GM_OPC_TAGS.push(tags[i].tag + ".State");
                GM_OPC_TAGS.push(tags[i].tag + ".ControlState"); 
                break;
            case "heat":

                    GM_OPC_TAGS.push(tags[i].tag + ".State");
                    GM_OPC_TAGS.push(tags[i].tag + ".ControlState");
                    break;
            // тревоги
            case "ekAlarmLevel":

                GM_OPC_TAGS.push(tags[i].tag + "HiAlarmLevel");
                GM_OPC_TAGS.push(tags[i].tag + "HiWorkLevel");
                GM_OPC_TAGS.push(tags[i].tag + "LowAlarmLevel");
                break;

                // насос
            case "ekPump":
                GM_OPC_TAGS.push(tags[i].tag + "Pump_On");
                GM_OPC_TAGS.push(tags[i].tag + "Pump_Power");
                GM_OPC_TAGS.push(tags[i].tag + "AutoMan");
                break;

                // задвижка
            case "valve":
                {

                    if (tags[i].tag == "ksh1" || tags[i].tag == "ksh2" || tags[i].tag == "mgbb.ksh2" || tags[i].tag == "mgbb.ksh3") {
                        GM_OPC_TAGS.push(tags[i].tag + ".Alarm");
                        GM_OPC_TAGS.push(tags[i].tag + ".State");
                        GM_OPC_TAGS.push(tags[i].tag + ".ControlState");
                        GM_OPC_TAGS.push(tags[i].tag + ".CloseZD");
                        GM_OPC_TAGS.push(tags[i].tag + ".OpenZD");
                        GM_OPC_TAGS.push(tags[i].tag + ".KvitZD");
                    }
                    else {
                        GM_OPC_TAGS.push(tags[i].tag + ".Alarm");
                        GM_OPC_TAGS.push(tags[i].tag + ".Power");
                        GM_OPC_TAGS.push(tags[i].tag + ".CloseZD");
                        GM_OPC_TAGS.push(tags[i].tag + ".Closed");
                        GM_OPC_TAGS.push(tags[i].tag + ".Closing");
                        GM_OPC_TAGS.push(tags[i].tag + ".OpenZD");
                        GM_OPC_TAGS.push(tags[i].tag + ".Opened");
                        GM_OPC_TAGS.push(tags[i].tag + ".Opening");
                        GM_OPC_TAGS.push(tags[i].tag + ".StopZD");
                        GM_OPC_TAGS.push(tags[i].tag + ".ControlRemote");
                        GM_OPC_TAGS.push(tags[i].tag + ".ControlLocal");
                        GM_OPC_TAGS.push(tags[i].tag + ".ControlChange");
                    }
                }
                break;
            case "rk":
            case "valveKL":
                if (tags[i].tag == "rk1") {
                    GM_OPC_TAGS.push(tags[i].tag + "_Fail");
                    GM_OPC_TAGS.push(tags[i].tag + "_Position");
                    GM_OPC_TAGS.push(tags[i].tag + "_SetPositionAuto");
                    GM_OPC_TAGS.push(tags[i].tag + "_SetPosition");
                    GM_OPC_TAGS.push(tags[i].tag + "_Ready");
                    GM_OPC_TAGS.push(tags[i].tag + "_Remote");
                    GM_OPC_TAGS.push(tags[i].tag + "_Local");
                    GM_OPC_TAGS.push(tags[i].tag + ".ModeAuto");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.Ustav");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.Zona");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.Kp");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.Ip");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.Dp");
                    break;
                } else {
                    GM_OPC_TAGS.push(tags[i].tag + ".State");
                    GM_OPC_TAGS.push(tags[i].tag + ".ControlState");
                    GM_OPC_TAGS.push(tags[i].tag + ".Position");
                    GM_OPC_TAGS.push(tags[i].tag + ".SetPosition");
                    GM_OPC_TAGS.push(tags[i].tag + ".Alarm");

                    GM_OPC_TAGS.push(tags[i].tag + ".PID.ActUstav");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.ActZona");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.ActKp");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.ActIp");
                    GM_OPC_TAGS.push(tags[i].tag + ".PID.ActDp");
                }

                break;
            default:
                GM_OPC_TAGS.push(tags[i].tag);
        }
    }
}
 function Calculate(tagname) {
     var n2;
     var n1;
     if (tagname=="dFIT"){
         n1 = parseFloat(GM_OPC_VALS["fit01.RSG"]);
         n2 = parseFloat(GM_OPC_VALS["fit02.RPG"]);
         return !isNaN((n1 - n2)) ? (n1 - n2) : '---';
}
 else 
 if (tagname=="dFITn"){
     n1 = parseFloat(GM_OPC_VALS["fit01.nRSG"]);
     n2 = parseFloat(GM_OPC_VALS["fit02.nRPG"]);
     return !isNaN((n1 - n2)) ? (n1 - n2) : '---';
 }
 else 
 if (tagname=="ProcFITn"){
     n1 = parseFloat(GM_OPC_VALS["fit01.nRSG"]);
     n2 = parseFloat(GM_OPC_VALS["fit02.nRPG"]);
     return !isNaN(parseInt((n1 - n2) / n1 * 100)) ? (parseInt((n1 - n2) / n1 * 100)) : '---'; 
 }
}
function SetTextpech(id, val) {

    var al = 1;
				if (val==6||val==8||val==9||val==10) al = 2;
				else  if (val==1||val==2||val==3||val==4||val==5) al = 0;
				else    al = 1; 
    var svg = Snap('#svg');
    var group = svg.select('#' + id);
    var states = ['good', 'warn', 'bad']; 
        var list = group.node.getElementsByClassName("stateColor");
    
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[al];
    } 
 
}
function UpdateTags() {

    for (var i = 0; i < tags.length; i++) {
        switch (tags[i].type) {
        
                //pg only

            case "calc":  
               SetText(tags[i].id, (Calculate(tags[i].tag)));
                break;
            case "textPG":
       //case "0": return "К Пуску готов";
       //case "1": return "Продувка";
       //case "2": return "Поджиг запальника";
       //case "3": return "Стабилизация забальника";
       //case "4": return "Работа";
       //case "5": return "Повторный розжиг";
       //case "6": return "Авария горелки";
       //case "7": return "Вентиляция топки";
       //case "8": return "Авария подогревателя";
       //case "9": return "Нет готовности";
       //case "10": return "Пуск запрещен с верхнего уровня";
       //default: return "Неизвестное состояние";
			var val=GM_OPC_VALS[tags[i].tag];
                SetText(tags[i].id, SetTextPG(val));
			SetTextpech(tags[i].id, val); 
                break;

            case "wind":
                animateWind(tags[i].id, tags[i].tag);
                break;
            case "heat":
                animateHeat(tags[i].id, tags[i].tag);
                break;
            //pk only
            // тревоги
            case "ekAlarmLevel":
                animateEK_AlarmLevel(tags[i].id, tags[i].tag);
                break;

                // насос
            case "ekPump":
                animateEK_Pump(tags[i].id, tags[i].tag);
                break; 
            //common
            case "valve":
                animateZD(tags[i].id, tags[i].tag);
                break;
            case "valveKL":
                animateKL(tags[i].id, tags[i].tag);
                break;
            case "valveRK":
                animateRK(tags[i].id, tags[i].tag);
                break;
            case "procent":
                SetProcent(tags[i].id, ProcFl(GM_OPC_VALS[tags[i].tag], 2));
                break;
            case "analog":
                SetText(tags[i].id, ProcFl(GM_OPC_VALS[tags[i].tag], 2));
                break;
            case "discrete":
                SetDiscrete(tags[i].id, ParseToBool(GM_OPC_VALS[tags[i].tag]));
                break;
            case "pg_valve":
                SetPgValve(tags[i].id, ParseToBool(GM_OPC_VALS[tags[i].tag]));
                break;

            case "gasan":
                SetTextGasAn(tags[i].id, ProcFl(GM_OPC_VALS[tags[i].tag], 2));
                break;
            case "visible":
                SetVis(tags[i].id, ParseToBool(GM_OPC_VALS[tags[i].tag]));
                break;
            default:
                SetText(tags[i].id, ProcFl(GM_OPC_VALS[tags[i].tag], 2));
                break;
        }
    }
     
}


/****************************AddStructs****************************************/

function F_alarm(id, alarm) {
    var state = 0;
    if (alarm == true) state = 2;
    else if (alarm == false) state = 1;
    var svg = Snap('#svg');
    var element = svg.select(id);
    var group = element.select("#" + element.node.getElementsByClassName("gStateAlarm")[0].id);
    var statesColor = ['unk', 'NoAlarm', 'Alarm'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + statesColor[state];
    }
} 
function SetText(id, value, al) {
    if (al == null) al = 0;
    var svg = Snap('#svg');
    var group = svg.select('#' + id);
    //var states = ['und', 'Close', 'Open'];
    var states = ['und', 'default'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[al];
    }
    list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = value;
    }
}
function SetProcent(id, value, al) {
    if (al == null) al = 0;
    var svg = Snap('#svg');
    var group = svg.select('#' + id);  
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].attributes.width.nodeValue = value / 2;
    } 
}

function SetTextGasAn(id, value) {
    var al = 0;
    if (value > 50) al = 2;
    else if (value > 20) al = 1;
    var svg = Snap('#svg');
    var group = svg.select('#' + id);
    var states = ['good', 'warn', 'bad'];
    var kvit = group.node.getElementsByClassName("kvit");
    if (kvit.length == 0) {
        var list = group.node.getElementsByClassName("stateColor");
    
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[al];
    }
}

list = group.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = value;
    }
}
function SetDiscrete(id, value, inv) {
    var svg = Snap('#svg');
    var group = svg.select('#' + id);
        var al = 2;
    if (value!=null) {
        if (group.node.getAttribute('data-inv') == "true") value = (!value);
        if (value) al = 1;
        else if (!value) al = 0;
    }
    var states = ['bad', 'good','unk'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[al];
    } 
}
function SetPgValve(id, value, inv) {
    var svg = Snap('#svg');
    var group = svg.select('#' + id);
    var al = 2;
    if (value != null) {
        if (group.node.getAttribute('data-inv') == "true") value = (!value);
        if (value) al = 1;
        else if (!value) al = 0;
    }
    var states = ['off', 'good', 'unk'];
    var list = group.node.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + states[al];
    }
}
 
function SetVis( id,st) {
    var svg = Snap('#svg');
    var el = svg.select("#" + id); 
    var states = ['none', 'block'];
    if (st == 0) 
        el.attr("display", states[0]);
    else if (st == 1)
        el.attr("display", states[1]);
    
}

/****************************RK****************************************/
function animateRK(z, tagsname) {
    var zd = '#' + z; 
    var alarm = ParseToBool(GM_OPC_VALS[tagsname + "_Fail"]);
    var position = ProcFl(GM_OPC_VALS[tagsname + "_Position"],2);
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
function showRKPositionState (ek, position) {

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
function cmdRKPosition( ) {
    var val = prompt();

    if (val != '') { OpcWrite("rk1_SetPosition", val+"0"); OkMessage("Команда обработана"); }
    else ErrMessage("Команда не обработана");
}

/****************************RK****************************************/

/****************************ZD****************************************/
function animateZD(z, tagsname) {
    var zd = '#' + z; 
    if (tagsname == "ksh1" || tagsname == "ksh2"||tagsname == "mgbb.ksh2" || tagsname == "mgbb.ksh3") {

        if (GM_OPC_VALS[tagsname + ".ControlState"] == 0) var controlremote = true; 
        if(GM_OPC_VALS[tagsname + ".ControlState"]==1) var controlauto =true;
        if(GM_OPC_VALS[tagsname + ".ControlState"]==2) var controllocal =true;

        if (GM_OPC_VALS[tagsname + ".State"] == 4) var isclosed = true;
        else isclosed=false;
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
    showZDRegState(zd, controlremote, controllocal,controlauto);
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
function showZDRegState(zd, dregim, mregim,auto) {

    var state = 0;
    if (dregim ==false && mregim==false) state = 0;
    else if (mregim == true) state = 1;
    else if (dregim == true) state = 2;
    else if (auto == true) state = 3;
    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gStateMode")[0].id);

    var statesColor = ['unk', 'remote', 'local'];
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
/****************************Heat****************************************/
function animateHeat(z, tagsname) {
    var zd = '#' + z;

    //if (zd == '#HeatPanel')
    showWindRegState(zd, GM_OPC_VALS[tagsname + ".ControlState"]);
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
            HeatPanel= tagname;
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
        var states = [ 'Close','DoOpen', 'Open', 'DoClose', 'unk', 'unk'];
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

        var statesColor = ['unk', 'remote', 'local'];
        var statesText = ['Р', 'А', 'М'];
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
function showWindPanel(st,tagname) {
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
function  cmdWindValve(tag) {  
    if (OpcWrite(tag, true)) OkMessage("Команда обработана");
    else ErrMessage("Команда не обработана");
}
function showWindState(zd, state) {
      
    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gState")[0].id);
    var states = [ 'Close','DoOpen', 'Open', 'DoClose', 'unk', 'unk'];
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

    var statesColor = ['unk', 'remote', 'local'];
    var statesText = ['Р', 'А', 'М'];
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
function showPanel(st, dispname, tagname) {
    var svg = Snap('#svg');
    var el = svg.select("#ZDPanel");
    var states = ['none', 'block'];
    if (st == 0) {
        el.attr("display", states[0]);
        ControlPanelTagsName  = null;
    } else {
        el.attr("display", states[1]);
        if (arguments.length > 2) {
            var gname = el.select("#gName");
            gname.node.innerHTML = dispname;
            ControlPanelTagsName = tagname;
            AnimateZDPanel(tagname);
        }
    }
}
function showEKPanel(st, dispname, tagname) {
    var svg = Snap('#svg');
    var el = svg.select("#EKPanel");
    var states = ['none', 'block'];
    if (st == 0) {
        el.attr("display", states[0]);
        ControlPanelTagsName = null;
    } else {
        el.attr("display", states[1]);
        if (arguments.length > 2) {
            var gname = el.select("#gName");
            gname.node.innerHTML = dispname;
            ControlPanelTagsName = tagname;
            AnimateZDPanel(tagname);
        }
    }
}

function update_gasState(svgElement, object) {
    var val = parseInt(parseFloat(object) % 3);
    if (val == 0) {
        svgElement.setAttribute("fill", "#FFFF00");
    }
    else if (val == 1)
    { svgElement.setAttribute("fill", "#FF0000"); }
    else
    { svgElement.setAttribute("fill", "#00FF00"); }

}
function update_gasText(svgElement, object) {
    var val = parseInt(parseFloat(object) % 3);
    if (val == 0) {
        svgElement.textContent = ">20";
    }
    else if (val == 1)
    { svgElement.textContent = ">50"; }
    else
    {
        svgElement.textContent = "<20";
    }

}

function getSVGById(ei) {
    var e = document.getElementById("svg");
    var b= e.contentDocument.getElementById(ei);
    return b;
}
function getSVGSByClassName(ei) {
    var e = document.getElementById("svg");
    var b = e.contentDocument.getElementsByClassName(ei);
    return b;
}
function SetInnerTextSVG(ei, t, d) {
    var e = document.getElementById("svg");
    var b = e.contentDocument.getElementById(ei);
    //svg.children("#"+ei).attr("fill", f ? "black" : "red").attr("stroke", f ? "red" : "black");

    if (b != null) {
        b.textContent = t;
    };
    if (arguments.length == 4) SetStyleSVG(ei + '.info', d);
}
function SetInnerText(ei, t, d) {
    var e = document.getElementById("svg");
    var b = e.contentDocument.getElementById(ei);
    //svg.children("#"+ei).attr("fill", f ? "black" : "red").attr("stroke", f ? "red" : "black");

    if (b != null) {
        b.textContent = t;
    };
    if (arguments.length == 4) SetStyleSVG(ei + '.info', d);
}
function SetStyleSVG(ei, cn) {
    var e = document.getElementById("svg");
    var b = e.contentDocument.getElementById(ei); if (b != null) b.setAttribute("fill", cn);
}
function SetAttrSVG(ei, attr, cn) {
    var e = document.getElementById("svg");
    var b = e.contentDocument.getElementById(ei); if (b != null) b.setAttribute(attr, cn);
}
function GetStyleSVG(al) {

    if (Math.abs(al) == 2) return "red";
    else if (Math.abs(al) == 1) return "yellow";
    else return "green";
    // var lo = tagslimit.lo;
    // var hi = tagslimit.hi;
    // if (lo == null || lo == "") return "gray";
    // lo = lo.ReplaceText(",", ".");
    // if (hi == null || hi == "") return "gray";
    // hi = hi.ReplaceText(",", ".");
    //
    // if (val > hi || val < lo) return "red";
    //
    // else return "green";
}
function FindTag(nameKey, myArray) {
    for (var i = 0; i < myArray.length; i++) {
        if (myArray[i].tag === nameKey) {
            return myArray[i];
        }
    } return null;
}
function GetStyle(TagsArray, tag, val) {

    var tagslimit = FindTag(tag, TagsArray); if (tagslimit == null) return "";
    var lo = tagslimit.lo;
    var hi = tagslimit.hi;
    if (lo == null || lo == "") return "und";
    lo = lo.ReplaceText(",", ".");
    if (hi == null || hi == "") return "und";
    hi = hi.ReplaceText(",", ".");

    if (val > hi || val < lo) return "bad";

    else return "good";
}


function OpcRead(tag) {
    var result;
    $.ajax({ type: "POST", url: "api/Opc/ReadOpcTag", data: { tag: tag }, async: false, success: function (returnedData) { result = returnedData; } });
    return result;
}

function OpcWrite(tag, value) {
    var data = false;
    $.ajax({ type: "POST", url: "/api/Opc/WriteOpcTagValue", data: { tag: tag, value: value }, async: false, success: function (_data) { data = _data; } });
    return data;
}
function WriteTag() {
    $.ajax({
        type: "POST", url: "/api/Opc/WriteOpcTagValue", async:
                false, data: { tag: "Sfera.TestChannel.State", value: "1az" }, success: after_WriteTag
    });
}


function cmdGasAnKvit(cmd,tag) {
     
    //Svar tag = cmd.getAttribute('data-tag');
    var list = cmd.getElementsByClassName("stateColor");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].classList = 'stateColor ' + "kvit";
    }  
        tag = tag + "_Remote"; 

        if (OpcWrite(tag, true)) OkMessage("Квитирование загазованности");
        else ErrMessage("Загазованность не квитирована");
}
function showMGB(st) {
    var svg = Snap('#svg');
    var el = svg.select("#mgb101");
    var states = ['none', 'block'];
    if (st == 0)  
        el.attr("display", states[0]); 
     else  
        el.attr("display", states[1]);  
    }
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
        else  mode = 2; 

        showRKReadyState(zd, power, alarm);
        //showRKRegState(zd, controlremote, controllocal); 

        showKLPIDState(ProcFl(GM_OPC_VALS[tagsname + "_Position"], 3),
            ProcFl(GM_OPC_VALS[tagsname + "_SetPosition"], 3)/10,
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
        showKLPosition(zd, ProcFl(GM_OPC_VALS[tagsname + ".Position"],2));
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
        var tag, tag2,val;
    if (KLPanelTagsName == "rk1") {
          if (cmd == 8) {tag = KLPanelTagsName + ".ModeAuto";val=true;}
        else if (cmd == 9) {tag = KLPanelTagsName + ".ModeAuto"; val=false; }
        else if (cmd == 101) {tag = KLPanelTagsName + ".PID.Ustav"; val = prompt(); }
        else if (cmd == 103) {tag = KLPanelTagsName + ".PID.Kp"; val = prompt(); }
        else if (cmd == 104) {tag = KLPanelTagsName + ".PID.Ip"; val = prompt(); }
        else if (cmd == 110) {tag = KLPanelTagsName + "_SetPosition"; val = prompt();val=val+'0' }
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
      
    var svg = Snap('#svg');
    var element = svg.select(zd);
    var group = element.select("#" + element.node.getElementsByClassName("gState")[0].id);
    var states = ['unk', 'DoOpen', 'Open', 'DoClose', 'Close', 'bad', 'bad'];
    var statesText = ['Исходное состояние ', 'Открывается ', 'Открыт', 'Закрывается ', 'Закрыт', 'Ошибка', 'Заблокирован'];
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

    var statesColor = ['remote', 'remote', 'local'];
    var statesText = ['Р', 'А', 'М'];
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
        list[i].innerHTML =  state;
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
        list[i].innerHTML = ust;} 
    element = svg.select('#Zona');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = zona;} 
    element = svg.select('#Pk');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = kp;} 
    element = svg.select('#Ik');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML =  ip;} 
    element = svg.select('#Dk');
    list = element.node.getElementsByClassName("stateText");
    for (var i = 0, len = list.length; i < len; i++) {
        list[i].innerHTML = dp;
    }

}

/****************************KL****************************************/
 
function ControlMGB101(cmd,val) {
    var tag;
    if (cmd == 0)
        tag = "mgb101.offN" + val;
    else if (cmd == 1) 
	tag = "mgb101.onN" + val;     
    {if (OpcWrite(tag, true)) OkMessage("Команда обработана");
    else ErrMessage("Команда не обработана");}
}
function SetTextPG(id) {
    switch (id) {
        case "0": return "К Пуску готов";
        case "1": return "Продувка";
        case "2": return "Поджиг запальника";
        case "3": return "Стабилизация забальника";
        case "4": return "Работа";
        case "5": return "Повторный розжиг";
        case "6": return "Авария горелки";
        case "7": return "Вентиляция топки";
        case "8": return "Авария подогревателя";
        case "9": return "Нет готовности";
        case "10": return "Пуск запрещен с верхнего уровня";
        default: return "Неизвестное состояние";
    }
}
function showLegend(st) {
    var svg = Snap('#svg');
    var el = svg.select("#gLegend");
    var states = ['none', 'block'];
    if (st == 0)
        el.attr("display", states[0]);
    else
        el.attr("display", states[1]);
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


// --- SHOW ---

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
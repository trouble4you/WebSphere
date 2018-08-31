
/****************************Main Funcs****************************************/

var GM_OPC_TAGS = new Array(),
        GM_OPC_VALS = new Array(),
        OBJ_I = 0,
        DHS = "---",
        tags = new Array();
        ObjectName = null;
function InitMnemoSVG(name) {
	if (name!=null)ObjectName=name;
    var mnemoName = getElementsByAttribute('mnemoName', 'svg');
    for (var i = 0; i < mnemoName.length; i++) { 
        SetText(mnemoName[i].id, name);
    } 
         var datatag = getElementsByAttribute('data-tag', 'svg');
         for (var i = 0; i < datatag.length; i++) {
             var el = {};
             el.tag = datatag[i].getAttribute('data-tag').replace("XObject", name);
             el.type = datatag[i].getAttribute('data-type');
             var koef=1;
             if ((datatag[i].getAttribute('data-koef') != null))
                 koef = parseFloat(datatag[i].getAttribute('data-koef'));
             el.koef = koef;
             el.inv = datatag[i].getAttribute('data-inv');
             el.id = datatag[i].id;
             if (tags.indexOf(el)>0) console, log(tag);
             else tags.push(el);
         }
            PrepareTags(tags); 
            RequestOPC(OBJ_I);
            setInterval(function () {RequestOPC(OBJ_I);}, 2000);
    
}  

// получение элемента по атрибутам
function getElementsByAttribute(attribute, context) {

    var svg = Snap('#svg');
    var nodeList = svg.node.getElementsByTagName('*');
    var nodeArray = [];
    var iterator = 0;
    var node = null;

    while (node = nodeList[iterator++]) {
        if (node.getAttribute(attribute)) nodeArray.push(node);
    }

    return nodeArray;
}
/* REQUEST OPC UPDATE */
function PrepareTags( ) {

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
				case "zd": 
                        GM_OPC_TAGS.push(tags[i].tag + ".Local");
                        GM_OPC_TAGS.push(tags[i].tag + ".Remote");
                        GM_OPC_TAGS.push(tags[i].tag + ".Power");
                        GM_OPC_TAGS.push(tags[i].tag + ".Fault");
                        GM_OPC_TAGS.push(tags[i].tag + ".IsOpen");
                        GM_OPC_TAGS.push(tags[i].tag + ".IsClose");  
                        GM_OPC_TAGS.push(tags[i].tag + ".Closing");  
                        GM_OPC_TAGS.push(tags[i].tag + ".Opening"); 
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
function RequestOPC(id) {
   //пересоберем массив
    var a = {}; for (var i = 0; i < GM_OPC_TAGS.length; i++) a[i] = GM_OPC_TAGS[i];
    $.ajax({
        type: "POST",
        url: "/api/Opc/GetFullTagValues",
        data: { Tags: a, TagsCount: GM_OPC_TAGS.length, Sender: id },
        async: true,
        success: s_RequestOPC,
        error: e_RequestOPC
    });
}
function s_RequestOPC(data) {
    GM_OPC_VALS = new Array();
    for (var i = 0; i < data.length; i++)
        if (data[i].OpcVals != null)
            GM_OPC_VALS[data[i].Tag] = data[i].OpcVals.LastValue;
        else GM_OPC_VALS[data[i].Tag] = null; 
    UpdateTags();
}
function e_RequestOPC() {
    ErrMessage("Не удалось получить значения OPC тегов.");
}


function Perecachka_check(id, tag) {
    var st=false;
    var val = parseFloat(GM_OPC_VALS[tag]);
    if (val < 1) st = true;
    
    var svg = Snap('#svg');
    var el = svg.select("#" + id);
    var inversion = ParseToBool(el.node.getAttribute('data-inv'));

    if (inversion)//если задана инверсия, то инвертируем
        st = !st;
    var states = ['none', 'block'];
    if (!st)
        el.attr("display", states[0]);
    else if (st)
        el.attr("display", states[1]);

}
function SecToTime(sec) { 
    var d1 = new Date(1970, 0, 1, 0, 0, 0);
    var d = new Date(parseInt(sec) * 1000);
    //var d = new Date(d1.getTime() + d2.getTime());
    var r = "";
    if (d.getHours() < 10) r += "0";
    r += d.getHours() + ":";
    if (d.getMinutes() < 10) r += "0";
    r += d.getMinutes() + " ";
    if (d.getDate() < 10) r += "0";
    r += d.getDate() + ".";
    if (d.getMonth() < 9) r += "0";
    r += (parseInt(d.getMonth()) + 1) + ".";
    r += d.getFullYear();
    return r;
}
function UpdateTags() {

    for (var i = 0; i < tags.length; i++) {
        switch (tags[i].type) {
        
                //pg only

            case "unixtime":
                 SetText(tags[i].id, (SecToTime(GM_OPC_VALS[tags[i].tag])));
                break;
            case "Perecachka_check":
                Perecachka_check(tags[i].id, tags[i].tag);
                break;
            case "calc":
                SetText(tags[i].id, (Calculate(tags[i].tag)));
                break;
            case "textPG": 
			var val=GM_OPC_VALS[tags[i].tag];
                SetText(tags[i].id, SetTextPG(val));
			SetTextpech(tags[i].id, val); 
                break;

            case "wind":
                animateWind(tags[i].id, tags[i].tag);
            case "discreteChange":
                discreteChange(tags[i].id, GM_OPC_VALS[tags[i].tag]);// tags[i].tag);
                break;
            case "analogChange":
                analogChange(tags[i].id, ProcFl(GM_OPC_VALS[tags[i].tag], 2));// tags[i].tag);
                break;
            case "discreteColor":
                discreteColor(tags[i].id, GM_OPC_VALS[tags[i].tag]);// tags[i].tag);
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
				
            case "zd":
                animateZD_samsyk(tags[i].id, tags[i].tag);
                break;
            case "zdstate":
                    animateZDState(tags[i].id, tags[i].tag);
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
                SetVis(tags[i].id, ParseToBool(GM_OPC_VALS[tags[i].tag]),ParseToBool(tags[i].inv));
                break;
            default:
                SetText(tags[i].id, GM_OPC_VALS[tags[i].tag]);
                break;
        }
    }
     
}
function openWindow(mnemo, object) {
    if (object != null)
        window.open('/Mnemo/MnemoObj/?mnemo=' + mnemo + '&objname=' + object, '_self');
    else
        window.open('/Mnemo/Mnemo/?mnemo=' + mnemo, '_self');
}
/****************************AddStructs****************************************/
 
var colorset_rg_grey = ['bad', 'good', 'unk'];
var colorset_rg_grey = ['off', 'good', 'unk'];


 
/****************************AddStructs****************************************/ 
function F_alarm(id, st) {

    var svg = Snap('#svg');
    var element = svg.select(id);
    var el = element.select("#" + element.node.getElementsByClassName("zdvAlarm")[0].id);
    var inversion =ParseToBool( el.node.getAttribute('data-inv'));
    if ( st != null) {
        if (inversion)//если задана инверсия, то инвертируем
            st = !st;
    }
    else
        console.log("Сигнал аварии задвижки равен null");
    var states = ['none', 'block'];
    if (!st)
        el.attr("display", states[0]);
    else if (st)
        el.attr("display", states[1]);
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
    if (!isNaN(value))
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
 
function SetVis( id,st,inv) { 
    if (inv&&st!=null) st = (!st);  
    var svg = Snap('#svg');
    var el = svg.select("#" + id); 
    var states = ['none', 'block'];
    if (!st) 
        el.attr("display", states[0]);
    else if (st)
        el.attr("display", states[1]);
    
}
 

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
function WriteTag(tag,val) {
	if(ObjectName!=null)
             tag = tag.replace("XObject", ObjectName);
    $.ajax({
        type: "POST", url: "/api/Opc/WriteOpcTagValue", async:
                false, data: { tag: tag, value: val }, success: OkMessage("Команда принята")
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
function set_tag(options) {
    var deferredObject = $.Deferred();
    var defaults = {
        action:"OpcTagWriteValue",
        type: "forced", //alert, boolean,input
        tag: "default", //alert, boolean,input
        value: 'default', //modal-sm, modal-lg
        messageText: 'Сообщение'
    }
    $.extend(defaults, options);


    $('BODY').append(
        '<div id="ezAlerts" style="position:fixed;top: 150px;right: 0;bottom: 0;left: 0;">' +
        '<div class="modal-dialog" class="modal-sm">' +
        '<div class="modal-content">' +
        '<div id="ezAlerts-body" class="modal-body">' +
        '<div id="ezAlerts-message" ></div>' +
        '</div>' +
        '<div id="ezAlerts-footer" class="modal-footer">' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>'
    );

    switch (defaults.type) {
        case 'forced':
            setTimeout(ElDel, 2000, document.getElementById("ezAlerts"));
            $('#ezAlerts-message').html(defaults.messageText);
            break;

        case 'boolean':
            $('#ezAlerts-message').html(defaults.messageText);
            var btnhtml = "<button id='ezok-btn' class='btn btn-primary'>Ok</button><button id='ezclose-btn' class='btn btn-default' onclick='CloseBSAlert()'>Отмена</button>";
             $('#ezAlerts-footer').html(btnhtml);
             document.getElementById("ezok-btn").setAttribute("onclick", "OpcWrite(  '" + defaults.tag + "','" + defaults.tagid + "','" + defaults.opcid + "','" + defaults.value + "','" + defaults.action + "');CloseBSAlert();");
             break;

        case 'input':
            $('#ezAlerts-message').html(defaults.messageText + '<br /><br /><div class="form-group"><input type="text" class="form-control" id="prompt" /></div>');
              btnhtml = "<button id='ezok-btn' class='btn btn-primary'>Ok</button><button id='ezclose-btn' class='btn btn-default' onclick='CloseBSAlert()'>Отмена</button>";
            $('#ezAlerts-footer').html(btnhtml);
            //defaults.opcid: '" + opcid + "', defaults.tagid
            document.getElementById("ezok-btn").setAttribute("onclick", "OpcModalWrite( '" + defaults.tag + "','" + defaults.tagid + "','" + defaults.opcid + "','" + defaults.action + "');CloseBSAlert();");
    }
    //document.getElementById("ezok-btn").onclick= OpcWrite(defaults.tag, document.getElementById("prompt").value);
}


function OpcWrite(tagName, tagId, pollerId, value, action) {
    if (action == null || action == undefined) action = "OpcTagWriteValue";
    if (ObjectName != null) tagName = tagName.replace("XObject", ObjectName);
    var data = false;
    $.ajax({
        type: "POST", url: "/Opc/" + action,
        data: { tagName: tagName, tagId: tagId, pollerId: pollerId, val: value }, async: false, success: function (_data) { data = _data; }
    });
    if (data) OkMessage("Команда принята");
    else ErrMessage("Команда не принята");
}

function OpcModalWrite(tagName, tagId, pollerId, action) {
    if (action == null || action == undefined) action = "OpcTagWriteValue";
    if (ObjectName != null) tagName = tagName.replace("XObject", ObjectName);
    var value = document.getElementById("prompt").value;
    var data = false;
    $.ajax({
        type: "POST", url: "/Opc/" + action,
        data: { tagName: tagName, tagId: tagId, pollerId: pollerId, val: value }, async: false, success: function (_data) { data = _data; }
    });
    if (data) OkMessage("Команда принята");
    else ErrMessage("Команда не принята");
}

function CloseBSAlert() {
    ElDel(document.getElementById("ezAlerts"));
}
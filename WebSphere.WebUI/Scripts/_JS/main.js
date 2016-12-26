var tree_loaded = false;

function Login() {
    var name = document.getElementById("user_name").value;
    var pass = document.getElementById("user_pass").value;
    var post = new MyPost();
    post.Add("user_name=" + name);
    post.Add("user_pass=" + pass);
    var req = new MyRequest("api/Auth/Login", post.Get(), after_Login);
    req.Execute();
}

function after_Login(data) {
    var root = new XMLnode(data, "root");
    var login = GetNodeByName(root, "Login").value;
    location.reload();
}

function Logout() {
    var post = new MyPost();
    var req = new MyRequest("api/Auth/Logout", post.Get(), page_Logout);
    req.Execute();
}

function page_Logout(data) {
    var root = new XMLnode(data, "root");
    var login = GetNodeByName(root, "Logout").value;
    location.href = "/";
}

function UpdateTime()
{
    $.ajax({ type: "POST", url: "api/Time/Get", data: {}, async: true, success: page_UpdateTime });
}
function page_UpdateTime(data) {
    var dt = Get_Date_FromUTCDateStr(data);
    var date = GetOnlyDateStrLabel_FromDate(dt);
    var time = GetOnlyTimeStrLabel_FromDate(dt);
    document.getElementById("a_time").innerHTML = time;
    document.getElementById("a_date").innerHTML = date;
}

function UpdateOpros() {
    /*
    var timer = OpcRead("system", "Опрос АЦДНГ-5._System.CyclePeriod", true);
    if (timer != null) {
        timer = timer.substring(13, timer.length);
        document.getElementById("CyclePeriod").innerHTML = 'Опрос:' + timer;
    }
    var b=0;
    var el = document.getElementById("nameOfObjectOpr");
    var a = OpcRead("system", "Опрос АЦДНГ-5._System.CurrentGZU", true);
    if (a != null) a = a;
    else a = "?";
    el.innerHTML = a;
    var a = OpcRead("system", "Опрос АЦДНГ-5._System.CycleGZU", true);
    if (a == "TIMEOUT") {
        el.style.backgroundColor = "#b66";
        return;
    }
    if (a == "") {
        el.style.backgroundColor = "#fff";
        return;
    }
    el.style.backgroundColor = "#bb1";
    */
}

function user_pass_press(e)
{
    if (e.keyCode === 13)
        Login();
    return false;
}

function GetHexValue(value)
{
    var v = parseInt(value);
    if (v == -1)
        return "";
    var s = v.toString(16).toUpperCase();
    while (4 - s.length > 0)
        s = "0" + s;
    s = "0x" + s;
    return s;
}

function GetObjTreeItems() {
    /*
    $.ajax({
        type: "POST",
        url: "/api/Objects/GetObjTreeItems",
        async: true,
        success: after_GetObjTreeItems
    });
    */
}


var menuTreeObjects = new Array();
function after_GetObjTreeItems(data) {
    // Find menuitems container.
    var objMenuIts = document.getElementById("tree_objects");
    if (objMenuIts == null)
        return;
    for (var itemIndex = 0; itemIndex < data.length; itemIndex++) {
        var objMenuIt = document.createElement("div");
        var imgPath = "/_Img/help.png";
        switch (data[itemIndex].TypeId) {
            case "3":
                imgPath = "/_Img/agzu2.png";
                break;
            case "10":
                imgPath = "/_Img/ktpnf.png";
                break;
            case "12":
                imgPath = "/_Img/mass.png";
                break;
        }
        var objMenuItImg = document.createElement("img");
        objMenuItImg.src = imgPath;
        objMenuItImg.style.height = "20px";
        objMenuIt.appendChild(objMenuItImg);
        var objMenuItTl = document.createElement("span");
        objMenuItTl.innerHTML = data[itemIndex].Name;
        objMenuIt.appendChild(objMenuItTl);
        objMenuIt.className = "objTreeItem";
        objMenuIt.id = "blockItemTree_" + itemIndex;
        objMenuIt.setAttribute("onclick",
            "OnObjTreeItemClick(" +
            itemIndex + "," +
            data[itemIndex].Id + ", " +
            data[itemIndex].TypeId + ")");
        objMenuIts.appendChild(objMenuIt);
        menuTreeObjects.push(data[itemIndex].Id);
    }
}

var blockTreeItemPrev = "";
var calcedTreeIndex = "";
function OnObjTreeItemClick(index, objId, objTypeId) {
    var objMenuIts = document.getElementById("tree_objects");
    if (index == -1) {
        for (var i = 0; i < menuTreeObjects.length; i++) {
            if (objId.toString() == menuTreeObjects[i].toString()) {
                index = i;
                break;
            }
        }
    }
    calcedTreeIndex = index;
    if (blockTreeItemPrev == ("blockItemTree_" + index))
        return;
    if (blockTreeItemPrev != "") {
        var elementPrev = document.getElementById(blockTreeItemPrev);
        if (elementPrev == null)
            return;
        elementPrev.className = "objTreeItem";
    }
    var element = document.getElementById("blockItemTree_" + index);
    if (element == null) {
        return;
    }
    element.className = "objTreeItemSel";
    blockTreeItemPrev = "blockItemTree_" + index;
    switch (objTypeId) {
        case 3:case 5:case 12:
            location.replace("/Mimic?object=" + objId + "&ind=" + index + "&scr=" + objMenuIts.scrollTop);
            break;
        default:
            if (arguments.length == 4) {
                if (arguments[3]==-1) {
                    arguments[3] = index * 25;
                }
                objMenuIts.scrollTop = arguments[3];
            }
    }
}

function HideButtons() {
    ElVis("lessButtons", false);
    ElVis("hideButton", false);
    ElVis("showButton", true);
    var element = document.getElementById("tree_objects");
    if (element != null)
        element.style.height = "750px";
}

function ShowButtons() {
    ElVis("lessButtons", true);
    ElVis("hideButton", true);
    ElVis("showButton", false);
    var element = document.getElementById("tree_objects");
    if (element != null)
        element.style.height = "520px";
}

function SearchObject(event, objectName) {
    if (event.keyCode != 13)
        return;
    $.ajax({ type: "POST", url: "api/Objects/FindObject", data: { objectName: objectName }, async: true, success: after_SearchObject });
    var element = document.getElementById('nameOfObjectToFind');
    if (element != null)
        element.value = "";
}

function after_SearchObject(data) {
    if (data=="") ErrMessageXY("Объект не найден", 250, 450);
    else window.location.href = data;
}

function ShowOprosMenu() {
    $.ajax({
        type: "POST",
        url: "/api/Alarms/CheckPermission",
        data: { a: PERMISSION_RIGHT_ChangeOpros },
        async: true,
        success: s_ShowOprosMenu
    });
}
function s_ShowOprosMenu(data) {
    if (data) {
        var el = document.getElementById("setObjectOpr");
        var name=document.getElementById("value_agzuName");
        if (name)
            el.value = name.innerHTML;
        ElVis("areaOfObjectOpr", true);
    }
}

function MInterviewCycle() {
    var el = document.getElementById("setObjectOpr");
    if (el == null) return;
    Interview(el.value);
    el.value = "";
    ElVis("areaOfObjectOpr", false);
}
function MInterviewStop() {
    InterviewStop();
    SetInnerText("setObjectOpr", "");
    ElVis("areaOfObjectOpr", false);
}
function MInterviewAll() {
    InterviewAll();
    SetInnerText("setObjectOpr", "");
    ElVis("areaOfObjectOpr", false);
}
function MInterviewCancel() {
    SetInnerText("setObjectOpr", "");
    ElVis("areaOfObjectOpr", false);
}
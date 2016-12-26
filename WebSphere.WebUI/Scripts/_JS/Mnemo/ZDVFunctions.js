 

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

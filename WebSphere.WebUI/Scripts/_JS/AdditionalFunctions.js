function toDecInt(v){var r="";var vi=parseInt(v);var d=[];d.push((vi>>8)%256);d.push(vi%256);for(var i=0;i<d.length;i++){var h=d[i].toString(16);if(h.length<2)h="0"+h;r+=h;}return r;}
function GetLastAfter(v,s){var a=v.SplitText(s);if(a.length>0)return a[a.length-1];return "-1";}
function StringToBool(v){switch(v){case"False":case"false":case"0":case"no":case"No":return false;default:return true;}}
function FindIndex(a,v){for(var i=0;i<a.length;i++)if(a[i]==v)return i;return -1;}
function CrWin(m){var d=document.createElement("div");d.style.textAlign="center";d.style.padding="20px 50px 20px 50px";d.style.borderRadius="10px";d.style.fontWeight="bold";d.style.fontSize="20px";d.style.border="1px solid #000";d.style.boxShadow="3px 3px 10px #000";var t=2000;switch(arguments.length){case 2:d.style.backgroundColor=arguments[1];break;case 3:d.style.backgroundColor=arguments[1];d.style.color=arguments[2];break;case 4:d.style.backgroundColor=arguments[1];d.style.color=arguments[2];t=arguments[3];break;default:d.style.backgroundColor="#fff";d.style.color="#000";}d.innerHTML=m;d.style.position="fixed";d.style.left=($("html").width()-d.style.width)/2+"px";d.style.top=($("html").height()-d.style.height)/2+"px";document.body.appendChild(d);setTimeout(ElDel,t,d);}
function CrWinXY(m,x,y){var d=document.createElement("div");d.style.textAlign="center";d.style.padding="20px 50px 20px 50px";d.style.borderRadius="10px";d.style.fontWeight="bold";d.style.fontSize="20px";d.style.border="1px solid #000";d.style.boxShadow="3px 3px 10px #000";var t=2000;switch (arguments.length) {case 4:d.style.backgroundColor=arguments[3];break;case 5:d.style.backgroundColor=arguments[3];d.style.color=arguments[4];break;case 6:d.style.backgroundColor=arguments[3];d.style.color=arguments[4];t=arguments[5];break;default:d.style.backgroundColor = "#fff";d.style.color = "#000";}d.innerHTML=m;d.style.position="fixed";d.style.left=x+"px";d.style.top=y+"px";document.body.appendChild(d);setTimeout(ElDel,t,d);}
function ErrMessage(m){CrWin(m,"#ffa3a3");}function OkMessage(m){CrWin(m,"#a3ffa3");}
function ErrMessageXY(m,x,y){CrWinXY(m,x,y,"#ffa3a3");}function OkMessageXY(m,x,y){CrWinXY(m,x,y,"#a3ffa3");}

function SetInnerText(ei, t, d) { var e = document.getElementById(ei); if (e != null) { e.innerHTML = t; if (arguments.length == 3) SetStyle(ei,d);}; }
function SetValue(ei, t, d) { var e = document.getElementById(ei); if (e != null) { e.value = t; if (arguments.length == 3) SetStyle(ei, d); }; }
function GetValue(ei) { var e = document.getElementById(ei); if (e != null) {return e.value ;} else return null; }
function ParseToBool(val) {
    if (null==val) return null;
    if (isNaN(val)) { 
        val = val.replace(',','.');
        if (val.toLowerCase() == "true") return true;
        else if (val.toLowerCase() == "false") return false;
        else if (val == "1") return true;
        else if (val == "0") return false;
        else return null;
    } 
    else if (parseInt(val) % 2 == 1) return true;
    else if (parseInt(val) % 2 == 0) return false;
    else return null;
}
function GetStyle(TagsArray, tag, val) {

    var tagslimit = FindTag(tag, TagsArray); if (tagslimit == null) return "";
    var lo = tagslimit.lo;
    var hi = tagslimit.hi;
    var lolo = tagslimit.lolo;
    var hihi = tagslimit.hihi;

   // if (!lo) return "und"; lo = lo.ReplaceText(",", ".");
        //    if (hi == null || hi == "") return "und";
          //  hi = hi.ReplaceText(",", ".");

            if (val > hihi || val < lolo) return "alal";
            if (val > hi || val < lo) return "al";


            else return "good"; 
}

      function FindTag(nameKey, myArray){
          for (var i=0; i < myArray.length; i++) {
              if (myArray[i].tag === nameKey) {
                  return myArray[i];
              }
          } return null;
      }

 
function GetInnerText(ei) { var e = document.getElementById(ei); if (e != null) return e.innerHTML; return "#"; }
function GetComboBoxValue(ei){var e=document.getElementById(ei);if(e!=null)return e.options[e.selectedIndex].value;return 0;}
function GetComboBoxValueDir(e){return e.options[e.selectedIndex].value;}
function SetStyle(ei,cn){var e=document.getElementById(ei);if(e!=null)e.className=cn;}
function ElFoc(ei, f) { var e = document.getElementById(ei); if (e != null) f ? e.focus() : e.blur(); }
function ElDel(e) { e.parentNode.removeChild(e); }
function ElVis(ei, v) { var e = document.getElementById(ei); if (e != null) e.style.display = v ? "block" : "none"; }
function SetAttribute(ei,at,atv){var e=document.getElementById(ei);if(e==null)return;e.setAttribute(at,atv);}
String.prototype.ReplaceText=function(m,c){var s=this;var r="";for(var i=0;i<s.length;i++)if(s[i]==m)r+=c;else r+=s[i];return r;}
String.prototype.SplitText=function(m){var s=this;var r=new Array();var t="";for(var i=0;i<s.length;i++)if(s[i]==m){r.push(t);t="";}else t += s[i];r.push(t);return r;}
String.prototype.isFloat=function(){return(this-0);}
function Fixed(n,d){var a="1";for(var i=0;i<d;i++)a+="0";return Math.round(n*a)/a;}

function ProcFl(v, f,k) { if (v == null || v == "") return "---"; v = v.ReplaceText(",", "."); if (isNaN(v)) return "---";if (arguments.length == 3 && !isNaN(k)) {   v = v * k; } return Fixed(v, f);}

Array.prototype.Contains=function(v){for(var i=0;i<this.length;i++)if(this[i]==v)return true;return false;}

function RemNan(a){if(isNaN(a))return"---";return a;}

RemNan=function() {if(isNaN(this))return "---"; else return this;}


function GetSecondsFromTime(v) {
	if(v==null)return "";
    var dt = v.SplitText(" ");
    if (dt.length != 2)
        return "";
    var t = dt[1].SplitText(":");
    if (t.length != 3)
        return "";
    return t[2] * 1 + t[1] * 60 + t[0] * 3600;
}

function GetDateTimeFromTime(v) {
	if(v==null)return "";
    var r = new Date();
    r.setYear(r.getYear - 2);
    var dt = v.SplitText(" ");
    if (dt.length != 2) 
        return r;
    
    var t = dt[0].SplitText(".");
    if (t.length != 3) {
        return r;
    }
    r.year=t[2];
    r.month=t[1];
    r.day = t[0];
    
    return r;
}

String.prototype.ToNum=function() {
    var a = this;
    if (!a.isFloat) return "*";
    if (a == "NaN" || a == "nan" || a == "Null" || a == "null") return "*";
    return a;
}


function RemoveChilds(n) {
    var e = document.getElementById(n);
    if (e == null) return;
    while (e.childNodes.length)
        e.removeChild(e.childNodes[0]);
}

function GetOnlyTimeStrLabel_FromMinStr(m) {
    m = parseInt(m);
    var min = m % 60;
    var hour = parseInt(m / 60);
    var r = "";
    if (hour < 10) r += "0";
    r += (hour) + ":";
    if (min < 10) r += "0";
    r += min;
    return r;
}
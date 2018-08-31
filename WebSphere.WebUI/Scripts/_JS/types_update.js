
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
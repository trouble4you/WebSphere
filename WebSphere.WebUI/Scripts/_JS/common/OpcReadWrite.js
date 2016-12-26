function OpcRead(group, tag, d) {
    var result;
    if (arguments.length == 3)var c = d ? "D" : "";else c = "";
    $.ajax({ type: "POST", url: "api/OPC/ReadOpcTag"+c, data: { group: group, tag: tag}, async: false, success: function(returnedData) { result = returnedData; } });
    return result;
}

function OpcWrite(group, tag, value, d) {
    var data = false;
    if (arguments.length == 4) var c = d ? "D" : ""; else c = "";
    $.ajax({ type: "POST", url: "api/OPC/WriteOpcTag"+c, data: { group: group, tag: tag, value: value }, async: false, success: function(_data) { data = _data; } });
    return data;
}
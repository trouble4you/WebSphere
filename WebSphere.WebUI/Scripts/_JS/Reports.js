 
function journal_list_(data) {
    data = data;
    var select = document.getElementById("journals");
    for (var i = 0; i < data.length; i++) {
        var option = document.createElement('option');
        option.text = data[i].Name;
            option.value = data[i].Id;
        select.add(option, 0);
    }
}

function page_(data) {
    var Head = data.Head;
    var Rows = data.Rows;
    var colsCount = Head.length;
    var rowsCount = Rows.length;
    function getData(head,rows) { 
        var arr = [];
        for (var i = 0; i < rows.length; i++) {
            arr[i] = [];
            var row = rows[i];
            for (var j = 0; j < head.length; j++)
            {
                arr[i][head[j]] = row[j]; 
            }

        } 
        return arr;
    }
 

    function getColModels(names) {
        var colNames = names;
        var colModelsArray = [];
        for (var i = 0; i < colNames.length; i++) {
            var str;
            if (i === 1) {
                str = {
                    name: colNames[i],
                    index: colNames[i], width: '70px',
                    key: true, classes: 'lambda_zam_col', align: 'center',
                    editable: true
                };
            } else {
                str = {
                    name: colNames[i],
                    index: colNames[i], width: '70px', classes: 'lambda_zam_col', align: 'center',
                    editable: true
                };
            }
            colModelsArray.push(str);
        }

        return colModelsArray;
    }
    var journalTable = $("#journal_table");
    journalTable.jqGrid('GridUnload');
    $("#journal_table").jqGrid({
        label: "azaza",
        datatype: "local",
        data: getData(Head,Rows),
        colNames: Head,
        colModel: getColModels(Head),
        rowNum: rowsCount,
        height: "850px", shrinkToFit: true,
        loadonce: true,
        grouping: true,
        viewrecords: true,
        // toppager:true,
        scroll: true,
    });
    colsCount = colsCount * 70;
    var journal = document.getElementById('journal').offsetWidth;
    if (colsCount > journal)
        $('#journal_table').jqGrid('setGridParam', { shrinkToFit: false });

    $("#journal_table").setGridWidth(journal - 5);
    $("#journal_table").jqGrid('setGridHeight', $("#journal_div").height() - ($("#gbox_journal_table").height() - $('#gbox_journal_table .ui-jqgrid-bdiv').height()));
 

};

function PeriodRange_journal() {
    ch1 = document.getElementById('ch1');
    ch2 = document.getElementById('ch2');
    ch3 = document.getElementById('ch3');
    ch4 = document.getElementById('ch4');

    $('#journal_table').jqGrid('clearGridData');
    var start_date = document.getElementById("datetimepicker_start").value;
    var end_date = document.getElementById("datetimepicker_end").value;
    var journal = document.getElementById("journals").value;
    var obj = document.getElementById("objects").value;
    var type = document.getElementById("types").value;

    var d = { s_d: start_date, e_d: end_date, val2: obj };

    if (journal == 11) { 
        ch1.disabled = true; ch2.disabled = false; ch3.disabled = true; ch4.disabled = true;
        $.ajax({ type: "POST", url: "/api/zerozam/Get_br_ud_journal", data: { sp: "Journal_BR", obj: obj, objid: obj, StartDate: "'" + start_date + "'", EndDate: "'" + end_date + "'" }, async: true, success: page_Get_brud_journal });
    }
    if (journal == 10) {

        ch1.disabled = true; ch2.disabled = false;  ch3.disabled = true; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Getzamjournal", data: d, async: true, success: page_Getzamjournal });
    }
    else if (journal == 9) {

        ch1.disabled = true; ch2.disabled = false; ch3.disabled = false; ch4.disabled = true;

        $.ajax({ type: "POST", url: "api/zerozam/Getdavjournal", data: d, async: true, success: page_Getdavjournal });
    }
    else if (journal == 8) {

        ch1.disabled = true;        ch2.disabled = false;  ch3.disabled = true;        ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Getsvyazjournal", data: d, async: true, success: page_Getsvyazjournal });
    }
    else if (journal == 7) {

        $.ajax({ type: "POST", url: "api/zerozam/Get_ev_agzu_journal", data: d, async: true, success: page_Get_ev_agzu_journal });
    }
    else if (journal == 6) {

        ch1.disabled = true;        ch2.disabled = true; ch3.disabled = true;        ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/GetzamAGZUjournal", data: d, async: true, success: page_GetzamAGZUjournal });
    }
    else if (journal == 5) {

        ch1.disabled = true;        ch2.disabled = false; ch3.disabled = true;        ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Getotklzamjournal", data: d, async: true, success: page_Getotklzamjournal });
    } 
    else if (journal == 4) { 
        ch1.disabled = true; ch2.disabled = false; ch3.disabled = false; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Get_lost_zam_journal", data: d, async: true, success: page_Get_lost_zam });
    }

    else if (journal == 3) {

        ch1.disabled = true; ch2.disabled = false;  ch3.disabled = false; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/GetPSMjournal", data: d, async: true, success: page_PSM_journal });
    }
    else if (journal == 2) {
        ch1.disabled = true; ch2.disabled = false; ch3.disabled = true; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Get_Avg_Dav", data: d, async: true, success: page_AvgDav });
    }
    else if (journal == 1) {
        ch1.disabled = true; ch2.disabled = false;  ch3.disabled = true; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Get_Avg_Zam", data: d, async: true, success: page_AvgZam });
    }
    else if (journal == 0) {

        ch1.disabled = true; ch2.disabled = false; ch3.disabled = false; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/ProtocolOIS", data: d, async: true, success: page_ProtocolOIS });
    }
    else if (journal == 20) {

        ch1.disabled = false; ch2.disabled = false;  ch3.disabled = false; ch4.disabled = false;
        $.ajax({ type: "POST", url: "api/zerozam/Get_ost_journal", data: d, async: true, success: page_Get_ost_journal });
    }
    else if (journal == 19) {

        ch1.disabled = false; ch2.disabled = false;  ch3.disabled = false; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Get_sost_KTPN", data: d, async: true, success: page_Get_sost_KTPN });
    }
    else if (journal == 18) {

        ch1.disabled = false; ch2.disabled = false; ch3.disabled = false; ch4.disabled = false;
        $.ajax({ type: "POST", url: "api/zerozam/Get_sost_scv", data: d, async: true, success: page_Get_sost_scv });
    }
    else if (journal == 17) {

        var type = document.getElementById("addit").value; 
        var d = { s_d: start_date, e_d: end_date, val2: obj,param:type};
        ch1.disabled = false; ch2.disabled = false; ch3.disabled = false; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Get_ev_ktpn_journal", data: d, async: true, success: page_Get_ev_ktpn_journal });
    }
    else if (journal == 16) {

        ch1.disabled = false; ch2.disabled = false;  ch3.disabled = false; ch4.disabled = true;
        $.ajax({ type: "POST", url: "api/zerozam/Get_start_journal", data: d, async: true, success: page_Get_start_journal });
    }
    else if (journal == 15) {

        ch1.disabled = false; ch2.disabled = true; ch3.disabled = false; ch4.disabled = false;
        $.ajax({ type: "POST", url: "api/zerozam/Get_nowork_journal", data: d, async: true, success: page_Get_nowork_journal });
    }
    else if (journal == 14) {
        ch1.disabled = false; ch2.disabled = true; ch3.disabled = false; ch4.disabled = false;
        $.ajax({ type: "POST", url: "api/zerozam/Get_not_def_journal", data: d, async: true, success: page_Get_not_def_journal });
    }
    else if (journal == 13) {
        ch1.disabled = false; ch2.disabled = false;  ch3.disabled = false; ch4.disabled = false;
        $.ajax({ type: "POST", url: "api/zerozam/Get_narab_journal", data: d, async: true, success: page_Get_narab_journal });
    }
    else if (journal == 12) {

        var type = document.getElementById("addit").value;
        var t = document.getElementById("objects");

        var selectedText = t.options[t.selectedIndex].text;
        var e = { type: type, date: selectedText, obj: obj };
        $.ajax({ type: "POST", url: "api/zerozam/Get_imports", data: e, async: true, success: page_Get_imp_journal });
    }
}

function page_AvgZam(data) {
    var string = data.split('//++');
    var number = string[1].split(' /-/ ');
    ////alert(string[0]);
    var zz = number.length;
    ////alert(string[1]);
    function getData(data) {
        var params = [];
        params.push('АГЗУ');
        params.push('Скважина');
        params.push('Отвод');
        params.push('Дебит жидкости');
        params.push('Дебит нефти');
        var dates = string[0].split('//');
        for (j = 0; j < dates.length; j += 1) {
            params.push(dates[j]);
        }
        colcount = params.length;
        params.push('Отклонение');
        for (j = 0; j < params.length; j += 1) {

        }

        var datarow = [];
        var datar = data.split(' /-/ ');

        var n = datar.length;

        var arr = [];
        for (i = 0; i < n - 1; i += 1) {
            arr[i] = [];
            var datacell = datar[i].split("/_/");
            var c = datacell.length;
            for (j = 0; j < params.length; j += 1) {
                arr[i][params[j]] = datacell[j];
                var k = params[j];

                ////alert(arr[i][k]);
                //arr[i].push({k :datacell[j]});
                //datarow[i][datacell[j]] = datar[i];
                ////alert(date[i]);
                //keys.push(date[i]);
            }

        }
        ////alert(arr);
        return arr;
    }
    function getColNames(string) {
        var keys = [];
        var date = string.split('//');

        keys.push('АГЗУ');
        keys.push('Скважина');
        keys.push('Отвод');
        keys.push('Дебит жидкости');
        keys.push('Дебит нефти');
        var n = date.length;
        ////alert(n);
        for (i = 0; i < n; i += 1) {
            keys[date[i]] = date[i];
            ////alert(date[i]);
            keys.push(date[i]);
        }
        keys.push('Отклонение');

        return keys;
    }

    function getColModels(string) {
        var colNames = getColNames(string);
        var colModelsArray = [];
        for (var i = 0; i < colNames.length; i++) {
            var str;
            if (i === 1) {
                str = {
                    name: colNames[i],
                    index: colNames[i], width: '70px',
                    key: true, classes: 'lambda_zam_col', align: 'center',
                    editable: true
                };
            } else {
                str = {
                    name: colNames[i],
                    index: colNames[i], width: '70px', classes: 'lambda_zam_col', align: 'center',
                    editable: true
                };
            }
            colModelsArray.push(str);
        }

        return colModelsArray;
    }
    var journal = $("#journal_table");
    journal.jqGrid('GridUnload');
    $("#journal_table").jqGrid({
        //url: "user.json",
        //datatype: "json",
        datatype: "local",
        data: getData(string[1]),
        colNames: getColNames(string[0]),
        colModel: getColModels(string[0]),
        rowNum: zz,
        height: "850px", shrinkToFit: true,
        loadonce: true,
        grouping: true, 
        viewrecords: true,
        // toppager:true,
        scroll: true,
    });
    colcount = colcount * 70;
    var journal = document.getElementById('journal').offsetWidth;
    if (colcount > journal)
        $('#journal_table').jqGrid('setGridParam', { shrinkToFit: false });

    $("#journal_table").setGridWidth(journal - 5);
    $("#journal_table").jqGrid('setGridHeight', $("#journal_div").height() - ($("#gbox_journal_table").height() - $('#gbox_journal_table .ui-jqgrid-bdiv').height()));

    

};

function colls() {
    if (document.getElementById('coll').value == "Свернуть") {
        document.getElementById('coll').value = "Развернуть";
        $(".ui-icon-circlesmall-minus","table#journal_table").each(function () {
            $(this).trigger("click");

        });
    }
    else if (document.getElementById('coll').value == "Развернуть") {
        document.getElementById('coll').value = "Свернуть";
        $(".ui-icon-circlesmall-plus", "table#journal_table").each(function () {
            $(this).trigger("click");
        });
    }

}
  
 
 
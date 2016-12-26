/////////Update журнал замеров
function on_change_type() {

    var val1 = document.getElementById("types").value;
   if (val1 == 3) {
        $("#journals").empty();
        $("#journals").append($('<option value="10">Журнал замеров</option>'));
        $("#journals").append($('<option value="9">Журнал давлений</option>'));
        $("#journals").append($('<option value="8">Журнал состояния связи</option>'));
        $("#journals").append($('<option value="7">Хронология событий АГЗУ</option>'));
        $("#journals").append($('<option value="6">Журнал замеров по АГЗУ</option>'));
        $("#journals").append($('<option value="5">Журнал отклонившихся замеров</option>'));
        $("#journals").append($('<option value="4">Журнал по отсутствию замеров</option>'));
        $("#journals").append($('<option value="3">Журнал анализа ПСМ</option>'));
        $("#journals").append($('<option value="0">Протокол импорта из OIS</option>'));
        $("#journals").append($('<option value="2">Журнал среднесуточных давлений</option>'));
        $("#journals").append($('<option value="1">Журнал среднесуточных замеров</option>'));
       
    }
    if (val1 == 2 || val1 == 10) {
        $("#journals").empty();
        $("#journals").append($('<option value="20">Хронология остановок</option>'));
        $("#journals").append($('<option value="19">Текущее состояние КТПН </option>'));
        $("#journals").append($('<option value="18">Текущее состояние скважин</option>'));
        $("#journals").append($('<option value="17">Хронология событий КТПН</option>'));
        $("#journals").append($('<option value="16">Хронология пусков</option>'));
        $("#journals").append($('<option value="15">Скважины в нерабочих режимах</option>'));
        $("#journals").append($('<option value="14">Журнал неопределенных состояний скважин</option>'));
        $("#journals").append($('<option value="13">Наработка скважин</option>'));
        $("#journals").append($('<option value="12">История импорта режимов работы скважин</option>'));
        document.getElementById('ch1').checked = false;
        document.getElementById('ch2').checked = false;
        document.getElementById('ch3').checked = false;
        document.getElementById('ch4').checked = false;
    }

    var val1 = document.getElementById("types").value;
    var d = { val1: val1 };
    $.ajax({ type: "POST", url: "api/zerozam/getobjects", data: d, async: true, success: page_getobjects });

    if (val1==3) {
        document.getElementById('ch1').disabled = true;
    }
    else { document.getElementById('ch1').disabled = false; }
}


function page_getobjects(data) {
    var journals= document.getElementById("journals").value;
    var count = data.types.length;

    $("#objects").empty();

    if (count > 1) {
        if (journals != 3 && journals != 6 && journals != 12) { 
            //alert(journals);
            $("#objects").append($('<option value="99">-Bce</option>'));
        }
    }

    for (var j = 0; j < count; j++) {
        $("#objects").append($("<option value=" + data.types[j].id + ">" + data.types[j].name + "</option>"));
    }

}
function getobject() {
   
    if (document.getElementById("journals").value == 12) {
        document.getElementById('addit').style.display = "block";
        $("#addit").empty();
        $("#addit").append($("<option value=121> Текущий месяц</option>"));
        $("#addit").append($("<option value=122> Следующий месяц</option>")); 
                    $.ajax({ type: "POST", url: "api/zerozam/getdates", data: d, async: true, success: import_journal });
    }
    else if (document.getElementById("journals").value == 17) {
        document.getElementById('addit').style.display = "block";
        $("#addit").empty();
        $("#addit").append($("<option value=99> Все</option>"));
        $("#addit").append($("<option value=0> Питание от сети</option>"));
        $("#addit").append($("<option value=1> Состояние аккумулятора</option>"));
        $("#addit").append($("<option value=3> Доступ к КТП</option>"));
        $("#addit").append($("<option value=6> Связь</option>")); 
    }
    else if (document.getElementById("journals").value == 8) {
        var val1 = document.getElementById("types").value;
        var d = { val1: 8 };
        $.ajax({ type: "POST", url: "api/zerozam/getobjects", data: d, async: true, success: page_getobjects });

        document.getElementById('addit').style.display = "none";
    }
    else {
        var val1 = document.getElementById("types").value;
        var d = { val1: val1 };
        $.ajax({ type: "POST", url: "api/zerozam/getobjects", data: d, async: true, success: page_getobjects });

        document.getElementById('addit').style.display = "none"; 
    }
}
function import_journal(data) {
    var dates = data.split('//');
    var count = dates.length;  ;
    $("#objects").empty();
    for (var j = 0; j < count-1; j++) { 
        $("#objects").append($("<option value=" + dates[j]+ ">" + dates[j] + "</option>"));
    }
}
 

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

function page_PSM_journal(data) {
    $("#journal_table").jqGrid('clearGridData');
    var countmess = data.psm.length;
    $("#journal_table").setGridParam({ rowNum: countmess });
    $("#journal_table").setGridParam({ data: data.psm });

    /*
      for (var i = 0; i < countmess; i++) {
          $('#journal_table').jqGrid('addRowData', i,
              {
                  rtu: data.psm[i].rtu,
                  object_name: data.psm[i].object_name,
                  brig: data.psm[i].brig,
                  date: data.psm[i].date,
                  value: data.psm[i].value,
              }); 
      }
      */
     $("#journal_table").trigger("reloadGrid");

}

function page_ProtocolOIS(data) {
    $("#journal_table").jqGrid('clearGridData');
    var countmess = data.protocolOIS.length;
    $("#journal_table").setGridParam({ data: data.protocolOIS });

    /*
      for (var i = 0; i < countmess; i++) {
          $('#journal_table').jqGrid('addRowData', i,
              {
                  rtu: data.psm[i].rtu,
                  object_name: data.psm[i].object_name,
                  brig: data.psm[i].brig,
                  date: data.psm[i].date,
                  value: data.psm[i].value,
              }); 
      }
      */
    if (countmess > 5000) { $("#journal_table").setGridParam({ grouping: false }); }
    $("#journal_table").setGridParam({ rowNum: 1000 }).trigger("reloadGrid");

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
    ////alert(journal);

};
function page_AvgDav(data) {
    var string = data.split('//++');
    var number = string[1].split(' /-/ ');
    ////alert(string[0]);
    var zz = number.length;
    ////alert(string[0]);
    var colcount = 5;
    ////alert(string[1]);
    function getData(data) {
        var params = [];
        params.push('АГЗУ');
        params.push('Минимум');
        params.push('Максимум');
        var dates = string[0].split('//');
        for (j = 0; j < dates.length; j += 1) {
            params.push(dates[j]);
        }
        colcount = params.length;
        params.push('Давление, атм');
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
                 
            }

        } 
        return arr;
    }
    function getColNames(string) {
        var keys = [];
        var date = string.split('//');

        keys.push('АГЗУ');
        keys.push('Минимум');
        keys.push('Максимум');
        var n = date.length;
        ////alert(n);
        for (i = 0; i < n; i += 1) {
            keys[date[i]] = date[i];
            ////alert(date[i]);
            keys.push(date[i]);
        }
        keys.push('Давление, атм');

        return keys;
    }
    function getColModels(string) {
        var colNames = getColNames(string);
        var colModelsArray = [];
        for (var i = 0; i < colNames.length; i++) {
            var str;
            if (i === 0) {
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
    if (colcount > journal) {
        $('#journal_table').jqGrid('setGridParam', { shrinkToFit: false });
        $('#journal_table').jqGrid('setGridParam', { autowidth: true });
        $('#journal_table').jqGrid('setGridParam', { forceFit: true });
    }

    $("#journal_table").setGridWidth(journal - 5);
    
};
function page_Get_imp_journal(data) {
    var countmess = data.imp.length;
    ////alert(countmess);
    $("#journal_table").setGridParam({ rowNum: countmess });


    for (var j = 0; j < countmess; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            {
                dt: data.imp[j].date,
                impdt: data.imp[j].date_imp,
                well :data.imp[j].well,
            d1 :data.imp[j].d1, 
            d2 :data.imp[j].d2, 
            d3 :data.imp[j].d3, 
            d4 :data.imp[j].d4, 
            d5 :data.imp[j].d5, 
            d6 :data.imp[j].d6, 
            d7 :data.imp[j].d7, 
            d8 :data.imp[j].d8, 
            d9 :data.imp[j].d9, 
            d10:data.imp[j].d10,
            d11:data.imp[j].d11,
            d12:data.imp[j].d12,
            d13:data.imp[j].d13,
            d14:data.imp[j].d14,
            d15:data.imp[j].d15,
            d16:data.imp[j].d16,
            d17:data.imp[j].d17,
            d18:data.imp[j].d18,
            d19:data.imp[j].d19,
            d20:data.imp[j].d20,
            d21:data.imp[j].d21,
            d22:data.imp[j].d22,
            d23:data.imp[j].d23,
            d24:data.imp[j].d24,
            d25:data.imp[j].d25,
            d26:data.imp[j].d26,
            d27:data.imp[j].d27,
            d28:data.imp[j].d28,
            d29:data.imp[j].d29,
            d30:data.imp[j].d30,
            d31:data.imp[j].d31,
            });
    }

    $("#load_journal_table").css("display", "none");
}
function page_Get_lost_zam(data) {
    var countmess = data.lost_zam.length; 
    $("#journal_table").setGridParam({ rowNum: countmess }); 
    for (var j = 0; j < data.lost_zam.length; j++) {

        $('#journal_table').jqGrid('addRowData', j,
            {
                date: data.lost_zam[j].date,
                object_name: data.lost_zam[j].object_name,
                brig: data.lost_zam[j].brig,
            }); 
    } 
    $("#journal_table").trigger("reloadGrid"); 
}
function page_Get_nowork_journal(data) {

    var countmess = data.no_work.length;
    $("#journal_table").setGridParam({ rowNum: countmess });
    for (var j = 0; j < countmess; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            {
                ceh: data.no_work[j].ceh,
                pl: data.no_work[j].pl,
                brig: data.no_work[j].brig,
                object_name: data.no_work[j].object_name,
                ktpn: data.no_work[j].ktpn,
                well: data.no_work[j].well,
                type: data.no_work[j].type,
                state: data.no_work[j].sost,
                regim: data.no_work[j].regim,
                time: data.no_work[j].date,
            });

    }

    $("#journal_table").trigger("reloadGrid");
}
function page_Get_sost_scv(data) {
    var countmess = data.sostscv.length;

    $("#journal_table").setGridParam({ rowNum: countmess }).trigger("reloadGrid");
    for (var j = 0; j < data.sostscv.length; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            {

                pl: data.sostscv[j].pl,
                brig: data.sostscv[j].brig,
                ceh: data.sostscv[j].ceh,
                object_name: data.sostscv[j].object_name,
                ktpn: data.sostscv[j].ktpn,
                well: data.sostscv[j].well,
                Grejim: data.sostscv[j].Grejim,
                Trejim: data.sostscv[j].Trejim,
                sostBD: data.sostscv[j].sostBD,
                sost: data.sostscv[j].sost,
                link: data.sostscv[j].link,
                date: data.sostscv[j].date,
                soot: data.sostscv[j].soot,
            });
    }

    $("#journal_table").trigger("reloadGrid");

}
function page_Get_not_def_journal(data) {

    var countmess = data.not_def.length;
    $("#journal_table").setGridParam({ rowNum: countmess });
    for (var j = 0; j < countmess; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            {
                ceh: data.not_def[j].ceh,
                pl: data.not_def[j].pl,
                brig: data.not_def[j].brig,
                object_name: data.not_def[j].object_name,
                ktpn: data.not_def[j].ktpn,
                well: data.not_def[j].well,
                date_st: data.not_def[j].date_st,
                date_end: data.not_def[j].date_end,
                duration: data.not_def[j].duration,
                reason: data.not_def[j].reason,
            });

    }

    $("#load_journal_table").css("display", "none");
}
function page_GetzamAGZUjournal(data) {


    $('#journal_table').jqGrid('footerData', 'set', {
         dt: 'Режим',
         otv1: data.sums[0].sum1,
         otv2: data.sums[0].sum2,
         otv3: data.sums[0].sum3,
         otv4: data.sums[0].sum4,
         otv5: data.sums[0].sum5,
         otv6: data.sums[0].sum6,
         otv7: data.sums[0].sum7,
         otv8: data.sums[0].sum8,
         otv9: data.sums[0].sum9,
        otv10: data.sums[0].sum10,
        otv11: data.sums[0].sum11,
        otv12: data.sums[0].sum12,
        otv13: data.sums[0].sum13,
        otv14: data.sums[0].sum14,
    });
    var countmess = data.journal_zam_agzu.length;

    for (var j = 0; j < countmess; j++) {

        $('#journal_table').jqGrid('addRowData', j,
            {
                dt: data.journal_zam_agzu[j].date,

                otv1: data.journal_zam_agzu[j].otv1,
                otv2: data.journal_zam_agzu[j].otv2,
                otv3: data.journal_zam_agzu[j].otv3,
                otv4: data.journal_zam_agzu[j].otv4,
                otv5: data.journal_zam_agzu[j].otv5,
                otv6: data.journal_zam_agzu[j].otv6,
                otv7: data.journal_zam_agzu[j].otv7,
                otv8: data.journal_zam_agzu[j].otv8,
                otv9: data.journal_zam_agzu[j].otv9,
                otv10: data.journal_zam_agzu[j].otv10,
                otv11: data.journal_zam_agzu[j].otv11,
                otv12: data.journal_zam_agzu[j].otv12,
                otv13: data.journal_zam_agzu[j].otv13,
                otv14: data.journal_zam_agzu[j].otv14,
            });
    }

    $("#journal_table").trigger("reloadGrid");



}
function page_Getotklzamjournal(data) {
    var countmess = data.zam.length;
    
    $("#journal_table").setGridParam({ rowNum: countmess }).trigger("reloadGrid");
    for (var j = 0; j < data.zam.length; j++) {
        
        $('#journal_table').jqGrid('addRowData', data.zam[j].id,
            {
                dt: data.zam[j].dt,
                object_name: data.zam[j].object_name,
                well: data.zam[j].well,
                bend: data.zam[j].bend,
                rejim: data.zam[j].rejim,
                Grejim: data.zam[j].Grejim,
                Gzamer: data.zam[j].Gzamer,
                b: data.zam[j].b,
                type: data.zam[j].type,
               // brig: data.zam[j].brig,
               // ceh: data.zam[j].ceh,
            });
    }

    $("#journal_table").trigger("reloadGrid");
    for (var j = 0; j < data.zam.length; j++) {
        $('#journal_table').jqGrid('setRowData', data.zam[j].id, false, data.zam[j].style);
    }
}
function page_Getzamjournal(data) {
    var countmess = data.journal_zam.length;

    $("#journal_table").setGridParam({ rowNum: countmess });
    
    $("#journal_table").setGridParam({ data: data.journal_zam });
   /*  
    for (var j = 0; j < data.journal_zam.length; j++) { 
        $('#journal_table').jqGrid('addRowData', data.journal_zam[j].id,
            { 
                object_name: data.journal_zam[j].object_name,
                well: data.journal_zam[j].well,
                bend: data.journal_zam[j].bend,   
                st_dt: data.journal_zam[j].st_dt,
                duration: data.journal_zam[j].duration,
                end_dt: data.journal_zam[j].end_dt,
                //brig: data.journal_zam[j].brig,
                //ceh: data.journal_zam[j].ceh,
                vns_v_z:  data.journal_zam[j].vns_v_z ,
                vns_m_z : data.journal_zam[j].vns_m_z , 
      n_v_z   :data.journal_zam[j].n_v_z  , 
      v_v_z   :data.journal_zam[j].v_v_z  , 
      n_m_z   :data.journal_zam[j].n_m_z  , 
      v_m_z   :data.journal_zam[j].v_m_z  , 
      vns_v_r :data.journal_zam[j].vns_v_r,
      vns_m_r :data.journal_zam[j].vns_m_r,
      n_v_r   :data.journal_zam[j].n_v_r  ,
      v_v_r   :data.journal_zam[j].v_v_r  ,
      n_m_r   :data.journal_zam[j].n_m_r  ,
      v_m_r: data.journal_zam[j].v_m_r,

      n_m_z: data.journal_zam[j].n_m_r,
      v_m_z: data.journal_zam[j].v_m_r,
            });
        
    }
*/
    $("#journal_table").trigger("reloadGrid");
     


}
function page_Getdavjournal(data) {
    var countmess = data.journal_dav.length;

    $("#journal_table").setGridParam({ rowNum: countmess });


    for (var j = 0; j < data.journal_dav.length; j++) {
        $('#journal_table').jqGrid('addRowData', data.journal_dav[j].id,
            {
                date: data.journal_dav[j].date,
                object_name: data.journal_dav[j].object_name,
                value: data.journal_dav[j].value,
                time: data.journal_dav[j].time,
                min: data.journal_dav[j].min,
                max: data.journal_dav[j].max,
                brig: data.journal_dav[j].brig,
                ceh: data.journal_dav[j].ceh,
            });
    }

    $("#journal_table").trigger("reloadGrid");
    for (var j = 0; j < data.journal_dav.length; j++) {
        $('#journal_table').jqGrid('setRowData', data.journal_dav[j].id, false, data.journal_dav[j].style);
    }

}
function page_Get_ev_ktpn_journal(data) {
    var countmess = data.journal_ev_ktpn.length;

    $("#journal_table").setGridParam({ rowNum: countmess }).trigger("reloadGrid");
    $("#journal_table").setGridParam({ data: data.journal_ev_ktpn });
 /*   for (var j = 0; j < countmess; j++) {
        $('#journal_table').jqGrid('addRowData', data.journal_ev_ktpn[j].id,
            {
                date: data.journal_ev_ktpn[j].date,
                ktpn: data.journal_ev_ktpn[j].ktpn,
                param: data.journal_ev_ktpn[j].param,
                status: data.journal_ev_ktpn[j].status,
                brig: data.journal_ev_ktpn[j].brig,
                
            });
    }
    */
    $("#journal_table").trigger("reloadGrid");

}
function page_Get_ev_agzu_journal(data) {
    var countmess = data.journal_ev_agzu.length;

    $("#journal_table").setGridParam({ rowNum: countmess }).trigger("reloadGrid");

    $("#journal_table").setGridParam({ data: data.journal_ev_agzu });
    $("#journal_table").trigger("reloadGrid");

}
function page_Getsvyazjournal(data) {
    var countmess = data.journal_svyaz.length;

    $("#journal_table").setGridParam({ rowNum: countmess }) ;

     for (var j = 0; j < data.journal_svyaz.length; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            {
                date_st: data.journal_svyaz[j].date_st,
                object_name: data.journal_svyaz[j].object_name,
                date_end: data.journal_svyaz[j].date_end,
                time: data.journal_svyaz[j].time, 
            });
    }
    
    $("#journal_table").trigger("reloadGrid");



}
function page_Get_ost_journal(data) {
    var countmess = data.journal_ost.length;

    $("#journal_table").setGridParam({ rowNum: countmess }).trigger("reloadGrid");
    $("#journal_table").setGridParam({ data: data.journal_ost });
    /*for (var j = 0; j < data.journal_ost.length; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            {
                object_name: data.journal_ost[j].object_name,
                pl: data.journal_ost[j].pl,
                date_st: data.journal_ost[j].date_st,
                ktpn: data.journal_ost[j].ktpn,
                date_end: data.journal_ost[j].date_end, 
                brig: data.journal_ost[j].brig,
                ceh: data.journal_ost[j].ceh,
                ceh: data.journal_ost[j].ceh,
                time: data.journal_ost[j].time,
                well: data.journal_ost[j].well,
            });
    }*/

    $("#journal_table").trigger("reloadGrid");
 
}
function page_Get_start_journal(data) {
    var countmess = data.journal_ost.length;

    $("#journal_table").setGridParam({ rowNum: countmess }).trigger("reloadGrid");
    $("#journal_table").setGridParam({ data: data.journal_ost });
    /*
    for (var j = 0; j < data.journal_ost.length; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            {
                object_name: data.journal_ost[j].object_name,
                pl: data.journal_ost[j].pl,
                date_st: data.journal_ost[j].date_st,
                ktpn: data.journal_ost[j].ktpn,
                date_end: data.journal_ost[j].date_end,
                brig: data.journal_ost[j].brig,
                ceh: data.journal_ost[j].ceh,
                time: data.journal_ost[j].time,
                well: data.journal_ost[j].well,
            });
    }
    */
    $("#journal_table").trigger("reloadGrid");

}
function page_Get_sost_KTPN(data) {
    var countmess = data.sost_ktpn.length;

    $("#journal_table").setGridParam({ rowNum: countmess }).trigger("reloadGrid");
    for (var j = 0; j < data.sost_ktpn.length; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            {
               
                brig: data.sost_ktpn[j].brig,
                ceh: data.sost_ktpn[j].ceh,
                object_name: data.sost_ktpn[j].object_name,
                ktpn: data.sost_ktpn[j].ktpn,
                link: data.sost_ktpn[j].link,
                date: data.sost_ktpn[j].date,
                door: data.sost_ktpn[j].door,
                power: data.sost_ktpn[j].power,
                battery: data.sost_ktpn[j].battery,
                col: data.sost_ktpn[j].col,
                work: data.sost_ktpn[j].work,
            });
    }

    $("#journal_table").trigger("reloadGrid");

}
function page_Get_narab_journal(data) {
    var countmess = data.narab.length;

    $("#journal_table").setGridParam({ rowNum: countmess }).trigger("reloadGrid");
    for (var j = 0; j < countmess; j++) {
        $('#journal_table').jqGrid('addRowData', j,
            { 
                ceh: data.narab[j].ceh,                     
                brig: data.narab[j].brig,                  
                pl: data.narab[j].pl,               
                object_name: data.narab[j].object_name,  
                ktpn: data.narab[j].ktpn,             
                well: data.narab[j].well,               
                oil: data.narab[j].oil,             
                time: data.narab[j].time,                  
                type: data.narab[j].type,           
                stop: data.narab[j].stop,           
                work: data.narab[j].work,           
                wait: data.narab[j].wait,           
                vsp: data.narab[j].vsp,             
                csp: data.narab[j].csp,             
                rr: data.narab[j].rr,               
                notdef: data.narab[j].notdef,      
            });                                     
        }                                           

    $("#journal_table").trigger("reloadGrid");

}
/////////Update журнал замеров

var global_last_alarm = 0;


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
  

global_zam_time = 12;

function Kvit_it_all(f) {
    var agree = confirm(f);
        if (!agree) return; 
    var r = $.ajax({ type: "POST", url: "api/Alarms/Kvit_it_all", async: true, success: page_Kvit_it_all });
}

function page_Kvit_it_all(data) {
    $("#lastmess_table").trigger("reloadGrid");
}

function Kvit_Alarm(global_last_mess_id, f) {

    var d = { id: global_last_mess_id };

    //alert(f);
    var r = $.ajax({ type: "POST", url: "api/Alarms/Kvit_Alarm", data: d, async: true, success: page_Kvit_Alarm });
}

function page_Kvit_Alarm(data) {
    $("#lastmess_table").trigger("reloadGrid");
}

function page_GetLast(data) {


    var countzam = data.zam.length;

    $('#lastzam_table').jqGrid('clearGridData');

    $("#lastzam_table").setGridParam({ data: data.zam });
/*
    for (var j = 0; j < data.zam.length; j++) {
        $('#lastzam_table').jqGrid('addRowData', data.zam[j].id,
            {
                dt: data.zam[j].dt,
                object_name: data.zam[j].object_name,
                well: data.zam[j].well,
                bend: data.zam[j].bend,
                rejim: data.zam[j].rejim,
                Grejim: data.zam[j].Grejim,
                val_2: data.zam[j].Gzamer,
                b: data.zam[j].b,
                type: data.zam[j].type
            });
    }
    */
    $("#lastzam_table").setGridParam({ rowNum: countzam }).trigger("reloadGrid");

    for (var j = 0; j < data.zam.length; j++) {
        $('#lastzam_table').jqGrid('setRowData', data.zam[j].id, false, data.zam[j].style);
    }
}

/////////Update журнал замеров

function GetLast() {

    var d = { date: global_zam_time };
    $.ajax({ type: "POST", url: "api/Zerozam/GetLastmess", data: d, async: true, success: page_GetLast });

}

function Zam_Param(date) {
    global_zam_time = date;
    GetLast();
}

function StartTimerLast() {

    $("#lastzam_table").jqGrid('setGridHeight', $("#zam_div").height() - ($("#gbox_lastzam_table").height() - $('#gbox_lastzam_table .ui-jqgrid-bdiv').height()));
    $("#lastalarms_table").jqGrid('setGridHeight', $("#mess_div").height() - ($("#gbox_lastalarms_table").height() - $('#gbox_lastalarms_table .ui-jqgrid-bdiv').height()));
    $("#lastzam_table").jqGrid('setGridWidth', $("#zam_div").width());
    $("#lastalarms_table").jqGrid('setGridWidth', $("#mess_div").width());

    var journal = document.getElementById('zam_div').offsetWidth;
    $("#lastzam_table").setGridWidth(journal - 5);
    setInterval('GetLast()', 60000);

   // setInterval('PeriodRange_journal()', 360000);
}

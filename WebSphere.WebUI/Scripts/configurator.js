function CheckUserPropsValidity(data) {
    if (data != "allGoog") {
        data = '{ "name": "John" }';
        var objProps = JSON.parse(data);
    }
    var content = $('#StdPropsValidityError').val();
    if (content != null) {
        var parseContent = JSON.parse(content);
        alert(content);
        var errorCount = Object.keys(parseContent);
        //for(var i=0; i<errorCount; i++)
        //for (var key in parseContent)
        for (var i = 0; i < parseContent.length; i++) {
            var wrongElem = $('[name="' + parseContent[i].Name + '"]');
            wrongElem.val(parseContent[i].Value);
            wrongElem.addClass('error');
            var labelContent = "<label class='error'>" + parseContent[i].Description + "</label>";
            //$('<label>').appendTo(wrongElem);
            wrongElem.after(labelContent);

        }
    }


}

//от настроек тега
var saveFlag = 0;
var activeTabName;


//при сохранении содержимого делает активной последнюю кликнутую вкладку. А не первую по умолчанию.
function saveTabsState() {
    $('.tabs ul .active').removeClass('active');
    //$('.tab-pane .active').removeClass('active');
    $('[href="' + activeTabName + '"]').parent().addClass('active');
    var href = activeTabName;
    var nodeType = $("#tagInfo form").attr("id");
    switch(activeTabName)
    {
        case "#commonPropTab":
            $("#commonPropTab div").show();
            break;
        case "#AlarmTab":
            showAlarmTab(nodeType);
            break;
        case "#HistoryTab":
            showHistoryTab(nodeType);
            break;
        case "#signalsTab":
            showSignalsTab();
            break;
        case "#rankTab":
            showRankTab();
            break;

    }
    //если активная не вкладка с общими свойствами, то скрыть кнопку добавления свойств
    if (activeTabName != "#commonPropTab") {
        $("#addNoTypeNodePropsBtn").css('display', 'none');
        $("#addObjPropsBtn").css('display', 'none'); 
    }


    var idActiveContainer = activeTabName.substring(1);
    //$('#' + idActiveContainer).addClass('active');
}
//собирает все свойства в строку, которую закидывает в Value для скрытого input #special модели
function getAdditionalProps() {
    var toHiddenInput = "";
    var userCounter = 0;
    var propsArr = new Object();
    //собираем строку для пользовательских свойств
    $('[data-description="userProp"]').each(function () {
        var Name = $(this).attr("name");
        var propStr = "";
        //если свойство строковое
        if ($(this).attr("data-type") == 10) {
            propStr = ",\"" + $(this).attr("name") + "\":\"" + $(this).val() + "\"";
        }
        //если свойство float или double
        else if ($(this).attr("data-type") == 7 || $(this).attr("data-type") == 8) {
            var replComma = ($(this).val()).replace(",", ".");
            propStr = ",\"" + $(this).attr("name") + "\":" + replComma;
        }
        else {
            propStr = ",\"" + $(this).attr("name") + "\":" + $(this).val();
        }
        //поскольку в документе всегда будет 2 input'а с одним именем,т.к. стандарное свойство может содержаться
        //как в общей вкладке, так и во вкладке модулей, то предотвратим повторную запись свойства в строку со свойствами
        //var repeatChecker = ",\"" + $(this).attr("name") + "\":";
        //if (toHiddenInput.indexOf(repeatChecker) == -1)
            toHiddenInput += propStr;
    });
    //Раскидываем строку по инпутам. Так как формы разные, то кажется каждой форме нужен свой input
    $('#specialInput').val(toHiddenInput);
}


//пересобирает строку с пользовательскими свойствами при изменении 
$('#TagPropsTab [data-description="userProp"]').on("change", function () {
    getAdditionalProps();
});
//скрывает диалоговое окно и чистит содержимое контейнера
function fadeDialogWindow() {
    $('#SaveModal').modal('hide');
    $('#dialogToAddModule').empty();
    $('.modal-backdrop.fade.in').remove();
    //ниже было закомменчено
    //$("#SaveModal").remove();
}

function showAlarmTab(nodeName) {

    $("#commonPropTab div").hide();
    //alert(propsAlarm.length);
    switch (nodeName) {
        case "NoTypeNodePropsTab":
            {
                for (var prop in propsAlarm) {
                    $('#' + propsAlarm[prop]).show();
                }
                $(".deleteProp").hide();
                break;
            }
        case "TagPropsTab":
            {
                var alarmArr = ["Alarm_IsPermit", "HiHiText", "HiText", "NormalText", "LoText", "LoLoText", "HiHiSeverity", "HiSeverity", "LoSeverity",
               "LoLoSeverity"];
                for (var prop in alarmArr) {
                    $('#' + alarmArr[prop]).show();
                }
            }
    }
}

function showHistoryTab(nodeName) {
    $("#commonPropTab div").hide();
    switch (nodeName) {
        case "NoTypeNodePropsTab":
            {
                for (var prop in propsHistory) {
                    $('#' + propsHistory[prop]).show();
                }
                $(".deleteProp").hide();
                break;
            }
        case "TagPropsTab":
            {
                var historyArr = ["History_IsPermit", "RegPeriod", "Deadbend"];
                for (var prop in historyArr) {
                    $('#' + historyArr[prop]).show();
                }
            }
    }
}




//сюда приходит обновленное содержимое настроек узла после сохранения или отмены сохранения
function reloadTabs(data) {
    $('#tagInfo').empty().html(data);
    saveTabsState();
    //fadeDialogWindow();
}
//Делает недоступными iput-ы при сброшенном checkbox Разрешено
function disableInputs(modName, state) {
    //alert("disableInputs " + modName + state);
    if (modName == "Alarm_IsPermit") {
        if (state == "True") {
            $('#AlarmTab input:not(#Alarm_IsPermit):not([type="submit"])').removeAttr('disabled');
            $('#hiddenAlarm_IsPermit').val(true);
        }
        else {
            $('#AlarmTab input:not(#Alarm_IsPermit):not([type="submit"])').attr('disabled', 'disabled');
            $('#hiddenAlarm_IsPermit').val(false);
        }
    }
    if (modName == "History_IsPermit") {
        if (state == "True") {
            $('#HistoryTab input:not(#History_IsPermit):not([type="submit"])').removeAttr('disabled');
            $('#hiddenHistory_IsPermit').val(true);
        }
        else {
            $('#HistoryTab input:not(#History_IsPermit):not([type="submit"])').attr('disabled', 'disabled');
            $('#hiddenHistory_IsPermit').val(false);
        }
    }
}

//от настроек тега


$(document).ready(function () {
    //сбрасывает флаг изменения содержания формы после нажатия на кнопку сохранения
    //$(document).on("mouseup", '#saveObjPropsBtn', function () {
    //    saveFlag = 0;
    //});
    $(document).on("submit", '#tagInfo form', function () {
        saveFlag = 0;
    });
    //чтобы кнопка добавить свойство была видна только для вкладки общих свойств
    $(document).on("click", "#tagInfo li", function () {
        var ahref = $(this).children().attr('href');
        //alert(ahref);
        if (ahref == "#commonPropTab") {
            $("#addObjPropsBtn").show();
            $('#addNoTypeNodePropsBtn').show();

        }
        else {
            $("#addObjPropsBtn").hide();
            $('#addNoTypeNodePropsBtn').hide();
        }
    });
    //показывает кнопку удалить, если есть выбранные для удаления пользовательские свойства
    $(document).on("change", ".deleteProp", function () { 
    //$('.deleteProp').change(function () {
        if ($('.deleteProp:checked').length > 0) {
            $('#deleteObjPropsBtn').show();

        }
        else {
            $('#deleteObjPropsBtn').hide();
        }

    });

    //слушатель удаления свойств
    $(document).on("click", "#deleteObjPropsBtn", function () {
        //saveFlag = 0;
        var currentClickId = $('[aria-selected="true"]');
        var currentClickId1 = parseInt(currentClickId.attr('id'));
        var deletePropsArr = new Array();
        $('.deleteProp:checked').each(function (i, elem) {
            var propName = $(this).attr("name");
            deletePropsArr.push($(this).attr("name"));
        });
        //alert(deletePropsArr[0]);
        $.ajax({
            type: "POST",
            url: '/Configurator/DeleteProps',
            data: { deletePropsArr: deletePropsArr, id: currentClickId1 },
            //async: true,
            async: false,
            success: function (data) { $('#tagInfo').html(data); },
            error: function () {
                alert("Неудача при отправке аякс запроса при удалении свойства");
            }
        });
        return false;
    });
    //слушатель добавления свойств
    $(document).on("click", "#addNoTypeNodePropsBtn", function () {
        var currentClickId = $('[aria-selected="true"]');
        var currentClickId1 = currentClickId.attr('id');
        //$('#dialogToAddModule').load('@Url.Action("AddPropDialogTypedNode", "Configurator")?idNode=' + currentClickId1);
        $('#dialogToAddModule').load('/Configurator/AddPropDialog?idNode=' + currentClickId1);
    });
    //слушатель добавления свойств
    $(document).on("click", "#addObjPropsBtn", function () {
        var currentClickId = $('[aria-selected="true"]');
        var currentClickId1 = currentClickId.attr('id');
        //$('#dialogToAddModule').load('@Url.Action("AddPropDialogTypedNode", "Configurator")?idNode=' + currentClickId1);
        $('#dialogToAddModule').load('/Configurator/AddPropDialogTypedNode?idNode=' + currentClickId1);
    });

    //кнопка сохранить диалогового окна сохранения изменений
    $("#dialogToAddModule").on("click", "#saveChangesBtn", function () {
        //обнуляем флаг изменения
        saveFlag = 0;
        var nodeType = $('#tagInfo form').attr('id');
        if (nodeType == "TagPropsTab")
        {
            var url = '/Configurator/EditTagProps';
        }
        if (nodeType == "NoTypeNodePropsTab")
        {
            var url = '/Configurator/EditNoTypesProps';
        }
            
        $.ajax({
            type: "POST",
            url: url,
            data: $('#' + nodeType).serialize(),
            async: false,
            success: reloadTabs,
            error: function () {
                alert("Неудача при отправке аякс запроса при сохранения сод-го вкладок");
            }
        });
        fadeDialogWindow();
    });
    //кнопка отменить диалогового окна сохранения изменений
    $("#dialogToAddModule").on("click", "#forgetChangesBtn", function () {
        saveFlag = 0;
        var currentClickId = $('[aria-selected="true"]');
        var currentClickId1 = parseInt(currentClickId.attr('id'));
        $.ajax({
            type: "GET",
            url: '/Configurator/showTabProps',
            data: { id: currentClickId1 },
            async: true,
            success: reloadTabs,
            error: function () {
                alert("Неудача при отправке аякс запроса при сохранения сод-го вкладок");
            }
        });
        fadeDialogWindow();
    });

    //слушатель кликов на ссылки
    $(document).on("click", "li a", function (e) {
        var df = $(this);
        //если ссылка распложения на вкладке, то проверяем состоние флага изменения полей ввода
        if ($(this).hasClass("liTab"))
        {
            activeTabName = $(this).attr('href');
            //если содержимое какого-либо инпута было изменено
            //alert("saveFlag " + saveFlag);
            if (saveFlag == 1) {
                $('#dialogToAddModule').load('/Configurator/SaveDialog');
            }
        }
        else {
            //обнуление флага при щелке на любые другие ссылки. При этом изменения не будут сохранены и диалоговое окно не всплывет. 
            //Потом предотвратить потерю данных
            saveFlag = 0;
        }
    });

    //disableInputs("Alarm_IsPermit", '@Model.Alarm_IsPermit');
    //disableInputs("History_IsPermit", '@Model.History_IsPermit');

    //слушает изменения полей ввода и выставляет флаг в 1
    //$(document).on("change", "#HistoryTab input, #AlarmTab input, #commonPropTab input:not('.deleteProp'), #rankTab input, #signalsTab input", function () {
    $(document).on("change", "#HistoryTab input, #AlarmTab input, #commonPropTab input:not('.deleteProp'),#commonPropTab select, #rankTab input, #signalsTab input, #signalsTab select", function () {
    //$('#HistoryTab input, #AlarmTab input, #commonPropTab input, #rankTab input, #signalsTab input').on("change", function () {
        //alert("Пойманы изменения");
        //при изменении инпута, выставляем флаг в 1
        saveFlag = 1;
        var ff = $(this);
        if(this.hasAttribute("data-description"))
        {
            getAdditionalProps();
        }
        var jj = $(this).attr('type');
        if($(this).attr('type')=="checkbox")
        {
            var id = $(this).attr("id");
                    //alert("первая" + name);
                    //var val = $(this).val();
                    ////изменим величину свойства на всех вкладках
                    //$('[type="checkbox"][id=' + name + ']').each(function () {
                    //    //alert("переприсвоение величины" + val);
                    //    $(this).val(val);
                    //});
                    //var name = $(this).attr("id");
                    if ($(this).is(":checked")) {
                        //alert("checked");
                        disableInputs(id, "True");
                    }
                    else {
                        //alert("notchecked");
                        //сделаем недоступными поля ввода
                        disableInputs(id, "False");
                    }
        }

    });



    $(document).on("click", "[href='#AlarmTab']", function () {
        var nodeType = $("#tagInfo form").attr("id");
        showAlarmTab(nodeType);
    });
    $(document).on("click", "[href='#HistoryTab']", function () {
        var nodeType = $("#tagInfo form").attr("id");
        showHistoryTab(nodeType);
    });
    $(document).on("click", "[href='#commonPropTab']", function () {
        $(".deleteProp").show();
        $("#commonPropTab div").show();
    });
    $(document).on("click", "[href='#signalsTab']", function () {
        showSignalsTab();
    });
    $(document).on("click", "[href='#rankTab']", function () {
        showRankTab();
    });

    });



function renameNode(Node) {
    $("#container").jstree("rename", Node);
}


var idCopyParentElemSaver;
function copyNode(idCopyParentElem) {
    //document.getElementById("loading").style.display = 'block';
    idCopyParentElemSaver = idCopyParentElem;
}

function pasteNode(idPasteParentElem) {
    LoadPageOn();
    //document.getElementById("loading").style.display = 'block';
    var dd = '@Url.Action("PasteNode","Configurator")';
    $.ajax({
        type: "POST",
        url: '/Configurator/PasteNode',
        data: { idPasteParentElem: idPasteParentElem, idCopyParentElem: idCopyParentElemSaver },
        //data: { idPasteParentElem: idPasteParentElem, idCopyParentElem: idCopyParentElemSaver, nameOPC: nameOPC },
        async: true,
        success: pageAddTemplate,
        error: function () {
            LoadPageOff();
            alert("Неудача при отправке аякс запроса при вставке");
        }
    });

}

function deleteNode(idDeleteElem) {
    LoadPageOn();
    $.ajax({
        type: "POST",
        url: '/Configurator/DeleteNode',
        data: { idDeleteElem: idDeleteElem },
        //data: { idPasteParentElem: idPasteParentElem, idCopyParentElem: idCopyParentElemSaver, nameOPC: nameOPC },
        async: true,
        success: pageAddTemplate,
        error: function () {
            alert("Неудача при отправке аякс запроса при удалении");
        }
    });
}
function createNode(idParentElem) {
    //$('#dialogToAddModule').load('/Configurator/AddNodeDialog?idParentElem=' + idParentElem);
    $('#dialogToAddModule').load('/Configurator/AddNodeDialog?idParentElem=' + idParentElem);

    //$.ajax({
    //    type: "GET",
    //    url: '/Configurator/AddNodeDialog',
    //    data: { idParentElem: idParentElem },
    //    //data: { idPasteParentElem: idPasteParentElem, idCopyParentElem: idCopyParentElemSaver, nameOPC: nameOPC },
    //    async: true,
    //    success: pageAddTemplate,
    //    error: function () {
    //        alert("Неудача при отправке аякс запроса при создании узла");
    //    }
    //});
}


function pageAddTemplate() {
    //alert("dfsdff");
    //document.getElementById("loading").style.display = 'none';
    //alert("dfsdff1111");

    location.reload();
    //alert("dfsdff2222");
}
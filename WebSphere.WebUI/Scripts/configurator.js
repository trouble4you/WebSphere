
//от настроек тега
var saveFlag = 0;
var activeTabName;
var permitDelete = false;


//при сохранении содержимого делает активной последнюю кликнутую вкладку. А не первую по умолчанию.
function saveTabsState() {
    $('.tabs ul .active').removeClass('active');
    $('[href="' + activeTab + '"]').parent().addClass('active');
    var nodeType = $("#tagInfo form").attr("id");
    switch (activeTab) {
        case "#commonPropTab":
            $("#commonPropTab div").show();
            break;
        case "#AlarmTab":
            showAlarmTab();
            break;
        case "#HistoryTab":
            showHistoryTab();
            break;
        case "#signalsTab":
            showSignalsTab();
            break;
        case "#rankTab":
            showRankTab();
            break;
    }
}
var activeTab;
function getTabState() {
    activeTab = $("#tagInfo .active .liTab").attr('href');

}

//скрывает диалоговое окно и чистит содержимое контейнера
function fadeDialogWindow() {
    $('#SaveModal').modal('hide');
    //$('#dialogToAddModule').empty();
    //$('.modal-backdrop.fade.in').remove();
    //ниже было закомменчено
    //$("#SaveModal").remove();
}

function showAlarmTab() {
    $("#commonPropTab div:not(#tabButtons):not(#propBody)").hide();
    var propsAlarm = ["Events", "Alarms"];
    for (var prop in propsAlarm) {
        $('#' + propsAlarm[prop]).show();
        $('#' + propsAlarm[prop] + " div").show();
    }
}

function showHistoryTab() {
    $("#commonPropTab div:not(#tabButtons):not(#propBody)").hide();
    var historyArr = ["History_IsPermit", "RegPeriod1", "Deadbend1"];

    for (var prop in historyArr) {
        $('#' + historyArr[prop]).show();
        $('#' + historyArr[prop] + " .col-xs-3").show();
        $('#' + historyArr[prop] + " .col-xs-9").show();
    }

}

//function calculateHeight() {

//    var rightBoxConfig = $(".right-boxConfig").height();
//    var accordionLabel = $(".accLabel").innerHeight();
//    var tabButtonsHeight = 0;
//    if ($("#tabButtons").length > 0) {
//        var tabButtonsHeight = $("#tabButtons").innerHeight();
//    }
//    var ulTabsHeight = 0;
//    if ($("#tagInfo .nav").length > 0) {
//        var ulTabsHeight = $("#tagInfo .nav").innerHeight();
//    }
//    var attensionMessage = 0;
//    if ($("#AttensionSave").length > 0) {
//        if ($("#AttensionSave").css("display") != "none") {
//            //attensionMessage = $("#AttensionSave").innerHeight() + 5;
//            attensionMessage = $("#AttensionSave").innerHeight() + 10;
//        }
//    }
//    var propsHeight = rightBoxConfig - (2 * accordionLabel + ulTabsHeight + 20);
//    $("#tagInfo .tab-content").height(propsHeight);

//    //сделаем так, чтобы основное окно браузера не надо было крутить
//    $("#tagInfo .tab-content").css("max-height", propsHeight + "px");

//    if ($("#tagInfo #propBody").length > 0) {
//        $("#tagInfo #propBody").height(propsHeight - tabButtonsHeight - 15 - attensionMessage);
//        $("#tagInfo #propBody").css("max-height", propsHeight - tabButtonsHeight - 15 - attensionMessage + "px");
//    }
//}

function calculateHeight() {

    var rightBoxConfig = $(".right-boxConfig").height();
    var accordionLabel = $(".accLabel").innerHeight();
    var tabButtonsHeight = 0;
    if ($("#tabButtons").length > 0) {
        var tabButtonsHeight = $("#tabButtons").innerHeight();
    }
    var ulTabsHeight = 0;
    if ($("#tagInfo .nav").length > 0) {
        var ulTabsHeight = $("#tagInfo .nav").innerHeight();
    }
    var attensionMessage = 0;

    if ($("#attensionSaveWrapper").length > 0) {
        attensionMessage = $("#attensionSaveWrapper").innerHeight();
    }

    var tabContentDiv = rightBoxConfig - (2 * accordionLabel + ulTabsHeight) - 10;
    $("#tagInfo .tab-content").height(tabContentDiv);

    //сделаем так, чтобы основное окно браузера не надо было крутить
    $("#tagInfo .tab-content").css("max-height", tabContentDiv + "px");

    if ($("#tagInfo #propBody").length > 0) {
        $("#tagInfo #propBody").height(tabContentDiv - tabButtonsHeight - attensionMessage);
        $("#tagInfo #propBody").css("max-height", tabContentDiv - tabButtonsHeight - attensionMessage + "px");
    }
}


//Скрывает сообщение о сохранении данных
function hideAboutSaveNotification() {
    $("#aboutSaveNotification").fadeOut();
}

//сюда приходит обновленное содержимое настроек узла после сохранения или отмены сохранения
function reloadTabs(data) {

    $('#tagInfo').empty().html(data);
    saveTabsState();
}



//Делает недоступными iput-ы при сброшенном checkbox Разрешено
function disableInputs(modName, state) {
    var noTypePrefix = "";
    var props;
    if (modName == "Alarms_Permit") {
        props = ["HiHiText", "HiText", "NormalText", "LoText", "LoLoText", "HiHiSeverity", "HiSeverity", "LoSeverity",
"LoLoSeverity"];
        if (state == "True") {
            for (i = 0; i < props.length; i++) {
                $('#Alarms #Alarms_' + props[i]).removeAttr('disabled');
            }
        }
        else {
            for (i = 0; i < props.length; i++) {
                $('#Alarms #Alarms_' + props[i] + ':not([type="hidden"])').attr('disabled', 'disabled');

                var type = $('#Alarms #Alarms_' + props[i]).attr("type");
                var value = $('#Alarms #Alarms_' + props[i]).val();
                var valueHidden = $('#Alarms #Alarms_' + props[i] + '[type="hidden"]').val();
                $('#Alarms #Alarms_' + props[i] + '[type="hidden"]').val(value);
            }
        }
    }
    if (modName == "History_IsPermit") {
        props = ["RegPeriod", "Deadbend"];
        if (state == "True") {
            for (i = 0; i < props.length; i++) {
                $('#tagInfo #' + props[i] + ':not(.deleteProp)').removeAttr('disabled');
            }
        }
        else {
            for (i = 0; i < props.length; i++) {
                var value = $('#tagInfo #' + props[i]).val();
                $('#tagInfo #' + props[i] + '[type="hidden"]').val(value);
                $('#tagInfo #' + props[i] + ':not([type="hidden"])').attr('disabled', 'disabled');
            }
        }
    }
}
//пересчитывает высоту контейнеров с деревом и свойствами
function calculateContainerSize() {
    var innerHeight = window.innerHeight;
    var containerFluidHeight = $(".container-fluid").height();
    var newHeight = innerHeight - containerFluidHeight;
    $(".right-boxConfig").height(newHeight);
    $(".left-boxConfig").height(newHeight);
}


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
    $('#bg_layer').show();
    var newContNameSaver = $('#newControllerName').val();
    $('#dialogToAddModule').empty();
    return $.ajax({
        type: "POST",
        url: '/Configurator/PasteNode',
        data: { idPasteParentElem: idPasteParentElem, idCopyParentElem: idCopyParentElemSaver, newContName: newContNameSaver },
    });
}

function deleteNode(idDeleteElem) {
    LoadPageOn();
    $('#bg_layer').show();
    $.ajax({
        type: "POST",
        url: '/Configurator/DeleteNode',
        data: { idDeleteElem: idDeleteElem },
        async: true,
        success: function () {
            $("#tagInfo").empty();
            $('#bg_layer').hide();
            LoadPageOff();
        },

        error: function () {
            alert("Неудача при отправке аякс запроса при удалении");
            LoadPageOff();
            $('#bg_layer').hide();
        }
    });
}

function deleteChannelNode(idChannel, idFolder) {
    LoadPageOn();
    $('#bg_layer').show();
    $.ajax({
        type: "POST",
        url: '/Configurator/DeleteChannelNode',
        data: { channelId: idChannel, folderId: idFolder },
        async: false,
        success: function (data) {
            $('#bg_layer').hide();
            LoadPageOff();

            if (data.valid == true)
                permitDelete = true;
            else {
                permitDelete = false;
            }

            $('#dialogToAddModule').html(data);

        },

        error: function () {
            alert("Неудача при отправке аякс запроса при удалении");
            LoadPageOff();
            $('#bg_layer').hide();
        }
    });
}

function createNode(idParentElem) {
    var tmp1 = $.jstree.defaults.contextmenu.items();
    $('#dialogToAddModule').load('/Configurator/AddNode?idParentElem=' + idParentElem);
}

function copyControllerNode(Id) {
    $('#dialogToAddModule').load('/Configurator/CopyControllerNode');
    $.ajax({
        type: "GET",
        url: '/Configurator/CopyControllerNode',
        async: true,
        data:{nodeId:Id},
        success: function (data) {
            //if (data.isCopyNow != true) {
                $('#dialogToAddModule').html(data);
            //}
        }
    });
}

//function copyTagNode() {
//    $.ajax({
//        type: "GET",
//        url: '/Configurator/CopyTagNode',
//        async:false
//    })
//}



$(document).ready(function () {
    $(document).on("submit", '#tagInfo form', function () {
        saveFlag = 0;

        $("#saveObjPropsBtn").attr("disabled", "disabled");
        $("#saveObjPropsBtn").empty().html('  Сохранение');
        var saving = $('<i/>', { class: 'fa fa-spinner fa-spin' });
        $("#saveObjPropsBtn").prepend(saving);
        $("#AttensionSave").fadeOut(400, function () {
        });

    });
    //чтобы кнопка добавить свойство была видна только для вкладки общих свойств
    $(document).on("click", "#tagInfo li", function () {
        var ahref = $(this).children().attr('href');
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
    $(document).on("click", "#removeEvent", function () {

        var ds = this.parentElement.parentElement;
        ds.remove();

    });
    $(document).on("click", "#addEvent", function () {
        $.ajax({
            type: "POST",
            url: '/Configurator/AddEvent',
            data: $('#TagPropsTab').serialize(),
            success: function (data) {
                var newEvent = $('<div/>', {
                    class: 'form-group row'
                });
                newEvent.html(data);
                $('#EventContainer').append(newEvent);
            }
        })
    });


    //кнопка сохранить диалогового окна сохранения изменений
    $("#dialogToAddModule").on("click", "#saveChangesBtn", function () {
        //обнуляем флаг изменения
        saveFlag = 0;
        var url = '/Configurator/EditTagProps';

        $.ajax({
            type: "POST",
            url: url,
            data: $('#TagPropsTab').serialize(),
            async: false,
            success: function () { reloadTabs(); calculateHeight(); },
            error: function () {
                alert("Неудача при отправке аякс запроса при сохранения сод-го вкладок");
            }
        });
        fadeDialogWindow();
    });



    //слушает изменения полей ввода и выставляет флаг в 1
    $(document).on("change", "#commonPropTab input,#commonPropTab select", function () {
        //при изменении инпута, выставляем флаг в 1
        saveFlag = 1;
        if ($("#AttensionSave").css("display") == "none") {//если еще не выводили напоминалку об изменениях
            $("#AttensionSave").show();
            var span = $('<span/>',
                {
                    id: 'AttensionSaveProps'
                }
                );
            $("#AttensionSave").append(span);
        }
        var attensionString = $("#AttensionSaveProps").html();
        if (attensionString.indexOf(this.id) == -1 && attensionString.length < 65)//ограничим количество выводимых измененных свойств длиной в 65 символов
        {
            var attStr2 = attensionString + " " + this.id;
            $("#AttensionSaveProps").html(attStr2);
        }

        if ($(this).attr('type') == "checkbox") {
            var id = $(this).attr("id");
            if ($(this).attr('class') == "chkBoxPropsDisable") {//если это чекбоксы History_IsPermit или Alarm_IsPermit

                if ($(this).is(":checked")) {
                    disableInputs(id, "True");
                }
                else {
                    //сделаем недоступными поля ввода
                    disableInputs(id, "False");
                }
            }
        }
    });

    $(document).on("click", "[href='#AlarmTab']", function () {
        showAlarmTab();
    });
    $(document).on("click", "[href='#HistoryTab']", function () {
        showHistoryTab();
    });
    $(document).on("click", "[href='#commonPropTab']", function () {
        $("#commonPropTab div").show();
    });
    $(document).on("click", "[href='#signalsTab']", function () {
        showSignalsTab();
    });
    $(document).on("click", "[href='#rankTab']", function () {
        showRankTab();
    });

});
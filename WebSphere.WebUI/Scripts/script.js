$(document).ready(function () {
    
    LoadPageOff(); // скрываем загрузку страницы

    // дабавить
    $('#id__add a').bind('click', function () {

        // перед запросом
        LoadPageOn();

        // в data-ref находится url для запроса
        var url = $(this).attr('data-ref');

        // запрос на частичное представление
        $.ajax({
            type: 'get',
            url: url,
            data: {},
            async: false,
            error: function (err) {
                LoadPageOff(); // скрываем загрузку страницы
                alert(err);
            },
            success: function (data) {
                // скрываем содержимое side-bar и часть контента
                $('#id_model_extend, #id_datalist').hide();
                // загружаем данные в контейнер
                $('#edit-container').html(data).show();
                LoadPageOff(); // скрываем загрузку страницы
            }
        });

    });

    // сброс пароля
    $(document).on('click', '#ResetPassword', function () {
        // если отмечен, показываем #reset-password-group
        if ($(this).is(':checked')) {
            $('#reset-password-group').show();
        } else {
            $('#reset-password-group').hide();
        }
    });

    // статус суперпользователя
    $(document).on('click', '#Superuser', function () {
        // если статус отмечен, скрываем роли, иначе показываем
        if ($(this).is(':checked')) {
            $('#user-groups').hide();
        } else {
            $('#user-groups').show();
        }
    });

    // отправка формы
    $(document).on('click', '#submit_button', function () {
        LoadPageOn();
        $('.account-form input').blur(); // теряем фокус у всех input
        cover(); // ставим обложку
    });

    // отмена действий
    $(document).on('click', '#btn_cancel', function () {
        // показываем содержимое side-bar и часть контента
        $('#id_model_extend, #id_datalist').show();
        // скрываем edit - контейнер
        $('#edit-container').hide();
    });

    // изменить объект
    $(document).on('click', '.obj_edit_href', function () {

        // перед запросом
        LoadPageOn();

        // в data-ref находится url для запроса
        var url = $(this).attr('data-ref'),

        // объект для изменения
        obj = $(this).attr('data-obj');

        // запрос на частичное представление
        $.ajax({
            type: 'get',
            url: url,
            data: { obj: obj },
            async: false,
            error: function (err) {
                LoadPageOff();
                alert("Ошибка.");
            },
            success: function (data) {
                // скрываем содержимое side-bar и часть контента
                $('#id_model_extend, #id_datalist').hide();
                // загружаем данные в контейнер
                $('#edit-container').html(data).show();
                LoadPageOff();
            }
        });

    });

    // отмечаем все строки с checkbox
    $(document).on('change', '.chk_selected_all', function () {
        if ($(this).is(":checked")) {
            $('.select_row').prop('checked', this.checked);
            $(this).closest('table').find('tbody tr').addClass('select_row_style');
        }
        if (!$(this).is(":checked")) {
            $('.select_row').prop('checked', this.checked);
            $(this).closest('table').find('tbody tr').removeClass('click_row').removeClass('select_row_style');
        }
    });

    // отмечаем строку с checkbox
    $(document).on('change', '.select_row', function () {
        if ($(this).is(":checked")) {
            $(this).closest('tr').addClass('click_row');
        }
        if (!$(this).is(":checked")) {
            $(this).closest('tr').removeClass('select_row_style').removeClass('click_row');
        }
        // проверяем соотношение кол-во отмеченных ко всем
        if ($('.select_row').length == $('.select_row:checked').length) {
            $('.chk_selected_all').prop('checked', true);
        } else {
            $('.chk_selected_all').prop('checked', false);
        }

    });

    // удаление выбранных объектов
    $(document).on('click', '.action a', function () {

        var objs = [], // массив
            url = $(this).attr('data-ref'); // в data-ref находится url для запроса

        if ($('.select_row:checked').length > 0) {

            // выбираем все отмеченные строки
            $('.select_row:checked').closest('tr').find('.data_link a').each(function () {
                var id = $(this).attr('data-id'), // id
                    name = $(this).attr('data-obj'); // name
                objs.push({ 'Id': parseInt(id), 'Name': name });
            });

            // отправляем json строку функции удаления
            delete_objs(url, objs);

        } else {
            alert('Не выбраны объекты для обработки');
        }

    });

    // удаление конкретного объекта
    $(document).on('click', '.obj_delete_href', function () {

        var objs = [], // массив
            url = $(this).attr('data-ref'), // в data-ref находится url для запроса
            id = $(this).attr('data-id'), // id
            name = $(this).attr('data-obj'); // name

        objs.push({ 'Id': parseInt(id), 'Name': name });

        // отправляем json строку функции удаления
        delete_objs(url, objs);
    });

    // обновление системного времени
    setInterval(function () {
        $.post('/Home/UpdatePoints', function (data) {
            $('.left-time').html(data.time);
        });
    }, 1000);

}); // end ready


// *** загрузка страницы ***
// показать
function LoadPageOn() {
    $('#id_page_load').show();
}
// скрыть
function LoadPageOff() {
    $('#id_page_load').hide();
}
// ***

// включение кнопки
function ButtonOn() {
    $('.account-form button').attr('disabled', false);
}

// отключение кнопки
function ButtonOff() {
    $('.account-form button').attr('disabled', true);
}

// обложка для формы
function cover() {

    // элемент обложки
    var wrapper = $('#submit_button'),
        o = wrapper[0],
        o2 = $('.cover')[0];

    // устанавливаем размеры обложки
    $(o2).css({
        width: o.offsetWidth + 'px',
        height: o.offsetHeight + 'px',
        display: ''
    }).fadeTo(200, 1);

    // ставим одинаковые размеры как у обложки
    $('.load').empty().css({
        width: o.offsetWidth + 'px',
        height: o.offsetHeight + 'px',
        display: ''
    });

    // включаем анимацию load
    $('.load').show();
}

// удаление объектов
function delete_objs(url, objs) {

    LoadPageOn(); // показываем страницу загрузки

    // скрываем содержимое side-bar и часть контента
    $('#id_model_extend, #id_datalist').hide();

    // отправляем список
    $.ajax({
        type: 'get',
        url: url,
        data: { json_string: JSON.stringify(objs) },
        async: false,
        error: function (num) {
            LoadPageOff(); // скрываем загрузку страницы
            alert(num);
        },
        success: function (data) {
            // загружаем данные в контейнер
            $('#edit-container').html(data).show();
            LoadPageOff(); // скрываем загрузку страницы
        }
    });
}

// *** функции Ajax.BeginForm ***

function OnBegin() {
    ButtonOff(); // отключаем кнопку
}

function OnSuccess(data) {
    if (data.result == 'Redirect') {
        window.location = data.url; // redirect на страницу
    } else {
        // обновляем валидацию формы
        //$('.update_target_id').html(data);
        $('#edit-container').html(data);
        // скрываем обложку
        $('.cover').fadeOut(300, function () {
            ButtonOn(); // включаем кнопку
            LoadPageOff(); // скрываем загрузку страницы
        });
    }
}

function OnFailure(request, error) {
    alert("Возникла ошибка при отправке формы, пожалуйста,\nпопробуйте отправить еще раз.");
    $('.cover').hide(); // скрываем обложку
    ButtonOn(); // включаем кнопку
    LoadPageOff(); // скрываем загрузку страницы
}

//// ajax запрос на частичное представление
//$.get(url, null, function (data) {
//    // обновляем данные в контейнере
//    $("#edit-container").html(data);
//    // убираем сообщение о загрузке страницы
//    $('#id_page_load').hide();
//    // показываем форму регистрации
//    $('#edit-container').show();
//});

// ***** ajax helper *****
//$.ajax({
//    type: 'get', //тип запроса: get, post либо head
//    url: '', //url адрес файла обработчика
//    data: {}, //параметры запроса
//    response: 'text', //тип возвращаемого ответа text либо xml
//    header: { //заголовки запроса, работают только если установлен response:'xml'
//        'Content-Type':'application/x-www-form-urlencoded; charset=windows-1251',
//        'Referer':location.href
//    },
//    async: false, //асинхронный если установлено true или синхронный запрос если false
//    username: '', //имя пользователя если требуется для авторизации
//    password: '', //пароль пользователя если требуется для авторизации
//    errrep: true, //отображение ошибок error если true
//    error: function(num) { //ошибки запроса
//        var arr=['Your browser does not support Ajax',
//				    'Request failed',
//				    'Address does not exist',
//				    'The waiting time left'];
//        alert(arr[num]);
//    },
//    status: function (number) { //код состояния отправки от 1 до 4
//        alert(number); //вывожу код состояния отправки
//    },
//    endstatus: function (number) { //код состояния запроса например 404, 200
//        alert(number); //вывожу код состояния запроса
//    },
//    success: function (data) {//возвращаемый результат от сервера
//        alert(data); //вывожу результат запроса
//    },
//    timeout: 5000 //таймаут запроса
//});
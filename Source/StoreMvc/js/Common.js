//common
jQuery.support.cors = true;

var debug = true;

//TODO: Tolik. not  used
//var serviceUrl = "https://pcs.com/svc/Dossier.svc/";
//var ContentType = "application/json; charset=utf-8";
//var DataType = "jsonp";

//var CallConfiguration = {
//    'GET': {
//        'contentType': 'application/json; charset=utf-8',
//        'dataType': 'jsonp',
//        'async': false
//    },
//    'POST': {
//        'contentType': 'application/json; charset=utf-8',
//        'dataType': 'json',
//        'async': true
//    },
//    'PUT': {
//        'contentType': 'application/json; charset=utf-8',
//        'dataType': 'json',
//        'async': true
//    },
//    'DELETE': {
//        'async': false
//    }
//};

var handleError = function (results, code) {
    if (debug) {
        console.log('Results: ' + results[0].userMessage);
        console.log(results[0]);
    }
};

// ---  Help functions --- //
function ajaxCall(type, uri, successFunction, data, element, errorFunction) {
    $.ajax({
        type: type,
        url: serviceUrl + uri,
        data: data != null ? JSON.stringify(data) : "{}", //jsonText,
        contentType: CallConfiguration[type].contentType,
        dataType: CallConfiguration[type].dataType,
        async: CallConfiguration[type].async,
        cache: false,
        crossDomain: true,
        success: function (results) {
            if (results != null && (results == 'errorCode' || typeof (results['errorCode']) != 'undefined')) {
                if (typeof (errorFunction) == 'undefined') {
                    handleError(arguments);
                }
                else {
                    handleError(arguments);
                    errorFunction();
                }
            } else {
                successFunction(arguments[0], element);
            }
        },
        error: function (x, e) {
            //showError(x, e);
            if (typeof (errorFunction) != 'undefined') {
                errorFunction();
            }
            if (debug) {
                console.log(x);
                console.log(e);
            }
        }
    });
}

function ConfirmBox(title, message, confirmButtonText, functionSuccess, functionCancel) {
    if (confirm(message))
        return true;
    else {
        return false;
    }
    //    var textObject = $("#dialog-confirm p").contents().filter(function () { return this.nodeType == 3; })[0];
    //    $(textObject).replaceWith(message);
    //    $("#dialog-confirm").dialog({
    //        resizable: false,
    //        height: 160,
    //        modal: true,
    //        title: title,
    //        position: "center",
    //        buttons: [
    //            {
    //                text: confirmButtonText,
    //                click: function () {
    //                    if (functionSuccess) functionSuccess();
    //                    $(this).dialog("close");
    //                }
    //            },
    //            {
    //                text: "Cancel",
    //                click: function () {
    //                    if (functionCancel) functionCancel();
    //                    $(this).dialog("close");
    //                }
    //            }
    //        ]
    //    });
}

function MessageBox(title, message, functionToDo, windowHeight) {
    /*var textObject = $("#dialog-message p").contents().filter(function () { return this.nodeType == 3; })[0];
    $(textObject).replaceWith(message);
    $("#dialog-message").dialog({
        resizable: false,
        height: windowHeight != null ? windowHeight : 160,
        modal: true,
        title: title,
        position: "center",
        buttons: {
            OK: function () {
                if (functionToDo) functionToDo();
                $(this).dialog("close");
            }
        }
    });*/
    alert(message);
}

//Constants for course types
var languageCourse = 1;
var socialCourse = 4;

//TODO: Tolik. Create them
function LoadingFormOpen() {
    //    $('<div id="dialog_loading" class="cloading">Loading ... <br/><img src="../images/loading_please_wait.gif" alt=""/></div>').appendTo("body");
    //    $("#dialog_loading").overlay({
    //        mask: {
    //            color: '#000',
    //            opacity: 0.7
    //        },
    //        closeOnClick: false,
    //        closeOnEsc: false,
    //        top: 'center'
    //    });
    //    $("#dialog_loading").overlay().load();
}

//LoadingFormClose
function LoadingFormClose() {
//    var overlay = $("#dialog_loading").data('overlay');
//    if (overlay == null || !overlay.isOpened()) return;
//    overlay.close();
//    $("#dialog_loading").remove();
}


function CreateTinyMce() {
    //$(".intelli-tabs").tabs();
    tinymce.init({
        selector: ".richtext",
        //mode : "textareas",
        editor_deselector: "simple",
        plugins: [
			"advlist autolink lists link image charmap print preview anchor",
			"searchreplace visualblocks code fullscreen",
			"insertdatetime media table contextmenu paste "
        ],
        toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
    });
}

function CreateTeamConsult() {
    var users = [];
    $.get('OtehrUsersGet', {}, function (result) {
        users = result.Data;
        $("#teamConsult").lightbox_me({
            centered: true, onLoad: function () {
                $("#teamConsult").find("input:first").focus();
            }
        });
        $("#otherStudents").empty();
        $.each(users, function (i, item) {
            $("#otherStudents").append("<option value='" + item.Id + "'>" + item.FirstName + " " + item.LastName + "</option>");
        })
        $("#otherStudents").select2({
            placeholder: "Select specialists..."
        });
    }, 'json');
    
}

function SaveTeamConsult() {
    var selectedUsers = $("#otherStudents").val();
    $.post('TeamConsultCreate', { users: selectedUsers.toString() }, function (result) {
        document.location.reload();
        //window.location = result.Data[0];
    }, 'json');

}

/*$(document).ready(function () {
    CreateTinyMce();
})*/

    //    var overlay = $("#dialog_loading").data('overlay');
    //    if (overlay == null || !overlay.isOpened()) return;
    //    overlay.close();
    //    $("#dialog_loading").remove();


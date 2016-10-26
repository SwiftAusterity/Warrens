function decodeHtml(html) {
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
}

function GetQueryStringParams(sParam) {
    var sPageURL = window.location.search.substring(1);
    var sURLVariables = sPageURL.split('&');

    for (var i = 0; i < sURLVariables.length; i++) {
        var sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] == sParam) {
            return decodeURI(sParameterName[1]);
        }
    }
}

//Stuff always run on doc load
$(function () {
    var dialogHeight = $("#modal-form").attr('data-height');
    var dialogWidth = $("#modal-form").attr('data-width');
    var dialogContainer = $("#modal-form");

    //Automates modal forms a bit, a function by the data-submitName MUST exist on the page otherwise this will error
    var dialog = dialogContainer.dialog({
        autoOpen: false,
        height: parseInt(dialogHeight),
        width: parseInt(dialogWidth),
        modal: true,
        buttons: {
            Cancel: function () {
                dialogContainer.dialog("close");
            }
        }
    });

    var form = dialogContainer.find("form").on("submit", function (event) {
        dialogContainer.dialog("close");
    });

    $("#modal-form-open").button().on("click", function () {
        dialogContainer.dialog("open");
    });
});
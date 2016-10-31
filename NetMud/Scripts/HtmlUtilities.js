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
function initModal() {
    var dialogContainer = $("#modal-form");
    var dialogHeight = dialogContainer.attr('data-height');
    var dialogWidth = dialogContainer.attr('data-width');

    dialogContainer.attr('data-returnUrl', window.location);

    //Automates modal forms a bit, a function by the data-submitName MUST exist on the page otherwise this will error
    var dialog = dialogContainer.dialog({
        autoOpen: false,
        height: parseInt(dialogHeight),
        width: parseInt(dialogWidth),
        modal: true,
        buttons: {
            Cancel: function () {
                dialog.dialog("close");
                document.location = $("#modal-form").attr('data-returnUrl');
            }
        }
    });

    $("#modal-form-open").button().on("click", function () {
        dialog.dialog("open");
    });

    dialogContainer.on('dialogclose', function (event) {
        dialog.dialog("close");
        document.location = $("#modal-form").attr('data-returnUrl');
    });
}
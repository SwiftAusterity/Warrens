$(document).ready(function () {
    window.connection;
    window.commandArray = ['look'];
    window.commandPointer = 0;

    TestBrowser();

    submitCharacter();

    //bind the change event of the currently selected character dropdown to call the ajax thingy to set the player's character
    $('#currentCharacter').change(function () {
        submitCharacter();
    });
});

function submitCharacter() {
    var cscVal = $('#currentCharacter').val();

    $.post("Player/SelectCharacter/" + cscVal, function (data) {
    });
}

function submitCommand() {
    var commandText = $('#input').val();

    window.connection.send(commandText);

    window.commandArray.push(commandText);
    window.commandPointer = commandArray.length - 1;
}

function TestBrowser() {
    if ('WebSocket' in window) {
        window.connection = new WebSocket('ws://localhost:2929');

        window.connection.onopen = function () {
            //Send a small message to the console once the connection is established
            AppendOutput('Connection established with host.');
            $('#clientReload').hide();
            $('#currentCharacter').prop('disabled', true);
        }

        window.connection.onclose = function () {
            AppendOutput('Connection closed by host.');

            //Unlock this on close
            $('#currentCharacter').prop('disabled', false);
            $('#clientReload').show();
        }

        window.connection.onerror = function (error) {
            $('#currentCharacter').prop('disabled', false);
            $('#clientReload').show();
            AppendOutput('Connection error detected: ' + error);
        }

        window.connection.onmessage = function (e) {
            var server_message = JSON.parse(e.data);
            AppendOutput(server_message);
        }

        $('#input').keydown(function (e) {
            switch (e.keyCode) {
                case 13:
                    submitCommand();

                    $(this).val('');
                    break;
                case 27: //esc
                    $(this).val('');
                    e.preventDefault();
                    break;
                case 33: //pgup
                    $('#OutputArea').scrollTop(Math.max(0, $('#OutputArea')[0].scrollTop - 700));
                    e.preventDefault();
                    break;
                case 34: //pgdown
                    $('#OutputArea').scrollTop(Math.min($('#OutputArea')[0].scrollHeight, $('#OutputArea')[0].scrollTop + 700));
                    e.preventDefault();
                    break;
                case 35: //END
                    $('#OutputArea').scrollTop($('#OutputArea')[0].scrollHeight);
                    e.preventDefault();
                    break;
                case 36: //home
                    $('#OutputArea').scrollTop(0);
                    e.preventDefault();
                    break;
                case 38: //up
                    if (window.commandPointer === 0) {
                        $(this).val('');
                    }
                    else {
                        window.commandPointer = window.commandPointer - 1;
                        $(this).val(window.commandArray[window.commandPointer]);
                    }
                    e.preventDefault();
                    break;
                case 40: //down
                    if (window.commandPointer === window.commandArray.length - 1) {
                        $(this).val('');
                    }
                    else {
                        window.commandPointer = window.commandPointer + 1;
                        $(this).val(commandArray[window.commandPointer]);
                    }
                    e.preventDefault();
                    break;
            }
        }).focus();
    } else {
        //No support, this is a websock only client page, tell them that.
        AppendOutput('Your browser does not support the WebSockets protocol. Please visit us with one that does.');
    }
}

function AppendOutput(output) {
    var $outputArea = $("#OutputArea");

    $outputArea.append(output.Occurrence);

    $('[output-data-binding]').each(function () {
        var $this = $(this);
        var sourceKey = $this.attr('output-data-binding');
        var dataToAppend = getObjects(output, sourceKey, 0);

        if (dataToAppend != undefined && dataToAppend.length > 0) {
            $this.html(dataToAppend)
        }
    });

    $('[output-eval-code]').each(function () {
        var $this = $(this);
        var evalKey = $this.attr('output-eval-key');
        var functionCode = $this.attr('output-eval-code');
        var dataToAppend = getObjects(output, evalKey, 0);

        var funct = new Function('element', 'data', functionCode);

        return funct($this, dataToAppend);
    });

    $outputArea[0].scrollTop = $outputArea[0].scrollHeight;
}

function getObjects(collection, key, depth) {
    var objects = [];
    var keyHierarchy = key.split('.');

    if (collection != undefined && collection[keyHierarchy[depth]] != undefined) {
        if (depth == keyHierarchy.length - 1) {
            objects.push(collection[keyHierarchy[depth]]);
        }
        else {
            objects = objects.concat(getObjects(collection[keyHierarchy[depth]], key, depth + 1));
        }
    }

    return objects;
}
function submitCharacter() {
    var cscVal = $('#currentCharacter').val();

    if (cscVal === undefined || cscVal === null || cscVal === '-1' || cscVal === '') {
        return;
    }

    $.post("Player/SelectCharacter/" + cscVal, function (data) {
    });
}

function submitCommand(overrideCommand) {
    var commandText = overrideCommand;
    var wipe = false;

    if (commandText === '' || commandText === undefined) {
        commandText = $('#input').val();
        wipe = true;
    }

    window.connection.send(commandText);

    if (wipe) {
        window.commandArray.push(commandText);
        window.commandPointer = commandArray.length - 1;
    }
}

function TestBrowser() {
    var cscVal = $('#currentCharacter').val();

    if (cscVal === undefined || cscVal === null || cscVal === '-1' || cscVal === '') {
        AppendTextToOutput('Please create a character first and select it above.');

        return;
    }

    if ('WebSocket' in window) {
        var protocol = window.location.protocol.replace('http', 'ws');

        window.connection = new WebSocket(protocol + '//' + window.location.hostname + ':' + window.location.port + '/WebSocketService.ashx');

        window.connection.onopen = function () {
            //Send a small message to the console once the connection is established
            AppendTextToOutput('Connection established with host.');
            $('#currentCharacter').prop('disabled', true);
        };

        window.connection.onclose = function () {
            AppendTextToOutput('Connection closed by host.');

            //Unlock this on close
            $('#currentCharacter').prop('disabled', false);
        };

        window.connection.onerror = function (error) {
            $('#currentCharacter').prop('disabled', false);
            AppendTextToOutput('Connection error detected: ' + error);
        };

        window.connection.onmessage = function (e) {
            var server_message = JSON.parse(e.data);

            window.lastOutput = server_message;
            AppendOutput(server_message);
        };

        $('#input').keydown(function (e) {
            switch (e.keyCode) {
                case 13: //enter
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
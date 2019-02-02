function submitCharacter() {
    var cscVal = $('#currentCharacter').val();

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


function GetModuleUI(originNumber, moduleName) {
    $.get('/api/ClientDataApi/GetUIModuleContent/' + moduleName, function (data) {
        var newContent = data.BodyHtml.Value;

        if (newContent !== '') {
            var $origin = $('div#quadrant-' + originNumber + '.quadrant');
            $origin.attr('data-module-name', data.Name);
            $origin.children('ul').children('li#quadrantName').text(data.Name);

            var myContent = $origin.children('.contentContainer');

            myContent.detach();

            $(newContent).appendTo($origin);

            ReloadUI();
        }
    });
}

function WipeUI() {
    $('.quadrant').each(function () {
        var $quad = $(this);

        var quadNumber = $quad.attr('quadrant-number');
        $quad.attr('data-module-name', 'Quadrant ' + quadNumber);
        $quad.children('ul').children('li#quadrantName').text('Quadrant ' + quadNumber);

        var myContent = $quad.children('.contentContainer');

        myContent.detach();
    });

    window.openedWindows.forEach(function (win) {
        win.Window.close();
    });
}

function SaveUIModule(originNumber, callback) {
    var $origin = $('div#quadrant-' + originNumber + '.quadrant');
    var moduleName = $origin.attr('data-module-name');

    if (moduleName !== '') {
        $.post('/api/ClientDataApi/SaveUIModuleContent/' + moduleName + '/' + originNumber, function () { if (callback !== null) { callback(); } });
    } else {
        //remove it
        $.post('/api/ClientDataApi/RemoveUIModuleContent/**anymodule**/' + originNumber, function () { if (callback !== null) { callback(); } });
    }
}

function LoadUIModules() {
    window.UILoading = true;

    $.get('/api/ClientDataApi/LoadUIModules', function (data) {
        for (var i = 0; i < data.length; i++) {
            var obj = data[i];
            var quadrant = obj.Item2;
            var module = obj.Item1;

            var moduleContent = module.BodyHtml.Value;

            if (moduleContent !== '') {
                var $origin = $('div#quadrant-' + quadrant + '.quadrant');

                $origin.children('.contentContainer').detach();

                if (quadrant === -1) {
                    openModularUI(500, 500, module.Name, moduleContent);
                } else {
                    $origin.attr('data-module-name', module.Name);
                    $origin.children('ul').children('li#quadrantName').text(module.Name);

                    $(moduleContent).appendTo($origin);
                }
            }
        }

        window.UILoading = false;

        ReloadUI();
    });
}

function TestBrowser() {
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
$(document).ready(function () {
    submitCharacter();

    //bind the change event of the currently selected character dropdown to call the ajax thingy to set the player's character
    $('#currentCharacter').change(function () {
        submitCharacter();
    });
    
    if (window.soundMuted) {
        $("#muteSounds").children().attr('class', 'glyphicon glyphicon-volume-off');
        $("#muteSounds").children().attr('style', 'color: red;');
    }
    else {
        $("#muteSounds").children().attr('class', 'glyphicon glyphicon-volume-up');
        $("#muteSounds").children().attr('style', 'color: green;');
    }

    if (window.musicMuted) {
        $("#muteMusic").children().attr('class', 'glyphicon glyphicon-volume-off');
        $("#muteMusic").children().attr('style', 'color: red;');
    }
    else {
        $("#muteMusic").children().attr('class', 'glyphicon glyphicon-volume-up');
        $("#muteMusic").children().attr('style', 'color: green;');
    }

    if ($('.audioTrackSelector') !== undefined && $('.audioTrackSelector').length > 0) {
        changeTrack($('.audioTrackSelector')[0]);
    }

    Tutorial($('#locationBreadcrumbs'), "This is where you are in the world.", window.tutorialMode);
    Tutorial($('.statusIndicators'), "The current level of illumination in the area as well as weather conditions and sun/moon cycle indicators.", window.tutorialMode);
    Tutorial($('.inputContainer'), "This is where you type commands. Use the INTERACT command to interact with things.", window.tutorialMode);
    Tutorial($('#userControls'), "Music tracks (and muting controls) in addition to disconnect/reconnect can be found here.", window.tutorialMode);
    Tutorial($('#healthBars'), "Health and stamina levels are shown here.", window.tutorialMode);
    Tutorial($('#inventoryContainers'), "Your inventory will show up here.", window.tutorialMode);

    $('#disconnect').click(function (e) {
        $('#input').val('');
        $("#parserClientOutput").html('');
        $("#parserClientOutput")[0].scrollTop = $("#parserClientOutput")[0].scrollHeight;

        AppendTextToOutput('Connection TERMINATED.');
        window.connection.close();

        $('disconnect').off('click');
        return false;
    });

    $("#clientReload").click(function (e) {
        ReloadUI();
        return false;
    });

    $("#keyMap").click(function (e) {
        $('.keyLegend').toggleClass('expanded');
        return false;
    });

    $('#loopTracks').click(function () {
        changeLoopTrackMode(this);
        return false;
    });


    $("#muteSounds").click(function (e) {
        window.soundMuted = !soundMuted;

        $.post('/api/ClientDataApi/ToggleSoundMute');

        if (window.soundMuted) {
            $(this).children().attr('class', 'glyphicon glyphicon-volume-off');
            $(this).children().attr('style', 'color: red;');
        }
        else {
            $(this).children().attr('class', 'glyphicon glyphicon-volume-up');
            $(this).children().attr('style', 'color: green;');
        }

        return false;
    });

    $("#muteMusic").click(function (e) {
        window.musicMuted = !musicMuted;

        $.post('/api/ClientDataApi/ToggleMusicMute');

        if (window.musicMuted) {
            $('#backgroundMusic')[0].pause();
            $(this).children().attr('class', 'glyphicon glyphicon-volume-off');
            $(this).children().attr('style', 'color: red;');
        }
        else {
            $('#backgroundMusic')[0].play();
            $(this).children().attr('class', 'glyphicon glyphicon-volume-up');
            $(this).children().attr('style', 'color: green;');
        }

        return false;
    });

    TestBrowser();
});

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
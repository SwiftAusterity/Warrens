function AppendTextToOutput(output) {
    if (output === '' || output === undefined) {
        return;
    }

    var $outputArea = $("#OutputArea");

    $outputArea.append('<div>' + output + '</div>');
    $outputArea[0].scrollTop = $outputArea[0].scrollHeight;
}


function PlaySound(foleyUri) {
    if (foleyUri === '' || foleyUri === undefined || window.soundMuted) {
        return;
    }

    var audio = new Audio(foleyUri);

    audio.play();
}

function AppendOutput(output, UIOnly) {
    if (output === undefined) {
        return;
    }

    $('[output-data-binding]').each(function () {
        var $this = $(this);
        var sourceKey = $this.attr('output-data-binding');
        var dataToAppend = getObjects(output, sourceKey, 0);

        if (dataToAppend !== undefined && dataToAppend.length > 0) {

            if (dataToAppend.length === 1 && dataToAppend[0] !== $this.html()) {
                $this.html(dataToAppend);

                //blink?
                $this.fadeOut(50, function () {
                    $this.fadeIn(100);
                });
            }
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

    //Output (just the map and partial map updates and text output)
    if (!UIOnly) {
        AppendTextToOutput(output.Occurrence);

        if (output.SoundToPlay !== undefined && output.SoundToPlay !== '' && output.SoundToPlay !== null) {
            PlaySound(output.SoundToPlay);
        }
    }
}

function getObjects(collection, key, depth) {
    var objects = [];

    if (key !== undefined) {
        var keyHierarchy = key.split('.');

        if (collection !== undefined && collection[keyHierarchy[depth]] !== undefined) {
            if (depth === keyHierarchy.length - 1) {
                objects.push(collection[keyHierarchy[depth]]);
            }
            else {
                objects = objects.concat(getObjects(collection[keyHierarchy[depth]], key, depth + 1));
            }
        }
    }

    return objects;
}

function ReloadUI() {
    AppendOutput(window.lastOutput, true);
}

function LoadSVG(fileName, containerElement, removeInner) {
    if (fileName === '' || fileName === undefined) {
        return;
    }

    if (!fileName.startsWith("/content/images/icons/")) {
        fileName = "/content/images/icons/" + fileName;
    }

    if (!fileName.endsWith(".svg")) {
        fileName = fileName + ".svg";
    }

    xhr = new XMLHttpRequest();
    xhr.open("GET", fileName, false);
    //xhr.overrideMimeType("image/svg+xml");

    xhr.onload = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                if (removeInner) {
                    $(containerElement).empty();
                }

                var svgXml = xhr.responseXML.documentElement;

                if (svgXml === null) {
                    return;
                }

                containerElement.appendChild(svgXml);
            }
        }
    };

    xhr.send(null);
}

function SetupTileTips($entities) {
    $entities.click(function () {
        var $this = $(this);
        var tipElement = this;
        var x = $this.attr('x-data');
        var y = $this.attr('y-data');
        var zoneId = $this.attr('zone-data');

        $.get('/api/ClientDataApi/GetTileInfo/' + zoneId + '/' + x + '/' + y, function (tipInfo) {
            MakeTipper($this, tipInfo, tipElement);
        });
    });
}

function MakeTooltip(parent, text) {
    var options = {
        title: text,
        trigger: 'hover',
        template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
        popperOptions: {
            positionFixed: true,
            position: 'top'
        }
    };

    var instance = new Tooltip(parent, options);
}

function MakeTipper($this, tipInfo, tipElement) {
    var poptions = {
        positionFixed: true,
        position: 'auto',
        removeOnDestroy: true
    };

    $this.append(decodeHtml(tipInfo));

    var pop = new Popper(tipElement, $this.find('div.helpItem'), poptions);
    $this.find('div.helpItem').click(function () {
        pop.destroy();
        return false;
    });
}

function GetEntityInfo(type, key) {

    $entities.click(function () {
        $.get('/api/ClientDataApi/GetEntityInfo/' + key + '/' + type, function (tipInfo) {
            var poptions = {
                positionFixed: true,
                position: 'auto',
                removeOnDestroy: true,
                onCreate: function (data) {
                    $(data.instance.popper).click(function () { $(this).hide(); });
                }
            };
            var pop = new Popper(this, decodeHtml(tipInfo), poptions);
            pop.show();
        });
    });
}

function changeTrack(me) {
    var $me = $(me);
    var newTrackUri = $me.attr('data-song-uri');
    var playlistIndex = $me.attr('data-playlist-index');
    window.currentTrack = parseInt(playlistIndex);

    var audioPlayer = $('#backgroundMusic')[0];

    if (!window.musicMuted) {
        audioPlayer.pause();
    }

    $('#backgroundMusic').attr('src', newTrackUri);

    audioPlayer.removeEventListener('ended', nextTrackLoop);
    audioPlayer.removeEventListener('ended', nextPlaylistLoop);
   if (!window.loopTrack) {
        audioPlayer.addEventListener('ended', nextTrackLoop);
    }

    $('.songSelection').attr('class', 'glyphicon glyphicon-volume-off songSelection');
    $('.songSelection').attr('style', 'color: red;');

    $me.children().attr('class', 'glyphicon glyphicon-volume-up songSelection');
    $me.children().attr('style', 'color: green;');

    if (!window.musicMuted) {
        audioPlayer.play();
    }

    return false;
}

function nextTrackLoop() {
    var audioPlayer = $('#backgroundMusic')[0];
    var maxTracks = $('.audioTrackSelector').length;

    var i = window.currentTrack + 1;

    if (i >= maxTracks) {
        i = 0;
    }

    window.currentTrack = i;

    var trackItem = $('.audioTrackSelector[data-playlist-index=' + i + ']');

    audioPlayer.src = trackItem.attr('data-song-uri');

    $('.songSelection').attr('class', 'glyphicon glyphicon-volume-off songSelection');
    $('.songSelection').attr('style', 'color: red;');

    trackItem.children().attr('class', 'glyphicon glyphicon-volume-up songSelection');
    trackItem.children().attr('style', 'color: green;');

    audioPlayer.pause();
    audioPlayer.load();
    audioPlayer.play();
}

function changePlaylist(me) {
    var $me = $(me);
    var audioPlayer = $('#backgroundMusic')[0];
    var playlistIndex = $me.attr('data-playlist-index');
    window.currentPlaylist = playlistIndex;
    var playlist = window.playlists[playlistIndex];

    audioPlayer.loop = false;

    if (!window.musicMuted) {
        audioPlayer.pause();
    }

    $('#backgroundMusic').attr('src', playlist.songs[0]);
    window.currentPlaylistTrack = 0;

    audioPlayer.removeEventListener('ended', nextTrackLoop);
    audioPlayer.removeEventListener('ended', nextPlaylistLoop);
    audioPlayer.addEventListener('ended', nextPlaylistLoop);

    $('.songSelection').attr('class', 'glyphicon glyphicon-volume-off songSelection');
    $('.songSelection').attr('style', 'color: red;');

    $me.children().attr('class', 'glyphicon glyphicon-volume-up songSelection');
    $me.children().attr('style', 'color: green;');

    if (!window.musicMuted) {
        audioPlayer.load();
        audioPlayer.play();
    }

    return false;
}

function nextPlaylistLoop() {
    var audioPlayer = $('#backgroundMusic')[0];
    var i = window.currentPlaylistTrack + 1;

    var playlist = window.playlists[window.currentPlaylist];

    if (playlist.songs.length <= i) {
        i = 0;
    }

    window.currentPlaylistTrack = i;
    audioPlayer.src = playlist.songs[i];
    audioPlayer.pause();
    audioPlayer.load();
    audioPlayer.play();
}

function changeLoopTrackMode(me) {
    var $me = $(me);
    var audioPlayer = $('#backgroundMusic')[0];

    window.loopTrack = !window.loopTrack;

    $('#backgroundMusic')[0].loop = window.loopTrack;

    if (window.loopTrack) {
        $me.children().attr('style', 'color: green;');
        audioPlayer.removeEventListener('ended', nextTrackLoop);
        AppendTextToOutput("Music looping toggled ON.");
   } else {
        $me.children().attr('style', 'color: red;');
        changePlaylist($('.audioTrackSelector')[window.currentTrack]);
        AppendTextToOutput("Music looping toggled OFF.");
   }

    return false;
}

function changeTutorialMode(me) {
    var $me = $(me);

    window.tutorialMode = !window.tutorialMode;

    $.post('/api/ClientDataApi/ToggleTutorialMode');

    if (window.tutorialMode) {
        $me.children().attr('style', 'color: green;');
    } else {
        $me.children().attr('style', 'color: red;');
        $('.tutorialContainer').remove();
    }

    return false;
}

function changeGossipMode(me) {
    var $me = $(me);

    window.gossipMode = !window.gossipMode;

    $.post('/api/ClientDataApi/ToggleGossipParticipation');

    if (window.gossipMode) {
        $me.children().attr('style', 'color: green;');
        AppendTextToOutput("Gossip network toggled ON.");
   } else {
        $me.children().attr('style', 'color: red;');
        AppendTextToOutput("Gossip network toggled OFF.");
   }

    return false;
}

function bindClientCommand(name, funct) {
    window.clientCommands[name] = funct;
}
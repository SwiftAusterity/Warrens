function AppendTextToOutput(output) {
    if (output === '' || output == undefined) {
        return;
    }

    var $outputArea = $("#OutputArea");

    $outputArea.append('<div>' + output + '</div>');
    $outputArea[0].scrollTop = $outputArea[0].scrollHeight;
};

function AppendOutput(output, UIOnly) {
    if (window.UILoading) {
        waitForUI();
    }

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

    window.openedWindows.forEach(function (win) {
        win.window.document.querySelectorAll('[output-data-binding]').forEach(function (element) {
            var $this = $(element);
            var sourceKey = $this.attr('output-data-binding');
            var dataToAppend = getObjects(output, sourceKey, 0);

            if (dataToAppend != undefined && dataToAppend.length > 0) {
                $this.html(dataToAppend)
            }
        });

        win.window.document.querySelectorAll('[output-eval-code]').forEach(function (element) {
            var $this = $(element);
            var evalKey = $this.attr('output-eval-key');
            var functionCode = $this.attr('output-eval-code');
            var dataToAppend = getObjects(output, evalKey, 0);

            var funct = new Function('element', 'data', functionCode);

            return funct($this, dataToAppend);
        });
    });

    if (!UIOnly) {
        AppendTextToOutput(output.Occurrence);
    }
}

function waitForUI() {
    if (window.UILoading == true) {
        window.setTimeout(waitForUI, 100); /* this checks the flag every 100 milliseconds*/
    } 
}

function getObjects(collection, key, depth) {
    var objects = [];

    if (key != undefined) {
        var keyHierarchy = key.split('.');

        if (collection != undefined && collection[keyHierarchy[depth]] != undefined) {
            if (depth == keyHierarchy.length - 1) {
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

function openModularUI(width, height, windowTitle, content) {
    window.UILoading = true;

    var s = 'menubar=no, toolbar=no, location=no, resizable=no, scrollbars=yes, status=no, width = ' + width + ', height = ' + height;
    var styles = '';
    var scripts = '';

    var w = window.open("", windowTitle);

    w.close();

    var NFW = window.open('/GameClient/ModularWindow', windowTitle, s, true);

    if (NFW != null) {
        NFW.addEventListener('load', function () {
            var contentArea = this.document.querySelector('#contentArea');

            $(contentArea).attr('data-module-name', windowTitle);
            $(contentArea).children('ul').children('li#quadrantName').text(windowTitle);

            $(content).appendTo(contentArea);

            this.document.querySelectorAll('[output-data-binding]').forEach(function (element) {
                var $this = $(element);
                var sourceKey = $this.attr('output-data-binding');
                var dataToAppend = getObjects(window.lastOutput, sourceKey, 0);

                if (dataToAppend != undefined && dataToAppend.length > 0) {
                    $this.html(dataToAppend)
                }
            });

            this.document.querySelectorAll('[output-eval-code]').forEach(function (element) {
                var $this = $(element);
                var evalKey = $this.attr('output-eval-key');
                var functionCode = $this.attr('output-eval-code');
                var dataToAppend = getObjects(window.lastOutput, evalKey, 0);

                var funct = new Function('element', 'data', functionCode);

                return funct($this, dataToAppend);
            });
        }, false);

        NFW.blur();

        window.focus();

        NFW.resizeTo(width, height);

        NFW.focus();

        window.openedWindows.push({ 'window': NFW, 'name': windowTitle });
    }

    window.UILoading = false;
}

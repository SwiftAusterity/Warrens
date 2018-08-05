function Tutorial(parent, text, tutorialMode) {
    if (!tutorialMode) {
        return;
    }

    var options = {
        title: text,
        trigger: 'hover',
        template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
        offset: 5,
        popperOptions: {
            removeOnDestroy: true,
            placements: 'auto',
        }
    };

    var instance = new Tooltip(parent, options);
    instance.show();

    parent.on('mouseenter', function () {
        instance.dispose();
        parent.off('mouseenter');
    });
}

function HelpTipTutorial(parent, text) {
    var options = {
        title: text,
        trigger: 'click',
        template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
        offset: 5,
        popperOptions: {
            placements: 'auto',
        }
    };

    var instance = new Tooltip(parent, options);
}

function openFrameless(width, height, fromLeft, fromTop, targetUrl, windowTitle) {
    var s = 'menubar=no, toolbar=no, location=no, resizable=no, scrollbars=yes, status=no, width = ' + width + ', height = ' + height;
    var styles = '';
    var scripts = '';

    if (windowTitle == '') {
        windowTitle = 'adminModal';
    }

    var NFW = window.open(targetUrl, windowTitle, s);

    NFW.blur();

    window.focus();

    NFW.resizeTo(width, height);

    NFW.moveTo(fromLeft, fromTop);

    NFW.focus();

    var timer = setInterval(function () {
        if (NFW.closed) {
            clearInterval(timer);
            submitFrameless(window);
        }
    }, 1000);
}

function submitFrameless(baseDocument) {
    baseDocument.location.reload(false);
}
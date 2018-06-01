function openFrameless(width, height, fromLeft, fromTop, targetUrl, windowTitle) {
    var s = 'menubar=no, toolbar=no, location=no, resizable=no, scrollbars=yes, status=no, width = ' + width + ', height = ' + height;
    var styles = '';
    var scripts = '';

    var NFW = window.open(targetUrl, 'adminModal', s);

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
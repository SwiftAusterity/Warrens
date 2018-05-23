function openFrameless(width, height, fromLeft, fromTop, targetUrl, windowTitle) {
    var s = 'menubar=no, location=no, resizable=no, scrollbars=yes, status=no, width = ' + width + ', height = ' + height;
    var styles = '';
    var scripts = '';

    var NFW = window.open(targetUrl, 'adminModal', s);

    NFW.blur();

    window.focus();

    NFW.resizeTo(width, height);

    NFW.moveTo(fromLeft, fromTop);

    NFW.focus();

    NFW.onsubmit = submitFrameless(NFW);
}

function submitFrameless(e, formWindow) {
    e.preventDefault();

    formWindow.getElementById('form').submit();

    setTimeout(formWindow.close, 5);
}
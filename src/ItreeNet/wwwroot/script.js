function topFunctionJS() {
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}

function CopyToClipboard(newClip) {
    navigator.clipboard.writeText(newClip).then(function () {
    });
}

window.DownloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

window.DownloadFileFromBase64 = (fileName, base64Data) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + base64Data;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

function OpenLink(url) {
    window.open(url, '_blank');
}

function getWidthFromElement(elementQuery) {
    const element = document.querySelector(elementQuery);
    var styles = window.getComputedStyle(element);
    var padding = parseFloat(styles.paddingLeft) +
        parseFloat(styles.paddingRight);

    return element.clientWidth - padding;
}

function raiseResizeEvent() {
    var namespace = 'ItreeNet'; // the namespace of the app, you will have to change this for your app
    var method = 'RaiseWindowResizeEvent'; //the name of the method in our "service"
    DotNet.invokeMethodAsync(namespace, method, Math.floor(window.innerWidth), Math.floor(window.innerHeight));
}

//throttle resize event, taken from https://stackoverflow.com/a/668185/812369
var timeout = false;
window.addEventListener("resize", function () {
    if (timeout !== false)
        clearTimeout(timeout);
    timeout = setTimeout(raiseResizeEvent, 200);
});

function FocusElementById(elementId) {
    setTimeout(function () {
        var element = document.getElementById(elementId);
        if (element instanceof HTMLElement) {
            element.focus();
        }
    },
        500
    );
}

function ScrollToElementId(elementId) {
    setTimeout(function () {
        var element = document.getElementById(elementId);
        if (element instanceof HTMLElement) {
            element.scrollIntoView({ behavior: 'smooth', block: 'end' });
        }
    },
        500
    );
}

function ScrollToElementId2(elementId, offset = 0) {
    setTimeout(function () {
        var element = document.getElementById(elementId);
        if (element instanceof HTMLElement) {
            // Bringt das Element an den Anfang der Ansicht.
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });

            // Scrollt um den zusätzlichen Offset nach unten, plus ein wenig extra, um zu versuchen, so weit wie möglich zu scrollen.
            const extraScroll = window.innerHeight - element.clientHeight;
            window.scrollBy(0, offset + extraScroll);
        }
    }, 500);
}


function getWidthFromElement(elementQuery) {
    const element = document.querySelector(elementQuery);
    if (element !== undefined) {
        return element.offsetWidth;
    }
}

function getHeightFromElement(elementQuery) {
    const element = document.querySelector(elementQuery);
    if (element != null) {
        return element.offsetHeight;
    }
    return 0;
}

function getTopPositionOfElement(elementQuery) {
    const element = document.querySelector(elementQuery);
    if (element != null) {
        var rect = element.getBoundingClientRect();
        var height = element.offsetHeight;
        var res = rect.top + height
        //console.log("top: " + rect.top + " height: " + height + " total: " + res);
        return Math.round(res);
    }
    return 0;
}

function getLeftPositionOfElement(elementQuery) {
    const element = document.querySelector(elementQuery);
    if (element != null) {
        var rect = element.getBoundingClientRect();
        return Math.round(rect.left);
    }
    return 0;
}

var themeChanger = {
    changeCss: function (mode) {
        document.cookie = 'app-theme=' + mode + '; path=/; SameSite=Lax; max-age=31536000';
        document.documentElement.classList.toggle('dark-mode', mode === 'dark');

        var appCssFileUrl = mode === "dark" ? "css/app-dark.css" : "css/app-light.css";

        var oldLinkApp = document.getElementById("AppThemeLink");
        var newLinkApp = document.createElement("link");
        newLinkApp.setAttribute("id", "AppThemeLink");
        newLinkApp.setAttribute("rel", "stylesheet");
        newLinkApp.setAttribute("type", "text/css");
        newLinkApp.setAttribute("href", appCssFileUrl);
        newLinkApp.onload = function () {
            if (oldLinkApp) oldLinkApp.parentElement.removeChild(oldLinkApp);
        };

        document.getElementsByTagName("head")[0].appendChild(newLinkApp);
    }
}

function openBase64Image(base64) {
    const win = window.open();
    if (win) {
        win.document.write('<img src="' + base64 + '" style="max-width:100%;height:auto;" />');
        win.document.title = "Bildvorschau";
        win.document.close();
    }
}

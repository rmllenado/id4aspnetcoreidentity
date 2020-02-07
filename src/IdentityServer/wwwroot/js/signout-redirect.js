window.addEventListener("load", function () {
    var a = document.querySelector("a.PostLogoutRedirectUri");
    if (a) {
        window.location = a.href;
        return;
    }
    var a = document.querySelector("a.RedirectUri");
    if (a) {
        this.setTimeout(function () {
            window.location = a.href;
            return;
        }, 5000);
    }
});

var collapsible = document.getElementsByClassName("wrap-collapsible");
for (var i = 0; i < collapsible.length; i++) {
    var header = collapsible[i].getElementsByClassName("collapsible");
    if (header[0]) {
        header[0].addEventListener("click", (function () {
            this.classList.toggle("collapsed");
        }).bind(collapsible[i]));
    }
}
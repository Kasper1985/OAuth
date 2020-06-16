function toggleRecentTopics(elem) {
    var allNews = document.getElementsByClassName('all-news')[0];
    if (elem.classList.contains('collapsed')) {
        elem.classList.remove('collapsed');
        allNews.classList.remove('collapsed');
    } else {
        elem.classList.add('collapsed');
        allNews.classList.add('collapsed');
    }
}

function toggleNewsBundle(elem) {
    if (elem.classList.contains('collapsed')) {
        elem.classList.remove('collapsed');
    } else {
        elem.classList.add('collapsed');
    }
}

function toggleSideNav(elem) {
    if (elem.classList.contains('collapsed')) {
        elem.classList.remove('collapsed');
    } else {
        elem.classList.add('collapsed');
    }
}
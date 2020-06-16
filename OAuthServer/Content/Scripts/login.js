function showRequirements() {
    var bgLock = document.getElementsByClassName('background-lock')[0];
    var requirements = document.getElementsByClassName('requirements')[0];
    bgLock.classList.add('visible');
    requirements.classList.add('visible');
}

function hideRequirements() {
    var bgLock = document.getElementsByClassName('background-lock')[0];
    var requirements = document.getElementsByClassName('requirements')[0];
    bgLock.classList.remove('visible');
    requirements.classList.remove('visible');
}
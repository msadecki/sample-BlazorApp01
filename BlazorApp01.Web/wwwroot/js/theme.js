export function setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
}

export function getTheme() {
    return localStorage.getItem('theme') || 'auto';
}

export function saveTheme(theme) {
    localStorage.setItem('theme', theme);
}
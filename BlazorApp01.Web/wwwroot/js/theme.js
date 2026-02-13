export function setTheme(theme) {
    const root = document.documentElement;
    
    // Handle 'auto' theme by detecting system preference
    if (theme === 'auto') {
        const systemTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        root.setAttribute('data-bs-theme', systemTheme);
    } else {
        root.setAttribute('data-bs-theme', theme);
    }
}

export function getTheme() {
    return localStorage.getItem('theme') || 'auto';
}

export function saveTheme(theme) {
    localStorage.setItem('theme', theme);
}

// Initialize theme and listen for system preference changes
export function initializeTheme() {
    const savedTheme = getTheme();
    setTheme(savedTheme);
    
    // Listen for system theme changes when using 'auto'
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        const currentTheme = getTheme();
        if (currentTheme === 'auto') {
            setTheme('auto');
        }
    });
}
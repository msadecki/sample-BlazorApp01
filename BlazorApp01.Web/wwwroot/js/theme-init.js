// Initialize and maintain theme across all navigation
(function() {
    function getTheme() {
        return localStorage.getItem('theme') || 'auto';
    }
    
    function applyTheme() {
        const theme = getTheme();
        const root = document.documentElement;
        
        // Handle 'auto' theme by detecting system preference
        if (theme === 'auto') {
            const systemTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
            root.setAttribute('data-bs-theme', systemTheme);
        } else {
            root.setAttribute('data-bs-theme', theme);
        }
    }
    
    // Apply theme immediately (before page renders)
    applyTheme();
    
    // Create a MutationObserver to maintain theme if it gets reset
    const observer = new MutationObserver(function(mutations) {
        const currentTheme = document.documentElement.getAttribute('data-bs-theme');
        const savedTheme = getTheme();
        
        // Determine what the theme should be
        let expectedTheme;
        if (savedTheme === 'auto') {
            expectedTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        } else {
            expectedTheme = savedTheme;
        }
        
        if (currentTheme !== expectedTheme) {
            document.documentElement.setAttribute('data-bs-theme', expectedTheme);
        }
    });
    
    // Start observing
    observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['data-bs-theme']
    });
    
    // Listen for system theme changes when using 'auto'
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function(e) {
        const savedTheme = getTheme();
        if (savedTheme === 'auto') {
            applyTheme();
        }
    });
    
    // Also listen for storage changes (if user changes theme in another tab)
    window.addEventListener('storage', function(e) {
        if (e.key === 'theme') {
            applyTheme();
        }
    });
})();
// Initialize and maintain theme across all navigation
(function() {
    function getTheme() {
        return localStorage.getItem('theme') || 'auto';
    }
    
    function applyTheme() {
        const theme = getTheme();
        document.documentElement.setAttribute('data-theme', theme);
    }
    
    // Apply theme immediately
    applyTheme();
    
    // Create a MutationObserver to maintain theme if it gets reset
    const observer = new MutationObserver(function(mutations) {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const expectedTheme = getTheme();
        
        if (currentTheme !== expectedTheme) {
            document.documentElement.setAttribute('data-theme', expectedTheme);
        }
    });
    
    // Start observing
    observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['data-theme']
    });
    
    // Also listen for storage changes (if user changes theme in another tab)
    window.addEventListener('storage', function(e) {
        if (e.key === 'theme') {
            applyTheme();
        }
    });
})();
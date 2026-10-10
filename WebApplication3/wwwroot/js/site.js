document.addEventListener('DOMContentLoaded', function () {
    // Theme toggle
    const themeToggle = document.getElementById('theme-toggle');
    const html = document.documentElement;

    const savedTheme = localStorage.getItem('theme') || 'light';
    html.setAttribute('data-theme', savedTheme);

    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            const current = html.getAttribute('data-theme');
            const next = current === 'dark' ? 'light' : 'dark';
            html.setAttribute('data-theme', next);
            localStorage.setItem('theme', next);
        });
    }

    // Copyright year (auto-updates)
    const yearEl = document.getElementById('copyright-year');
    if (yearEl) {
        const year = new Date().getFullYear();
        yearEl.textContent = year;
    }

    // Form loading state
    const form = document.getElementById('lookup-form');
    const btn = document.getElementById('search-btn');

    if (form && btn) {
        form.addEventListener('submit', function () {
            btn.classList.add('is-loading');
            btn.disabled = true;
        });
    }
});

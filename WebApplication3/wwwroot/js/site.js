document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('lookup-form');
    const btn = document.getElementById('search-btn');

    if (form && btn) {
        form.addEventListener('submit', function () {
            btn.classList.add('is-loading');
            btn.disabled = true;
        });
    }
});

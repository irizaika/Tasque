// hide success message
window.addEventListener('DOMContentLoaded', () => {
    const msg = document.getElementById('floating-message');
    if (msg) {
        setTimeout(() => {
            msg.classList.add('hide');    // optional if you want to completely remove
        }, 4000); // 4 seconds
    }
});

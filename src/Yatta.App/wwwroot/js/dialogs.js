(() => {
    let activeDialog = null;
    let previousFocus = null;

    function getDialog() {
        const dialogs = document.querySelectorAll('.dialog-backdrop');
        return dialogs.length ? dialogs[dialogs.length - 1] : null;
    }

    function focusable(dialog) {
        return [...dialog.querySelectorAll('button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), a[href], [tabindex]:not([tabindex="-1"])')]
            .filter(element => element.getClientRects().length > 0);
    }

    function updateDialog() {
        const dialog = getDialog();
        if (dialog === activeDialog) return;
        if (dialog) {
            previousFocus = document.activeElement;
            activeDialog = dialog;
            requestAnimationFrame(() => {
                if (getDialog() === dialog) (focusable(dialog)[0] || dialog).focus();
            });
        } else {
            activeDialog = null;
            if (previousFocus?.isConnected) previousFocus.focus();
            previousFocus = null;
        }
    }

    document.addEventListener('keydown', event => {
        const dialog = getDialog();
        if (!dialog) return;
        if (event.key === 'Escape') {
            event.preventDefault();
            (dialog.querySelector('[data-dialog-cancel]') || dialog).click();
        } else if (event.key === 'Tab') {
            const items = focusable(dialog);
            if (!items.length) { event.preventDefault(); dialog.focus(); return; }
            const first = items[0], last = items[items.length - 1];
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
        }
    });

    new MutationObserver(updateDialog).observe(document.getElementById('app'), { childList: true, subtree: true });
})();

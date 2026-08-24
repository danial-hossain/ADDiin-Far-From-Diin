// Ad-Diin Interactive Scripts
document.addEventListener('DOMContentLoaded', function () {

    // ==========================================
    // Auto-dismiss alerts after 6 seconds
    // ==========================================
    const alerts = document.querySelectorAll('.alert-dismissible');

    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert?.close();
        }, 6000);
    });


    // ==========================================
    // Remove SDG-related UI elements
    // ==========================================
    function containsSdgText(element) {
        try {
            const text = (element.textContent || '').trim();

            return /\bSDG\b|Sustainable Development Goal/i.test(text);
        } catch (error) {
            return false;
        }
    }


    // Remove visible SDG labels/text
    function removeSdgTextElements() {
        const selectors = [
            'th',
            'td',
            'label',
            'span',
            'p',
            'a',
            'small',
            'strong',
            'em',
            'li'
        ];

        document.querySelectorAll(selectors.join(',')).forEach(function (element) {
            if (containsSdgText(element)) {
                element.remove();
            }
        });
    }


    // Remove SDG-related form controls
    function removeSdgInputs() {
        document.querySelectorAll(
            'input, select, textarea, button'
        ).forEach(function (element) {

            const combined = (
                (element.id || '') + ' ' +
                (element.name || '') + ' ' +
                (element.className || '') + ' ' +
                (element.getAttribute('data-field') || '') + ' ' +
                (element.getAttribute('data-sdg') || '')
            ).toLowerCase();

            if (combined.includes('sdg')) {

                const parent = element.closest(
                    '.form-group, .form-row, .field, .form-group-row'
                ) || element.parentElement;

                if (parent) {
                    parent.remove();
                } else {
                    element.remove();
                }
            }
        });
    }


    // Remove elements with SDG-related data attributes
    function removeSdgDataElements() {
        document.querySelectorAll(
            '[data-sdg], [data-field]'
        ).forEach(function (element) {

            const dataSdg = (
                element.getAttribute('data-sdg') || ''
            ).toLowerCase();

            const dataField = (
                element.getAttribute('data-field') || ''
            ).toLowerCase();

            if (
                dataSdg.includes('sdg') ||
                dataField.includes('sdg')
            ) {
                element.remove();
            }
        });
    }


    // Remove remaining leaf elements containing SDG
    function removeRemainingSdgElements() {
        document.querySelectorAll('body *').forEach(function (element) {

            if (
                element.children.length === 0 &&
                containsSdgText(element)
            ) {
                element.remove();
            }
        });
    }


    // Run SDG removal
    function removeSdgElements() {
        removeSdgTextElements();
        removeSdgInputs();
        removeSdgDataElements();
        removeRemainingSdgElements();
    }


    // Initial run
    removeSdgElements();

});
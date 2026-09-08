// Ad-Diin Interactive Scripts
document.addEventListener('DOMContentLoaded', function () {

    // Lightweight motion layer shared by all views. It only enhances existing
    // elements and never changes form, navigation, or feature behavior.
    document.body.classList.add('page-enter');

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const revealTargets = document.querySelectorAll(
        'main > section, main > div > section, main .card-custom, main .glass-card, main .rounded-3xl.border'
    );

    if (!reduceMotion && 'IntersectionObserver' in window) {
        const revealObserver = new IntersectionObserver(function (entries, observer) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('ad-reveal', 'is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.08, rootMargin: '0px 0px -40px' });

        revealTargets.forEach(function (element) {
            element.classList.add('ad-reveal');
            revealObserver.observe(element);
        });
    }

    if (!reduceMotion) {
        document.querySelectorAll('.card-custom, .glass-card, .prayer-time-card').forEach(function (card) {
            card.classList.add('ad-tilt');
            card.addEventListener('pointermove', function (event) {
                if (event.pointerType === 'touch') return;
                const rect = card.getBoundingClientRect();
                const x = (event.clientX - rect.left) / rect.width - 0.5;
                const y = (event.clientY - rect.top) / rect.height - 0.5;
                card.style.transform = `perspective(900px) rotateX(${(y * -2.2).toFixed(2)}deg) rotateY(${(x * 2.2).toFixed(2)}deg) translateY(-2px)`;
            });
            card.addEventListener('pointerleave', function () {
                card.style.transform = '';
            });
        });
    }

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
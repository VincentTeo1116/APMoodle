/* ═══════════════════════════════════════════════════════
   APMoodle  ·  Global page loader controller
   - Hides loader once page is fully loaded
   - Shows it again briefly during in-app link navigation
   - Skips external links, downloads, target=_blank, and
     anchors / mailto / tel / javascript:
   ═══════════════════════════════════════════════════════ */
(function () {
    'use strict';

    var MIN_VISIBLE_MS = 1400;  // keep loader visible long enough to actually see the animation
    var FADE_MS        = 800;   // matches CSS opacity transition (+buffer)
    var STORAGE_KEY    = 'apmLoaderVariant'; // "paper" (default) | "rings"
    var shownAt        = Date.now();

    function getLoader() {
        return document.getElementById('apm-loader');
    }

    // Apply user-selected variant on every page. Default = 'rings'.
    // Paper loader is opt-in via localStorage.apmLoaderVariant = 'paper'.
    function applyVariant() {
        var loader = getLoader();
        if (!loader) return;
        var variant;
        try { variant = localStorage.getItem(STORAGE_KEY); } catch (e) { variant = null; }
        if (variant === 'paper') {
            loader.classList.remove('apm-loader--rings');
        } else {
            loader.classList.add('apm-loader--rings');
            ensureRingsMarkup(loader);
        }
    }

    // Inject ring DOM if loader was rendered without it (layout loader markup is paper-only)
    function ensureRingsMarkup(loader) {
        var stage = loader.querySelector('.apm-loader-stage');
        if (!stage) return;
        if (!stage.querySelector('.apm-loader-rings')) {
            var rings = document.createElement('div');
            rings.className = 'apm-loader-rings';
            rings.innerHTML = '<div class="ring r1"></div><div class="ring r2"></div><div class="ring r3"></div>';
            // Insert before the wordmark so layout reads: rings → wordmark → subtext
            var wordmark = stage.querySelector('.apm-loader-wordmark');
            if (wordmark) stage.insertBefore(rings, wordmark);
            else stage.appendChild(rings);
        }
        if (!stage.querySelector('.apm-loader-subtext')) {
            var sub = document.createElement('div');
            sub.className = 'apm-loader-subtext';
            sub.textContent = 'Loading Experience';
            stage.appendChild(sub);
        }
    }

    applyVariant();

    function hideLoader() {
        var loader = getLoader();
        if (!loader) return;

        var elapsed = Date.now() - shownAt;
        var wait = Math.max(0, MIN_VISIBLE_MS - elapsed);

        setTimeout(function () {
            loader.classList.add('is-hidden');
            // Remove from DOM after fade so it can't trap clicks
            setTimeout(function () {
                if (loader && loader.parentNode) {
                    loader.style.display = 'none';
                }
            }, FADE_MS);
        }, wait);
    }

    function showLoader() {
        var loader = getLoader();
        if (!loader) return;
        loader.style.display = '';
        // Force reflow so the transition runs
        void loader.offsetWidth;
        loader.classList.remove('is-hidden');
        shownAt = Date.now();
    }

    function isInternalNavigation(anchor) {
        if (!anchor || !anchor.href) return false;
        if (anchor.target && anchor.target !== '' && anchor.target !== '_self') return false;
        if (anchor.hasAttribute('download')) return false;
        if (anchor.dataset && anchor.dataset.noLoader === 'true') return false;

        var href = anchor.getAttribute('href');
        if (!href) return false;
        if (href.charAt(0) === '#') return false;
        if (/^(mailto:|tel:|javascript:)/i.test(href)) return false;

        try {
            var url = new URL(anchor.href, window.location.href);
            if (url.origin !== window.location.origin) return false;
            // Same page + only hash change → no real navigation
            if (url.pathname === window.location.pathname &&
                url.search   === window.location.search   &&
                url.hash     !== window.location.hash) {
                return false;
            }
        } catch (e) {
            return false;
        }

        return true;
    }

    // ── Hide loader once the page finishes loading ──────────
    if (document.readyState === 'complete') {
        hideLoader();
    } else {
        window.addEventListener('load', hideLoader);
    }

    // Safety net: if 'load' never fires (slow asset), hide after 6s
    setTimeout(function () { hideLoader(); }, 6000);

    // ── Re-show on internal link clicks ─────────────────────
    document.addEventListener('click', function (e) {
        // Honor modifier keys for new-tab behavior
        if (e.defaultPrevented) return;
        if (e.button !== 0) return;
        if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

        var target = e.target;
        while (target && target.nodeType === 1 && target.tagName !== 'A') {
            target = target.parentNode;
        }
        if (target && target.tagName === 'A' && isInternalNavigation(target)) {
            showLoader();
        }
    }, true);

    // ── Re-show on form submits to internal endpoints ──────
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form || form.tagName !== 'FORM') return;
        if (form.dataset && form.dataset.noLoader === 'true') return;

        var action = form.getAttribute('action') || window.location.href;
        try {
            var url = new URL(action, window.location.href);
            if (url.origin === window.location.origin) {
                showLoader();
            }
        } catch (err) { /* ignore */ }
    }, true);

    // ── Browser back/forward (bfcache restore) → hide loader ─
    window.addEventListener('pageshow', function (e) {
        if (e.persisted) hideLoader();
    });
})();

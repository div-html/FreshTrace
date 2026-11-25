(function () {
  // Password eye visibility toggle
  document.addEventListener('click', function (e) {
    const toggleBtn = e.target.closest('[data-toggle-password]');
    if (!toggleBtn) return;

    const targetId = toggleBtn.getAttribute('data-toggle-password');
    const input = document.getElementById(targetId);
    if (!input) return;

    const isPassword = input.type === 'password';
    input.type = isPassword ? 'text' : 'password';

    const eyeOpen = toggleBtn.querySelector('[data-eye-open]');
    const eyeClosed = toggleBtn.querySelector('[data-eye-closed]');
    if (eyeOpen && eyeClosed) {
      eyeOpen.style.display = isPassword ? 'none' : '';
      eyeClosed.style.display = isPassword ? '' : 'none';
    }
  });

  // Modal helpers (open/close via data attributes + Escape key)
  window.FreshTraceModal = {
    open: function (id) {
      const el = document.getElementById(id);
      if (el) el.style.display = 'flex';
    },
    close: function (id) {
      const el = document.getElementById(id);
      if (el) el.style.display = 'none';
    },
    closeAll: function () {
      document.querySelectorAll('[data-modal-backdrop]').forEach(function (el) {
        if (el.getAttribute('data-server-open') === 'true') {
          const closeUrl = el.getAttribute('data-close-url');
          if (closeUrl) {
            window.location.href = closeUrl;
            return;
          }
        }
        el.style.display = 'none';
      });
    }
  };

  document.addEventListener('click', function (e) {
    const openBtn = e.target.closest('[data-open-modal]');
    if (openBtn) {
      window.FreshTraceModal.open(openBtn.getAttribute('data-open-modal'));
      return;
    }

    const closeBtn = e.target.closest('[data-close-modal]');
    if (closeBtn) {
      const modalId = closeBtn.getAttribute('data-close-modal');
      const modal = document.getElementById(modalId);
      if (modal && modal.getAttribute('data-server-open') === 'true') {
        const closeUrl = modal.getAttribute('data-close-url');
        if (closeUrl) {
          window.location.href = closeUrl;
          return;
        }
      }
      window.FreshTraceModal.close(modalId);
      return;
    }

    if (e.target.hasAttribute('data-modal-backdrop')) {
      if (e.target.getAttribute('data-server-open') === 'true') {
        const closeUrl = e.target.getAttribute('data-close-url');
        if (closeUrl) {
          window.location.href = closeUrl;
          return;
        }
      }
      e.target.style.display = 'none';
    }
  });

  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
      window.FreshTraceModal.closeAll();
    }
  });

  // Copy Batch Code
  document.addEventListener('click', function (e) {
    const copyBtn = e.target.closest('[data-copy-text]');
    if (!copyBtn) return;

    const text = copyBtn.getAttribute('data-copy-text') || '';
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).catch(function () {});
    }
    const labelSpan = copyBtn.querySelector('[data-copy-label]') || copyBtn;
    const original = labelSpan.textContent;
    labelSpan.textContent = 'Copied Batch ID';
    setTimeout(function () {
      labelSpan.textContent = original;
    }, 1800);
  });

  // GPS Auto-Detect Location
  document.addEventListener('click', function (e) {
    const gpsBtn = e.target.closest('[data-gps-detect]');
    if (!gpsBtn) return;

    const coordsInput = document.getElementById(gpsBtn.getAttribute('data-coords-target'));
    const districtInput = document.getElementById(gpsBtn.getAttribute('data-district-target'));
    const badgeEl = document.getElementById(gpsBtn.getAttribute('data-badge-target'));
    const badgeText = document.getElementById(gpsBtn.getAttribute('data-badge-text'));
    const btnLabel = gpsBtn.querySelector('span');

    gpsBtn.disabled = true;
    if (btnLabel) btnLabel.textContent = 'Detecting...';

    const applyResult = function (coordsStr) {
      if (coordsInput) coordsInput.value = coordsStr;
      if (districtInput) districtInput.value = 'Shimla District, Himachal Pradesh';
      if (badgeEl) {
        badgeEl.classList.add('visible');
        badgeEl.style.display = 'flex';
      }
      if (badgeText) {
        badgeText.textContent = 'Location detected: Shimla District (' + coordsStr + ')';
      }
      gpsBtn.disabled = false;
      if (btnLabel) btnLabel.textContent = 'Use Current Location';
    };

    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        function (pos) {
          const lat = pos.coords.latitude.toFixed(4);
          const lng = pos.coords.longitude.toFixed(4);
          const coordsStr =
            Math.abs(lat) + '° ' + (lat >= 0 ? 'N' : 'S') + ', ' +
            Math.abs(lng) + '° ' + (lng >= 0 ? 'E' : 'W');
          applyResult(coordsStr);
        },
        function () {
          setTimeout(function () {
            applyResult('31.1048° N, 77.1734° E');
          }, 350);
        },
        { timeout: 5000, enableHighAccuracy: true }
      );
    } else {
      applyResult('31.1048° N, 77.1734° E');
    }
  });

  // Profile Farm Card Float-Right-In Reveal
  const farmCard = document.querySelector('[data-farm-reveal]');
  if (farmCard) {
    const reveal = function () {
      const rect = farmCard.getBoundingClientRect();
      if (rect.top <= window.innerHeight * 0.88) {
        farmCard.classList.add('float-right-in');
      }
    };
    if ('IntersectionObserver' in window) {
      const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            farmCard.classList.add('float-right-in');
          }
        });
      }, { threshold: 0.15 });
      observer.observe(farmCard);
    }
    window.addEventListener('scroll', reveal, { passive: true });
    reveal();
  }
})();

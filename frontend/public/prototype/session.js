(function () {
  const api = window.OmniStayApi;

  const hotelImages = {
    1: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80',
    2: 'https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=1200&q=80',
    3: 'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1200&q=80',
    4: 'https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=1200&q=80',
    5: 'https://images.unsplash.com/photo-1519681393784-d120267933ba?auto=format&fit=crop&w=1200&q=80',
    6: 'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1200&q=80',
    7: 'https://images.unsplash.com/photo-1528127269322-539801943592?auto=format&fit=crop&w=1200&q=80',
    8: 'https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&w=1200&q=80',
    9: 'https://images.unsplash.com/photo-1500375592092-40eb2168fd21?auto=format&fit=crop&w=1200&q=80',
    10: 'https://images.unsplash.com/photo-1539650116574-75c0c6d73f6e?auto=format&fit=crop&w=1200&q=80'
  };

  const fallbackImage = 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1200&q=80';

  function pageName() {
    return window.location.pathname.split('/').pop() || 'index.html';
  }

  function currentRelativeUrl() {
    return `${pageName()}${window.location.search}`;
  }

  function safeReturnUrl(value) {
    if (!value || value.startsWith('http://') || value.startsWith('https://') || value.startsWith('//')) {
      return null;
    }

    return value.includes('index.html') ? null : value;
  }

  function requireAuth(options = {}) {
    const session = api.getSession();
    if (!session.token || !session.user) {
      window.location.replace(`index.html?returnUrl=${encodeURIComponent(currentRelativeUrl())}`);
      return null;
    }

    if (options.role && session.user.role !== options.role) {
      window.location.replace('home.html');
      return null;
    }

    return session;
  }

  function redirectIfAuthenticated() {
    const session = api.getSession();
    if (!session.token || !session.user) {
      return;
    }

    const params = new URLSearchParams(window.location.search);
    window.location.replace(safeReturnUrl(params.get('returnUrl')) || 'home.html');
  }

  function hydrateNav(active) {
    const session = api.getSession();
    const user = session.user;

    document.querySelectorAll('[data-session-name]').forEach((target) => {
      target.textContent = user ? `${user.fullName} (${user.role})` : 'Khach';
    });

    document.querySelectorAll('[data-api-base]').forEach((target) => {
      target.textContent = api.apiBase;
    });

    document.querySelectorAll('[data-nav]').forEach((item) => {
      item.classList.toggle('active', item.dataset.nav === active);
    });

    document.querySelectorAll('[data-nav-admin]').forEach((item) => {
      item.hidden = !user || user.role !== 'Admin';
    });

    document.querySelectorAll('[data-logout]').forEach((button) => {
      button.addEventListener('click', () => {
        api.clearSession();
        window.location.replace('index.html');
      });
    });
  }

  function defaultDates() {
    const checkIn = new Date();
    checkIn.setDate(checkIn.getDate() + 14);
    const checkOut = new Date(checkIn);
    checkOut.setDate(checkOut.getDate() + 3);

    return {
      checkIn: checkIn.toISOString().slice(0, 10),
      checkOut: checkOut.toISOString().slice(0, 10)
    };
  }

  function readSearchParams() {
    const params = new URLSearchParams(window.location.search);
    const dates = defaultDates();
    return {
      city: params.get('city') || 'Da Nang',
      checkIn: params.get('checkIn') || dates.checkIn,
      checkOut: params.get('checkOut') || dates.checkOut,
      guests: params.get('guests') || '2',
      hotelId: params.get('hotelId') || '',
      roomTypeId: params.get('roomTypeId') || '',
      code: params.get('code') || ''
    };
  }

  function toQuery(params) {
    const query = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && String(value).length > 0) {
        query.set(key, value);
      }
    });
    return query.toString();
  }

  function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0
    }).format(value);
  }

  function formatDate(value) {
    return new Intl.DateTimeFormat('vi-VN').format(new Date(`${value}T00:00:00`));
  }

  function escapeHtml(value) {
    return String(value ?? '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  function statusLabel(value) {
    return {
      PendingPayment: 'Cho thanh toan',
      Confirmed: 'Da xac nhan',
      Cancelled: 'Da huy',
      Pending: 'Cho thanh toan',
      Paid: 'Da thanh toan'
    }[value] || value;
  }

  function statusClass(value) {
    return {
      PendingPayment: 'status-pending',
      Pending: 'status-pending',
      Confirmed: 'status-good',
      Paid: 'status-good',
      Cancelled: 'status-bad'
    }[value] || 'status-muted';
  }

  function hotelImage(hotelId, src) {
    if (src && /^https?:\/\//i.test(src)) {
      return src;
    }

    return hotelImages[Number(hotelId)] || fallbackImage;
  }

  function roomImage(room, hotelId) {
    if (room && room.imageUrl && /^https?:\/\//i.test(room.imageUrl)) {
      return room.imageUrl;
    }

    return hotelImage(hotelId || (room && room.hotelId));
  }

  function renderMessage(target, message, tone = 'muted') {
    target.className = `message ${tone}`;
    target.textContent = message;
    target.hidden = false;
  }

  window.OmniStaySession = {
    requireAuth,
    redirectIfAuthenticated,
    hydrateNav,
    defaultDates,
    readSearchParams,
    toQuery,
    safeReturnUrl,
    formatCurrency,
    formatDate,
    escapeHtml,
    statusLabel,
    statusClass,
    hotelImage,
    roomImage,
    renderMessage
  };
})();

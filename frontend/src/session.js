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

  const vietnamCities = [
    { value: '', label: 'Tất cả tỉnh/thành' },
    { value: 'An Giang', label: 'An Giang' },
    { value: 'Ba Ria - Vung Tau', label: 'Bà Rịa - Vũng Tàu' },
    { value: 'Bac Giang', label: 'Bắc Giang' },
    { value: 'Bac Kan', label: 'Bắc Kạn' },
    { value: 'Bac Lieu', label: 'Bạc Liêu' },
    { value: 'Bac Ninh', label: 'Bắc Ninh' },
    { value: 'Ben Tre', label: 'Bến Tre' },
    { value: 'Binh Dinh', label: 'Bình Định' },
    { value: 'Binh Duong', label: 'Bình Dương' },
    { value: 'Binh Phuoc', label: 'Bình Phước' },
    { value: 'Binh Thuan', label: 'Bình Thuận' },
    { value: 'Ca Mau', label: 'Cà Mau' },
    { value: 'Can Tho', label: 'Cần Thơ' },
    { value: 'Cao Bang', label: 'Cao Bằng' },
    { value: 'Da Nang', label: 'Đà Nẵng' },
    { value: 'Dak Lak', label: 'Đắk Lắk' },
    { value: 'Dak Nong', label: 'Đắk Nông' },
    { value: 'Dien Bien', label: 'Điện Biên' },
    { value: 'Dong Nai', label: 'Đồng Nai' },
    { value: 'Dong Thap', label: 'Đồng Tháp' },
    { value: 'Gia Lai', label: 'Gia Lai' },
    { value: 'Ha Giang', label: 'Hà Giang' },
    { value: 'Ha Nam', label: 'Hà Nam' },
    { value: 'Ha Noi', label: 'Hà Nội' },
    { value: 'Ha Tinh', label: 'Hà Tĩnh' },
    { value: 'Hai Duong', label: 'Hải Dương' },
    { value: 'Hai Phong', label: 'Hải Phòng' },
    { value: 'Hau Giang', label: 'Hậu Giang' },
    { value: 'Ho Chi Minh', label: 'TP. Hồ Chí Minh' },
    { value: 'Hoa Binh', label: 'Hòa Bình' },
    { value: 'Hung Yen', label: 'Hưng Yên' },
    { value: 'Khanh Hoa', label: 'Khánh Hòa' },
    { value: 'Kien Giang', label: 'Kiên Giang' },
    { value: 'Kon Tum', label: 'Kon Tum' },
    { value: 'Lai Chau', label: 'Lai Châu' },
    { value: 'Lam Dong', label: 'Lâm Đồng' },
    { value: 'Lang Son', label: 'Lạng Sơn' },
    { value: 'Lao Cai', label: 'Lào Cai' },
    { value: 'Long An', label: 'Long An' },
    { value: 'Nam Dinh', label: 'Nam Định' },
    { value: 'Nghe An', label: 'Nghệ An' },
    { value: 'Ninh Binh', label: 'Ninh Bình' },
    { value: 'Ninh Thuan', label: 'Ninh Thuận' },
    { value: 'Phu Tho', label: 'Phú Thọ' },
    { value: 'Phu Yen', label: 'Phú Yên' },
    { value: 'Quang Binh', label: 'Quảng Bình' },
    { value: 'Quang Nam', label: 'Quảng Nam' },
    { value: 'Quang Ngai', label: 'Quảng Ngãi' },
    { value: 'Quang Ninh', label: 'Quảng Ninh' },
    { value: 'Quang Tri', label: 'Quảng Trị' },
    { value: 'Soc Trang', label: 'Sóc Trăng' },
    { value: 'Son La', label: 'Sơn La' },
    { value: 'Tay Ninh', label: 'Tây Ninh' },
    { value: 'Thai Binh', label: 'Thái Bình' },
    { value: 'Thai Nguyen', label: 'Thái Nguyên' },
    { value: 'Thanh Hoa', label: 'Thanh Hóa' },
    { value: 'Thua Thien Hue', label: 'Thừa Thiên Huế' },
    { value: 'Tien Giang', label: 'Tiền Giang' },
    { value: 'Tra Vinh', label: 'Trà Vinh' },
    { value: 'Tuyen Quang', label: 'Tuyên Quang' },
    { value: 'Vinh Long', label: 'Vĩnh Long' },
    { value: 'Vinh Phuc', label: 'Vĩnh Phúc' },
    { value: 'Yen Bai', label: 'Yên Bái' },
    { value: 'Da Lat', label: 'Đà Lạt' },
    { value: 'Hoi An', label: 'Hội An' },
    { value: 'Nha Trang', label: 'Nha Trang' },
    { value: 'Phu Quoc', label: 'Phú Quốc' },
    { value: 'Quy Nhon', label: 'Quy Nhơn' },
    { value: 'Sa Pa', label: 'Sa Pa' }
  ];

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

    const allowedRoles = options.roles || (options.role ? [options.role] : []);
    if (allowedRoles.length > 0 && !allowedRoles.includes(session.user.role)) {
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

  function visibleElement(element) {
    return element && !element.hidden && element.getAttribute('hidden') === null;
  }

  function mobileNavLinkFrom(source, active) {
    const link = document.createElement('a');
    link.href = source.getAttribute('href') || '#';
    link.textContent = source.textContent.trim();
    link.className = 'omni-mobile-nav-item';
    if (source.dataset.nav === active || source.classList.contains('active')) {
      link.classList.add('active');
      link.setAttribute('aria-current', 'page');
    }
    return link;
  }

  function enhanceMobileNav(active) {
    const header = document.querySelector('header');
    const desktopNav = header && header.querySelector('nav');
    if (!header || !desktopNav) {
      return;
    }

    const shell = header.firstElementChild;
    const brandRow = desktopNav.parentElement;
    if (!shell || !brandRow) {
      return;
    }

    let toggle = header.querySelector('[data-mobile-nav-toggle]');
    let panel = header.querySelector('[data-mobile-nav-panel]');

    header.dataset.mobileNavReady = 'true';

    if (!toggle) {
      toggle = document.createElement('button');
      toggle.type = 'button';
      toggle.className = 'omni-mobile-toggle';
      toggle.dataset.mobileNavToggle = 'true';
      toggle.setAttribute('aria-expanded', 'false');
      toggle.setAttribute('aria-label', 'Mở menu');
      toggle.innerHTML = '<span></span><span></span><span></span>';
      brandRow.appendChild(toggle);
    }

    if (!panel) {
      panel = document.createElement('div');
      panel.className = 'omni-mobile-panel';
      panel.dataset.mobileNavPanel = 'true';
      panel.hidden = true;
      shell.appendChild(panel);
    }

    const setOpen = (open) => {
      panel.hidden = !open;
      toggle.setAttribute('aria-expanded', String(open));
      header.classList.toggle('omni-mobile-open', open);
    };

    if (!toggle.dataset.mobileNavBound) {
      toggle.dataset.mobileNavBound = 'true';
      toggle.addEventListener('click', () => {
        setOpen(panel.hidden);
      });

      document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
          setOpen(false);
        }
      });

      document.addEventListener('click', (event) => {
        if (!header.contains(event.target)) {
          setOpen(false);
        }
      });
    }

    panel.innerHTML = '';

    Array.from(desktopNav.querySelectorAll('a'))
      .filter(visibleElement)
      .forEach((source) => {
        panel.appendChild(mobileNavLinkFrom(source, active));
      });

    const actionSource = Array.from(shell.children)
      .find((child) => child !== brandRow && child !== panel);

    if (actionSource) {
      const actionLinks = Array.from(actionSource.querySelectorAll('a')).filter(visibleElement);
      if (actionLinks.length > 0) {
        const divider = document.createElement('div');
        divider.className = 'omni-mobile-divider';
        panel.appendChild(divider);
      }

      actionLinks.forEach((source) => {
        panel.appendChild(mobileNavLinkFrom(source, active));
      });

      const sessionName = actionSource.querySelector('[data-session-name]');
      if (visibleElement(sessionName)) {
        const userBadge = document.createElement('div');
        userBadge.className = 'omni-mobile-user';
        userBadge.textContent = sessionName.textContent;
        panel.appendChild(userBadge);
      }

      if (actionSource.querySelector('[data-logout]')) {
        const logout = document.createElement('button');
        logout.type = 'button';
        logout.className = 'omni-mobile-nav-item omni-mobile-logout';
        logout.textContent = 'Đăng xuất';
        logout.addEventListener('click', () => {
          api.clearSession();
          window.location.replace('index.html');
        });
        panel.appendChild(logout);
      }
    }
  }

  function hydrateNav(active) {
    const session = api.getSession();
    const user = session.user;
    const isAdmin = user && user.role === 'Admin';
    const canManageHotels = user && (user.role === 'Admin' || user.role === 'HotelOwner');

    document.querySelectorAll('[data-session-name]').forEach((target) => {
      target.textContent = user ? `${user.fullName} (${roleLabel(user.role)})` : 'Khách';
      if (user) {
        const goProfile = () => {
          window.location.href = 'profile.html';
        };
        target.setAttribute('role', 'button');
        target.setAttribute('tabindex', '0');
        target.setAttribute('title', 'Trang cá nhân');
        target.classList.add('cursor-pointer');
        target.style.cursor = 'pointer';
        target.addEventListener('click', goProfile);
        target.addEventListener('keydown', (event) => {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            goProfile();
          }
        });
      }
    });

    document.querySelectorAll('[data-api-base]').forEach((target) => {
      target.textContent = api.apiBase;
    });

    document.querySelectorAll('[data-profile-link]').forEach((profileLink) => {
      if (!user || profileLink.parentElement?.querySelector('[data-notifications-link]')) {
        return;
      }

      const notificationLink = document.createElement('a');
      notificationLink.href = 'notifications.html';
      notificationLink.dataset.notificationsLink = 'true';
      notificationLink.className = 'px-3 py-2 bg-white/10 rounded-lg text-white text-sm font-semibold hover:bg-white/20 transition-colors';
      notificationLink.textContent = 'Thông báo';
      profileLink.parentElement.insertBefore(notificationLink, profileLink);

      api.getMyNotifications()
        .then((notifications) => {
          const unread = notifications.filter((item) => !item.isRead).length;
          if (unread > 0) {
            notificationLink.textContent = `Thông báo (${unread})`;
          }
        })
        .catch(() => {});

      if (user.role === 'HotelOwner' && !profileLink.parentElement.querySelector('[data-owner-profile-link]')) {
        const ownerLink = document.createElement('a');
        ownerLink.href = 'owner-profile.html';
        ownerLink.dataset.ownerProfileLink = 'true';
        ownerLink.className = 'px-3 py-2 bg-white/10 rounded-lg text-white text-sm font-semibold hover:bg-white/20 transition-colors';
        ownerLink.textContent = 'Hồ sơ chủ KS';
        profileLink.parentElement.insertBefore(ownerLink, profileLink);
      }
    });

    document.querySelectorAll('[data-nav]').forEach((item) => {
      item.classList.toggle('active', item.dataset.nav === active);
    });

    document.querySelectorAll('[data-nav-admin]').forEach((item) => {
      item.hidden = !canManageHotels;
      if (user && user.role === 'HotelOwner') {
        item.textContent = 'Quản lý';
      }
    });

    document.querySelectorAll('[data-nav="status"]').forEach((item) => {
      item.hidden = !isAdmin;
    });

    document.querySelectorAll('[data-logout]').forEach((button) => {
      button.addEventListener('click', () => {
        api.clearSession();
        window.location.replace('index.html');
      });
    });

    enhanceMobileNav(active);
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
      keyword: params.get('keyword') || '',
      minRating: params.get('minRating') || '',
      sortBy: params.get('sortBy') || '',
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
    const rawValue = String(value || '');
    const date = rawValue.includes('T')
      ? new Date(rawValue)
      : new Date(`${rawValue}T00:00:00`);

    if (Number.isNaN(date.getTime())) {
      return rawValue;
    }

    return new Intl.DateTimeFormat('vi-VN').format(date);
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
      PendingPayment: 'Chờ thanh toán',
      Confirmed: 'Đã xác nhận',
      Cancelled: 'Đã hủy',
      Pending: 'Chờ thanh toán',
      Paid: 'Đã thanh toán',
      Refunded: 'Đã hoàn tiền'
    }[value] || value;
  }

  function roleLabel(value) {
    return {
      Admin: 'Admin',
      HotelOwner: 'Chủ khách sạn',
      Customer: 'Khách'
    }[value] || value;
  }

  function statusClass(value) {
    return {
      PendingPayment: 'status-pending',
      Pending: 'status-pending',
      Confirmed: 'status-good',
      Paid: 'status-good',
      Refunded: 'status-good',
      Cancelled: 'status-bad'
    }[value] || 'status-muted';
  }

  function assetUrl(src) {
    if (!src) {
      return '';
    }

    if (/^(https?:)?\/\//i.test(src) || /^(data|blob):/i.test(src)) {
      return src;
    }

    if (src.startsWith('/api/')) {
      return `${api.apiOrigin}${src}`;
    }

    return '';
  }

  function hotelImage(hotelId, src) {
    const resolved = assetUrl(src);
    if (resolved) {
      return resolved;
    }

    return hotelImages[Number(hotelId)] || fallbackImage;
  }

  function roomImage(room, hotelId) {
    const resolved = assetUrl(room && room.imageUrl);
    if (resolved) {
      return resolved;
    }

    return hotelImage(hotelId || (room && room.hotelId));
  }

  function populateCitySelect(select, selectedValue, options = {}) {
    if (!select) {
      return;
    }

    const selected = selectedValue ?? select.value;
    const cities = options.includeAll === false
      ? vietnamCities.filter((city) => city.value)
      : vietnamCities;

    select.innerHTML = cities
      .map((city) => `<option value="${escapeHtml(city.value)}">${escapeHtml(city.label)}</option>`)
      .join('');
    select.value = selected;

    if (selected && select.value !== selected) {
      const option = document.createElement('option');
      option.value = selected;
      option.textContent = selected;
      select.appendChild(option);
      select.value = selected;
    }
  }

  function renderMessage(target, message, tone = 'muted') {
    if (tone === 'error') {
      target.className = 'p-6 bg-error-container text-error rounded-xl border border-error/30';
    } else if (tone === 'success') {
      target.className = 'p-6 bg-success-green/10 text-success-green rounded-xl border border-success-green/30';
    } else {
      target.className = 'p-6 bg-surface-container-lowest text-on-surface-variant rounded-xl border border-outline-variant';
    }
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
    roleLabel,
    statusClass,
    assetUrl,
    hotelImage,
    roomImage,
    populateCitySelect,
    renderMessage
  };
})();

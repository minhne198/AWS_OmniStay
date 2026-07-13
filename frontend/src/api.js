(function () {
  const config = window.__APP_CONFIG__ || {};
  const apiBase = (config.API_BASE || 'http://localhost:5010/api').replace(/\/$/, '');
  const tokenKey = 'omnistay_token';
  const userKey = 'omnistay_user';
  const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
  const uploadImageExtensions = new Set(['jpg', 'jpeg', 'png', 'webp', 'gif', 'jfif', 'bmp', 'avif']);
  const unsupportedBrowserImageExtensions = new Set(['heic', 'heif']);
  const uploadMaxDimension = 1600;
  const uploadJpegQuality = 0.84;

  function clearStoredSession() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(userKey);
  }

  function parseJwtPayload(token) {
    try {
      const payload = token.split('.')[1];
      if (!payload) {
        return null;
      }

      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
      const json = decodeURIComponent(Array.from(atob(padded), (char) =>
        `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`).join(''));
      return JSON.parse(json);
    } catch {
      return null;
    }
  }

  function tokenRole(claims) {
    return claims && (claims[roleClaim] || claims.role);
  }

  function isExpired(claims) {
    return Boolean(claims && typeof claims.exp === 'number' && claims.exp * 1000 <= Date.now() + 5000);
  }

  function pageName() {
    return window.location.pathname.split('/').pop() || 'index.html';
  }

  function currentRelativeUrl() {
    return `${pageName()}${window.location.search}`;
  }

  function redirectToLogin() {
    if (pageName() === 'index.html') {
      return;
    }

    window.location.replace(`index.html?returnUrl=${encodeURIComponent(currentRelativeUrl())}`);
  }

  function errorMessage(response, body) {
    if (body && body.error) {
      return body.error;
    }

    if (body && body.errors) {
      const messages = Object.values(body.errors).flat().filter(Boolean);
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }

    if (body && (body.detail || body.title)) {
      return body.detail || body.title;
    }

    if (response.status === 401) {
      return 'Phien dang nhap da het han. Vui long dang nhap lai.';
    }

    if (response.status === 403) {
      return 'May chu tu choi thao tac nay (HTTP 403). Vui long dang xuat, dang nhap lai va thu lai.';
    }

    return `HTTP ${response.status}`;
  }

  function apiOrigin() {
    if (apiBase.startsWith('http://') || apiBase.startsWith('https://')) {
      return new URL(apiBase).origin;
    }

    return window.location.origin;
  }

  function isJsonResponse(contentType) {
    return contentType.includes('json');
  }

  function fileExtension(file) {
    const name = file && file.name ? file.name : '';
    const dotIndex = name.lastIndexOf('.');
    return dotIndex >= 0 ? name.slice(dotIndex + 1).toLowerCase() : '';
  }

  function jpgFileName(file) {
    const name = file && file.name ? file.name : 'image';
    const dotIndex = name.lastIndexOf('.');
    const baseName = dotIndex > 0 ? name.slice(0, dotIndex) : name;
    return `${baseName || 'image'}.jpg`;
  }

  function loadImage(file) {
    return new Promise((resolve, reject) => {
      const image = new Image();
      const url = URL.createObjectURL(file);

      image.onload = () => {
        URL.revokeObjectURL(url);
        resolve(image);
      };

      image.onerror = () => {
        URL.revokeObjectURL(url);
        reject(new Error('Khong the doc file anh nay. Vui long chon anh JPG, PNG, WEBP hoac GIF.'));
      };

      image.src = url;
    });
  }

  function canvasToBlob(canvas, type, quality) {
    return new Promise((resolve, reject) => {
      canvas.toBlob((blob) => {
        if (blob) {
          resolve(blob);
          return;
        }

        reject(new Error('Khong the nen anh truoc khi upload.'));
      }, type, quality);
    });
  }

  async function prepareImageForUpload(file) {
    const extension = fileExtension(file);
    if (unsupportedBrowserImageExtensions.has(extension)) {
      throw new Error('File HEIC/HEIF chua duoc ho tro tren web. Anh doi anh sang JPG hoac PNG roi upload lai.');
    }

    if (!file || (!file.type.startsWith('image/') && !uploadImageExtensions.has(extension))) {
      throw new Error('Vui long chon file anh JPG, PNG, WEBP hoac GIF.');
    }

    if (file.type === 'image/gif' || extension === 'gif') {
      return file;
    }

    try {
      const image = await loadImage(file);
      const scale = Math.min(1, uploadMaxDimension / Math.max(image.naturalWidth, image.naturalHeight));
      const width = Math.max(1, Math.round(image.naturalWidth * scale));
      const height = Math.max(1, Math.round(image.naturalHeight * scale));
      const canvas = document.createElement('canvas');
      canvas.width = width;
      canvas.height = height;
      const context = canvas.getContext('2d');
      if (!context) {
        throw new Error('Trinh duyet khong the nen anh truoc khi upload.');
      }

      context.drawImage(image, 0, 0, width, height);

      const blob = await canvasToBlob(canvas, 'image/jpeg', uploadJpegQuality);
      if (typeof File === 'function') {
        return new File([blob], jpgFileName(file), { type: 'image/jpeg' });
      }

      blob.name = jpgFileName(file);
      return blob;
    } catch (error) {
      if (uploadImageExtensions.has(extension)) {
        return file;
      }

      throw error;
    }
  }

  async function refreshSessionToken(token) {
    try {
      const response = await fetch(`${apiBase}/auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        }
      });

      const contentType = response.headers.get('content-type') || '';
      const body = isJsonResponse(contentType) ? await response.json() : null;

      if (!response.ok || !body || !body.token || !body.user) {
        return false;
      }

      localStorage.setItem(tokenKey, body.token);
      localStorage.setItem(userKey, JSON.stringify(body.user));
      return true;
    } catch {
      return false;
    }
  }

  async function request(path, options = {}) {
    const { retryOnForbidden = true, ...fetchOptions } = options;
    const token = localStorage.getItem(tokenKey);
    const isFormData = typeof FormData !== 'undefined' && fetchOptions.body instanceof FormData;
    const headers = {
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(fetchOptions.headers || {})
    };

    const response = await fetch(`${apiBase}${path}`, {
      ...fetchOptions,
      headers
    });

    const contentType = response.headers.get('content-type') || '';
    const body = isJsonResponse(contentType) ? await response.json() : null;

    if (!response.ok) {
      const error = new Error(errorMessage(response, body));
      error.status = response.status;
      error.body = body;

      if (response.status === 401 && token && !path.startsWith('/auth/login')) {
        clearStoredSession();
        redirectToLogin();
      }

      if (response.status === 403 && retryOnForbidden && token && !path.startsWith('/auth/')) {
        const refreshed = await refreshSessionToken(token);
        if (refreshed) {
          return request(path, {
            ...fetchOptions,
            retryOnForbidden: false
          });
        }
      }

      throw error;
    }

    return body;
  }

  window.OmniStayApi = {
    apiBase,
    apiOrigin: apiOrigin(),
    getSession() {
      const token = localStorage.getItem(tokenKey);
      const userJson = localStorage.getItem(userKey);
      if (!token || !userJson) {
        return {
          token: null,
          user: null
        };
      }

      const claims = parseJwtPayload(token);
      if (!claims || isExpired(claims)) {
        clearStoredSession();
        return {
          token: null,
          user: null
        };
      }

      let user;
      try {
        user = JSON.parse(userJson);
      } catch {
        clearStoredSession();
        return {
          token: null,
          user: null
        };
      }

      const role = tokenRole(claims);
      if (role && user.role !== role) {
        user = { ...user, role };
        localStorage.setItem(userKey, JSON.stringify(user));
      }

      return {
        token,
        user
      };
    },
    setSession(auth) {
      localStorage.setItem(tokenKey, auth.token);
      localStorage.setItem(userKey, JSON.stringify(auth.user));
    },
    setUser(user) {
      localStorage.setItem(userKey, JSON.stringify(user));
    },
    clearSession() {
      clearStoredSession();
    },
    register(payload) {
      return request('/auth/register', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    login(payload) {
      return request('/auth/login', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    me() {
      return request('/auth/me');
    },
    updateProfile(payload) {
      return request('/auth/me', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    changePassword(payload) {
      return request('/auth/me/password', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    searchHotels(params) {
      const query = new URLSearchParams(params);
      return request(`/hotels/search?${query.toString()}`);
    },
    getHotel(hotelId) {
      return request(`/hotels/${encodeURIComponent(hotelId)}`);
    },
    getRooms(hotelId) {
      return request(`/hotels/${encodeURIComponent(hotelId)}/rooms`);
    },
    getHotelReviews(hotelId) {
      return request(`/hotels/${encodeURIComponent(hotelId)}/reviews`);
    },
    createHotelReview(hotelId, payload) {
      return request(`/hotels/${encodeURIComponent(hotelId)}/reviews`, {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    createBooking(payload) {
      return request('/bookings', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    getBookingByCode(bookingCode) {
      return request(`/bookings/${encodeURIComponent(bookingCode)}`);
    },
    getMyBookings() {
      return request('/bookings/my');
    },
    payBooking(bookingCode) {
      return request(`/bookings/${encodeURIComponent(bookingCode)}/pay`, {
        method: 'POST',
        body: JSON.stringify({ paymentMethod: 'DemoCard' })
      });
    },
    cancelBooking(bookingCode) {
      return request(`/bookings/${encodeURIComponent(bookingCode)}/cancel`, {
        method: 'POST'
      });
    },
    async getAwsStatus() {
      const response = await fetch(`${apiOrigin()}/health/aws`);
      const contentType = response.headers.get('content-type') || '';
      const body = isJsonResponse(contentType) ? await response.json() : null;

      if (!response.ok) {
        const error = new Error((body && body.error) || `HTTP ${response.status}`);
        error.status = response.status;
        error.body = body;
        throw error;
      }

      return body;
    },
    getAdminBookings() {
      return request('/admin/bookings');
    },
    getAdminDashboard() {
      return request('/admin/dashboard');
    },
    getAdminActivity() {
      return request('/admin/activity');
    },
    getAdminBalanceTransactions() {
      return request('/admin/balance-transactions');
    },
    getAdminUsers() {
      return request('/admin/users');
    },
    createAdminUser(payload) {
      return request('/admin/users', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    updateAdminUser(userId, payload) {
      return request(`/admin/users/${encodeURIComponent(userId)}`, {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    topUpAdminUser(userId, payload) {
      return request(`/admin/users/${encodeURIComponent(userId)}/balance/top-up`, {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    deleteAdminUser(userId) {
      return request(`/admin/users/${encodeURIComponent(userId)}/delete`, {
        method: 'POST'
      });
    },
    getAdminHotels() {
      return request('/admin/hotels');
    },
    getAdminRoomTypes(hotelId) {
      const query = hotelId ? `?hotelId=${encodeURIComponent(hotelId)}` : '';
      return request(`/admin/room-types${query}`);
    },
    createAdminHotel(payload) {
      return request('/admin/hotels', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    updateAdminHotel(hotelId, payload) {
      return request(`/admin/hotels/${encodeURIComponent(hotelId)}`, {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    createAdminRoomType(payload) {
      return request('/admin/room-types', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    updateAdminRoomType(roomTypeId, payload) {
      return request(`/admin/room-types/${encodeURIComponent(roomTypeId)}`, {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    },
    async uploadImage(file) {
      const preparedFile = await prepareImageForUpload(file);
      const formData = new FormData();
      formData.append('file', preparedFile, preparedFile.name || 'image.jpg');
      return request('/uploads/images', {
        method: 'POST',
        body: formData
      });
    },
    getMyNotifications() {
      return request('/notifications/my');
    },
    markNotificationRead(notificationId) {
      return request(`/notifications/${encodeURIComponent(notificationId)}/read`, {
        method: 'POST'
      });
    },
    markAllNotificationsRead() {
      return request('/notifications/read-all', {
        method: 'POST'
      });
    },
    getMyBalanceTransactions() {
      return request('/account/balance-transactions/my');
    },
    getOwnerProfile() {
      return request('/account/owner-profile');
    }
  };
})();

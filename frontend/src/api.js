(function () {
  const config = window.__APP_CONFIG__ || {};
  const apiBase = (config.API_BASE || 'http://localhost:5010/api').replace(/\/$/, '');
  const tokenKey = 'omnistay_token';
  const userKey = 'omnistay_user';

  function apiOrigin() {
    if (apiBase.startsWith('http://') || apiBase.startsWith('https://')) {
      return new URL(apiBase).origin;
    }

    return window.location.origin;
  }

  async function request(path, options = {}) {
    const token = localStorage.getItem(tokenKey);
    const isFormData = typeof FormData !== 'undefined' && options.body instanceof FormData;
    const headers = {
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers || {})
    };

    const response = await fetch(`${apiBase}${path}`, {
      ...options,
      headers
    });

    const contentType = response.headers.get('content-type') || '';
    const body = contentType.includes('application/json') ? await response.json() : null;

    if (!response.ok) {
      const error = new Error((body && body.error) || `HTTP ${response.status}`);
      error.status = response.status;
      error.body = body;
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
      return {
        token,
        user: userJson ? JSON.parse(userJson) : null
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
      localStorage.removeItem(tokenKey);
      localStorage.removeItem(userKey);
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
        method: 'PUT',
        body: JSON.stringify(payload)
      });
    },
    changePassword(payload) {
      return request('/auth/me/password', {
        method: 'PUT',
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
      return request(`/bookings/${encodeURIComponent(bookingCode)}`, {
        method: 'DELETE'
      });
    },
    async getAwsStatus() {
      const response = await fetch(`${apiOrigin()}/health/aws`);
      const contentType = response.headers.get('content-type') || '';
      const body = contentType.includes('application/json') ? await response.json() : null;

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
        method: 'PUT',
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
      return request(`/admin/users/${encodeURIComponent(userId)}`, {
        method: 'DELETE'
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
        method: 'PUT',
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
        method: 'PUT',
        body: JSON.stringify(payload)
      });
    },
    uploadImage(file) {
      const formData = new FormData();
      formData.append('file', file);
      return request('/admin/uploads/images', {
        method: 'POST',
        body: formData
      });
    },
    getMyNotifications() {
      return request('/notifications/my');
    },
    markNotificationRead(notificationId) {
      return request(`/notifications/${encodeURIComponent(notificationId)}/read`, {
        method: 'PUT'
      });
    },
    markAllNotificationsRead() {
      return request('/notifications/read-all', {
        method: 'PUT'
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

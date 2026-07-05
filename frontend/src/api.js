(function () {
  const config = window.__APP_CONFIG__ || {};
  const apiBase = (config.API_BASE || 'http://localhost:5000/api').replace(/\/$/, '');

  async function request(path, options) {
    const response = await fetch(`${apiBase}${path}`, {
      headers: {
        'Content-Type': 'application/json',
        ...(options && options.headers ? options.headers : {})
      },
      ...options
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
    searchHotels(params) {
      const query = new URLSearchParams(params);
      return request(`/hotels/search?${query.toString()}`);
    },
    getHotel(hotelId) {
      return request(`/hotels/${hotelId}`);
    },
    getRooms(hotelId) {
      return request(`/hotels/${hotelId}/rooms`);
    },
    createBooking(payload) {
      return request('/bookings', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    }
  };
})();

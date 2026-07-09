(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('bookings');

  const form = document.getElementById('lookupForm');
  const codeInput = document.getElementById('bookingCode');
  const list = document.getElementById('myBookingsList');

  form.addEventListener('submit', (event) => {
    event.preventDefault();
    const code = codeInput.value.trim();
    if (code) {
      window.location.href = `booking-confirmation.html?code=${encodeURIComponent(code)}`;
    }
  });

  async function loadMyBookings() {
    ui.renderMessage(list, 'Dang tai booking cua ban...', 'muted');

    try {
      const bookings = await api.getMyBookings();
      renderBookings(bookings);
    } catch (error) {
      ui.renderMessage(list, error.message, 'error');
    }
  }

  function renderBookings(bookings) {
    if (!bookings.length) {
      ui.renderMessage(list, 'Tai khoan nay chua co booking.', 'muted');
      return;
    }

    list.className = 'booking-list';
    list.hidden = false;
    list.innerHTML = bookings.map((booking) => `
      <article class="booking-row">
        <div>
          <p class="eyebrow">${ui.escapeHtml(booking.bookingCode)}</p>
          <h3>${ui.escapeHtml(booking.hotelName)}</h3>
          <p>${ui.escapeHtml(booking.roomTypeName)} · ${ui.formatDate(booking.checkIn)} - ${ui.formatDate(booking.checkOut)}</p>
          <p>${ui.formatCurrency(booking.totalPrice)} · <span class="pill ${ui.statusClass(booking.paymentStatus)}">${ui.statusLabel(booking.paymentStatus)}</span></p>
        </div>
        <div class="row-actions">
          <a class="ghost-button as-link" href="booking-confirmation.html?code=${encodeURIComponent(booking.bookingCode)}">Chi tiet</a>
          ${booking.paymentStatus === 'Pending' ? `<button class="primary-button small" type="button" data-pay="${ui.escapeHtml(booking.bookingCode)}">Thanh toan</button>` : ''}
          ${booking.status !== 'Cancelled' ? `<button class="secondary-button small" type="button" data-cancel="${ui.escapeHtml(booking.bookingCode)}">Huy</button>` : ''}
        </div>
      </article>
    `).join('');

    list.querySelectorAll('[data-pay]').forEach((button) => {
      button.addEventListener('click', () => updateBooking(() => api.payBooking(button.dataset.pay)));
    });

    list.querySelectorAll('[data-cancel]').forEach((button) => {
      button.addEventListener('click', () => updateBooking(() => api.cancelBooking(button.dataset.cancel)));
    });
  }

  async function updateBooking(action) {
    try {
      await action();
      await loadMyBookings();
    } catch (error) {
      ui.renderMessage(list, error.message, 'error');
    }
  }

  loadMyBookings();
})();

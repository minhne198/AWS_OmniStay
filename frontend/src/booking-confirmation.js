(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('bookings');

  const params = ui.readSearchParams();
  const target = document.getElementById('confirmationPanel');

  async function loadBooking() {
    if (!params.code) {
      ui.renderMessage(target, 'Thieu ma booking.', 'error');
      return;
    }

    ui.renderMessage(target, 'Dang tai booking...', 'muted');

    try {
      const booking = await api.getBookingByCode(params.code);
      renderBooking(booking);
    } catch (error) {
      ui.renderMessage(target, error.message, 'error');
    }
  }

  function renderBooking(booking) {
    target.className = 'confirmation-card';
    target.hidden = false;
    target.innerHTML = `
      <div>
        <p class="eyebrow">Ma booking</p>
        <h1>${ui.escapeHtml(booking.bookingCode)}</h1>
        <p>${ui.escapeHtml(booking.hotelName)} · ${ui.escapeHtml(booking.roomTypeName)}</p>
      </div>
      <dl class="details-list">
        <div><dt>Ngay o</dt><dd>${ui.formatDate(booking.checkIn)} - ${ui.formatDate(booking.checkOut)}</dd></div>
        <div><dt>So dem</dt><dd>${booking.nights}</dd></div>
        <div><dt>So khach</dt><dd>${booking.guests}</dd></div>
        <div><dt>Tong tien</dt><dd>${ui.formatCurrency(booking.totalPrice)}</dd></div>
        <div><dt>Booking</dt><dd><span class="pill ${ui.statusClass(booking.status)}">${ui.statusLabel(booking.status)}</span></dd></div>
        <div><dt>Thanh toan</dt><dd><span class="pill ${ui.statusClass(booking.paymentStatus)}">${ui.statusLabel(booking.paymentStatus)}</span></dd></div>
      </dl>
      <div class="actions-row">
        ${booking.paymentStatus === 'Pending' ? '<button class="primary-button" type="button" data-pay>Thanh toan gia lap</button>' : ''}
        ${booking.status !== 'Cancelled' ? '<button class="secondary-button" type="button" data-cancel>Huy booking</button>' : ''}
        <a class="ghost-button as-link" href="booking-lookup.html">Booking cua toi</a>
      </div>
    `;

    const payButton = target.querySelector('[data-pay]');
    if (payButton) {
      payButton.addEventListener('click', () => updateBooking(() => api.payBooking(booking.bookingCode)));
    }

    const cancelButton = target.querySelector('[data-cancel]');
    if (cancelButton) {
      cancelButton.addEventListener('click', () => updateBooking(() => api.cancelBooking(booking.bookingCode)));
    }
  }

  async function updateBooking(action) {
    ui.renderMessage(target, 'Dang cap nhat booking...', 'muted');
    try {
      renderBooking(await action());
    } catch (error) {
      ui.renderMessage(target, error.message, 'error');
    }
  }

  loadBooking();
})();

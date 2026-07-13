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
    ui.renderMessage(list, 'Đang tải booking của bạn...', 'muted');

    try {
      const bookings = await api.getMyBookings();
      renderBookings(bookings);
    } catch (error) {
      ui.renderMessage(list, error.message, 'error');
    }
  }

  function renderBookings(bookings) {
    if (!bookings.length) {
      ui.renderMessage(list, 'Tài khoản này chưa có booking.', 'muted');
      return;
    }

    list.className = 'space-y-4';
    list.hidden = false;
    list.innerHTML = bookings.map((booking) => `
      <article class="bg-surface-container-low border border-outline-variant rounded-xl p-5 flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div class="min-w-0">
          <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-1">${ui.escapeHtml(booking.bookingCode)}</p>
          <h3 class="text-headline-md font-headline-md text-on-surface">${ui.escapeHtml(booking.hotelName)}</h3>
          <p class="text-body-md text-on-surface-variant mt-1">${ui.escapeHtml(booking.roomTypeName)} · ${ui.formatDate(booking.checkIn)} - ${ui.formatDate(booking.checkOut)}</p>
          <p class="text-body-md text-on-surface-variant mt-2">${ui.formatCurrency(booking.totalPrice)} · <span class="px-2 py-1 rounded-full text-label-sm font-label-sm ${statusToneClass(booking.paymentStatus)}">${ui.statusLabel(booking.paymentStatus)}</span></p>
        </div>
        <div class="flex flex-wrap gap-2 md:justify-end">
          <a class="bg-surface-container-high hover:bg-surface-container-highest text-on-surface font-label-md text-label-md px-4 py-2 rounded-lg transition-colors" href="booking-confirmation.html?code=${encodeURIComponent(booking.bookingCode)}">Chi tiết</a>
          ${booking.paymentStatus === 'Pending' ? `
            <button class="bg-action-blue hover:bg-primary-container text-white font-label-md text-label-md px-4 py-2 rounded-lg transition-colors" type="button" data-payos="${ui.escapeHtml(booking.bookingCode)}">payOS</button>
            <button class="bg-surface-container-high hover:bg-surface-container-highest text-on-surface font-label-md text-label-md px-4 py-2 rounded-lg transition-colors" type="button" data-pay-balance="${ui.escapeHtml(booking.bookingCode)}">So du</button>
          ` : ''}
          ${booking.status !== 'Cancelled' && booking.canCancel !== false ? `<button class="bg-error-container hover:bg-error text-error hover:text-white font-label-md text-label-md px-4 py-2 rounded-lg transition-colors" type="button" data-cancel="${ui.escapeHtml(booking.bookingCode)}">Hủy</button>` : ''}
        </div>
      </article>
    `).join('');

    list.querySelectorAll('[data-payos]').forEach((button) => {
      button.addEventListener('click', () => {
        const booking = bookings.find((item) => item.bookingCode === button.dataset.payos);
        if (!booking) {
          return;
        }

        startPayOsPayment(booking);
      });
    });

    list.querySelectorAll('[data-pay-balance]').forEach((button) => {
      button.addEventListener('click', () => {
        const booking = bookings.find((item) => item.bookingCode === button.dataset.payBalance);
        if (!booking || !confirmBalancePayment(booking)) {
          return;
        }

        updateBooking(() => api.payBooking(button.dataset.payBalance));
      });
    });

    list.querySelectorAll('[data-cancel]').forEach((button) => {
      button.addEventListener('click', () => {
        const bookingCode = button.dataset.cancel;
        const confirmed = window.confirm(`Bạn có chắc muốn hủy booking ${bookingCode}? Nếu booking đã thanh toán, hệ thống sẽ hoàn tiền theo quy định.`);
        if (!confirmed) {
          return;
        }

        updateBooking(() => api.cancelBooking(bookingCode));
      });
    });
  }

  async function updateBooking(action) {
    try {
      await action();
      api.setUser(await api.me());
      await loadMyBookings();
    } catch (error) {
      ui.renderMessage(list, error.message, 'error');
    }
  }

  function statusToneClass(value) {
    return {
      PendingPayment: 'bg-highlight-gold/20 text-on-surface',
      Pending: 'bg-highlight-gold/20 text-on-surface',
      Confirmed: 'bg-success-green/10 text-success-green',
      Paid: 'bg-success-green/10 text-success-green',
      Refunded: 'bg-success-green/10 text-success-green',
      RefundPending: 'bg-highlight-gold/20 text-on-surface',
      Cancelled: 'bg-error-container text-error'
    }[value] || 'bg-surface-container-high text-on-surface-variant';
  }

  async function startPayOsPayment(booking) {
    ui.renderMessage(list, 'Dang tao link thanh toan payOS...', 'muted');

    try {
      const payment = await api.createPayOsPayment(booking.bookingCode, {
        returnUrl: bookingUrl(booking.bookingCode, 'payos-return'),
        cancelUrl: bookingUrl(booking.bookingCode, 'payos-cancel')
      });
      window.location.href = payment.checkoutUrl;
    } catch (error) {
      ui.renderMessage(list, error.message, 'error');
    }
  }

  function bookingUrl(bookingCode, state) {
    const url = new URL('booking-confirmation.html', window.location.href);
    url.searchParams.set('bookingCode', bookingCode);
    url.searchParams.set('payment', state);
    return url.toString();
  }

  function confirmBalancePayment(booking) {
    return window.confirm(`Xac nhan thanh toan booking ${booking.bookingCode} bang so du voi so tien ${ui.formatCurrency(booking.totalPrice)}? So du tai khoan se bi tru sau khi xac nhan.`);
  }

  loadMyBookings();
})();

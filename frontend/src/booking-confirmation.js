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
      ui.renderMessage(target, 'Thiếu mã booking.', 'error');
      return;
    }

    ui.renderMessage(target, 'Đang tải booking...', 'muted');

    try {
      const booking = await api.getBookingByCode(params.code);
      renderBooking(booking);
    } catch (error) {
      ui.renderMessage(target, error.message, 'error');
    }
  }

  function renderBooking(booking) {
    target.className = 'bg-surface-container-lowest border border-outline-variant rounded-xl p-6 space-y-6';
    target.hidden = false;
    target.innerHTML = `
      <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-4">
        <div>
          <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-2">Mã booking</p>
          <h2 class="text-headline-lg font-headline-lg text-on-surface">${ui.escapeHtml(booking.bookingCode)}</h2>
          <p class="text-body-md text-on-surface-variant mt-2">${ui.escapeHtml(booking.hotelName)} - ${ui.escapeHtml(booking.roomTypeName)}</p>
        </div>
        <div class="text-left md:text-right">
          <p class="text-label-sm font-label-sm text-on-surface-variant">Tổng tiền</p>
          <strong class="text-headline-md font-headline-md text-primary">${ui.formatCurrency(booking.totalPrice)}</strong>
        </div>
      </div>
      <dl class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div class="bg-surface-container-low border border-outline-variant rounded-lg p-4"><dt class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Ngày ở</dt><dd class="mt-2 text-on-surface">${ui.formatDate(booking.checkIn)} - ${ui.formatDate(booking.checkOut)}</dd></div>
        <div class="bg-surface-container-low border border-outline-variant rounded-lg p-4"><dt class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Số đêm</dt><dd class="mt-2 text-on-surface">${booking.nights}</dd></div>
        <div class="bg-surface-container-low border border-outline-variant rounded-lg p-4"><dt class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Số khách</dt><dd class="mt-2 text-on-surface">${booking.guests}</dd></div>
        <div class="bg-surface-container-low border border-outline-variant rounded-lg p-4"><dt class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Booking</dt><dd class="mt-2"><span class="px-2 py-1 rounded-full text-label-sm font-label-sm ${statusToneClass(booking.status)}">${ui.statusLabel(booking.status)}</span></dd></div>
        <div class="bg-surface-container-low border border-outline-variant rounded-lg p-4"><dt class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Thanh toán</dt><dd class="mt-2"><span class="px-2 py-1 rounded-full text-label-sm font-label-sm ${statusToneClass(booking.paymentStatus)}">${ui.statusLabel(booking.paymentStatus)}</span></dd></div>
      </dl>
      <div class="flex flex-wrap gap-3">
        ${booking.paymentStatus === 'Pending' ? '<button class="bg-action-blue hover:bg-primary-container text-white font-label-md text-label-md px-6 py-3 rounded-lg transition-colors" type="button" data-pay>Thanh toán</button>' : ''}
        ${booking.status !== 'Cancelled' && booking.canCancel !== false ? '<button class="bg-surface-container-high hover:bg-error-container text-on-surface font-label-md text-label-md px-6 py-3 rounded-lg transition-colors" type="button" data-cancel>Hủy booking</button>' : ''}
        <a class="bg-surface-container-low hover:bg-surface-container-high text-on-surface font-label-md text-label-md px-6 py-3 rounded-lg transition-colors" href="booking-lookup.html">Booking của tôi</a>
      </div>
      ${booking.canReview ? `
        <form id="reviewForm" class="bg-surface-container-low border border-outline-variant rounded-xl p-5 space-y-4">
          <div>
            <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-1">Đánh giá khách sạn</p>
            <h3 class="text-headline-md font-headline-md text-on-surface">Chia sẻ trải nghiệm của bạn</h3>
          </div>
          <label class="block">
            <span class="text-label-md font-label-md text-on-surface-variant block mb-1">Số sao</span>
            <select id="reviewRating" class="w-full px-4 py-3 border border-outline-variant rounded-lg bg-surface-container-lowest focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all">
              <option value="5">5 sao</option>
              <option value="4">4 sao</option>
              <option value="3">3 sao</option>
              <option value="2">2 sao</option>
              <option value="1">1 sao</option>
            </select>
          </label>
          <label class="block">
            <span class="text-label-md font-label-md text-on-surface-variant block mb-1">Nhận xét</span>
            <textarea id="reviewComment" rows="4" class="w-full px-4 py-3 border border-outline-variant rounded-lg bg-surface-container-lowest focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all" placeholder="Phòng sạch, vị trí thuận tiện..."></textarea>
          </label>
          <button class="bg-action-blue hover:bg-primary-container text-white font-label-md text-label-md px-6 py-3 rounded-lg transition-colors" type="submit">Gửi đánh giá</button>
          <div id="reviewMessage" hidden></div>
        </form>
      ` : booking.hasReview ? '<div class="bg-success-green/10 text-success-green border border-success-green/30 rounded-xl p-4">Bạn đã đánh giá booking này.</div>' : ''}
    `;

    const payButton = target.querySelector('[data-pay]');
    if (payButton) {
      payButton.addEventListener('click', () => updateBooking(() => api.payBooking(booking.bookingCode)));
    }

    const cancelButton = target.querySelector('[data-cancel]');
    if (cancelButton) {
      cancelButton.addEventListener('click', () => {
        const confirmed = window.confirm(`Bạn có chắc muốn hủy booking ${booking.bookingCode}? Nếu booking đã thanh toán, hệ thống sẽ hoàn tiền theo quy định.`);
        if (!confirmed) {
          return;
        }

        updateBooking(() => api.cancelBooking(booking.bookingCode));
      });
    }

    const reviewForm = target.querySelector('#reviewForm');
    if (reviewForm) {
      reviewForm.addEventListener('submit', async (event) => {
        event.preventDefault();
        const reviewMessage = target.querySelector('#reviewMessage');
        ui.renderMessage(reviewMessage, 'Đang gửi đánh giá...', 'muted');

        try {
          await api.createHotelReview(booking.hotelId, {
            bookingCode: booking.bookingCode,
            rating: Number(target.querySelector('#reviewRating').value),
            comment: target.querySelector('#reviewComment').value.trim()
          });
          renderBooking(await api.getBookingByCode(booking.bookingCode));
        } catch (error) {
          ui.renderMessage(reviewMessage, error.message, 'error');
        }
      });
    }
  }

  function statusToneClass(value) {
    return {
      PendingPayment: 'bg-highlight-gold/20 text-on-surface',
      Pending: 'bg-highlight-gold/20 text-on-surface',
      Confirmed: 'bg-success-green/10 text-success-green',
      Paid: 'bg-success-green/10 text-success-green',
      Refunded: 'bg-success-green/10 text-success-green',
      Cancelled: 'bg-error-container text-error'
    }[value] || 'bg-surface-container-high text-on-surface-variant';
  }

  async function updateBooking(action) {
    ui.renderMessage(target, 'Đang cập nhật booking...', 'muted');
    try {
      const booking = await action();
      api.setUser(await api.me());
      renderBooking(booking);
    } catch (error) {
      ui.renderMessage(target, error.message, 'error');
    }
  }

  loadBooking();
})();

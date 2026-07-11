(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('booking');

  const params = ui.readSearchParams();
  const summary = document.getElementById('bookingSummary');
  const form = document.getElementById('bookingForm');
  const message = document.getElementById('bookingMessage');
  let hotel = null;
  let room = null;

  document.getElementById('guestName').value = session.user.fullName;
  document.getElementById('guestEmail').value = session.user.email;

  async function loadBookingContext() {
    if (!params.hotelId || !params.roomTypeId) {
      ui.renderMessage(summary, 'Thiếu thông tin phòng. Vui lòng chọn phòng từ trang chi tiết khách sạn.', 'error');
      form.hidden = true;
      return;
    }

    ui.renderMessage(summary, 'Đang tải thông tin phòng...', 'muted');

    try {
      const [hotelData, rooms] = await Promise.all([
        api.getHotel(params.hotelId),
        api.getRooms(params.hotelId)
      ]);
      hotel = hotelData;
      room = rooms.find((item) => item.roomTypeId === Number(params.roomTypeId));

      if (!room) {
        throw new Error('Room type was not found.');
      }

      renderSummary();
      form.hidden = false;
    } catch (error) {
      ui.renderMessage(summary, error.message, 'error');
      form.hidden = true;
    }
  }

  function nights() {
    const diff = new Date(`${params.checkOut}T00:00:00`) - new Date(`${params.checkIn}T00:00:00`);
    return Math.max(1, Math.round(diff / 86400000));
  }

  function renderSummary() {
    const total = nights() * room.pricePerNight;
    summary.className = 'bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden';
    summary.hidden = false;
    summary.innerHTML = `
      <div class="grid grid-cols-1 md:grid-cols-3">
        <img src="${ui.roomImage(room, hotel.hotelId)}" alt="${ui.escapeHtml(room.name)}" loading="lazy" class="w-full h-full min-h-[240px] object-cover">
        <div class="md:col-span-2 p-6 space-y-5">
          <div>
            <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-2">${ui.escapeHtml(hotel.city)} · ${hotel.starRating} sao</p>
            <h2 class="text-headline-lg font-headline-lg text-on-surface">${ui.escapeHtml(hotel.name)}</h2>
            <p class="text-body-md text-on-surface-variant mt-2">${ui.escapeHtml(room.name)} · ${params.guests} khách</p>
          </div>
          <dl class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div class="bg-surface-container-low border border-outline-variant rounded-lg p-4">
              <dt class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Nhận phòng</dt>
              <dd class="mt-1 text-on-surface">${ui.formatDate(params.checkIn)}</dd>
            </div>
            <div class="bg-surface-container-low border border-outline-variant rounded-lg p-4">
              <dt class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Trả phòng</dt>
              <dd class="mt-1 text-on-surface">${ui.formatDate(params.checkOut)}</dd>
            </div>
            <div class="bg-surface-container-low border border-outline-variant rounded-lg p-4">
              <dt class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Số đêm</dt>
              <dd class="mt-1 text-on-surface">${nights()}</dd>
            </div>
          </dl>
          <div class="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3 border-t border-outline-variant pt-5">
            <div>
              <p class="text-label-sm font-label-sm text-on-surface-variant">Tạm tính</p>
              <strong class="text-headline-md font-headline-md text-primary">${ui.formatCurrency(total)}</strong>
            </div>
            <p class="text-body-sm text-on-surface-variant">Còn ${room.availableRooms ?? room.totalRooms} phòng đang mở bán.</p>
          </div>
        </div>
      </div>
    `;
  }

  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    ui.renderMessage(message, 'Đang giữ phòng...', 'muted');

    try {
      const booking = await api.createBooking({
        roomTypeId: room.roomTypeId,
        guestName: document.getElementById('guestName').value,
        guestEmail: document.getElementById('guestEmail').value,
        checkIn: params.checkIn,
        checkOut: params.checkOut,
        guests: Number(params.guests)
      });

      window.location.href = `booking-confirmation.html?code=${encodeURIComponent(booking.bookingCode)}`;
    } catch (error) {
      ui.renderMessage(message, error.message, 'error');
    }
  });

  loadBookingContext();
})();

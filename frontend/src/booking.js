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
      ui.renderMessage(summary, 'Thieu thong tin phong. Vui long chon phong tu trang chi tiet khach san.', 'error');
      form.hidden = true;
      return;
    }

    ui.renderMessage(summary, 'Dang tai thong tin phong...', 'muted');

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
    summary.className = 'summary-card';
    summary.hidden = false;
    summary.innerHTML = `
      <img src="${ui.roomImage(room, hotel.hotelId)}" alt="${ui.escapeHtml(room.name)}" loading="lazy">
      <div>
        <p class="eyebrow">${ui.escapeHtml(hotel.city)} · ${hotel.starRating} sao</p>
        <h2>${ui.escapeHtml(hotel.name)}</h2>
        <p>${ui.escapeHtml(room.name)} · ${params.guests} khach</p>
        <p>${ui.formatDate(params.checkIn)} - ${ui.formatDate(params.checkOut)} · ${nights()} dem</p>
        <strong>${ui.formatCurrency(total)}</strong>
      </div>
    `;
  }

  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    ui.renderMessage(message, 'Dang giu phong...', 'muted');

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

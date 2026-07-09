(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('search');

  const params = ui.readSearchParams();
  const hero = document.getElementById('hotelHero');
  const roomsList = document.getElementById('roomsList');
  const bookingCta = document.getElementById('bookingCta');
  let hotel = null;
  let rooms = [];
  let selectedRoomTypeId = Number(params.roomTypeId);

  async function loadDetail() {
    if (!params.hotelId) {
      ui.renderMessage(hero, 'Thieu hotelId. Vui long quay lai trang tim kiem.', 'error');
      return;
    }

    ui.renderMessage(hero, 'Dang tai khach san...', 'muted');
    ui.renderMessage(roomsList, 'Dang tai danh sach phong...', 'muted');

    try {
      [hotel, rooms] = await Promise.all([
        api.getHotel(params.hotelId),
        api.getRooms(params.hotelId)
      ]);
      renderHotel();
      renderRooms();
      renderCta();
    } catch (error) {
      ui.renderMessage(hero, error.message, 'error');
      roomsList.innerHTML = '';
    }
  }

  function renderHotel() {
    hero.className = 'detail-hero';
    hero.hidden = false;
    hero.innerHTML = `
      <img src="${ui.hotelImage(hotel.hotelId, hotel.mainImageUrl)}" alt="${ui.escapeHtml(hotel.name)}" loading="lazy">
      <div class="detail-copy">
        <p class="eyebrow">${ui.escapeHtml(hotel.city)} · ${hotel.starRating} sao</p>
        <h1>${ui.escapeHtml(hotel.name)}</h1>
        <p>${ui.escapeHtml(hotel.address)}</p>
        <p>${ui.escapeHtml(hotel.description)}</p>
      </div>
    `;
  }

  function renderRooms() {
    roomsList.className = 'room-grid';
    roomsList.hidden = false;
    roomsList.innerHTML = rooms.map((room) => `
      <button class="room-card ${room.roomTypeId === selectedRoomTypeId ? 'active' : ''}" type="button" data-room="${room.roomTypeId}">
        <span>
          <strong>${ui.escapeHtml(room.name)}</strong>
          <small>${room.maxGuests} khach · ${room.totalRooms} phong</small>
          <small>${ui.escapeHtml(room.description)}</small>
        </span>
        <span>${ui.formatCurrency(room.pricePerNight)}</span>
      </button>
    `).join('');

    roomsList.querySelectorAll('[data-room]').forEach((button) => {
      button.addEventListener('click', () => {
        selectedRoomTypeId = Number(button.dataset.room);
        const query = ui.toQuery({ ...params, roomTypeId: selectedRoomTypeId });
        window.history.replaceState({}, '', `hotel-detail.html?${query}`);
        renderRooms();
        renderCta();
      });
    });
  }

  function renderCta() {
    const selected = rooms.find((room) => room.roomTypeId === selectedRoomTypeId) || rooms[0];
    if (!selected) {
      ui.renderMessage(bookingCta, 'Khach san nay chua co phong.', 'muted');
      return;
    }

    selectedRoomTypeId = selected.roomTypeId;
    const query = ui.toQuery({
      city: params.city,
      checkIn: params.checkIn,
      checkOut: params.checkOut,
      guests: params.guests,
      hotelId: hotel.hotelId,
      roomTypeId: selected.roomTypeId
    });

    bookingCta.className = 'booking-callout';
    bookingCta.hidden = false;
    bookingCta.innerHTML = `
      <p class="eyebrow">Dat phong</p>
      <h2>${ui.escapeHtml(selected.name)}</h2>
      <p>${ui.formatCurrency(selected.pricePerNight)} / dem · toi da ${selected.maxGuests} khach</p>
      <a class="primary-button as-link" href="booking.html?${query}">Tiep tuc dat phong</a>
    `;
  }

  loadDetail();
})();

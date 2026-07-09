(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth({ role: 'Admin' });
  if (!session) {
    return;
  }

  ui.hydrateNav('admin');

  const hotelForm = document.getElementById('hotelForm');
  const roomForm = document.getElementById('roomForm');
  const hotelMessage = document.getElementById('hotelMessage');
  const roomMessage = document.getElementById('roomMessage');
  const hotelsList = document.getElementById('adminHotelsList');
  const roomsList = document.getElementById('adminRoomsList');
  const bookingsList = document.getElementById('adminBookingsList');
  const hotelSelect = document.getElementById('roomHotelId');
  const resetHotel = document.getElementById('resetHotelForm');
  const resetRoom = document.getElementById('resetRoomForm');

  let hotels = [];
  let rooms = [];

  hotelForm.addEventListener('submit', saveHotel);
  roomForm.addEventListener('submit', saveRoom);
  resetHotel.addEventListener('click', clearHotelForm);
  resetRoom.addEventListener('click', clearRoomForm);

  async function loadAdmin() {
    await Promise.all([
      loadHotelsAndRooms(),
      loadBookings()
    ]);
  }

  async function loadHotelsAndRooms() {
    ui.renderMessage(hotelsList, 'Dang tai khach san...', 'muted');
    ui.renderMessage(roomsList, 'Dang tai loai phong...', 'muted');

    try {
      [hotels, rooms] = await Promise.all([
        api.getAdminHotels(),
        api.getAdminRoomTypes()
      ]);
      renderHotels();
      renderHotelOptions();
      renderRooms();
    } catch (error) {
      ui.renderMessage(hotelsList, error.message, 'error');
      ui.renderMessage(roomsList, error.message, 'error');
    }
  }

  async function loadBookings() {
    ui.renderMessage(bookingsList, 'Dang tai booking...', 'muted');

    try {
      const bookings = await api.getAdminBookings();
      renderBookings(bookings);
    } catch (error) {
      ui.renderMessage(bookingsList, error.message, 'error');
    }
  }

  function renderHotels() {
    if (!hotels.length) {
      ui.renderMessage(hotelsList, 'Chua co khach san.', 'muted');
      return;
    }

    hotelsList.className = 'admin-list';
    hotelsList.hidden = false;
    hotelsList.innerHTML = hotels.map((hotel) => `
      <article class="admin-row">
        <img src="${ui.hotelImage(hotel.hotelId, hotel.mainImageUrl)}" alt="${ui.escapeHtml(hotel.name)}" loading="lazy">
        <div>
          <p class="eyebrow">${ui.escapeHtml(hotel.city)} · ${hotel.starRating} sao</p>
          <h3>${ui.escapeHtml(hotel.name)}</h3>
          <p>${ui.escapeHtml(hotel.address)}</p>
          <p>${hotel.roomTypeCount} loai phong</p>
        </div>
        <button class="ghost-button small" type="button" data-edit-hotel="${hotel.hotelId}">Sua</button>
      </article>
    `).join('');

    hotelsList.querySelectorAll('[data-edit-hotel]').forEach((button) => {
      button.addEventListener('click', () => fillHotelForm(Number(button.dataset.editHotel)));
    });
  }

  function renderHotelOptions() {
    hotelSelect.innerHTML = hotels.map((hotel) => `
      <option value="${hotel.hotelId}">${ui.escapeHtml(hotel.name)}</option>
    `).join('');
  }

  function renderRooms() {
    if (!rooms.length) {
      ui.renderMessage(roomsList, 'Chua co loai phong.', 'muted');
      return;
    }

    roomsList.className = 'admin-list compact-list';
    roomsList.hidden = false;
    roomsList.innerHTML = rooms.map((room) => {
      const hotel = hotels.find((item) => item.hotelId === room.hotelId);
      return `
        <article class="admin-row">
          <div>
            <p class="eyebrow">${ui.escapeHtml(hotel ? hotel.name : `Hotel ${room.hotelId}`)}</p>
            <h3>${ui.escapeHtml(room.name)}</h3>
            <p>${room.maxGuests} khach · ${room.totalRooms} phong · ${ui.formatCurrency(room.pricePerNight)}</p>
          </div>
          <button class="ghost-button small" type="button" data-edit-room="${room.roomTypeId}">Sua</button>
        </article>
      `;
    }).join('');

    roomsList.querySelectorAll('[data-edit-room]').forEach((button) => {
      button.addEventListener('click', () => fillRoomForm(Number(button.dataset.editRoom)));
    });
  }

  function renderBookings(bookings) {
    if (!bookings.length) {
      ui.renderMessage(bookingsList, 'Chua co booking.', 'muted');
      return;
    }

    bookingsList.className = 'booking-list';
    bookingsList.hidden = false;
    bookingsList.innerHTML = bookings.map((booking) => `
      <article class="booking-row">
        <div>
          <p class="eyebrow">${ui.escapeHtml(booking.bookingCode)}</p>
          <h3>${ui.escapeHtml(booking.hotelName)}</h3>
          <p>${ui.escapeHtml(booking.roomTypeName)} · ${ui.escapeHtml(booking.guestName)} · ${ui.escapeHtml(booking.guestEmail)}</p>
          <p>${ui.formatDate(booking.checkIn)} - ${ui.formatDate(booking.checkOut)} · ${ui.formatCurrency(booking.totalPrice)}</p>
          <p><span class="pill ${ui.statusClass(booking.status)}">${ui.statusLabel(booking.status)}</span> <span class="pill ${ui.statusClass(booking.paymentStatus)}">${ui.statusLabel(booking.paymentStatus)}</span></p>
        </div>
      </article>
    `).join('');
  }

  async function saveHotel(event) {
    event.preventDefault();
    ui.renderMessage(hotelMessage, 'Dang luu khach san...', 'muted');
    const hotelId = document.getElementById('hotelId').value;
    const payload = {
      name: document.getElementById('hotelName').value,
      city: document.getElementById('hotelCity').value,
      address: document.getElementById('hotelAddress').value,
      description: document.getElementById('hotelDescription').value,
      starRating: Number(document.getElementById('hotelStarRating').value),
      mainImageUrl: document.getElementById('hotelImageUrl').value
    };

    try {
      if (hotelId) {
        await api.updateAdminHotel(hotelId, payload);
      } else {
        await api.createAdminHotel(payload);
      }
      clearHotelForm();
      ui.renderMessage(hotelMessage, 'Da luu khach san.', 'success');
      await loadHotelsAndRooms();
    } catch (error) {
      ui.renderMessage(hotelMessage, error.message, 'error');
    }
  }

  async function saveRoom(event) {
    event.preventDefault();
    ui.renderMessage(roomMessage, 'Dang luu loai phong...', 'muted');
    const roomTypeId = document.getElementById('roomTypeId').value;
    const payload = {
      hotelId: Number(hotelSelect.value),
      name: document.getElementById('roomName').value,
      description: document.getElementById('roomDescription').value,
      maxGuests: Number(document.getElementById('roomMaxGuests').value),
      pricePerNight: Number(document.getElementById('roomPrice').value),
      totalRooms: Number(document.getElementById('roomTotalRooms').value),
      imageUrl: document.getElementById('roomImageUrl').value
    };

    try {
      if (roomTypeId) {
        await api.updateAdminRoomType(roomTypeId, payload);
      } else {
        await api.createAdminRoomType(payload);
      }
      clearRoomForm();
      ui.renderMessage(roomMessage, 'Da luu loai phong.', 'success');
      await loadHotelsAndRooms();
    } catch (error) {
      ui.renderMessage(roomMessage, error.message, 'error');
    }
  }

  function fillHotelForm(hotelId) {
    const hotel = hotels.find((item) => item.hotelId === hotelId);
    if (!hotel) {
      return;
    }

    document.getElementById('hotelId').value = hotel.hotelId;
    document.getElementById('hotelName').value = hotel.name;
    document.getElementById('hotelCity').value = hotel.city;
    document.getElementById('hotelAddress').value = hotel.address;
    document.getElementById('hotelDescription').value = hotel.description;
    document.getElementById('hotelStarRating').value = hotel.starRating;
    document.getElementById('hotelImageUrl').value = hotel.mainImageUrl;
    document.getElementById('hotelSubmit').textContent = 'Cap nhat khach san';
    hotelForm.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  function fillRoomForm(roomTypeId) {
    const room = rooms.find((item) => item.roomTypeId === roomTypeId);
    if (!room) {
      return;
    }

    document.getElementById('roomTypeId').value = room.roomTypeId;
    hotelSelect.value = room.hotelId;
    document.getElementById('roomName').value = room.name;
    document.getElementById('roomDescription').value = room.description;
    document.getElementById('roomMaxGuests').value = room.maxGuests;
    document.getElementById('roomPrice').value = room.pricePerNight;
    document.getElementById('roomTotalRooms').value = room.totalRooms;
    document.getElementById('roomImageUrl').value = room.imageUrl;
    document.getElementById('roomSubmit').textContent = 'Cap nhat loai phong';
    roomForm.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  function clearHotelForm() {
    hotelForm.reset();
    document.getElementById('hotelId').value = '';
    document.getElementById('hotelStarRating').value = '4';
    document.getElementById('hotelSubmit').textContent = 'Tao khach san';
  }

  function clearRoomForm() {
    roomForm.reset();
    document.getElementById('roomTypeId').value = '';
    document.getElementById('roomMaxGuests').value = '2';
    document.getElementById('roomTotalRooms').value = '3';
    document.getElementById('roomSubmit').textContent = 'Tao loai phong';
    if (hotels[0]) {
      hotelSelect.value = hotels[0].hotelId;
    }
  }

  loadAdmin();
})();

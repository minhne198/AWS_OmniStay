(function () {
  const api = window.OmniStayApi;
  const state = {
    search: null,
    selectedHotel: null,
    selectedRoom: null
  };

  const imageByHotel = {
    1: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1100&q=80',
    2: 'https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=1100&q=80',
    3: 'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1100&q=80',
    4: 'https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=1100&q=80',
    5: 'https://images.unsplash.com/photo-1519681393784-d120267933ba?auto=format&fit=crop&w=1100&q=80',
    6: 'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1100&q=80',
    7: 'https://images.unsplash.com/photo-1528127269322-539801943592?auto=format&fit=crop&w=1100&q=80',
    8: 'https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&w=1100&q=80',
    9: 'https://images.unsplash.com/photo-1500375592092-40eb2168fd21?auto=format&fit=crop&w=1100&q=80',
    10: 'https://images.unsplash.com/photo-1539650116574-75c0c6d73f6e?auto=format&fit=crop&w=1100&q=80'
  };

  const elements = {
    apiStatus: document.getElementById('apiStatus'),
    searchForm: document.getElementById('searchForm'),
    cityInput: document.getElementById('cityInput'),
    checkInInput: document.getElementById('checkInInput'),
    checkOutInput: document.getElementById('checkOutInput'),
    guestsInput: document.getElementById('guestsInput'),
    resultCount: document.getElementById('resultCount'),
    selectedCity: document.getElementById('selectedCity'),
    resultsList: document.getElementById('resultsList'),
    hotelDetail: document.getElementById('hotelDetail'),
    roomsList: document.getElementById('roomsList'),
    bookingForm: document.getElementById('bookingForm'),
    bookingTitle: document.getElementById('bookingTitle'),
    guestNameInput: document.getElementById('guestNameInput'),
    guestEmailInput: document.getElementById('guestEmailInput'),
    bookingResult: document.getElementById('bookingResult')
  };

  function init() {
    setDefaultDates();
    elements.apiStatus.textContent = api.apiBase;
    elements.searchForm.addEventListener('submit', handleSearch);
    elements.bookingForm.addEventListener('submit', handleBooking);
    runSearch();
  }

  function setDefaultDates() {
    const today = new Date();
    const checkIn = new Date(today);
    checkIn.setDate(today.getDate() + 14);
    const checkOut = new Date(checkIn);
    checkOut.setDate(checkIn.getDate() + 3);

    elements.checkInInput.value = toDateInput(checkIn);
    elements.checkOutInput.value = toDateInput(checkOut);
  }

  function toDateInput(date) {
    return date.toISOString().slice(0, 10);
  }

  function handleSearch(event) {
    event.preventDefault();
    runSearch();
  }

  async function runSearch() {
    const search = {
      city: elements.cityInput.value,
      checkIn: elements.checkInInput.value,
      checkOut: elements.checkOutInput.value,
      guests: elements.guestsInput.value
    };

    state.search = search;
    state.selectedHotel = null;
    state.selectedRoom = null;
    elements.selectedCity.textContent = search.city;
    elements.bookingForm.hidden = true;
    elements.bookingResult.hidden = true;
    renderLoading(elements.resultsList, 'Đang tải kết quả...');
    renderEmptyDetail();

    try {
      const results = await api.searchHotels(search);
      renderSearchResults(results);
      if (results.length > 0) {
        await selectSearchResult(results[0]);
      }
    } catch (error) {
      elements.resultCount.textContent = '0';
      renderError(elements.resultsList, error);
    }
  }

  function renderSearchResults(results) {
    elements.resultCount.textContent = String(results.length);
    elements.resultsList.classList.remove('empty-state', 'error-state');
    elements.resultsList.replaceChildren();

    if (results.length === 0) {
      renderEmpty(elements.resultsList, 'Không có phòng phù hợp.');
      return;
    }

    for (const result of results) {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'result-card';
      button.dataset.roomTypeId = String(result.roomTypeId);
      button.addEventListener('click', () => selectSearchResult(result));
      button.innerHTML = `
        <img src="${hotelImage(result.hotelId)}" alt="${escapeHtml(result.hotelName)}" loading="lazy">
        <span class="result-content">
          <strong>${escapeHtml(result.hotelName)}</strong>
          <span>${escapeHtml(result.city)} · ${escapeHtml(result.roomTypeName)}</span>
          <span>${formatCurrency(result.pricePerNight)} / đêm · còn ${result.availableRooms} phòng</span>
        </span>
      `;
      elements.resultsList.append(button);
    }
  }

  async function selectSearchResult(result) {
    elements.bookingResult.hidden = true;
    setActiveResult(result.roomTypeId);
    renderLoading(elements.roomsList, 'Đang tải phòng...');
    renderLoading(elements.hotelDetail, 'Đang tải khách sạn...');

    try {
      const [hotel, rooms] = await Promise.all([
        api.getHotel(result.hotelId),
        api.getRooms(result.hotelId)
      ]);
      const selectedRoom = rooms.find(room => room.roomTypeId === result.roomTypeId) || rooms[0] || null;
      state.selectedHotel = hotel;
      state.selectedRoom = selectedRoom;
      renderHotelDetail(hotel);
      renderRooms(rooms, selectedRoom ? selectedRoom.roomTypeId : null);
      renderBookingForm();
    } catch (error) {
      renderError(elements.hotelDetail, error);
      elements.roomsList.replaceChildren();
      elements.bookingForm.hidden = true;
    }
  }

  function setActiveResult(roomTypeId) {
    for (const card of elements.resultsList.querySelectorAll('.result-card')) {
      card.classList.toggle('active', card.dataset.roomTypeId === String(roomTypeId));
    }
  }

  function renderHotelDetail(hotel) {
    elements.hotelDetail.className = 'hotel-detail';
    elements.hotelDetail.innerHTML = `
      <img src="${hotelImage(hotel.hotelId)}" alt="${escapeHtml(hotel.name)}" loading="lazy">
      <div class="hotel-copy">
        <p class="eyebrow">${escapeHtml(hotel.city)} · ${hotel.starRating} sao</p>
        <h2>${escapeHtml(hotel.name)}</h2>
        <p>${escapeHtml(hotel.address)}</p>
        <p>${escapeHtml(hotel.description)}</p>
      </div>
    `;
  }

  function renderRooms(rooms, selectedRoomTypeId) {
    elements.roomsList.classList.remove('empty-state', 'error-state');
    elements.roomsList.replaceChildren();

    for (const room of rooms) {
      const item = document.createElement('button');
      item.type = 'button';
      item.className = `room-card${room.roomTypeId === selectedRoomTypeId ? ' active' : ''}`;
      item.addEventListener('click', () => {
        state.selectedRoom = room;
        renderRooms(rooms, room.roomTypeId);
        renderBookingForm();
        elements.bookingResult.hidden = true;
      });
      item.innerHTML = `
        <span>
          <strong>${escapeHtml(room.name)}</strong>
          <small>${room.maxGuests} khách · ${room.totalRooms} phòng</small>
        </span>
        <span>${formatCurrency(room.pricePerNight)}</span>
      `;
      elements.roomsList.append(item);
    }
  }

  function renderBookingForm() {
    if (!state.selectedHotel || !state.selectedRoom) {
      elements.bookingForm.hidden = true;
      return;
    }

    elements.bookingForm.hidden = false;
    elements.bookingTitle.textContent = state.selectedRoom.name;
  }

  async function handleBooking(event) {
    event.preventDefault();

    if (!state.selectedRoom || !state.search) {
      return;
    }

    elements.bookingResult.hidden = false;
    elements.bookingResult.className = 'booking-result';
    elements.bookingResult.textContent = 'Đang tạo đặt phòng...';

    try {
      const booking = await api.createBooking({
        roomTypeId: state.selectedRoom.roomTypeId,
        guestName: elements.guestNameInput.value,
        guestEmail: elements.guestEmailInput.value,
        checkIn: state.search.checkIn,
        checkOut: state.search.checkOut,
        guests: Number(state.search.guests)
      });

      elements.bookingResult.innerHTML = `
        <strong>${escapeHtml(booking.bookingCode)}</strong>
        <span>${escapeHtml(booking.hotelName)} · ${escapeHtml(booking.roomTypeName)}</span>
        <span>${booking.nights} đêm · ${formatCurrency(booking.totalPrice)}</span>
      `;
    } catch (error) {
      elements.bookingResult.className = 'booking-result error';
      elements.bookingResult.textContent = error.message;
    }
  }

  function renderEmptyDetail() {
    elements.hotelDetail.className = 'hotel-detail empty-state';
    elements.hotelDetail.textContent = 'Chọn một phòng để xem chi tiết.';
    elements.roomsList.replaceChildren();
  }

  function renderLoading(target, text) {
    target.classList.remove('error-state');
    target.classList.add('empty-state');
    target.textContent = text;
  }

  function renderEmpty(target, text) {
    target.classList.remove('error-state');
    target.classList.add('empty-state');
    target.textContent = text;
  }

  function renderError(target, error) {
    target.classList.remove('empty-state');
    target.classList.add('error-state');
    target.textContent = error.message || 'Không thể tải dữ liệu.';
  }

  function hotelImage(hotelId) {
    return imageByHotel[hotelId] || imageByHotel[1];
  }

  function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0
    }).format(value);
  }

  function escapeHtml(value) {
    return String(value)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  init();
})();

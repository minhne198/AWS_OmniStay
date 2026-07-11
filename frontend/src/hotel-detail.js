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
  const reviewsList = document.getElementById('reviewsList');
  let hotel = null;
  let rooms = [];
  let reviews = [];
  let selectedRoomTypeId = Number(params.roomTypeId);

  async function loadDetail() {
    if (!params.hotelId) {
      ui.renderMessage(hero, 'Thiếu hotelId. Vui lòng quay lại trang tìm kiếm.', 'error');
      return;
    }

    ui.renderMessage(hero, 'Đang tải khách sạn...', 'muted');
    ui.renderMessage(roomsList, 'Đang tải danh sách phòng...', 'muted');

    try {
      [hotel, rooms, reviews] = await Promise.all([
        api.getHotel(params.hotelId),
        api.getRooms(params.hotelId),
        api.getHotelReviews(params.hotelId)
      ]);
      renderHotel();
      renderRooms();
      renderCta();
      renderReviews();
    } catch (error) {
      ui.renderMessage(hero, error.message, 'error');
      roomsList.innerHTML = '';
    }
  }

  function renderHotel() {
    hero.className = 'bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden';
    hero.hidden = false;
    hero.innerHTML = `
      <div class="grid grid-cols-1 lg:grid-cols-5">
        <img src="${ui.hotelImage(hotel.hotelId, hotel.mainImageUrl)}" alt="${ui.escapeHtml(hotel.name)}" loading="lazy" class="lg:col-span-2 w-full h-full min-h-[280px] object-cover">
        <div class="lg:col-span-3 p-6 md:p-8">
          <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-2">${ui.escapeHtml(hotel.city)} · ${hotel.starRating} sao</p>
          <h1 class="text-headline-lg font-headline-lg text-on-surface">${ui.escapeHtml(hotel.name)}</h1>
          <p class="inline-flex mt-3 px-3 py-1 rounded-full bg-highlight-gold/20 text-on-surface text-label-md font-label-md">★ ${(hotel.averageRating || 0).toFixed(1)} (${hotel.reviewCount || 0} đánh giá)</p>
          <p class="text-body-md text-on-surface-variant mt-3">${ui.escapeHtml(hotel.address)}</p>
          <p class="text-body-md text-on-surface-variant mt-4">${ui.escapeHtml(hotel.description)}</p>
        </div>
      </div>
    `;
  }

  function renderRooms() {
    if (rooms.length === 0) {
      ui.renderMessage(roomsList, 'Khách sạn này chưa có phòng đang mở bán.', 'muted');
      return;
    }

    roomsList.className = 'space-y-4';
    roomsList.hidden = false;
    roomsList.innerHTML = rooms.map((room) => `
      <button class="w-full text-left bg-surface-container-low border ${room.roomTypeId === selectedRoomTypeId ? 'border-primary ring-2 ring-primary/20' : 'border-outline-variant'} rounded-xl p-5 hover:border-primary transition-all" type="button" data-room="${room.roomTypeId}">
        <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-4">
          <div class="min-w-0">
            <h3 class="text-headline-md font-headline-md text-on-surface">${ui.escapeHtml(room.name)}</h3>
            <p class="text-body-md text-on-surface-variant mt-1">${room.maxGuests} khách · ${room.totalRooms} phòng · còn ${room.availableRooms ?? room.totalRooms}</p>
            <p class="text-body-sm text-on-surface-variant mt-2">${ui.escapeHtml(room.description)}</p>
          </div>
          <div class="md:text-right shrink-0">
            <strong class="text-headline-md font-headline-md text-primary">${ui.formatCurrency(room.pricePerNight)}</strong>
            <p class="text-body-sm text-on-surface-variant">/ đêm</p>
          </div>
        </div>
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
      ui.renderMessage(bookingCta, 'Khách sạn này chưa có phòng.', 'muted');
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

    bookingCta.className = 'bg-surface-container-lowest border border-outline-variant rounded-xl p-6 h-fit lg:sticky lg:top-24';
    bookingCta.hidden = false;
    bookingCta.innerHTML = `
      <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-2">Đặt phòng</p>
      <h2 class="text-headline-md font-headline-md text-on-surface">${ui.escapeHtml(selected.name)}</h2>
      <p class="text-body-md text-on-surface-variant mt-2">${ui.formatCurrency(selected.pricePerNight)} / đêm · tối đa ${selected.maxGuests} khách</p>
      <p class="text-body-sm text-on-surface-variant mt-1">Còn ${selected.availableRooms ?? selected.totalRooms} phòng đang mở bán.</p>
      <a class="mt-5 inline-flex w-full justify-center bg-action-blue hover:bg-primary-container text-white font-label-md text-label-md px-6 py-3 rounded-lg transition-colors" href="booking.html?${query}">Tiếp tục đặt phòng</a>
    `;
  }

  function renderReviews() {
    if (!reviewsList) {
      return;
    }

    if (reviews.length === 0) {
      ui.renderMessage(reviewsList, 'Khách sạn này chưa có đánh giá.', 'muted');
      return;
    }

    reviewsList.className = 'space-y-4';
    reviewsList.hidden = false;
    reviewsList.innerHTML = reviews.map((review) => `
      <article class="bg-surface-container-low border border-outline-variant rounded-xl p-4">
        <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-3">
          <div>
            <p class="text-label-md font-label-md text-on-surface">${ui.escapeHtml(review.reviewerName)}</p>
            <p class="text-label-sm font-label-sm text-on-surface-variant mt-1">Booking ${ui.escapeHtml(review.bookingCode)} · ${ui.formatDate(review.createdAt)}</p>
          </div>
          <span class="px-3 py-1 rounded-full bg-highlight-gold/20 text-on-surface text-label-md font-label-md">★ ${review.rating}/5</span>
        </div>
        ${review.comment ? `<p class="text-body-md text-on-surface-variant mt-3">${ui.escapeHtml(review.comment)}</p>` : ''}
      </article>
    `).join('');
  }

  loadDetail();
})();

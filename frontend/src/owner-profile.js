(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth({ role: 'HotelOwner' });
  if (!session) {
    return;
  }

  ui.hydrateNav('admin');

  const summary = document.getElementById('ownerSummary');
  const hotelsList = document.getElementById('ownerHotelsList');

  async function loadProfile() {
    ui.renderMessage(summary, 'Đang tải hồ sơ chủ khách sạn...', 'muted');
    try {
      const profile = await api.getOwnerProfile();
      renderSummary(profile);
      renderHotels(profile.hotels || []);
    } catch (error) {
      ui.renderMessage(summary, error.message, 'error');
    }
  }

  function renderSummary(profile) {
    summary.className = 'bg-surface-container-lowest border border-outline-variant rounded-xl p-6';
    summary.hidden = false;
    summary.innerHTML = `
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
        <div>
          <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-2">Chủ khách sạn</p>
          <h1 class="text-headline-lg font-headline-lg text-on-surface">${ui.escapeHtml(profile.fullName)}</h1>
          <p class="text-body-md text-on-surface-variant mt-2">${ui.escapeHtml(profile.email)}</p>
          <span class="inline-flex mt-3 px-3 py-1 rounded-full ${profile.verificationStatus === 'Verified' ? 'bg-success-green/10 text-success-green' : 'bg-highlight-gold/20 text-on-surface'} text-label-md font-label-md">
            ${profile.verificationStatus === 'Verified' ? 'Đã xác minh' : 'Chờ xác minh'}
          </span>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 w-full lg:w-auto">
          <div class="bg-surface-container-low border border-outline-variant rounded-xl p-4">
            <p class="text-label-sm font-label-sm text-on-surface-variant uppercase tracking-wider">Doanh thu</p>
            <strong class="block mt-1 text-headline-md font-headline-md text-primary">${ui.formatCurrency(profile.totalRevenue || 0)}</strong>
          </div>
          <div class="bg-surface-container-low border border-outline-variant rounded-xl p-4">
            <p class="text-label-sm font-label-sm text-on-surface-variant uppercase tracking-wider">Khách sạn</p>
            <strong class="block mt-1 text-headline-md font-headline-md text-on-surface">${profile.ownedHotelCount || 0}</strong>
          </div>
          <a class="bg-action-blue text-white rounded-xl p-4 hover:bg-primary-container transition-colors" href="admin.html">
            <p class="text-label-sm font-label-sm uppercase tracking-wider">Quản lý</p>
            <strong class="block mt-1 text-headline-md font-headline-md">Mở</strong>
          </a>
        </div>
      </div>
    `;
  }

  function renderHotels(hotels) {
    if (hotels.length === 0) {
      ui.renderMessage(hotelsList, 'Bạn chưa sở hữu khách sạn nào.', 'muted');
      return;
    }

    hotelsList.className = 'space-y-4';
    hotelsList.hidden = false;
    hotelsList.innerHTML = hotels.map((hotel) => `
      <article class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col md:flex-row gap-4">
        <img src="${ui.hotelImage(hotel.hotelId, hotel.mainImageUrl)}" alt="${ui.escapeHtml(hotel.name)}" class="w-full md:w-44 h-32 object-cover rounded-lg" />
        <div class="flex-1 min-w-0">
          <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-3">
            <div>
              <h3 class="text-headline-md font-headline-md text-on-surface">${ui.escapeHtml(hotel.name)}</h3>
              <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mt-1">${ui.escapeHtml(hotel.city)}</p>
            </div>
            <span class="px-3 py-1 rounded-full bg-highlight-gold/20 text-on-surface text-label-md font-label-md">★ ${(hotel.averageRating || 0).toFixed(1)} (${hotel.reviewCount || 0})</span>
          </div>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-3 mt-4 text-label-md font-label-md">
            <div><p class="text-on-surface-variant">Doanh thu</p><p class="text-primary">${ui.formatCurrency(hotel.totalRevenue || 0)}</p></div>
            <div><p class="text-on-surface-variant">Booking</p><p class="text-on-surface">${hotel.bookingCount || 0}</p></div>
            <div><p class="text-on-surface-variant">Loại phòng</p><p class="text-on-surface">${hotel.roomTypeCount || 0}</p></div>
            <div><a class="text-primary hover:underline" href="hotel-detail.html?hotelId=${hotel.hotelId}">Xem chi tiết</a></div>
          </div>
        </div>
      </article>
    `).join('');
  }

  loadProfile();
})();

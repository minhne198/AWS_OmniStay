(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('search');

  const state = {
    search: ui.readSearchParams(),
    results: []
  };

  const form = document.getElementById('searchForm');
  const resultsList = document.getElementById('resultsList');
  const count = document.getElementById('resultsCount');
  const summary = document.getElementById('searchSummary');
  const cityInput = document.getElementById('city');
  const keywordInput = document.getElementById('keyword');
  const minRatingInput = document.getElementById('minRating');
  const sortByInput = document.getElementById('sortBy');

  ui.populateCitySelect(cityInput, state.search.city);
  keywordInput.value = state.search.keyword;
  if (minRatingInput) {
    minRatingInput.value = state.search.minRating;
  }
  if (sortByInput) {
    sortByInput.value = state.search.sortBy || 'price';
  }
  document.getElementById('checkIn').value = state.search.checkIn;
  document.getElementById('checkOut').value = state.search.checkOut;
  document.getElementById('guests').value = state.search.guests;

  form.addEventListener('submit', (event) => {
    event.preventDefault();
    const query = ui.toQuery({
      city: cityInput.value.trim(),
      keyword: keywordInput.value.trim(),
      minRating: minRatingInput ? minRatingInput.value : '',
      sortBy: sortByInput ? sortByInput.value : '',
      checkIn: document.getElementById('checkIn').value,
      checkOut: document.getElementById('checkOut').value,
      guests: document.getElementById('guests').value
    });
    window.location.href = `search.html?${query}`;
  });

  async function runSearch() {
    ui.renderMessage(resultsList, 'Đang tải kết quả...', 'muted');
    const cityLabel = state.search.city || 'Tất cả tỉnh/thành';
    const keywordLabel = state.search.keyword ? ` - ${state.search.keyword}` : '';
    summary.textContent = `${cityLabel}${keywordLabel} - ${ui.formatDate(state.search.checkIn)} - ${ui.formatDate(state.search.checkOut)} - ${state.search.guests} khách`;

    try {
      state.results = await api.searchHotels({
        city: state.search.city,
        keyword: state.search.keyword,
        minRating: state.search.minRating,
        sortBy: state.search.sortBy,
        checkIn: state.search.checkIn,
        checkOut: state.search.checkOut,
        guests: state.search.guests
      });
      renderResults();
    } catch (error) {
      count.textContent = '0';
      ui.renderMessage(resultsList, error.message, 'error');
    }
  }

  function renderResults() {
    count.textContent = String(state.results.length);

    if (state.results.length === 0) {
      ui.renderMessage(resultsList, 'Chưa có phòng phù hợp với điều kiện này.', 'muted');
      return;
    }

    resultsList.innerHTML = state.results.map((result) => {
      const query = ui.toQuery({
        city: state.search.city,
        keyword: state.search.keyword,
        minRating: state.search.minRating,
        sortBy: state.search.sortBy,
        checkIn: state.search.checkIn,
        checkOut: state.search.checkOut,
        guests: state.search.guests,
        hotelId: result.hotelId,
        roomTypeId: result.roomTypeId
      });

      return `
        <a href="hotel-detail.html?${query}" class="bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden hover:border-primary hover:shadow-lg transition-all group">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-0">
            <div class="md:col-span-1">
              <img src="${ui.roomImage({ imageUrl: result.roomImageUrl, hotelId: result.hotelId }, result.hotelId) || ui.hotelImage(result.hotelId, result.mainImageUrl)}" alt="${ui.escapeHtml(result.hotelName)}" loading="lazy" class="w-full h-full object-cover aspect-video md:aspect-auto">
            </div>
            <div class="md:col-span-2 p-6">
              <div class="flex flex-col gap-2">
                <div class="flex items-center gap-2">
                  <span class="text-label-sm font-label-sm text-outline uppercase tracking-wider">${ui.escapeHtml(result.city)} · ${result.maxGuests} khách</span>
                  <span class="px-2 py-1 rounded-full bg-highlight-gold/20 text-on-surface text-label-sm font-label-sm">★ ${(result.averageRating || 0).toFixed(1)} (${result.reviewCount || 0})</span>
                </div>
                <h3 class="text-headline-md font-headline-md text-on-surface group-hover:text-primary transition-colors">${ui.escapeHtml(result.hotelName)}</h3>
                <p class="text-body-md text-on-surface-variant">${ui.escapeHtml(result.roomTypeName)}</p>
                <div class="flex items-end justify-between mt-4">
                  <div class="flex items-center gap-2">
                    <span class="text-label-md font-label-md ${result.availableRooms > 0 ? 'text-success-green' : 'text-error'}">
                      ${result.availableRooms > 0 ? `Còn ${result.availableRooms} phòng` : 'Hết phòng'}
                    </span>
                  </div>
                  <div class="text-right">
                    <span class="text-headline-md font-headline-md text-primary">${ui.formatCurrency(result.pricePerNight)}</span>
                    <span class="text-body-sm text-on-surface-variant"> / đêm</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </a>
      `;
    }).join('');
  }

  runSearch();
})();

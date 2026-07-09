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

  document.getElementById('city').value = state.search.city;
  document.getElementById('checkIn').value = state.search.checkIn;
  document.getElementById('checkOut').value = state.search.checkOut;
  document.getElementById('guests').value = state.search.guests;

  form.addEventListener('submit', (event) => {
    event.preventDefault();
    const query = ui.toQuery({
      city: document.getElementById('city').value.trim(),
      checkIn: document.getElementById('checkIn').value,
      checkOut: document.getElementById('checkOut').value,
      guests: document.getElementById('guests').value
    });
    window.location.href = `search.html?${query}`;
  });

  async function runSearch() {
    ui.renderMessage(resultsList, 'Dang tai ket qua...', 'muted');
    summary.textContent = `${state.search.city} · ${ui.formatDate(state.search.checkIn)} - ${ui.formatDate(state.search.checkOut)} · ${state.search.guests} khach`;

    try {
      state.results = await api.searchHotels({
        city: state.search.city,
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
      ui.renderMessage(resultsList, 'Chua co phong phu hop voi dieu kien nay.', 'muted');
      return;
    }

    resultsList.innerHTML = state.results.map((result) => {
      const query = ui.toQuery({
        city: state.search.city,
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
              <img src="${ui.hotelImage(result.hotelId)}" alt="${ui.escapeHtml(result.hotelName)}" loading="lazy" class="w-full h-full object-cover aspect-video md:aspect-auto">
            </div>
            <div class="md:col-span-2 p-6">
              <div class="flex flex-col gap-2">
                <div class="flex items-center gap-2">
                  <span class="text-label-sm font-label-sm text-outline uppercase tracking-wider">${ui.escapeHtml(result.city)} · ${result.maxGuests} khach</span>
                </div>
                <h3 class="text-headline-md font-headline-md text-on-surface group-hover:text-primary transition-colors">${ui.escapeHtml(result.hotelName)}</h3>
                <p class="text-body-md text-on-surface-variant">${ui.escapeHtml(result.roomTypeName)}</p>
                <div class="flex items-end justify-between mt-4">
                  <div class="flex items-center gap-2">
                    <span class="text-label-md font-label-md ${result.availableRooms > 0 ? 'text-success-green' : 'text-error'}">
                      ${result.availableRooms > 0 ? `Con ${result.availableRooms} phong` : 'Het phong'}
                    </span>
                  </div>
                  <div class="text-right">
                    <span class="text-headline-md font-headline-md text-primary">${ui.formatCurrency(result.pricePerNight)}</span>
                    <span class="text-body-sm text-on-surface-variant"> / dem</span>
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

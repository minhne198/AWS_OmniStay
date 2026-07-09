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

    resultsList.className = 'result-list';
    resultsList.hidden = false;
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
        <a class="result-card" href="hotel-detail.html?${query}">
          <img src="${ui.hotelImage(result.hotelId)}" alt="${ui.escapeHtml(result.hotelName)}" loading="lazy">
          <span class="result-main">
            <span class="eyebrow">${ui.escapeHtml(result.city)} · ${result.maxGuests} khach</span>
            <strong>${ui.escapeHtml(result.hotelName)}</strong>
            <span>${ui.escapeHtml(result.roomTypeName)}</span>
            <span>${ui.formatCurrency(result.pricePerNight)} / dem · con ${result.availableRooms} phong</span>
          </span>
        </a>
      `;
    }).join('');
  }

  runSearch();
})();

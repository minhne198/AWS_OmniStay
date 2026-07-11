(function () {
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('home');

  const dates = ui.defaultDates();
  const form = document.getElementById('homeSearchForm');
  const cityInput = document.getElementById('city');
  const checkInInput = document.getElementById('checkIn');
  const checkOutInput = document.getElementById('checkOut');
  const guestsInput = document.getElementById('guests');

  ui.populateCitySelect(cityInput, cityInput.value || 'Da Nang', { includeAll: false });
  checkInInput.value = dates.checkIn;
  checkOutInput.value = dates.checkOut;

  form.addEventListener('submit', (event) => {
    event.preventDefault();
    const query = ui.toQuery({
      city: cityInput.value.trim() || 'Da Nang',
      checkIn: checkInInput.value,
      checkOut: checkOutInput.value,
      guests: guestsInput.value || '2'
    });
    window.location.href = `search.html?${query}`;
  });

  document.querySelectorAll('[data-city]').forEach((button) => {
    button.addEventListener('click', () => {
      cityInput.value = button.dataset.city;
      form.requestSubmit();
    });
  });
})();

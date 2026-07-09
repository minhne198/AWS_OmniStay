(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const params = new URLSearchParams(window.location.search);

  ui.redirectIfAuthenticated();

  const loginForm = document.getElementById('loginForm');
  const message = document.getElementById('authMessage');

  if (loginForm) {
    function redirectAfterAuth() {
      window.location.replace(ui.safeReturnUrl(params.get('returnUrl')) || 'home.html');
    }

    function setBusy(form, busy) {
      form.querySelectorAll('button, input').forEach((element) => {
        element.disabled = busy;
      });
    }

    loginForm.addEventListener('submit', async (event) => {
      event.preventDefault();
      setBusy(loginForm, true);
      ui.renderMessage(message, 'Đang đăng nhập...', 'muted');

      try {
        const auth = await api.login({
          email: document.getElementById('loginEmail').value,
          password: document.getElementById('loginPassword').value
        });
        api.setSession(auth);
        redirectAfterAuth();
      } catch (error) {
        ui.renderMessage(message, error.message, 'error');
      } finally {
        setBusy(loginForm, false);
      }
    });
  }
})();

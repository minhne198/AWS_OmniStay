(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const params = new URLSearchParams(window.location.search);

  ui.redirectIfAuthenticated();

  const loginForm = document.getElementById('loginForm');
  const registerForm = document.getElementById('registerForm');
  const message = document.getElementById('authMessage');

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
    ui.renderMessage(message, 'Dang dang nhap...', 'muted');

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

  registerForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    setBusy(registerForm, true);
    ui.renderMessage(message, 'Dang tao tai khoan...', 'muted');

    try {
      const auth = await api.register({
        fullName: document.getElementById('registerName').value,
        email: document.getElementById('registerEmail').value,
        password: document.getElementById('registerPassword').value
      });
      api.setSession(auth);
      redirectAfterAuth();
    } catch (error) {
      ui.renderMessage(message, error.message, 'error');
    } finally {
      setBusy(registerForm, false);
    }
  });
})();

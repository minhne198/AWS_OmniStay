(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('profile');

  const profileForm = document.getElementById('profileForm');
  const passwordForm = document.getElementById('passwordForm');
  const fullNameInput = document.getElementById('profileFullName');
  const emailInput = document.getElementById('profileEmail');
  const avatarUrlInput = document.getElementById('profileAvatarUrl');
  const avatarFileInput = document.getElementById('profileAvatarFile');
  const avatarPreview = document.getElementById('avatarPreview');
  const avatarFallback = document.getElementById('avatarFallback');
  const balance = document.getElementById('profileBalance');
  const profileMessage = document.getElementById('profileMessage');
  const passwordMessage = document.getElementById('passwordMessage');
  const balanceTransactionsList = document.getElementById('balanceTransactionsList');

  function syncUser(user) {
    api.setUser(user);
    fullNameInput.value = user.fullName || '';
    emailInput.value = user.email || '';
    avatarUrlInput.value = user.avatarUrl || '';
    balance.textContent = ui.formatCurrency(user.balance || 0);
    document.querySelectorAll('[data-session-name]').forEach((target) => {
      target.textContent = `${user.fullName} (${ui.roleLabel(user.role)})`;
    });
    renderAvatar(user);
  }

  function renderAvatar(user) {
    const avatarUrl = ui.assetUrl ? ui.assetUrl(user.avatarUrl) : user.avatarUrl;
    const initials = String(user.fullName || 'OS')
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0])
      .join('')
      .toUpperCase();

    avatarFallback.textContent = initials || 'OS';

    if (avatarUrl) {
      avatarPreview.src = avatarUrl;
      avatarPreview.hidden = false;
      avatarFallback.hidden = true;
      return;
    }

    avatarPreview.hidden = true;
    avatarFallback.hidden = false;
  }

  async function refreshProfile() {
    try {
      syncUser(await api.me());
      renderBalanceTransactions(await api.getMyBalanceTransactions());
    } catch (error) {
      ui.renderMessage(profileMessage, error.message, 'error');
    }
  }

  function renderBalanceTransactions(transactions) {
    if (!balanceTransactionsList) {
      return;
    }

    if (!transactions || transactions.length === 0) {
      ui.renderMessage(balanceTransactionsList, 'Chưa có giao dịch số dư.', 'muted');
      return;
    }

    balanceTransactionsList.className = 'space-y-3';
    balanceTransactionsList.hidden = false;
    balanceTransactionsList.innerHTML = transactions.map((item) => `
      <article class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <p class="text-label-md font-label-md text-on-surface">${transactionTypeLabel(item.type)}${item.bookingCode ? ` · ${ui.escapeHtml(item.bookingCode)}` : ''}</p>
          <p class="text-body-sm text-on-surface-variant mt-1">${ui.escapeHtml(item.description || '')}</p>
          <p class="text-label-sm font-label-sm text-on-surface-variant mt-1">${ui.formatDate(item.createdAt)}</p>
        </div>
        <div class="md:text-right">
          <p class="text-headline-md font-headline-md ${item.amount >= 0 ? 'text-success-green' : 'text-error'}">${item.amount >= 0 ? '+' : ''}${ui.formatCurrency(item.amount || 0)}</p>
          <p class="text-label-sm font-label-sm text-on-surface-variant">Sau giao dịch: ${ui.formatCurrency(item.balanceAfter || 0)}</p>
        </div>
      </article>
    `).join('');
  }

  function transactionTypeLabel(type) {
    return {
      AdminBalanceSet: 'Số dư ban đầu',
      AdminBalanceAdjustment: 'Admin chỉnh số dư',
      TestTopUp: 'Nạp tiền test',
      BookingPayment: 'Trừ tiền booking',
      OwnerBookingCredit: 'Cộng tiền booking',
      BookingRefund: 'Hoàn tiền booking',
      OwnerBookingReversal: 'Trừ lại booking hủy'
    }[type] || type;
  }

  avatarUrlInput.addEventListener('input', () => {
    renderAvatar({
      fullName: fullNameInput.value,
      avatarUrl: avatarUrlInput.value
    });
  });

  avatarFileInput.addEventListener('change', async () => {
    const file = avatarFileInput.files && avatarFileInput.files[0];
    if (!file) {
      return;
    }

    ui.renderMessage(profileMessage, 'Đang upload avatar...', 'muted');

    try {
      const result = await api.uploadImage(file);
      avatarUrlInput.value = result.imageUrl;
      renderAvatar({
        fullName: fullNameInput.value,
        avatarUrl: result.imageUrl
      });
      ui.renderMessage(profileMessage, 'Upload avatar thành công. Bấm cập nhật hồ sơ để lưu.', 'success');
    } catch (error) {
      avatarFileInput.value = '';
      ui.renderMessage(profileMessage, error.message, 'error');
    }
  });

  profileForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    ui.renderMessage(profileMessage, 'Đang cập nhật hồ sơ...', 'muted');

    try {
      const updated = await api.updateProfile({
        fullName: fullNameInput.value.trim(),
        avatarUrl: avatarUrlInput.value.trim()
      });
      syncUser(updated);
      ui.renderMessage(profileMessage, 'Cập nhật hồ sơ thành công.', 'success');
    } catch (error) {
      ui.renderMessage(profileMessage, error.message, 'error');
    }
  });

  passwordForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (newPassword !== confirmPassword) {
      ui.renderMessage(passwordMessage, 'Mật khẩu mới nhập lại chưa khớp.', 'error');
      return;
    }

    ui.renderMessage(passwordMessage, 'Đang đổi mật khẩu...', 'muted');

    try {
      await api.changePassword({ currentPassword, newPassword });
      passwordForm.reset();
      ui.renderMessage(passwordMessage, 'Đổi mật khẩu thành công.', 'success');
    } catch (error) {
      ui.renderMessage(passwordMessage, error.message, 'error');
    }
  });

  refreshProfile();
})();

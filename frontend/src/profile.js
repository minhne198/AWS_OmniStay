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
  const bankForm = document.getElementById('bankForm');
  const topUpForm = document.getElementById('topUpForm');
  const withdrawForm = document.getElementById('withdrawForm');
  const fullNameInput = document.getElementById('profileFullName');
  const emailInput = document.getElementById('profileEmail');
  const avatarUrlInput = document.getElementById('profileAvatarUrl');
  const avatarFileInput = document.getElementById('profileAvatarFile');
  const bankNameInput = document.getElementById('bankName');
  const bankAccountNumberInput = document.getElementById('bankAccountNumber');
  const bankAccountHolderInput = document.getElementById('bankAccountHolder');
  const topUpAmountInput = document.getElementById('topUpAmount');
  const withdrawAmountInput = document.getElementById('withdrawAmount');
  const withdrawNoteInput = document.getElementById('withdrawNote');
  const avatarPreview = document.getElementById('avatarPreview');
  const avatarFallback = document.getElementById('avatarFallback');
  const balance = document.getElementById('profileBalance');
  const profileMessage = document.getElementById('profileMessage');
  const passwordMessage = document.getElementById('passwordMessage');
  const bankMessage = document.getElementById('bankMessage');
  const topUpMessage = document.getElementById('topUpMessage');
  const withdrawMessage = document.getElementById('withdrawMessage');
  const balanceTransactionsList = document.getElementById('balanceTransactionsList');
  const withdrawalsList = document.getElementById('withdrawalsList');
  const walletState = new URLSearchParams(window.location.search).get('wallet') || '';

  function syncUser(user) {
    api.setUser(user);
    fullNameInput.value = user.fullName || '';
    emailInput.value = user.email || '';
    avatarUrlInput.value = user.avatarUrl || '';
    bankNameInput.value = user.bankName || '';
    bankAccountNumberInput.value = user.bankAccountNumber || '';
    bankAccountHolderInput.value = user.bankAccountHolder || '';
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
      const [user, transactions, withdrawals] = await Promise.all([
        api.me(),
        api.getMyBalanceTransactions(),
        api.getMyWithdrawals()
      ]);
      syncUser(user);
      renderBalanceTransactions(transactions);
      renderWithdrawals(withdrawals);
      renderWalletReturnMessage();
    } catch (error) {
      ui.renderMessage(profileMessage, error.message, 'error');
    }
  }

  function renderBalanceTransactions(transactions) {
    if (!balanceTransactionsList) {
      return;
    }

    if (!transactions || transactions.length === 0) {
      ui.renderMessage(balanceTransactionsList, 'Chua co giao dich so du.', 'muted');
      return;
    }

    balanceTransactionsList.className = 'space-y-3';
    balanceTransactionsList.hidden = false;
    balanceTransactionsList.innerHTML = transactions.map((item) => `
      <article class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <p class="text-label-md font-label-md text-on-surface">${transactionTypeLabel(item.type)}${item.bookingCode ? ` - ${ui.escapeHtml(item.bookingCode)}` : ''}</p>
          <p class="text-body-sm text-on-surface-variant mt-1">${ui.escapeHtml(item.description || '')}</p>
          <p class="text-label-sm font-label-sm text-on-surface-variant mt-1">${ui.formatDate(item.createdAt)}</p>
        </div>
        <div class="md:text-right">
          <p class="text-headline-md font-headline-md ${item.amount >= 0 ? 'text-success-green' : 'text-error'}">${item.amount >= 0 ? '+' : ''}${ui.formatCurrency(item.amount || 0)}</p>
          <p class="text-label-sm font-label-sm text-on-surface-variant">Sau giao dich: ${ui.formatCurrency(item.balanceAfter || 0)}</p>
        </div>
      </article>
    `).join('');
  }

  function renderWithdrawals(withdrawals) {
    if (!withdrawalsList) {
      return;
    }

    if (!withdrawals || withdrawals.length === 0) {
      ui.renderMessage(withdrawalsList, 'Chua co lenh rut tien.', 'muted');
      return;
    }

    withdrawalsList.className = 'space-y-3';
    withdrawalsList.hidden = false;
    withdrawalsList.innerHTML = withdrawals.map((item) => `
      <article class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <div class="flex items-center gap-2">
            <p class="text-label-md font-label-md text-on-surface">${ui.formatCurrency(item.amount || 0)}</p>
            <span class="px-2 py-1 rounded-full text-label-sm font-label-sm ${withdrawalStatusClass(item.status)}">${withdrawalStatusLabel(item.status)}</span>
          </div>
          <p class="text-body-sm text-on-surface-variant mt-1">${ui.escapeHtml(item.bankName)} - ${ui.escapeHtml(item.bankAccountNumber)} - ${ui.escapeHtml(item.bankAccountHolder)}</p>
          ${item.note ? `<p class="text-body-sm text-on-surface-variant mt-1">${ui.escapeHtml(item.note)}</p>` : ''}
          ${item.adminNote ? `<p class="text-body-sm text-on-surface-variant mt-1">Admin: ${ui.escapeHtml(item.adminNote)}</p>` : ''}
        </div>
        <div class="md:text-right">
          <p class="text-label-sm font-label-sm text-on-surface-variant">Tao: ${ui.formatDate(item.requestedAt)}</p>
          ${item.completedAt ? `<p class="text-label-sm font-label-sm text-on-surface-variant">Xu ly: ${ui.formatDate(item.completedAt)}</p>` : ''}
        </div>
      </article>
    `).join('');
  }

  function transactionTypeLabel(type) {
    return {
      AdminBalanceSet: 'So du ban dau',
      AdminBalanceAdjustment: 'Admin chinh so du',
      TestTopUp: 'Nap tien test',
      PayOsTopUp: 'Nap tien payOS',
      BookingPayment: 'Tru tien booking',
      OwnerBookingCredit: 'Cong tien booking',
      BookingRefund: 'Hoan tien booking',
      OwnerBookingReversal: 'Tru lai booking huy',
      WithdrawalCompleted: 'Rut tien'
    }[type] || type;
  }

  function withdrawalStatusLabel(status) {
    return {
      Pending: 'Cho xu ly',
      Completed: 'Da hoan tat',
      Rejected: 'Bi tu choi'
    }[status] || status;
  }

  function withdrawalStatusClass(status) {
    return {
      Pending: 'bg-highlight-gold/20 text-on-surface',
      Completed: 'bg-success-green/10 text-success-green',
      Rejected: 'bg-error-container text-error'
    }[status] || 'bg-surface-container-high text-on-surface-variant';
  }

  function renderWalletReturnMessage() {
    if (walletState === 'topup-return') {
      ui.renderMessage(topUpMessage, 'Neu ban vua nap tien qua payOS, he thong dang cho webhook xac nhan tu ngan hang.', 'muted');
    } else if (walletState === 'topup-cancel') {
      ui.renderMessage(topUpMessage, 'Ban da quay lai tu payOS. So du chua thay doi.', 'muted');
    }
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

    ui.renderMessage(profileMessage, 'Dang upload avatar...', 'muted');

    try {
      const result = await api.uploadImage(file);
      avatarUrlInput.value = result.imageUrl;
      renderAvatar({
        fullName: fullNameInput.value,
        avatarUrl: result.imageUrl
      });
      ui.renderMessage(profileMessage, 'Upload avatar thanh cong. Bam cap nhat ho so de luu.', 'success');
    } catch (error) {
      avatarFileInput.value = '';
      ui.renderMessage(profileMessage, error.message, 'error');
    }
  });

  profileForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    ui.renderMessage(profileMessage, 'Dang cap nhat ho so...', 'muted');

    try {
      const updated = await api.updateProfile({
        fullName: fullNameInput.value.trim(),
        avatarUrl: avatarUrlInput.value.trim()
      });
      syncUser(updated);
      ui.renderMessage(profileMessage, 'Cap nhat ho so thanh cong.', 'success');
    } catch (error) {
      ui.renderMessage(profileMessage, error.message, 'error');
    }
  });

  bankForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    ui.renderMessage(bankMessage, 'Dang luu tai khoan ngan hang...', 'muted');

    try {
      const updated = await api.updateBankAccount({
        bankName: bankNameInput.value.trim(),
        bankAccountNumber: bankAccountNumberInput.value.trim(),
        bankAccountHolder: bankAccountHolderInput.value.trim()
      });
      syncUser(updated);
      ui.renderMessage(bankMessage, 'Da luu tai khoan ngan hang.', 'success');
    } catch (error) {
      ui.renderMessage(bankMessage, error.message, 'error');
    }
  });

  topUpForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    ui.renderMessage(topUpMessage, 'Dang tao ma QR nap tien...', 'muted');

    try {
      const payment = await api.createPayOsTopUp({
        amount: Number(topUpAmountInput.value || 0),
        returnUrl: walletUrl('topup-return'),
        cancelUrl: walletUrl('topup-cancel')
      });
      window.location.href = payment.checkoutUrl;
    } catch (error) {
      ui.renderMessage(topUpMessage, error.message, 'error');
    }
  });

  withdrawForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    ui.renderMessage(withdrawMessage, 'Dang gui yeu cau rut tien...', 'muted');

    try {
      await api.createWithdrawal({
        amount: Number(withdrawAmountInput.value || 0),
        note: withdrawNoteInput.value.trim()
      });
      withdrawForm.reset();
      const [user, withdrawals] = await Promise.all([
        api.me(),
        api.getMyWithdrawals()
      ]);
      syncUser(user);
      renderWithdrawals(withdrawals);
      ui.renderMessage(withdrawMessage, 'Da gui yeu cau rut tien. Admin se xu ly thu cong.', 'success');
    } catch (error) {
      ui.renderMessage(withdrawMessage, error.message, 'error');
    }
  });

  passwordForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (newPassword !== confirmPassword) {
      ui.renderMessage(passwordMessage, 'Mat khau moi nhap lai chua khop.', 'error');
      return;
    }

    ui.renderMessage(passwordMessage, 'Dang doi mat khau...', 'muted');

    try {
      await api.changePassword({ currentPassword, newPassword });
      passwordForm.reset();
      ui.renderMessage(passwordMessage, 'Doi mat khau thanh cong.', 'success');
    } catch (error) {
      ui.renderMessage(passwordMessage, error.message, 'error');
    }
  });

  function walletUrl(state) {
    const url = new URL('profile.html', window.location.href);
    url.searchParams.set('wallet', state);
    return url.toString();
  }

  refreshProfile();
})();

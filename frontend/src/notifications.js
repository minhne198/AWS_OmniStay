(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('notifications');

  const list = document.getElementById('notificationsList');
  const markAllRead = document.getElementById('markAllRead');
  let notifications = [];

  async function loadNotifications() {
    ui.renderMessage(list, 'Đang tải thông báo...', 'muted');
    try {
      notifications = await api.getMyNotifications();
      render();
    } catch (error) {
      ui.renderMessage(list, error.message, 'error');
    }
  }

  function render() {
    if (notifications.length === 0) {
      ui.renderMessage(list, 'Bạn chưa có thông báo nào.', 'muted');
      return;
    }

    list.className = 'space-y-4';
    list.hidden = false;
    list.innerHTML = notifications.map((item) => `
      <article class="bg-surface-container-lowest border ${item.isRead ? 'border-outline-variant' : 'border-primary'} rounded-xl p-5">
        <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-3">
          <div>
            <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-1">${ui.escapeHtml(notificationTypeLabel(item.type))}</p>
            <h2 class="text-headline-md font-headline-md text-on-surface">${ui.escapeHtml(normalizeNotificationText(item.title))}</h2>
            <p class="text-body-md text-on-surface-variant mt-2">${ui.escapeHtml(normalizeNotificationText(item.message))}</p>
            <p class="text-label-sm font-label-sm text-on-surface-variant mt-3">${ui.formatDate(item.createdAt)}</p>
          </div>
          <div class="flex gap-2 shrink-0">
            ${item.linkUrl ? `<a class="px-4 py-2 rounded-lg bg-surface-container-high text-on-surface hover:bg-surface-container-highest transition-colors" href="${ui.escapeHtml(item.linkUrl)}">Mở</a>` : ''}
            ${item.isRead ? '' : `<button class="px-4 py-2 rounded-lg bg-action-blue text-white hover:bg-primary-container transition-colors" type="button" data-read="${item.notificationId}">Đã đọc</button>`}
          </div>
        </div>
      </article>
    `).join('');

    list.querySelectorAll('[data-read]').forEach((button) => {
      button.addEventListener('click', async () => {
        await api.markNotificationRead(button.dataset.read);
        await loadNotifications();
      });
    });
  }

  function notificationTypeLabel(type) {
    return {
      BookingCreated: 'Tạo booking',
      OwnerNewBooking: 'Booking mới',
      BookingPaid: 'Thanh toán',
      OwnerBookingPaid: 'Booking đã thanh toán',
      BookingCancelled: 'Hủy booking',
      OwnerBookingCancelled: 'Booking bị hủy',
      ReviewCreated: 'Đánh giá',
      AdminBalanceSet: 'Số dư ban đầu',
      AdminBalanceAdjustment: 'Admin chỉnh sửa số dư',
      TestTopUp: 'Nạp tiền test'
    }[type] || type;
  }

  function normalizeNotificationText(value) {
    return String(value || '')
      .replaceAll('Co danh gia khach san moi', 'Có đánh giá khách sạn mới')
      .replaceAll('Co danh gia moi', 'Có đánh giá mới')
      .replaceAll('vua danh gia', 'vừa đánh giá')
      .replaceAll('nhan danh gia', 'nhận đánh giá')
      .replaceAll('Booking da duoc tao', 'Booking đã được tạo')
      .replaceAll('dang cho thanh toan', 'đang chờ thanh toán')
      .replaceAll('Co booking moi', 'Có booking mới')
      .replaceAll('Booking moi', 'Booking mới')
      .replaceAll('vua dat', 'vừa đặt')
      .replaceAll('vua tao', 'vừa tạo')
      .replaceAll('Thanh toan thanh cong', 'Thanh toán thành công')
      .replaceAll('Booking da thanh toan', 'Booking đã thanh toán')
      .replaceAll('da thanh toan thanh cong', 'đã thanh toán thành công')
      .replaceAll('da duoc thanh toan', 'đã được thanh toán')
      .replaceAll('Booking da huy', 'Booking đã hủy')
      .replaceAll('Booking bi huy', 'Booking bị hủy')
      .replaceAll('da duoc huy', 'đã được hủy')
      .replaceAll('da bi huy', 'đã bị hủy')
      .replaceAll('Nap tien test', 'Nạp tiền test')
      .replaceAll('Admin tao tai khoan voi so du ban dau', 'Admin tạo tài khoản với số dư ban đầu')
      .replaceAll('Admin tao so du ban dau', 'Admin tạo số dư ban đầu')
      .replaceAll('Admin chinh sua so du', 'Admin chỉnh sửa số dư')
      .replaceAll('duoc dieu chinh so du tu', 'được điều chỉnh số dư từ')
      .replaceAll('duoc nap them', 'được nạp thêm')
      .replaceAll('co so du ban dau', 'có số dư ban đầu')
      .replaceAll(' thanh ', ' thành ')
      .replaceAll(' tai ', ' tại ')
      .replaceAll(' khach san', ' khách sạn');
  }

  markAllRead.addEventListener('click', async () => {
    await api.markAllNotificationsRead();
    await loadNotifications();
  });

  loadNotifications();
})();

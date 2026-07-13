(function () {
    const api = window.OmniStayApi;
    const ui = window.OmniStaySession;
    const session = ui.requireAuth();
    if (!session) {
        return;
    }

    ui.hydrateNav('admin');
    const isAdmin = session.user.role === 'Admin';
    const minRoomPrice = 2000;

    // State
    let state = {
        hotels: [],
        roomTypes: [],
        bookings: [],
        users: [],
        dashboard: null,
        transactions: [],
        withdrawals: [],
        activity: [],
        selectedHotelId: null
    };

    // Elements
    const elements = {
        // Tabs
        tabDashboard: document.getElementById('tab-dashboard'),
        tabUsers: document.getElementById('tab-users'),
        tabHotels: document.getElementById('tab-hotels'),
        tabBookings: document.getElementById('tab-bookings'),
        tabTransactions: document.getElementById('tab-transactions'),
        tabWithdrawals: document.getElementById('tab-withdrawals'),
        tabActivity: document.getElementById('tab-activity'),
        tabContentDashboard: document.getElementById('tab-content-dashboard'),
        tabContentUsers: document.getElementById('tab-content-users'),
        tabContentHotels: document.getElementById('tab-content-hotels'),
        tabContentBookings: document.getElementById('tab-content-bookings'),
        tabContentTransactions: document.getElementById('tab-content-transactions'),
        tabContentWithdrawals: document.getElementById('tab-content-withdrawals'),
        tabContentActivity: document.getElementById('tab-content-activity'),

        // Dashboard
        dashboardStats: document.getElementById('dashboardStats'),
        dashboardRevenueByDay: document.getElementById('dashboardRevenueByDay'),
        dashboardRevenueByMonth: document.getElementById('dashboardRevenueByMonth'),

        // Users
        userForm: document.getElementById('userForm'),
        userId: document.getElementById('userId'),
        userFullName: document.getElementById('userFullName'),
        userEmail: document.getElementById('userEmail'),
        userPassword: document.getElementById('userPassword'),
        userPasswordConfirm: document.getElementById('userPasswordConfirm'),
        userRole: document.getElementById('userRole'),
        userBalance: document.getElementById('userBalance'),
        userSubmit: document.getElementById('userSubmit'),
        resetUserForm: document.getElementById('resetUserForm'),
        userMessage: document.getElementById('userMessage'),
        adminUsersList: document.getElementById('adminUsersList'),

        // Hotels
        hotelForm: document.getElementById('hotelForm'),
        hotelId: document.getElementById('hotelId'),
        hotelName: document.getElementById('hotelName'),
        hotelCity: document.getElementById('hotelCity'),
        hotelStarRating: document.getElementById('hotelStarRating'),
        hotelAddress: document.getElementById('hotelAddress'),
        hotelDescription: document.getElementById('hotelDescription'),
        hotelImageUrl: document.getElementById('hotelImageUrl'),
        hotelImageFile: document.getElementById('hotelImageFile'),
        hotelSubmit: document.getElementById('hotelSubmit'),
        resetHotelForm: document.getElementById('resetHotelForm'),
        hotelMessage: document.getElementById('hotelMessage'),
        adminHotelsList: document.getElementById('adminHotelsList'),

        // Rooms
        roomSection: document.getElementById('roomSection'),
        roomForm: document.getElementById('roomForm'),
        roomTypeId: document.getElementById('roomTypeId'),
        roomHotelId: document.getElementById('roomHotelId'),
        roomName: document.getElementById('roomName'),
        roomMaxGuests: document.getElementById('roomMaxGuests'),
        roomTotalRooms: document.getElementById('roomTotalRooms'),
        roomPrice: document.getElementById('roomPrice'),
        roomDescription: document.getElementById('roomDescription'),
        roomImageUrl: document.getElementById('roomImageUrl'),
        roomImageFile: document.getElementById('roomImageFile'),
        roomIsHidden: document.getElementById('roomIsHidden'),
        roomSubmit: document.getElementById('roomSubmit'),
        resetRoomForm: document.getElementById('resetRoomForm'),
        roomMessage: document.getElementById('roomMessage'),
        adminRoomsList: document.getElementById('adminRoomsList'),
        selectedHotelName: document.getElementById('selectedHotelName'),
        closeRoomSection: document.getElementById('closeRoomSection'),

        // Bookings
        adminBookingsList: document.getElementById('adminBookingsList'),

        // Transactions and activity
        adminTransactionsList: document.getElementById('adminTransactionsList'),
        adminWithdrawalsList: document.getElementById('adminWithdrawalsList'),
        adminActivityList: document.getElementById('adminActivityList')
    };

    ui.populateCitySelect(elements.hotelCity, 'Da Nang', { includeAll: false });

    // Tab switching
    function switchTab(tab) {
        if (!isAdmin && !['dashboard', 'hotels', 'bookings'].includes(tab)) {
            tab = 'dashboard';
        }

        // Reset all tabs
        [elements.tabDashboard, elements.tabUsers, elements.tabHotels, elements.tabBookings, elements.tabTransactions, elements.tabWithdrawals, elements.tabActivity].forEach(t => {
            if (!t) return;
            t.classList.remove('border-booking-blue', 'text-booking-blue');
            t.classList.add('border-transparent', 'text-on-surface-variant');
        });

        // Hide all content
        [elements.tabContentDashboard, elements.tabContentUsers, elements.tabContentHotels, elements.tabContentBookings, elements.tabContentTransactions, elements.tabContentWithdrawals, elements.tabContentActivity].forEach(c => {
            if (!c) return;
            c.classList.add('hidden');
        });

        // Activate selected tab
        if (tab === 'dashboard') {
            elements.tabDashboard.classList.remove('border-transparent', 'text-on-surface-variant');
            elements.tabDashboard.classList.add('border-booking-blue', 'text-booking-blue');
            elements.tabContentDashboard.classList.remove('hidden');
        } else if (tab === 'users') {
            elements.tabUsers.classList.remove('border-transparent', 'text-on-surface-variant');
            elements.tabUsers.classList.add('border-booking-blue', 'text-booking-blue');
            elements.tabContentUsers.classList.remove('hidden');
        } else if (tab === 'hotels') {
            elements.tabHotels.classList.remove('border-transparent', 'text-on-surface-variant');
            elements.tabHotels.classList.add('border-booking-blue', 'text-booking-blue');
            elements.tabContentHotels.classList.remove('hidden');
        } else if (tab === 'bookings') {
            elements.tabBookings.classList.remove('border-transparent', 'text-on-surface-variant');
            elements.tabBookings.classList.add('border-booking-blue', 'text-booking-blue');
            elements.tabContentBookings.classList.remove('hidden');
        } else if (tab === 'transactions') {
            elements.tabTransactions.classList.remove('border-transparent', 'text-on-surface-variant');
            elements.tabTransactions.classList.add('border-booking-blue', 'text-booking-blue');
            elements.tabContentTransactions.classList.remove('hidden');
        } else if (tab === 'withdrawals') {
            elements.tabWithdrawals.classList.remove('border-transparent', 'text-on-surface-variant');
            elements.tabWithdrawals.classList.add('border-booking-blue', 'text-booking-blue');
            elements.tabContentWithdrawals.classList.remove('hidden');
        } else if (tab === 'activity') {
            elements.tabActivity.classList.remove('border-transparent', 'text-on-surface-variant');
            elements.tabActivity.classList.add('border-booking-blue', 'text-booking-blue');
            elements.tabContentActivity.classList.remove('hidden');
        }
    }

    // Render user message
    function renderUserMessage(msg, tone) {
        if (!elements.userMessage) return;
        if (tone === 'error') {
            elements.userMessage.className = 'mt-4 p-4 bg-error-container text-error rounded-lg border border-error/30';
        } else if (tone === 'success') {
            elements.userMessage.className = 'mt-4 p-4 bg-success-green/10 text-success-green rounded-lg border border-success-green/30';
        } else {
            elements.userMessage.className = 'mt-4 p-4 bg-surface-container-lowest text-on-surface-variant rounded-lg border border-outline-variant';
        }
        elements.userMessage.textContent = msg;
        elements.userMessage.hidden = false;
    }

    // Render hotel message
    function renderHotelMessage(msg, tone) {
        if (!elements.hotelMessage) return;
        if (tone === 'error') {
            elements.hotelMessage.className = 'mt-4 p-4 bg-error-container text-error rounded-lg border border-error/30';
        } else if (tone === 'success') {
            elements.hotelMessage.className = 'mt-4 p-4 bg-success-green/10 text-success-green rounded-lg border border-success-green/30';
        } else {
            elements.hotelMessage.className = 'mt-4 p-4 bg-surface-container-lowest text-on-surface-variant rounded-lg border border-outline-variant';
        }
        elements.hotelMessage.textContent = msg;
        elements.hotelMessage.hidden = false;
    }

    // Render room message
    function renderRoomMessage(msg, tone) {
        if (!elements.roomMessage) return;
        if (tone === 'error') {
            elements.roomMessage.className = 'mt-4 p-4 bg-error-container text-error rounded-lg border border-error/30';
        } else if (tone === 'success') {
            elements.roomMessage.className = 'mt-4 p-4 bg-success-green/10 text-success-green rounded-lg border border-success-green/30';
        } else {
            elements.roomMessage.className = 'mt-4 p-4 bg-surface-container-lowest text-on-surface-variant rounded-lg border border-outline-variant';
        }
        elements.roomMessage.textContent = msg;
        elements.roomMessage.hidden = false;
    }

    function renderDashboard() {
        if (!elements.dashboardStats || !state.dashboard) return;
        const dashboard = state.dashboard;
        const availableRooms = Math.max(0, (dashboard.totalRooms || 0) - (dashboard.bookedRooms || 0));

        elements.dashboardStats.innerHTML = [
            ['Doanh thu', ui.formatCurrency(dashboard.totalRevenue || 0), 'Tổng booking đã thanh toán'],
            ['Số booking', String(dashboard.bookingCount || 0), 'Tất cả booking trong phạm vi'],
            ['Phòng nổi bật', dashboard.mostBookedRoomName || 'Chưa có', `${dashboard.mostBookedRoomBookings || 0} booking`],
            ['Tỷ lệ đã đặt', `${dashboard.occupancyRate || 0}%`, `${dashboard.bookedRooms || 0} đã đặt · ${availableRooms} còn trống`]
        ].map(([label, value, caption]) => `
            <article class="bg-surface-container-lowest border border-outline-variant rounded-xl p-5">
                <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider">${label}</p>
                <strong class="block mt-2 text-headline-md font-headline-md text-primary">${value}</strong>
                <p class="text-body-sm text-on-surface-variant mt-1">${caption}</p>
            </article>
        `).join('');

        renderRevenuePoints(elements.dashboardRevenueByDay, dashboard.revenueByDay || []);
        renderRevenuePoints(elements.dashboardRevenueByMonth, dashboard.revenueByMonth || []);
    }

    function renderRevenuePoints(target, points) {
        if (!target) return;
        if (points.length === 0) {
            target.innerHTML = '<p class="text-on-surface-variant">Chưa có doanh thu.</p>';
            return;
        }

        const maxRevenue = Math.max(...points.map(point => point.revenue || 0), 1);
        target.innerHTML = points.map(point => `
            <div class="space-y-1">
                <div class="flex items-center justify-between gap-3 text-label-md font-label-md">
                    <span class="text-on-surface">${ui.escapeHtml(point.label)}</span>
                    <span class="text-primary">${ui.formatCurrency(point.revenue || 0)} · ${point.bookingCount || 0} booking</span>
                </div>
                <div class="h-2 bg-surface-container-high rounded-full overflow-hidden">
                    <div class="h-full bg-action-blue" style="width: ${Math.max(4, Math.round((point.revenue || 0) * 100 / maxRevenue))}%"></div>
                </div>
            </div>
        `).join('');
    }

    async function uploadImageFromInput(fileInput, urlInput, renderMessage) {
        const file = fileInput.files && fileInput.files[0];
        if (!file) {
            return;
        }

        renderMessage('Đang upload ảnh...', 'muted');

        try {
            const result = await api.uploadImage(file);
            urlInput.value = result.imageUrl;
            renderMessage('Upload ảnh thành công. URL đã được điền vào form.', 'success');
        } catch (err) {
            fileInput.value = '';
            renderMessage(err.message, 'error');
        }
    }

    // Reset user form
    function resetUserForm() {
        elements.userId.value = '';
        elements.userFullName.value = '';
        elements.userEmail.value = '';
        elements.userPassword.value = '';
        elements.userPasswordConfirm.value = '';
        elements.userRole.value = 'Customer';
        elements.userBalance.value = '100000000';
        elements.userSubmit.textContent = 'Tạo người dùng';
        elements.userMessage.hidden = true;
    }

    // Reset hotel form
    function resetHotelForm() {
        elements.hotelId.value = '';
        elements.hotelName.value = '';
        ui.populateCitySelect(elements.hotelCity, 'Da Nang', { includeAll: false });
        elements.hotelStarRating.value = '4';
        elements.hotelAddress.value = '';
        elements.hotelDescription.value = '';
        elements.hotelImageUrl.value = 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1200&q=80';
        elements.hotelImageFile.value = '';
        elements.hotelSubmit.textContent = 'Tạo khách sạn';
        elements.hotelMessage.hidden = true;
    }

    // Reset room form
    function resetRoomForm() {
        elements.roomTypeId.value = '';
        elements.roomName.value = '';
        elements.roomMaxGuests.value = '2';
        elements.roomTotalRooms.value = '3';
        elements.roomPrice.value = '1200000';
        elements.roomDescription.value = '';
        elements.roomImageUrl.value = 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80';
        elements.roomImageFile.value = '';
        elements.roomIsHidden.checked = false;
        elements.roomSubmit.textContent = 'Tạo loại phòng';
        elements.roomMessage.hidden = true;
    }

    // Render users
    function renderUsers() {
        if (!elements.adminUsersList) return;
        if (state.users.length === 0) {
            elements.adminUsersList.innerHTML = '<p class="text-on-surface-variant">Không có người dùng nào.</p>';
            return;
        }

        elements.adminUsersList.innerHTML = state.users.map(user => `
            <div class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2 mb-1">
                        <h3 class="text-headline-md font-headline-md text-on-surface truncate">${ui.escapeHtml(user.fullName)}</h3>
                        <span class="px-2 py-1 text-label-sm font-label-sm rounded-full ${user.role === 'Admin' ? 'bg-booking-blue text-on-primary' : 'bg-surface-container-high text-on-surface-variant'}">${ui.escapeHtml(user.role)}</span>
                    </div>
                    <p class="text-body-md text-on-surface-variant">${ui.escapeHtml(user.email)}</p>
                    <p class="text-label-sm font-label-sm text-primary mt-1">Số dư: ${ui.formatCurrency(user.balance || 0)}</p>
                    <p class="text-label-sm font-label-sm text-on-surface-variant mt-1">Ngày tạo: ${ui.formatDate(user.createdAt)}</p>
                </div>
                <div class="flex gap-2">
                    <button class="px-4 py-2 bg-success-green/10 text-success-green rounded-lg hover:bg-success-green hover:text-white transition-colors font-label-md text-label-sm" data-topup-user="${user.userId}">Nạp 10M</button>
                    <button class="px-4 py-2 bg-primary-container text-primary rounded-lg hover:bg-action-blue hover:text-white transition-colors font-label-md text-label-sm" data-edit-user="${user.userId}">Sửa</button>
                    <button class="px-4 py-2 bg-error-container text-error rounded-lg hover:bg-error hover:text-on-error transition-colors font-label-md text-label-sm" data-delete-user="${user.userId}">Xóa</button>
                </div>
            </div>
        `).join('');

        // Attach edit/delete handlers
        state.users.forEach(user => {
            const editBtn = document.querySelector(`[data-edit-user="${user.userId}"]`);
            const deleteBtn = document.querySelector(`[data-delete-user="${user.userId}"]`);
            const topUpBtn = document.querySelector(`[data-topup-user="${user.userId}"]`);

            if (topUpBtn) {
                topUpBtn.addEventListener('click', async () => {
                    try {
                        await api.topUpAdminUser(user.userId, {
                            amount: 10000000,
                            description: 'Nạp tiền test từ admin'
                        });
                        await Promise.all([loadUsers(), loadTransactions(), loadDashboard()]);
                        renderUserMessage('Đã nạp thêm 10 triệu vào tài khoản.', 'success');
                    } catch (err) {
                        renderUserMessage(err.message, 'error');
                    }
                });
            }

            if (editBtn) {
                editBtn.addEventListener('click', () => {
                    elements.userId.value = user.userId;
                    elements.userFullName.value = user.fullName;
                    elements.userEmail.value = user.email;
                    elements.userPassword.value = '';
                    elements.userPasswordConfirm.value = '';
                    elements.userRole.value = user.role;
                    elements.userBalance.value = user.balance || 0;
                    elements.userSubmit.textContent = 'Cập nhật người dùng';
                    elements.userMessage.hidden = true;
                });
            }

            if (deleteBtn) {
                deleteBtn.addEventListener('click', async () => {
                    if (!confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
                        return;
                    }
                    try {
                        await api.deleteAdminUser(user.userId);
                        await loadUsers();
                        renderUserMessage('Xóa người dùng thành công!', 'success');
                    } catch (err) {
                        renderUserMessage(err.message, 'error');
                    }
                });
            }
        });
    }

    // Render hotels
    function renderHotels() {
        if (!elements.adminHotelsList) return;
        if (state.hotels.length === 0) {
            elements.adminHotelsList.innerHTML = '<p class="text-on-surface-variant">Không có khách sạn nào.</p>';
            return;
        }

        elements.adminHotelsList.innerHTML = state.hotels.map(hotel => `
            <div class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col md:flex-row gap-4 cursor-pointer hover:border-primary transition-colors" data-hotel-id="${hotel.hotelId}">
                <img src="${ui.hotelImage(hotel.hotelId, hotel.mainImageUrl)}" alt="${ui.escapeHtml(hotel.name)}" class="w-full md:w-40 h-32 object-cover rounded-lg" />
                <div class="flex-1 min-w-0">
                    <div class="flex items-start justify-between gap-4">
                        <div class="flex-1 min-w-0">
                            <h3 class="text-headline-md font-headline-md text-on-surface">${ui.escapeHtml(hotel.name)}</h3>
                            <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider mb-1">${ui.escapeHtml(hotel.city)}</p>
                        </div>
                        <div class="flex gap-2 flex-shrink-0">
                            <button class="px-3 py-1 bg-secondary-container text-on-secondary-container rounded hover:bg-highlight-gold transition-colors font-label-md text-label-sm" data-manage-rooms="${hotel.hotelId}">Phòng</button>
                            <button class="px-3 py-1 bg-primary-container text-primary rounded hover:bg-action-blue hover:text-white transition-colors font-label-md text-label-sm" data-edit-hotel="${hotel.hotelId}">Sửa</button>
                        </div>
                    </div>
                    <p class="text-body-sm font-body-sm text-on-surface-variant mt-1 truncate">${ui.escapeHtml(hotel.address)}</p>
                    <p class="text-body-sm font-body-sm text-on-surface-variant mt-1 line-clamp-2">${ui.escapeHtml(hotel.description)}</p>
                    <div class="flex items-center gap-2 mt-2">
                        <span class="px-2 py-1 bg-highlight-gold text-secondary-container rounded-full font-label-md text-label-sm">⭐ ${hotel.starRating}</span>
                        <span class="text-label-md font-label-md text-on-surface-variant">${hotel.roomTypeCount} loại phòng</span>
                    </div>
                </div>
            </div>
        `).join('');

        // Attach hotel handlers
        state.hotels.forEach(hotel => {
            const hotelCard = document.querySelector(`[data-hotel-id="${hotel.hotelId}"]`);
            const editBtn = document.querySelector(`[data-edit-hotel="${hotel.hotelId}"]`);
            const roomsBtn = document.querySelector(`[data-manage-rooms="${hotel.hotelId}"]`);

            if (hotelCard) {
                hotelCard.addEventListener('click', async (e) => {
                    if (e.target.closest('button')) return;
                    state.selectedHotelId = hotel.hotelId;
                    elements.selectedHotelName.textContent = hotel.name;
                    elements.roomHotelId.value = hotel.hotelId;
                    elements.roomSection.classList.remove('hidden');
                    await loadRoomTypes(hotel.hotelId);
                    renderRoomTypes();
                });
            }

            if (editBtn) {
                editBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    elements.hotelId.value = hotel.hotelId;
                    elements.hotelName.value = hotel.name;
                    ui.populateCitySelect(elements.hotelCity, hotel.city, { includeAll: false });
                    elements.hotelStarRating.value = hotel.starRating;
                    elements.hotelAddress.value = hotel.address;
                    elements.hotelDescription.value = hotel.description;
                    elements.hotelImageUrl.value = hotel.mainImageUrl;
                    elements.hotelImageFile.value = '';
                    elements.hotelSubmit.textContent = 'Cập nhật khách sạn';
                    elements.hotelMessage.hidden = true;
                });
            }

            if (roomsBtn) {
                roomsBtn.addEventListener('click', async (e) => {
                    e.stopPropagation();
                    state.selectedHotelId = hotel.hotelId;
                    elements.selectedHotelName.textContent = hotel.name;
                    elements.roomHotelId.value = hotel.hotelId;
                    elements.roomSection.classList.remove('hidden');
                    await loadRoomTypes(hotel.hotelId);
                    renderRoomTypes();
                });
            }
        });
    }

    // Render room types
    function renderRoomTypes() {
        if (!elements.adminRoomsList) return;
        const hotelRooms = state.roomTypes.filter(rt => rt.hotelId === state.selectedHotelId);
        if (hotelRooms.length === 0) {
            elements.adminRoomsList.innerHTML = '<p class="text-on-surface-variant">Không có loại phòng nào cho khách sạn này.</p>';
            return;
        }

        elements.adminRoomsList.innerHTML = hotelRooms.map(roomType => `
            <div class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col md:flex-row gap-4">
                <img src="${ui.roomImage(roomType, state.selectedHotelId)}" alt="${ui.escapeHtml(roomType.name)}" class="w-full md:w-32 h-24 object-cover rounded-lg" />
                <div class="flex-1 min-w-0">
                    <div class="flex items-start justify-between gap-4">
                        <div class="flex-1 min-w-0">
                            <h3 class="text-headline-md font-headline-md text-on-surface">${ui.escapeHtml(roomType.name)}</h3>
                            <div class="flex flex-wrap gap-2 mt-2">
                                <span class="px-2 py-1 text-label-sm font-label-sm rounded-full ${roomStatusClass(roomType)}">${roomStatusLabel(roomType)}</span>
                                <span class="px-2 py-1 text-label-sm font-label-sm rounded-full bg-surface-container-high text-on-surface-variant">Đã đặt: ${roomType.bookedRooms || 0}</span>
                                <span class="px-2 py-1 text-label-sm font-label-sm rounded-full bg-surface-container-high text-on-surface-variant">Còn trống: ${roomType.availableRooms ?? roomType.totalRooms}</span>
                            </div>
                        </div>
                        <div class="flex gap-2 flex-shrink-0">
                            <button class="px-3 py-1 ${roomType.isHidden ? 'bg-success-green/10 text-success-green' : 'bg-error-container text-error'} rounded hover:bg-action-blue hover:text-white transition-colors font-label-md text-label-sm" data-toggle-room-hidden="${roomType.roomTypeId}">${roomType.isHidden ? 'Hiện' : 'Ẩn'}</button>
                            <button class="px-3 py-1 bg-primary-container text-primary rounded hover:bg-action-blue hover:text-white transition-colors font-label-md text-label-sm" data-edit-room="${roomType.roomTypeId}">Sửa</button>
                        </div>
                    </div>
                    <p class="text-body-sm font-body-sm text-on-surface-variant mt-1 truncate">${ui.escapeHtml(roomType.description)}</p>
                    <div class="flex items-center gap-4 mt-2">
                        <span class="text-label-md font-label-md text-primary">${ui.formatCurrency(roomType.pricePerNight)} / đêm</span>
                        <span class="text-label-md font-label-md text-on-surface-variant">${roomType.maxGuests} khách</span>
                        <span class="text-label-md font-label-md text-on-surface-variant">${roomType.totalRooms} phòng</span>
                    </div>
                </div>
            </div>
        `).join('');

        // Attach edit room handlers
        hotelRooms.forEach(roomType => {
            const editBtn = document.querySelector(`[data-edit-room="${roomType.roomTypeId}"]`);
            if (editBtn) {
                editBtn.addEventListener('click', () => {
                    elements.roomTypeId.value = roomType.roomTypeId;
                    elements.roomHotelId.value = roomType.hotelId;
                    elements.roomName.value = roomType.name;
                    elements.roomMaxGuests.value = roomType.maxGuests;
                    elements.roomTotalRooms.value = roomType.totalRooms;
                    elements.roomPrice.value = roomType.pricePerNight;
                    elements.roomDescription.value = roomType.description;
                    elements.roomImageUrl.value = roomType.imageUrl;
                    elements.roomImageFile.value = '';
                    elements.roomIsHidden.checked = Boolean(roomType.isHidden);
                    elements.roomSubmit.textContent = 'Cập nhật loại phòng';
                    elements.roomMessage.hidden = true;
                });
            }

            const toggleHiddenBtn = document.querySelector(`[data-toggle-room-hidden="${roomType.roomTypeId}"]`);
            if (toggleHiddenBtn) {
                toggleHiddenBtn.addEventListener('click', async () => {
                    try {
                        await api.updateAdminRoomType(roomType.roomTypeId, {
                            hotelId: roomType.hotelId,
                            name: roomType.name,
                            description: roomType.description,
                            maxGuests: roomType.maxGuests,
                            pricePerNight: roomType.pricePerNight,
                            totalRooms: roomType.totalRooms,
                            imageUrl: roomType.imageUrl,
                            isHidden: !roomType.isHidden
                        });
                        await loadRoomTypes(state.selectedHotelId);
                        renderRoomTypes();
                        renderRoomMessage(roomType.isHidden ? 'Đã hiện phòng trở lại.' : 'Đã ẩn phòng khỏi trang tìm kiếm.', 'success');
                    } catch (err) {
                        renderRoomMessage(err.message, 'error');
                    }
                });
            }
        });
    }

    function roomStatusLabel(roomType) {
        if (roomType.isHidden) {
            return 'Đã ẩn';
        }
        if ((roomType.availableRooms ?? roomType.totalRooms) <= 0) {
            return 'Hết phòng';
        }
        if ((roomType.bookedRooms || 0) > 0) {
            return 'Đã đặt';
        }
        return 'Còn trống';
    }

    function roomStatusClass(roomType) {
        if (roomType.isHidden) {
            return 'bg-error-container text-error';
        }
        if ((roomType.availableRooms ?? roomType.totalRooms) <= 0) {
            return 'bg-error-container text-error';
        }
        if ((roomType.bookedRooms || 0) > 0) {
            return 'bg-highlight-gold/20 text-on-surface';
        }
        return 'bg-success-green/10 text-success-green';
    }

    // Render bookings
    function renderBookings() {
        if (!elements.adminBookingsList) return;
        if (state.bookings.length === 0) {
            elements.adminBookingsList.innerHTML = '<p class="text-on-surface-variant">Không có booking nào.</p>';
            return;
        }

        elements.adminBookingsList.innerHTML = state.bookings.map(booking => `
            <div class="bg-surface-container-low border border-outline-variant rounded-xl p-4">
                <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-3">
                    <div>
                        <div class="flex items-center gap-2">
                            <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider">Booking #${ui.escapeHtml(booking.bookingCode)}</p>
                            <span class="px-2 py-1 text-label-sm font-label-sm rounded-full ${booking.status === 'Confirmed' ? 'bg-success-green/10 text-success-green' : booking.status === 'PendingPayment' ? 'bg-highlight-gold/10 text-secondary' : 'bg-error-container text-error'}">${ui.escapeHtml(ui.statusLabel(booking.status))}</span>
                            <span class="px-2 py-1 text-label-sm font-label-sm rounded-full ${booking.paymentStatus === 'Paid' ? 'bg-success-green/10 text-success-green' : 'bg-surface-container-high text-on-surface-variant'}">${ui.escapeHtml(ui.statusLabel(booking.paymentStatus))}</span>
                        </div>
                        <h3 class="text-headline-md font-headline-md text-on-surface mt-1">${ui.escapeHtml(booking.hotelName)} - ${ui.escapeHtml(booking.roomTypeName)}</h3>
                    </div>
                    <div class="text-right">
                        <p class="text-headline-md font-headline-md text-primary">${ui.formatCurrency(booking.totalPrice)}</p>
                        <p class="text-label-sm font-label-sm text-on-surface-variant">Ngày tạo: ${ui.formatDate(booking.createdAt)}</p>
                    </div>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 text-label-md font-label-md">
                    <div>
                        <p class="text-on-surface-variant">Khách</p>
                        <p class="text-on-surface">${ui.escapeHtml(booking.guestName)}</p>
                        <p class="text-on-surface-variant text-label-sm font-label-sm">${ui.escapeHtml(booking.guestEmail)}</p>
                    </div>
                    <div>
                        <p class="text-on-surface-variant">Ngày đến</p>
                        <p class="text-on-surface">${ui.formatDate(booking.checkIn)}</p>
                    </div>
                    <div>
                        <p class="text-on-surface-variant">Ngày đi</p>
                        <p class="text-on-surface">${ui.formatDate(booking.checkOut)}</p>
                    </div>
                    <div>
                        <p class="text-on-surface-variant">Số khách</p>
                        <p class="text-on-surface">${booking.guests}</p>
                    </div>
                </div>
            </div>
        `).join('');
    }

    function renderTransactions() {
        if (!elements.adminTransactionsList) return;
        if (state.transactions.length === 0) {
            elements.adminTransactionsList.innerHTML = '<p class="text-on-surface-variant">Chưa có giao dịch số dư.</p>';
            return;
        }

        elements.adminTransactionsList.innerHTML = state.transactions.map((item) => `
            <article class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col md:flex-row md:items-center md:justify-between gap-3">
                <div>
                    <p class="text-label-md font-label-md text-on-surface">${ui.escapeHtml(item.userEmail)}${item.bookingCode ? ` · ${ui.escapeHtml(item.bookingCode)}` : ''}</p>
                    <p class="text-body-sm text-on-surface-variant mt-1">${transactionTypeLabel(item.type)} · ${ui.escapeHtml(item.description || '')}</p>
                    <p class="text-label-sm font-label-sm text-on-surface-variant mt-1">${ui.formatDate(item.createdAt)}</p>
                </div>
                <div class="md:text-right">
                    <p class="text-headline-md font-headline-md ${item.amount >= 0 ? 'text-success-green' : 'text-error'}">${item.amount >= 0 ? '+' : ''}${ui.formatCurrency(item.amount || 0)}</p>
                    <p class="text-label-sm font-label-sm text-on-surface-variant">Sau giao dịch: ${ui.formatCurrency(item.balanceAfter || 0)}</p>
                </div>
            </article>
        `).join('');
    }

    function renderWithdrawals() {
        if (!elements.adminWithdrawalsList) return;
        if (state.withdrawals.length === 0) {
            elements.adminWithdrawalsList.innerHTML = '<p class="text-on-surface-variant">Chua co yeu cau rut tien.</p>';
            return;
        }

        elements.adminWithdrawalsList.innerHTML = state.withdrawals.map((item) => `
            <article class="bg-surface-container-low border border-outline-variant rounded-xl p-4 flex flex-col gap-4">
                <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-3">
                    <div>
                        <div class="flex items-center gap-2">
                            <p class="text-label-md font-label-md text-on-surface">${ui.escapeHtml(item.userEmail)}</p>
                            <span class="px-2 py-1 rounded-full text-label-sm font-label-sm ${withdrawalStatusClass(item.status)}">${withdrawalStatusLabel(item.status)}</span>
                        </div>
                        <p class="text-headline-md font-headline-md text-primary mt-1">${ui.formatCurrency(item.amount || 0)}</p>
                        <p class="text-body-sm text-on-surface-variant mt-1">${ui.escapeHtml(item.bankName)} - ${ui.escapeHtml(item.bankAccountNumber)} - ${ui.escapeHtml(item.bankAccountHolder)}</p>
                        ${item.note ? `<p class="text-body-sm text-on-surface-variant mt-1">Ghi chu: ${ui.escapeHtml(item.note)}</p>` : ''}
                        ${item.adminNote ? `<p class="text-body-sm text-on-surface-variant mt-1">Admin: ${ui.escapeHtml(item.adminNote)}</p>` : ''}
                    </div>
                    <div class="md:text-right">
                        <p class="text-label-sm font-label-sm text-on-surface-variant">Tao: ${ui.formatDate(item.requestedAt)}</p>
                        ${item.completedAt ? `<p class="text-label-sm font-label-sm text-on-surface-variant">Xu ly: ${ui.formatDate(item.completedAt)}</p>` : ''}
                    </div>
                </div>
                ${item.status === 'Pending' ? `
                    <div class="flex flex-wrap gap-2">
                        <button class="px-4 py-2 bg-success-green text-white rounded-lg hover:bg-success-green/90 transition-colors font-label-md text-label-sm" data-complete-withdrawal="${item.withdrawalRequestId}">Da chuyen tien, xac nhan</button>
                        <button class="px-4 py-2 bg-error-container text-error rounded-lg hover:bg-error hover:text-on-error transition-colors font-label-md text-label-sm" data-reject-withdrawal="${item.withdrawalRequestId}">Tu choi</button>
                    </div>
                ` : ''}
            </article>
        `).join('');

        state.withdrawals.forEach((item) => {
            const completeButton = document.querySelector(`[data-complete-withdrawal="${item.withdrawalRequestId}"]`);
            const rejectButton = document.querySelector(`[data-reject-withdrawal="${item.withdrawalRequestId}"]`);

            if (completeButton) {
                completeButton.addEventListener('click', async () => {
                    const confirmed = window.confirm(`Xac nhan da chuyen ${ui.formatCurrency(item.amount || 0)} cho ${item.userEmail}? So du nguoi dung se bi tru sau khi xac nhan.`);
                    if (!confirmed) return;
                    try {
                        await api.completeAdminWithdrawal(item.withdrawalRequestId, {
                            adminNote: 'Admin da chuyen tien thu cong'
                        });
                        await Promise.all([loadWithdrawals(), loadTransactions(), loadUsers()]);
                    } catch (err) {
                        alert(err.message);
                    }
                });
            }

            if (rejectButton) {
                rejectButton.addEventListener('click', async () => {
                    const confirmed = window.confirm(`Tu choi lenh rut ${ui.formatCurrency(item.amount || 0)} cua ${item.userEmail}?`);
                    if (!confirmed) return;
                    try {
                        await api.rejectAdminWithdrawal(item.withdrawalRequestId, {
                            adminNote: 'Admin tu choi lenh rut tien'
                        });
                        await loadWithdrawals();
                    } catch (err) {
                        alert(err.message);
                    }
                });
            }
        });
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

    function renderActivity() {
        if (!elements.adminActivityList) return;
        if (state.activity.length === 0) {
            elements.adminActivityList.innerHTML = '<p class="text-on-surface-variant">Chưa có hoạt động nào.</p>';
            return;
        }

        elements.adminActivityList.innerHTML = state.activity.map((item) => `
            <article class="bg-surface-container-low border border-outline-variant rounded-xl p-4">
                <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-3">
                    <div>
                        <p class="text-label-sm font-label-sm text-outline uppercase tracking-wider">${ui.escapeHtml(item.type)}</p>
                        <h3 class="text-headline-md font-headline-md text-on-surface mt-1">${ui.escapeHtml(item.title)}</h3>
                        <p class="text-body-md text-on-surface-variant mt-2">${ui.escapeHtml(item.message)}</p>
                        <p class="text-label-sm font-label-sm text-on-surface-variant mt-2">${ui.formatDate(item.createdAt)}</p>
                    </div>
                    ${item.linkUrl ? `<a class="px-4 py-2 rounded-lg bg-surface-container-high text-on-surface hover:bg-surface-container-highest transition-colors" href="${ui.escapeHtml(item.linkUrl)}">Mở</a>` : ''}
                </div>
            </article>
        `).join('');
    }

    function transactionTypeLabel(type) {
        return {
            AdminBalanceSet: 'Số dư ban đầu',
            AdminBalanceAdjustment: 'Admin chỉnh số dư',
            TestTopUp: 'Nạp tiền test',
            PayOsTopUp: 'Nạp tiền payOS',
            BookingPayment: 'Trừ tiền booking',
            OwnerBookingCredit: 'Cộng tiền cho chủ KS',
            BookingRefund: 'Hoàn tiền booking',
            OwnerBookingReversal: 'Trừ lại booking hủy',
            WithdrawalCompleted: 'Rút tiền'
        }[type] || type;
    }

    // Load functions
    async function loadDashboard() {
        try {
            state.dashboard = await api.getAdminDashboard();
            renderDashboard();
        } catch (err) {
            console.error('Failed to load dashboard:', err);
        }
    }

    async function loadUsers() {
        try {
            state.users = await api.getAdminUsers();
            renderUsers();
        } catch (err) {
            console.error('Failed to load users:', err);
        }
    }

    async function loadHotels() {
        try {
            state.hotels = await api.getAdminHotels();
            renderHotels();
        } catch (err) {
            console.error('Failed to load hotels:', err);
        }
    }

    async function loadRoomTypes(hotelId) {
        try {
            state.roomTypes = await api.getAdminRoomTypes(hotelId);
        } catch (err) {
            console.error('Failed to load room types:', err);
        }
    }

    async function loadBookings() {
        try {
            state.bookings = await api.getAdminBookings();
            renderBookings();
        } catch (err) {
            console.error('Failed to load bookings:', err);
        }
    }

    async function loadTransactions() {
        if (!isAdmin) return;
        try {
            state.transactions = await api.getAdminBalanceTransactions();
            renderTransactions();
        } catch (err) {
            console.error('Failed to load transactions:', err);
        }
    }

    async function loadWithdrawals() {
        if (!isAdmin) return;
        try {
            state.withdrawals = await api.getAdminWithdrawals();
            renderWithdrawals();
        } catch (err) {
            console.error('Failed to load withdrawals:', err);
        }
    }

    async function loadActivity() {
        if (!isAdmin) return;
        try {
            state.activity = await api.getAdminActivity();
            renderActivity();
        } catch (err) {
            console.error('Failed to load activity:', err);
        }
    }

    // Form handlers
    elements.userForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            fullName: elements.userFullName.value.trim(),
            email: elements.userEmail.value.trim(),
            password: elements.userPassword.value.trim(),
            role: elements.userRole.value,
            balance: Number(elements.userBalance.value || 0)
        };
        const passwordConfirm = elements.userPasswordConfirm.value.trim();

        if (data.password || passwordConfirm) {
            if (data.password !== passwordConfirm) {
                renderUserMessage('Mật khẩu nhập lại không khớp!', 'error');
                return;
            }
        }

        if (elements.userId.value) {
            // Update
            try {
                await api.updateAdminUser(parseInt(elements.userId.value), data);
                await Promise.all([loadUsers(), loadTransactions(), loadDashboard(), loadActivity()]);
                resetUserForm();
                renderUserMessage('Cập nhật người dùng thành công!', 'success');
            } catch (err) {
                renderUserMessage(err.message, 'error');
            }
        } else {
            // Create
            if (!data.password) {
                renderUserMessage('Vui lòng nhập mật khẩu!', 'error');
                return;
            }
            try {
                await api.createAdminUser(data);
                await Promise.all([loadUsers(), loadTransactions(), loadDashboard(), loadActivity()]);
                resetUserForm();
                renderUserMessage('Tạo người dùng thành công!', 'success');
            } catch (err) {
                renderUserMessage(err.message, 'error');
            }
        }
    });

    elements.hotelForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            name: elements.hotelName.value.trim(),
            city: elements.hotelCity.value.trim(),
            address: elements.hotelAddress.value.trim(),
            description: elements.hotelDescription.value.trim(),
            starRating: parseInt(elements.hotelStarRating.value),
            mainImageUrl: elements.hotelImageUrl.value.trim()
        };

        if (elements.hotelId.value) {
            // Update
            try {
                await api.updateAdminHotel(parseInt(elements.hotelId.value), data);
                await Promise.all([loadHotels(), loadDashboard()]);
                resetHotelForm();
                renderHotelMessage('Cập nhật khách sạn thành công!', 'success');
            } catch (err) {
                renderHotelMessage(err.message, 'error');
            }
        } else {
            // Create
            try {
                await api.createAdminHotel(data);
                await Promise.all([loadHotels(), loadDashboard()]);
                resetHotelForm();
                renderHotelMessage('Tạo khách sạn thành công!', 'success');
            } catch (err) {
                renderHotelMessage(err.message, 'error');
            }
        }
    });

    elements.roomForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            hotelId: parseInt(elements.roomHotelId.value),
            name: elements.roomName.value.trim(),
            description: elements.roomDescription.value.trim(),
            maxGuests: parseInt(elements.roomMaxGuests.value),
            pricePerNight: parseInt(elements.roomPrice.value),
            totalRooms: parseInt(elements.roomTotalRooms.value),
            imageUrl: elements.roomImageUrl.value.trim(),
            isHidden: elements.roomIsHidden.checked
        };

        if (!Number.isFinite(data.pricePerNight) || data.pricePerNight < minRoomPrice) {
            renderRoomMessage(`Gia moi dem toi thieu la ${ui.formatCurrency(minRoomPrice)}.`, 'error');
            return;
        }

        if (elements.roomTypeId.value) {
            // Update
            try {
                await api.updateAdminRoomType(parseInt(elements.roomTypeId.value), data);
                await Promise.all([loadRoomTypes(state.selectedHotelId), loadDashboard()]);
                renderRoomTypes();
                resetRoomForm();
                renderRoomMessage('Cập nhật loại phòng thành công!', 'success');
            } catch (err) {
                renderRoomMessage(err.message, 'error');
            }
        } else {
            // Create
            try {
                await api.createAdminRoomType(data);
                await Promise.all([loadRoomTypes(state.selectedHotelId), loadDashboard()]);
                renderRoomTypes();
                resetRoomForm();
                renderRoomMessage('Tạo loại phòng thành công!', 'success');
            } catch (err) {
                renderRoomMessage(err.message, 'error');
            }
        }
    });

    // Reset buttons
    elements.resetUserForm.addEventListener('click', resetUserForm);
    elements.resetHotelForm.addEventListener('click', resetHotelForm);
    elements.resetRoomForm.addEventListener('click', resetRoomForm);

    elements.hotelImageFile.addEventListener('change', () => {
        uploadImageFromInput(elements.hotelImageFile, elements.hotelImageUrl, renderHotelMessage);
    });

    elements.roomImageFile.addEventListener('change', () => {
        uploadImageFromInput(elements.roomImageFile, elements.roomImageUrl, renderRoomMessage);
    });

    // Tab buttons
    elements.tabDashboard.addEventListener('click', () => switchTab('dashboard'));
    elements.tabUsers.addEventListener('click', () => switchTab('users'));
    elements.tabHotels.addEventListener('click', () => switchTab('hotels'));
    elements.tabBookings.addEventListener('click', () => switchTab('bookings'));
    elements.tabTransactions.addEventListener('click', () => switchTab('transactions'));
    elements.tabWithdrawals.addEventListener('click', () => switchTab('withdrawals'));
    elements.tabActivity.addEventListener('click', () => switchTab('activity'));

    // Close room section
    elements.closeRoomSection.addEventListener('click', () => {
        state.selectedHotelId = null;
        elements.roomSection.classList.add('hidden');
    });

    if (!isAdmin) {
        elements.tabUsers.hidden = true;
        elements.tabTransactions.hidden = true;
        elements.tabWithdrawals.hidden = true;
        elements.tabActivity.hidden = true;
        switchTab('dashboard');
        Promise.all([
            loadDashboard(),
            loadHotels(),
            loadBookings()
        ]);
    } else {
        switchTab('dashboard');
        Promise.all([
            loadDashboard(),
            loadUsers(),
            loadHotels(),
            loadBookings(),
            loadTransactions(),
            loadWithdrawals(),
            loadActivity()
        ]);
    }
})();

(function () {
    const api = window.OmniStayApi;
    const ui = window.OmniStaySession;
    const session = ui.requireAuth({ role: 'Admin' });
    if (!session) {
        return;
    }

    ui.hydrateNav('admin');

    // State
    let state = {
        hotels: [],
        roomTypes: [],
        bookings: [],
        users: [],
        selectedHotelId: null
    };

    // Elements
    const elements = {
        // Tabs
        tabUsers: document.getElementById('tab-users'),
        tabHotels: document.getElementById('tab-hotels'),
        tabBookings: document.getElementById('tab-bookings'),
        tabContentUsers: document.getElementById('tab-content-users'),
        tabContentHotels: document.getElementById('tab-content-hotels'),
        tabContentBookings: document.getElementById('tab-content-bookings'),

        // Users
        userForm: document.getElementById('userForm'),
        userId: document.getElementById('userId'),
        userFullName: document.getElementById('userFullName'),
        userEmail: document.getElementById('userEmail'),
        userPassword: document.getElementById('userPassword'),
        userRole: document.getElementById('userRole'),
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
        roomSubmit: document.getElementById('roomSubmit'),
        resetRoomForm: document.getElementById('resetRoomForm'),
        roomMessage: document.getElementById('roomMessage'),
        adminRoomsList: document.getElementById('adminRoomsList'),
        selectedHotelName: document.getElementById('selectedHotelName'),
        closeRoomSection: document.getElementById('closeRoomSection'),

        // Bookings
        adminBookingsList: document.getElementById('adminBookingsList')
    };

    // Tab switching
    function switchTab(tab) {
        // Reset all tabs
        [elements.tabUsers, elements.tabHotels, elements.tabBookings].forEach(t => {
            t.classList.remove('border-booking-blue', 'text-booking-blue');
            t.classList.add('border-transparent', 'text-on-surface-variant');
        });

        // Hide all content
        [elements.tabContentUsers, elements.tabContentHotels, elements.tabContentBookings].forEach(c => {
            c.classList.add('hidden');
        });

        // Activate selected tab
        if (tab === 'users') {
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

    // Reset user form
    function resetUserForm() {
        elements.userId.value = '';
        elements.userFullName.value = '';
        elements.userEmail.value = '';
        elements.userPassword.value = '';
        elements.userRole.value = 'Customer';
        elements.userSubmit.textContent = 'Tạo người dùng';
        elements.userMessage.hidden = true;
    }

    // Reset hotel form
    function resetHotelForm() {
        elements.hotelId.value = '';
        elements.hotelName.value = '';
        elements.hotelCity.value = 'Da Nang';
        elements.hotelStarRating.value = '4';
        elements.hotelAddress.value = '';
        elements.hotelDescription.value = '';
        elements.hotelImageUrl.value = 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1200&q=80';
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
                    <p class="text-label-sm font-label-sm text-on-surface-variant mt-1">Ngày tạo: ${ui.formatDate(user.createdAt)}</p>
                </div>
                <div class="flex gap-2">
                    <button class="px-4 py-2 bg-primary-container text-primary rounded-lg hover:bg-action-blue hover:text-white transition-colors font-label-md text-label-sm" data-edit-user="${user.userId}">Sửa</button>
                    <button class="px-4 py-2 bg-error-container text-error rounded-lg hover:bg-error hover:text-on-error transition-colors font-label-md text-label-sm" data-delete-user="${user.userId}">Xóa</button>
                </div>
            </div>
        `).join('');

        // Attach edit/delete handlers
        state.users.forEach(user => {
            const editBtn = document.querySelector(`[data-edit-user="${user.userId}"]`);
            const deleteBtn = document.querySelector(`[data-delete-user="${user.userId}"]`);

            if (editBtn) {
                editBtn.addEventListener('click', () => {
                    elements.userId.value = user.userId;
                    elements.userFullName.value = user.fullName;
                    elements.userEmail.value = user.email;
                    elements.userPassword.value = '';
                    elements.userRole.value = user.role;
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
                    elements.hotelCity.value = hotel.city;
                    elements.hotelStarRating.value = hotel.starRating;
                    elements.hotelAddress.value = hotel.address;
                    elements.hotelDescription.value = hotel.description;
                    elements.hotelImageUrl.value = hotel.mainImageUrl;
                    elements.hotelSubmit.textContent = 'Cập nhật khách sạn';
                    elements.hotelMessage.hidden = true;
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
                        </div>
                        <div class="flex gap-2 flex-shrink-0">
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
                    elements.roomSubmit.textContent = 'Cập nhật loại phòng';
                    elements.roomMessage.hidden = true;
                });
            }
        });
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

    // Load functions
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

    // Form handlers
    elements.userForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            fullName: elements.userFullName.value.trim(),
            email: elements.userEmail.value.trim(),
            password: elements.userPassword.value.trim(),
            role: elements.userRole.value
        };

        if (elements.userId.value) {
            // Update
            try {
                await api.updateAdminUser(parseInt(elements.userId.value), data);
                await loadUsers();
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
                await loadUsers();
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
                await loadHotels();
                resetHotelForm();
                renderHotelMessage('Cập nhật khách sạn thành công!', 'success');
            } catch (err) {
                renderHotelMessage(err.message, 'error');
            }
        } else {
            // Create
            try {
                await api.createAdminHotel(data);
                await loadHotels();
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
            imageUrl: elements.roomImageUrl.value.trim()
        };

        if (elements.roomTypeId.value) {
            // Update
            try {
                await api.updateAdminRoomType(parseInt(elements.roomTypeId.value), data);
                await loadRoomTypes(state.selectedHotelId);
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
                await loadRoomTypes(state.selectedHotelId);
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

    // Tab buttons
    elements.tabUsers.addEventListener('click', () => switchTab('users'));
    elements.tabHotels.addEventListener('click', () => switchTab('hotels'));
    elements.tabBookings.addEventListener('click', () => switchTab('bookings'));

    // Close room section
    elements.closeRoomSection.addEventListener('click', () => {
        state.selectedHotelId = null;
        elements.roomSection.classList.add('hidden');
    });

    // Load initial data
    Promise.all([
        loadUsers(),
        loadHotels(),
        loadBookings()
    ]);
})();

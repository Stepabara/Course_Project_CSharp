document.addEventListener('DOMContentLoaded', function() {
    var dropdown = document.getElementById('headerUserDropdown');
    var menuBtn = document.getElementById('headerUserMenuBtn');
    var menu = document.getElementById('headerDropdownMenu');
    var profileModal = document.getElementById('profileModal');
    var profileContent = document.getElementById('profileModalContent');
    var profileCloseBtn = document.getElementById('profileModalClose');
    var favoritesModal = document.getElementById('favoritesModal');
    var favoritesContent = document.getElementById('favoritesModalContent');

    if (!dropdown || !menuBtn || !menu) return;

    // ═══ Dropdown toggle ═══
    menuBtn.addEventListener('click', function(e) {
        e.stopPropagation();
        e.preventDefault();
        console.log('profile-modal.js: Menu button clicked');
        dropdown.classList.toggle('open');
        console.log('profile-modal.js: Dropdown open =', dropdown.classList.contains('open'));
    });

    document.addEventListener('click', function(e) {
        if (dropdown && dropdown.classList.contains('open')) {
            if (!dropdown.contains(e.target)) {
                dropdown.classList.remove('open');
            }
        }
    });

    // ═══ Profile Modal ═══
    window.openProfileModal = function() {
        console.log('profile-modal.js: openProfileModal called');
        if (dropdown) dropdown.classList.remove('open');
        if (profileModal) {
            profileModal.classList.add('open');
            document.body.style.overflow = 'hidden';
            loadProfile();
        }
    };

    if (profileCloseBtn) {
        profileCloseBtn.addEventListener('click', function() {
            profileModal.classList.remove('open');
            document.body.style.overflow = '';
        });
    }
    if (profileModal) {
        profileModal.addEventListener('click', function(e) {
            if (e.target === profileModal) {
                profileModal.classList.remove('open');
                document.body.style.overflow = '';
            }
        });
    }

    // ═══ Favorites Modal ═══
    window.openFavoritesModal = function() {
        console.log('profile-modal.js: openFavoritesModal called');
        if (dropdown) dropdown.classList.remove('open');
        if (favoritesModal) {
            favoritesModal.classList.add('open');
            document.body.style.overflow = 'hidden';
            loadFavorites();
        }
    };

    window.closeFavoritesModal = function() {
        if (favoritesModal) {
            favoritesModal.classList.remove('open');
            document.body.style.overflow = '';
        }
    };

    if (favoritesModal) {
        favoritesModal.addEventListener('click', function(e) {
            if (e.target === favoritesModal) {
                favoritesModal.classList.remove('open');
                document.body.style.overflow = '';
            }
        });
    }

    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            if (profileModal && profileModal.classList.contains('open')) { profileModal.classList.remove('open'); document.body.style.overflow = ''; }
            if (favoritesModal && favoritesModal.classList.contains('open')) { favoritesModal.classList.remove('open'); document.body.style.overflow = ''; }
        }
    });

    // ═══ Load Profile Data ═══
    function getAntiForgeryToken() {
        var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenEl ? tokenEl.value : '';
    }

    function loadProfile() {
        if (!profileContent) return;
        profileContent.innerHTML = '<div class="text-center py-4"><div class="spinner-border text-dark"></div></div>';
        fetch('/Profile/GetProfileData')
            .then(function(r) { return r.json(); })
            .then(function(d) {
                if (!d || d.error) {
                    profileContent.innerHTML = '<div class="text-center py-4 text-muted"><p>Не удалось загрузить профиль</p><a href="/Account/Login" class="btn btn-dark btn-sm">Войти</a></div>';
                    return;
                }
                var av = d.avatarPath
                    ? '<img src="' + d.avatarPath + '" class="profile-modal-avatar" />'
                    : '<div class="profile-modal-avatar">' + (d.name || '?').charAt(0).toUpperCase() + '</div>';
                var company = d.role === 'Partner'
                    ? '<div class="mb-3"><label class="form-label">Компания</label><input type="text" class="form-control" id="pComp" value="' + (d.companyName || '') + '" /></div>'
                    : '';
                var token = getAntiForgeryToken();
                var html = '<div class="profile-modal-body">';
                html += '<div class="text-center mb-3">' + av + '<h4 class="mt-2">' + d.name + '</h4><span class="badge bg-dark">' + d.role + '</span></div>';
                html += '<form id="pForm">';
                html += '<input type="hidden" name="__RequestVerificationToken" value="' + token + '" />';
                html += '<div class="mb-3"><label class="form-label">Имя</label><input type="text" class="form-control" id="pName" value="' + d.name + '" required /></div>';
                html += '<div class="mb-3"><label class="form-label">Email</label><input type="email" class="form-control" id="pMail" value="' + d.email + '" required /></div>';
                html += company;
                html += '<div class="mb-3"><label class="form-label">Новый пароль</label><input type="password" class="form-control" id="pPass" placeholder="Минимум 6 символов" /></div>';
                html += '<div class="d-flex gap-2"><button type="submit" class="btn btn-dark flex-fill"><i class="fas fa-save"></i> Сохранить</button><button type="button" class="btn btn-outline-dark" id="pCancel">Отмена</button></div>';
                html += '</form><hr /><div class="text-center"><small class="text-muted">На сайте с ' + new Date(d.registeredAt).toLocaleDateString('ru-RU') + '</small></div></div>';
                profileContent.innerHTML = html;

                document.getElementById('pForm').addEventListener('submit', function(e) {
                    e.preventDefault();
                    var body = '__RequestVerificationToken=' + encodeURIComponent(document.querySelector('#pForm input[name="__RequestVerificationToken"]').value)
                        + '&name=' + encodeURIComponent(document.getElementById('pName').value.trim())
                        + '&email=' + encodeURIComponent(document.getElementById('pMail').value.trim())
                        + '&companyName=' + encodeURIComponent(document.getElementById('pComp') ? document.getElementById('pComp').value.trim() : '');
                    var p = document.getElementById('pPass').value;
                    if (p) body += '&password=' + encodeURIComponent(p);
                    fetch('/Profile/Update', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: body })
                        .then(function(r) { if (r.ok) { alert('Профиль обновлён! ✅'); profileModal.classList.remove('open'); document.body.style.overflow = ''; } else alert('Ошибка'); })
                        .catch(function() { alert('Ошибка'); });
                });
                var cancelBtn = document.getElementById('pCancel');
                if (cancelBtn) cancelBtn.addEventListener('click', function() { profileModal.classList.remove('open'); document.body.style.overflow = ''; });
            })
            .catch(function() {
                profileContent.innerHTML = '<div class="text-center py-4 text-muted"><p>Ошибка загрузки</p></div>';
            });
    }

    // ═══ Load Favorites ═══
    function loadFavorites() {
        if (!favoritesContent) return;
        favoritesContent.innerHTML = '<div class="text-center py-4"><div class="spinner-border text-dark"></div></div>';
        fetch('/Route/GetMyRoutes')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                console.log('loadFavorites raw data:', JSON.stringify(data));
                var routes = Array.isArray(data) ? data : (data.routes || data.Results || []);
                console.log('loadFavorites parsed routes:', routes);
                var favRoutes = routes.filter(function(r) { return r.isFavorite || r.IsFavorite; });
                console.log('favRoutes:', favRoutes);
                if (!favRoutes.length) {
                    favoritesContent.innerHTML =
                        '<div class="favorites-empty">' +
                            '<i class="fas fa-star"></i>' +
                            '<h5>Нет избранных маршрутов</h5>' +
                            '<p class="text-muted">Нажмите на звёздочку в Моих маршрутах, чтобы добавить сюда</p>' +
                        '</div>';
                    return;
                }
                var html = '<div class="favorites-header"><h5><i class="fas fa-star"></i> Избранные маршруты</h5><span class="badge bg-dark">' + favRoutes.length + '</span></div>';
                html += '<div class="favorites-list">';
                favRoutes.forEach(function(r) {
                    var routeId = r.id || r.Id || r.ID;
                    console.log('Route:', r, 'routeId:', routeId);
                    var dateStr = r.createdAt ? new Date(r.createdAt).toLocaleDateString('ru-RU') : '';
                    html +=
                        '<div class="favorite-item" id="fav-route-' + routeId + '">' +
                            '<div class="favorite-item-info">' +
                                '<div class="favorite-item-name"><i class="fas fa-route"></i> ' + r.name + '</div>' +
                                '<div class="favorite-item-meta">' +
                                    '<span><i class="fas fa-map-marker-alt"></i> ' + (r.pointCount || 0) + ' точек</span>' +
                                    '<span><i class="fas fa-calendar"></i> ' + dateStr + '</span>' +
                                '</div>' +
                            '</div>' +
                            '<div class="favorite-item-actions">' +
                                '<button class="btn btn-sm btn-outline-dark" onclick="shareFavRoute(' + routeId + ')" title="Поделиться"><i class="fas fa-share-alt"></i></button>' +
                                '<button class="btn btn-sm btn-outline-danger" onclick="removeFavorite(' + routeId + ')" title="Удалить"><i class="fas fa-trash"></i></button>' +
                            '</div>' +
                        '</div>';
                });
                html += '</div>';
                favoritesContent.innerHTML = html;
            })
            .catch(function() {
                favoritesContent.innerHTML = '<div class="favorites-empty"><i class="fas fa-exclamation-circle"></i><h5>Ошибка загрузки</h5></div>';
            });
    }

    // ═══ Share Route with coordinates ═══
    window.shareRoute = function(routeId) {
        console.log('shareRoute called with id:', routeId);
        fetch('/Route/GetRouteData?id=' + routeId)
            .then(function(r) {
                console.log('GetRouteData response:', r.status);
                return r.json();
            })
            .then(function(data) {
                console.log('GetRouteData data:', data);
                if (data && data.points && data.points.length > 0) {
                    var coords = data.points.map(function(p) { return p.latitude + ',' + p.longitude; }).join('~');
                    var yandexUrl = 'https://yandex.ru/maps/?rtext=' + coords + '&rtt=auto';
                    if (navigator.clipboard) {
                        navigator.clipboard.writeText(yandexUrl).then(function() {
                            alert('Ссылка скопирована! ✅');
                        });
                    } else {
                        prompt('Скопируйте ссылку:', yandexUrl);
                    }
                } else {
                    alert('Не удалось получить точки маршрута. Data: ' + JSON.stringify(data));
                }
            })
            .catch(function(e) { console.error('shareRoute error:', e); alert('Ошибка: ' + e); });
    };

    window.removeFavorite = function(routeId) {
        if (!confirm('Удалить маршрут из избранного?')) return;
        fetch('/Route/Delete?id=' + routeId, { method: 'DELETE' })
            .then(function(r) {
                if (r.ok) {
                    var el = document.getElementById('fav-route-' + routeId);
                    if (el) el.remove();
                } else {
                    alert('Ошибка при удалении');
                }
            })
            .catch(function() { alert('Ошибка'); });
    };
});

(function() {
    var modal = document.getElementById('profileModal');
    var content = document.getElementById('profileModalContent');
    var btn = document.getElementById('headerProfileBtn');
    if (!modal || !content || !btn) return;

    btn.addEventListener('click', function () {
        modal.classList.add('open');
        loadProfile();
    });

    modal.addEventListener('click', function (e) {
        if (e.target === modal) modal.classList.remove('open');
    });

    var closeBtn = document.getElementById('profileModalClose');
    if (closeBtn) closeBtn.addEventListener('click', function () { modal.classList.remove('open'); });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') modal.classList.remove('open');
    });

    function getAntiForgeryToken() {
        var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenEl ? tokenEl.value : '';
    }

    function loadProfile() {
        content.innerHTML = '<div class="text-center py-4"><div class="spinner-border text-dark"></div></div>';
        fetch('/Profile/GetProfileData')
            .then(function (r) { return r.json(); })
            .then(function (d) {
                if (!d || d.error) {
                    content.innerHTML = '<div class="text-center py-4 text-muted"><p>Не удалось загрузить профиль</p><a href="/Account/Login" class="btn btn-dark btn-sm">Войти</a></div>';
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
                content.innerHTML = html;

                document.getElementById('pForm').addEventListener('submit', function (e) {
                    e.preventDefault();
                    var body = '__RequestVerificationToken=' + encodeURIComponent(document.querySelector('#pForm input[name="__RequestVerificationToken"]').value)
                        + '&name=' + encodeURIComponent(document.getElementById('pName').value.trim())
                        + '&email=' + encodeURIComponent(document.getElementById('pMail').value.trim())
                        + '&companyName=' + encodeURIComponent(document.getElementById('pComp') ? document.getElementById('pComp').value.trim() : '');
                    var p = document.getElementById('pPass').value;
                    if (p) body += '&password=' + encodeURIComponent(p);
                    fetch('/Profile/Update', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: body })
                        .then(function (r) { if (r.ok) { alert('Профиль обновлён! ✅'); modal.classList.remove('open'); } else alert('Ошибка'); })
                        .catch(function () { alert('Ошибка'); });
                });
                document.getElementById('pCancel').addEventListener('click', function () { modal.classList.remove('open'); });
            })
            .catch(function () {
                content.innerHTML = '<div class="text-center py-4 text-muted"><p>Ошибка загрузки</p></div>';
            });
    }
})();

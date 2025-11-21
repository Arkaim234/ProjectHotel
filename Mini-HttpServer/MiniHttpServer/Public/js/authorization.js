document.addEventListener('DOMContentLoaded', function () {
    // ===== Переключение "Сотрудникам / Туристам" =====
    window.samo = {
        changeLoginType: function (type) {
            var loginCaption = document.getElementById('login_caption');
            var passwordCaption = document.getElementById('password_caption');
            var loginInput = document.getElementById('login');
            var passwordInput = document.getElementById('password');
            var forgotPasswordLink = document.getElementById('forgot_password');
            var logonTypeField = document.getElementById('logon_type');

            if (!loginCaption || !passwordCaption || !loginInput || !passwordInput || !logonTypeField) {
                return;
            }

            if (type === 'claim') {
                // Туристам
                if (loginCaption.dataset.claimCaption) {
                    loginCaption.textContent = loginCaption.dataset.claimCaption;
                }
                if (passwordCaption.dataset.claimPswdCaption) {
                    passwordCaption.textContent = passwordCaption.dataset.claimPswdCaption;
                }

                loginInput.placeholder = 'Например: 123456';
                passwordInput.placeholder = 'Номер паспорта без серии или дата в формате 31.12.2000';

                if (forgotPasswordLink) {
                    forgotPasswordLink.classList.add('hidden');
                }

                logonTypeField.value = 'claim';
            } else {
                // Сотрудникам
                if (loginCaption.dataset.loginCaption) {
                    loginCaption.textContent = loginCaption.dataset.loginCaption;
                }
                if (passwordCaption.dataset.loginPswdCaption) {
                    passwordCaption.textContent = passwordCaption.dataset.loginPswdCaption;
                }

                loginInput.placeholder = 'Логин';
                passwordInput.placeholder = 'Введите пароль';

                if (forgotPasswordLink) {
                    forgotPasswordLink.classList.remove('hidden');
                }

                logonTypeField.value = 'login';
            }
        }
    };

    var loginRadio = document.getElementById('login_radio');
    var claimRadio = document.getElementById('claim_radio');

    if (loginRadio) {
        loginRadio.addEventListener('change', function () {
            if (loginRadio.checked) {
                window.samo.changeLoginType('login');
            }
        });
    }

    if (claimRadio) {
        claimRadio.addEventListener('change', function () {
            if (claimRadio.checked) {
                window.samo.changeLoginType('claim');
            }
        });
    }

    // Инициализация при загрузке: по умолчанию "Сотрудникам"
    if (loginRadio && loginRadio.checked) {
        window.samo.changeLoginType('login');
    }

    // ===== Отправка формы на /auth/login (только для Сотрудников) =====
    var form = document.getElementById('loginForm');
    var notify = document.getElementById('notify-container');
    var loginBox = document.getElementById('loginbox');

    // исходный URL из макета (data-orig-url="/cl_refer_person")
    var origUrl = loginBox ? loginBox.getAttribute('data-orig-url') : '';

    function getTouristUrl() {
        // если в data-orig-url уже полный URL – используем его
        if (origUrl && /^https?:\/\//i.test(origUrl)) {
            return origUrl;
        }

        // иначе считаем, что это относительный путь боевого сайта
        var path = origUrl || '/cl_refer_person';
        return 'https://online.onetouch.travel' + path;
    }

    function showError(message) {
        if (!notify) return;
        notify.innerText = message;
        notify.style.color = 'red';
        notify.style.display = 'block';
    }

    function clearError() {
        if (!notify) return;
        notify.innerText = '';
        notify.style.display = 'none';
    }

    if (form) {
        form.addEventListener('submit', function (e) {
            var loginInput = document.getElementById('login');
            var passwordInput = document.getElementById('password');
            var logonTypeField = document.getElementById('logon_type');

            var login = loginInput ? loginInput.value.trim() : '';
            var password = passwordInput ? passwordInput.value.trim() : '';
            var logonType = logonTypeField ? logonTypeField.value : 'login';

            // ==== ВЕТКА "Туристам" ====
            if (logonType === 'claim') {
                // Никакого fetch к нашему /auth/login.
                // Отдаём форму внешнему сервису, как было на оригинальном сайте.
                var touristUrl = getTouristUrl();

                // подставляем action и позволяем браузеру отправить форму
                form.action = touristUrl;
                form.target = '_self';
                // ВАЖНО: не вызываем preventDefault, чтобы submit сработал штатно
                return;
            }

            // ==== ВЕТКА "Сотрудникам" ====
            e.preventDefault();
            clearError();

            if (!login || !password) {
                showError('Заполните логин и пароль');
                return;
            }

            var payload = {
                Login: login,
                Password: password,
                IsEmployee: true,          // здесь всегда сотрудники
                ClaimValue: ''             // для сотрудников не используется
            };

            fetch('/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;charset=utf-8'
                },
                credentials: 'include',
                body: JSON.stringify(payload)
            })
                .then(function (response) {
                    return response.json()
                        .catch(function () { return {}; })
                        .then(function (data) {
                            return { ok: response.ok, data: data };
                        });
                })
                .then(function (result) {
                    var ok = result.ok;
                    var data = result.data || {};

                    if (!ok) {
                        showError(data.message || 'Ошибка авторизации');
                        return;
                    }

                    console.log('Auth success response:', data);

                    // Пытаемся вытащить токен из разных вариантов ответа
                    var token =
                        data.token ||
                        data.Token ||
                        (data.data && (data.data.token || data.data.Token));

                    if (!token) {
                        // если вдруг бэк не вернул token, всё равно пробуем перейти,
                        // опираясь на куку (если она была выставлена)
                        console.warn('Сервер не вернул токен в JSON, продолжаем по куке');
                        window.location.href = '/admin';
                        return;
                    }

                    // кладём токен в cookie, чтобы админка цепляла сессию
                    try {
                        document.cookie = 'token=' + encodeURIComponent(token) + '; path=/';
                    } catch (e) {
                        console.warn('Не удалось сохранить токен в cookie:', e);
                    }

                    // переход в админ-панель (для сотрудников)
                    window.location.href = '/admin';
                })
                .catch(function (err) {
                    console.error(err);
                    showError('Ошибка соединения с сервером');
                });
        });
    }
});

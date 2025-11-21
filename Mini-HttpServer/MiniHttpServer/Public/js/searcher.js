/* ===================================================
   ЗАГРУЗКА ЛОГИКИ ПОИСКА (ТОЛЬКО МОДАЛКИ/ПОИСК)
   Нижние фильтры (города/категории/отели/питание)
   инициализируются в filters.js
=================================================== */
document.addEventListener("DOMContentLoaded", () => {
    loadCityModal();
    loadCountryModal();
    initNightsModal();
    initTouristsModal();
    initSearchSubmit();
});

/* ===================================================
    Показ / скрытие дополнительных фильтров
=================================================== */
function toggleExtraFilters() {
    const box = document.getElementById("ot-filters-extra");
    const text = document.getElementById("ot-filters-toggle-text");

    if (box.classList.contains("open")) {
        box.classList.remove("open");
        box.style.display = "none";
        text.textContent = "Показать дополнительные поля";
    } else {
        box.classList.add("open");
        box.style.display = "grid";
        text.textContent = "Скрыть дополнительные поля";
    }
}

/* ===================================================
        Локальный поиск по спискам
=================================================== */
function filterLocalList(id, value) {
    value = (value || "").toLowerCase();
    document.querySelectorAll(`#${id} label`).forEach(label => {
        label.style.display = label.textContent.toLowerCase().includes(value)
            ? "flex"
            : "none";
    });
}

/* ===================================================
                Нижний блок — города
=================================================== */
async function loadCities(countryId) {
    try {
        // если страна выбрана – берём города только этой страны
        const url = countryId
            ? `/api/cities/by-country?countryId=${encodeURIComponent(countryId)}`
            : "/api/cities";

        const res = await fetch(url);
        if (!res.ok) throw new Error("HTTP " + res.status);

        const list = await res.json();

        const box = document.getElementById("ot-city-list");
        if (!box) return;

        box.innerHTML = list.map(x => `
            <label>
                <input type="checkbox" value="${x.id}">
                ${x.name}
            </label>
        `).join("");
    } catch (e) {
        console.error("Ошибка загрузки городов:", e);
    }
}

/* ===================================================
               Нижний блок — категории
=================================================== */
async function loadCategories() {
    try {
        const res = await fetch("/api/categories");
        if (!res.ok) throw new Error("HTTP " + res.status);

        const list = await res.json();

        const box = document.getElementById("ot-type-list");
        if (!box) return;

        box.innerHTML = list.map(c => `
            <label>
                <input type="checkbox" value="${c.id}">
                ${c.name}
            </label>
        `).join("");
    } catch (e) {
        console.error("Ошибка категорий:", e);
    }
}

/* ===================================================
               Нижний блок — отели
=================================================== */
async function loadHotelsList() {
    try {
        const res = await fetch("/api/hotels/all");
        if (!res.ok) throw new Error("HTTP " + res.status);

        const list = await res.json();
        renderHotelsList(list);
    } catch (e) {
        console.error("Ошибка отелей:", e);
    }
}

function renderHotelsList(list) {
    const box = document.getElementById("ot-hotels-list");
    if (!box) return;

    box.innerHTML = list.map(h => `
        <label>
            <input type="checkbox" value="${h.id}">
            ${h.name}
        </label>
    `).join("");
}

/* ===================================================
               Нижний блок — типы питания
=================================================== */
async function loadMealPlans() {
    try {
        const res = await fetch("/api/mealplans");
        if (!res.ok) throw new Error("HTTP " + res.status);

        const list = await res.json();

        const box = document.getElementById("ot-meal-list");
        if (!box) return;

        box.innerHTML = list.map(m => `
            <label>
                <input type="checkbox" value="${m.code}">
                ${m.code}
            </label>
        `).join("");
    } catch (e) {
        console.error("Ошибка питания:", e);
    }
}

/* ===================================================
                    МОДАЛКА "Вылет из"
=================================================== */
function loadCityModal() {
    const modal = document.getElementById("modal-city");
    const input = document.querySelector("[data-field='from']");

    if (!modal || !input) return;

    // стартовое валидное значение
    if (!input.dataset.lastValue && input.value.trim()) {
        input.dataset.lastValue = input.value.trim();
        if (input.dataset.value) {
            input.dataset.lastId = input.dataset.value;
        }
    }

    // фильтрация ТОЛЬКО когда юзер меняет текст
    input.addEventListener("input", () => {
        filterModalList(modal, input.value);
    });

    // при blur — если фигня, откат к прошлому валидному
    input.addEventListener("blur", () => {
        const current = input.value.trim().toLowerCase();
        const listWrap = modal.querySelector(".ot-msf-modal-list");

        let matchedItem = null;

        if (listWrap) {
            listWrap.querySelectorAll(".ot-msf-modal-item").forEach(item => {
                const text = item.textContent.trim().toLowerCase();
                if (text === current) matchedItem = item;
            });
        }

        if (matchedItem) {
            input.value = matchedItem.textContent.trim();
            input.dataset.value = matchedItem.dataset.id;
            input.dataset.lastValue = input.value;
            input.dataset.lastId = matchedItem.dataset.id;
        } else if (input.dataset.lastValue) {
            input.value = input.dataset.lastValue;
            if (input.dataset.lastId) {
                input.dataset.value = input.dataset.lastId;
            } else {
                delete input.dataset.value;
            }
        }
    });

    input.addEventListener("click", () => {
        fetch("/api/cities/russia")
            .then(r => r.json())
            .then(cities => {
                modal.innerHTML = `
                    <div class="ot-msf-modal-list">
                        ${cities.map(c => `
                            <div class="ot-msf-modal-item" data-id="${c.id}">
                                ${c.name}
                            </div>
                        `).join("")}
                    </div>
                `;

                // просто показываем модалку, БЕЗ предварительной фильтрации
                showModal(modal, input);

                modal.querySelectorAll(".ot-msf-modal-item")
                    .forEach(item => item.addEventListener("click", () => {
                        const text = item.textContent.trim();
                        const id = item.dataset.id;

                        input.value = text;
                        input.dataset.value = id;

                        input.dataset.lastValue = text;
                        input.dataset.lastId = id;

                        modal.classList.remove("open");
                    }));
            })
            .catch(err => console.error("Ошибка загрузки городов вылета:", err));
    });
}

/* ===================================================
                    МОДАЛКА "Куда?"
=================================================== */
function loadCountryModal() {
    const modal = document.getElementById("modal-country");
    const input = document.querySelector("[data-field='to']");

    if (!modal || !input) return;

    if (!input.dataset.lastValue && input.value.trim()) {
        input.dataset.lastValue = input.value.trim();
        if (input.dataset.value) {
            input.dataset.lastId = input.dataset.value;
        }
    }

    input.addEventListener("input", () => {
        filterModalList(modal, input.value);
    });

    input.addEventListener("blur", () => {
        const current = input.value.trim().toLowerCase();
        const listWrap = modal.querySelector(".ot-msf-modal-list");

        let matchedItem = null;

        if (listWrap) {
            listWrap.querySelectorAll(".ot-msf-modal-item").forEach(item => {
                const text = item.textContent.trim().toLowerCase();
                if (text === current) matchedItem = item;
            });
        }

        if (matchedItem) {
            input.value = matchedItem.textContent.trim();
            input.dataset.value = matchedItem.dataset.id;
            input.dataset.lastValue = input.value;
            input.dataset.lastId = matchedItem.dataset.id;
        } else if (input.dataset.lastValue) {
            input.value = input.dataset.lastValue;
            if (input.dataset.lastId) {
                input.dataset.value = input.dataset.lastId;
            } else {
                delete input.dataset.value;
            }
        }
    });

    input.addEventListener("click", () => {
        fetch("/api/countries")
            .then(r => r.json())
            .then(countries => {
                modal.innerHTML = `
                    <div class="ot-msf-modal-list">
                        ${countries
                        .filter(x => x.id !== 1)
                        .map(c => `
                                <div class="ot-msf-modal-item" data-id="${c.id}">
                                    ${c.name}
                                </div>
                            `).join("")}
                    </div>
                `;

                // просто открыть со всеми вариантами
                showModal(modal, input);

                modal.querySelectorAll(".ot-msf-modal-item")
                    .forEach(item => item.addEventListener("click", () => {
                        const countryId = item.dataset.id;

                        input.value = item.textContent.trim();
                        input.dataset.value = countryId;
                        input.dataset.lastValue = input.value;
                        input.dataset.lastId = countryId;

                        modal.classList.remove("open");

                        // обновляем нижний фильтр "Город" под выбранную страну
                        loadCities(countryId);
                    }));
            })
            .catch(err => console.error("Ошибка загрузки стран:", err));
    });
}

/* ===================================================
                    МОДАЛКА "Ночей"
=================================================== */
function initNightsModal() {
    const modal = document.getElementById("modal-nights");
    const btn = document.querySelector("[data-field='nights']");

    if (!modal || !btn) return;

    btn.addEventListener("click", () => {
        // читаем текущие значения из текста кнопки "6 - 9"
        let from = 6, to = 9;
        const match = btn.value.match(/(\d+)\s*-\s*(\d+)/);
        if (match) {
            from = parseInt(match[1], 10);
            to = parseInt(match[2], 10);
        }

        modal.innerHTML = `
            <div class="ot-msf-modal-row">
                <div class="ot-msf-modal-label">Ночей от</div>
                <div class="ot-msf-modal-spinner">
                    <span class="ot-msf-modal-value" id="nightsFromValue">${from}</span>
                    <div class="ot-msf-modal-arrows">
                        <button type="button" class="ot-arrow up" data-target="from">▲</button>
                        <button type="button" class="ot-arrow down" data-target="from">▼</button>
                    </div>
                </div>
            </div>
            <div class="ot-msf-modal-row">
                <div class="ot-msf-modal-label">Ночей до</div>
                <div class="ot-msf-modal-spinner">
                    <span class="ot-msf-modal-value" id="nightsToValue">${to}</span>
                    <div class="ot-msf-modal-arrows">
                        <button type="button" class="ot-arrow up" data-target="to">▲</button>
                        <button type="button" class="ot-arrow down" data-target="to">▼</button>
                    </div>
                </div>
            </div>
        `;

        showModal(modal, btn);

        const state = { from, to };

        function syncView() {
            modal.querySelector("#nightsFromValue").textContent = state.from;
            modal.querySelector("#nightsToValue").textContent = state.to;
            btn.value = `${state.from} - ${state.to}`;
        }

        modal.querySelectorAll(".ot-arrow").forEach(control => {
            control.addEventListener("click", () => {
                const target = control.dataset.target;
                const dir = control.classList.contains("up") ? 1 : -1;

                if (target === "from") {
                    state.from = Math.max(1, state.from + dir);
                    if (state.from > state.to) state.to = state.from;
                } else {
                    state.to = Math.max(state.from, state.to + dir);
                }

                syncView();
            });
        });
    });
}

/* ===================================================
                   МОДАЛКА "Туристы"
=================================================== */
function initTouristsModal() {
    const modal = document.getElementById("modal-tourists");
    const btn = document.querySelector("[data-field='tourists']");

    if (!modal || !btn) return;

    btn.addEventListener("click", () => {
        modal.innerHTML = `
            <div class="ot-msf-tourists-row">
                <div class="ot-msf-tourists-label">Взрослые</div>
                <div class="ot-msf-tourists-controls">
                    <button type="button" class="ot-circle-btn minusA">−</button>
                    <span class="ot-msf-count countA">1</span>
                    <button type="button" class="ot-circle-btn plusA">+</button>
                </div>
            </div>

            <div class="ot-msf-tourists-row">
                <div class="ot-msf-tourists-label">Дети</div>
                <div class="ot-msf-tourists-controls">
                    <button type="button" class="ot-circle-btn minusC">−</button>
                    <span class="ot-msf-count countC">0</span>
                    <button type="button" class="ot-circle-btn plusC">+</button>
                </div>
            </div>
        `;

        showModal(modal, btn);

        let adults = modal.querySelector(".countA");
        let childs = modal.querySelector(".countC");

        modal.querySelector(".plusA").onclick = () => {
            adults.textContent = Number(adults.textContent) + 1;
            updateText();
        };
        modal.querySelector(".minusA").onclick = () => {
            adults.textContent = Math.max(1, Number(adults.textContent) - 1);
            updateText();
        };

        modal.querySelector(".plusC").onclick = () => {
            childs.textContent = Number(childs.textContent) + 1;
            updateText();
        };
        modal.querySelector(".minusC").onclick = () => {
            childs.textContent = Math.max(0, Number(childs.textContent) - 1);
            updateText();
        };

        function updateText() {
            btn.value = `${adults.textContent} взр. / ${childs.textContent} реб.`;
        }

        // сразу синхронизируем подпись
        updateText();
    });
}

/* ===================================================
           Общая функция показа модалки
=================================================== */
function showModal(modal, trigger) {
    if (!modal || !trigger) return;

    // запомним, от какого инпута открывали модалку
    modal._trigger = trigger;

    // сначала закрываем все остальные
    document.querySelectorAll(".ot-msf-modal").forEach(x => x.classList.remove("open"));

    const rect = trigger.getBoundingClientRect();
    modal.style.top = rect.bottom + window.scrollY + "px";
    modal.style.left = rect.left + window.scrollX + "px";

    modal.classList.add("open");
}

/* ===================================================
   Глобально закрываем модалки при клике вне
=================================================== */
document.addEventListener("click", (e) => {
    const openModals = document.querySelectorAll(".ot-msf-modal.open");

    openModals.forEach(modal => {
        const trigger = modal._trigger;

        const clickInsideModal = modal.contains(e.target);
        const clickOnTrigger = trigger && (trigger === e.target || trigger.contains(e.target));

        // если кликнули НЕ по модалке и НЕ по её инпуту — закрываем
        if (!clickInsideModal && !clickOnTrigger) {
            modal.classList.remove("open");
        }
    });
});

/* ===================================================
                AJAX — кнопка "Найти"
=================================================== */
function initSearchSubmit() {
    const form = document.querySelector(".ot-msf-form");
    if (!form) return;

    form.addEventListener("submit", async e => {
        e.preventDefault();

        const fromInput = document.querySelector("[data-field='from']");
        const toInput = document.querySelector("[data-field='to']");
        const dateBtn = document.querySelector(".ot-date-button");
        const nightsInput = document.querySelector("[data-field='nights']");
        const touristsInp = document.querySelector("[data-field='tourists']");

        // id города вылета / страны
        const fromId = fromInput && fromInput.dataset ? fromInput.dataset.value || "" : "";
        const toId = toInput && toInput.dataset ? toInput.dataset.value || "" : "";

        // даты " 11.11.25 - 14.11.25"
        let dateFrom = "";
        let dateTo = "";
        if (dateBtn) {
            const m = dateBtn.textContent.match(/(\d{2}\.\d{2}\.\d{2}).+?(\d{2}\.\d{2}\.\d{2})/);
            if (m) {
                dateFrom = m[1];
                dateTo = m[2];
            }
        }

        // ночи "6 - 9"
        let nightsFrom = "";
        let nightsTo = "";
        if (nightsInput && nightsInput.value) {
            const nm = nightsInput.value.match(/(\d+)\s*-\s*(\d+)/);
            if (nm) {
                nightsFrom = nm[1];
                nightsTo = nm[2];
            }
        }

        // туристы "2 взр. / 0 реб."
        let adults = "";
        let childs = "";
        if (touristsInp && touristsInp.value) {
            const tm = touristsInp.value.match(/(\d+)\s*взр\.\s*\/\s*(\d+)\s*реб\./i);
            if (tm) {
                adults = tm[1];
                childs = tm[2];
            }
        }

        // нижние фильтры (чекбоксы)
        const selectedCities = Array.from(
            document.querySelectorAll("#ot-city-list input:checked")
        ).map(i => i.value);

        const selectedCategories = Array.from(
            document.querySelectorAll("#ot-type-list input:checked")
        ).map(i => i.value);

        const selectedHotels = Array.from(
            document.querySelectorAll("#ot-hotels-list input:checked")
        ).map(i => i.value);

        const selectedMeals = Array.from(
            document.querySelectorAll("#ot-meal-list input:checked")
        ).map(i => i.value);

        // собираем query string
        const params = new URLSearchParams();

        if (fromId) params.set("fromCityId", fromId);
        if (toId) params.set("countryId", toId);
        if (dateFrom) params.set("dateFrom", dateFrom);
        if (dateTo) params.set("dateTo", dateTo);
        if (nightsFrom) params.set("nightsFrom", nightsFrom);
        if (nightsTo) params.set("nightsTo", nightsTo);
        if (adults) params.set("adults", adults);
        if (childs) params.set("children", childs);

        if (selectedCities.length) params.set("cityIds", selectedCities.join(","));
        if (selectedCategories.length) params.set("categoryIds", selectedCategories.join(","));
        if (selectedHotels.length) params.set("hotelIds", selectedHotels.join(","));
        if (selectedMeals.length) params.set("mealCodes", selectedMeals.join(","));

        try {
            const response = await fetch("/api/hotels/search?" + params.toString(), {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("HTTP " + response.status);
            }

            const data = await response.json();

            // выводим результаты и скроллим вверх 
            showResults(data);
            cutPageToSearchbar();

        } catch (err) {
            console.error("Ошибка при поиске туров:", err);
            alert("Ошибка при поиске туров. Попробуйте ещё раз позже.");
        }
    });
}

/* ===================================================
            Обрезка страницы до поисковика
=================================================== */
function cutPageToSearchbar() {
    // сам блок с поиском (тот, что с фильтрами)
    const searchWrap = document.querySelector(".js-ot-msf");
    if (!searchWrap) {
        window.scrollTo({ top: 0, behavior: "smooth" });
        return;
    }

    // основной контент страницы
    const content = document.querySelector(".cmn-l-content_main");
    if (content) {
        // ищем ближайший общий wrapper вокруг поисковика
        let wrapper = searchWrap.closest(".cmn-l-block-wrap") || searchWrap;

        // прячем все блоки, которые идут ПЕРЕД поисковым блоком
        let prev = wrapper.previousElementSibling;
        while (prev) {
            const el = prev;
            prev = prev.previousElementSibling;
            el.style.display = "none"; 
        }
    }

    // если есть какой-то верхний баннер — тоже уберём
    const banner = document.querySelector(".page-top-banner");
    if (banner) banner.style.display = "none";

    // скроллим наверх, чтобы поисковик оказался сразу под хедером
    window.scrollTo({ top: 0, behavior: "smooth" });
}

/* ===================================================
                 Отрисовка результата
=================================================== */
// Экраним текст, чтобы не словить XSS
function escapeHtml(str) {
    if (!str) return "";
    return String(str).replace(/[&<>"']/g, ch => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#039;"
    }[ch]));
}

// Красиво форматируем цену: 24500 -> 24 500
function formatPrice(value) {
    if (value == null) return "";
    return String(value).replace(/\B(?=(\d{3})+(?!\d))/g, " ");
}

// Подстановка {{Placeholders}} в шаблон
function renderTemplate(template, data) {
    return template.replace(/{{(\w+)}}/g, (match, key) => {
        if (data[key] == null) return "";
        return escapeHtml(String(data[key]));
    });
}

/* ===================================================
                    РЕЗУЛЬТАТЫ ПОИСКА
=================================================== */
function showResults(list) {
    const container = document.getElementById("ot-search-results");
    if (!container) return;

    const tplEl = document.getElementById("tour-card-template");
    if (!tplEl) {
        console.warn("Не найден шаблон #tour-card-template");
        return;
    }
    const templateHtml = tplEl.innerHTML.trim();

    container.innerHTML = "";

    if (!Array.isArray(list) || list.length === 0) {
        container.innerHTML = "<p>По вашему запросу ничего не найдено.</p>";
        return;
    }

    // Заголовок "Найдено туров: N"
    const title = document.createElement("h2");
    title.textContent = `Найдено туров: ${list.length}`;
    container.appendChild(title);

    // Дата заезда и ночи берём из формы
    const dateBtn = document.querySelector(".ot-date-button");
    const nightsInput = document.querySelector("[data-field='nights']");

    let checkInText = "";
    let nightsText = "";

    if (dateBtn) {
        const m = dateBtn.textContent.match(/(\d{2}\.\d{2}\.\d{2})/);
        if (m) checkInText = m[1];
    }

    if (nightsInput && nightsInput.value) {
        nightsText = nightsInput.value; // "6 - 9" 
    }

    list.forEach(hotel => {
        // hotel — это объект API:
        // { id, name, city, price, slug, photoUrl, mealPlans }

        const meal =
            Array.isArray(hotel.mealPlans) && hotel.mealPlans.length
                ? hotel.mealPlans[0]
                : "";

        const dataForTemplate = {
            HotelUrl: `/hotels/${hotel.slug}`,       
            PhotoUrl: hotel.photoUrl || "/images/no-photo.png",
            HotelName: hotel.name || "",
            StarsHtml: "",                            
            CityName: hotel.city || "",
            RegionName: "",                            
            CheckInDate: checkInText,
            Nights: nightsText,
            RoomName: "",                            
            MealPlan: meal,
            Price: formatPrice(hotel.price)
        };

        const html = renderTemplate(templateHtml, dataForTemplate);

        const wrapper = document.createElement("div");
        wrapper.innerHTML = html;

        // В шаблоне один корневой .tour-card, его и добавляем
        const card = wrapper.firstElementChild;
        if (card) {
            container.appendChild(card);
        }
    });
}

/* ===================================================
   ВСПОМОГАТЕЛЬНАЯ ФУНКЦИЯ: фильтрация + "ничего не найдено"
=================================================== */
function filterModalList(modal, searchValue) {
    const listWrap = modal.querySelector(".ot-msf-modal-list");
    if (!listWrap) return;

    const items = listWrap.querySelectorAll(".ot-msf-modal-item");
    const q = (searchValue || "").trim().toLowerCase();

    let visibleCount = 0;

    items.forEach(item => {
        const text = item.textContent.toLowerCase();
        const match = !q || text.includes(q);
        item.style.display = match ? "block" : "none";
        if (match) visibleCount++;
    });

    // Блок "ничего не найдено"
    let emptyEl = listWrap.querySelector(".ot-msf-modal-empty");
    if (!emptyEl) {
        emptyEl = document.createElement("div");
        emptyEl.className = "ot-msf-modal-empty";
        emptyEl.textContent = "Ничего не найдено";
        emptyEl.style.padding = "8px 10px";
        emptyEl.style.color = "#666";
        listWrap.appendChild(emptyEl);
    }
    emptyEl.style.display = visibleCount === 0 ? "block" : "none";
}

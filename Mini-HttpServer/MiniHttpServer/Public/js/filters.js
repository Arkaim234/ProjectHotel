document.addEventListener("DOMContentLoaded", () => {
    loadCities();
    loadCategories();
    loadHotelsList();
    loadMealPlans();
});

/* ---------------------------------------------------
      Показ / скрытие дополнительных фильтров
---------------------------------------------------- */
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

/* ---------------------------------------------------
            Локальный поиск внутри списка
---------------------------------------------------- */
function filterLocalList(id, value) {
    value = value.toLowerCase();
    document.querySelectorAll(`#${id} label`).forEach(label => {
        let text = label.textContent.toLowerCase();
        label.style.display = text.includes(value) ? "flex" : "none";
    });
}

/* ---------------------------------------------------
                 Загрузка городов
---------------------------------------------------- */
async function loadCities() {
    try {
        const res = await fetch("/api/cities");
        const list = await res.json();

        document.getElementById("ot-city-list").innerHTML = list.map(x => `
            <label>
                <input type="checkbox" value="${x.id}">
                ${x.name}
            </label>
        `).join("");
    } catch (e) {
        console.error("Ошибка загрузки городов:", e);
    }
}

/* ---------------------------------------------------
              Загрузка категорий
---------------------------------------------------- */
async function loadCategories() {
    try {
        const res = await fetch("/api/categories");
        const list = await res.json();

        document.getElementById("ot-type-list").innerHTML = list.map(c => `
            <label>
                <input type="checkbox" value="${c.id}">
                ${c.name}
            </label>
        `).join("");
    } catch (e) {
        console.error("Ошибка загрузки категорий:", e);
    }
}

/* ---------------------------------------------------
                Первая загрузка всех отелей
---------------------------------------------------- */
async function loadHotelsList() {
    try {
        const res = await fetch("/api/hotels/all");
        const list = await res.json();

        renderHotelsList(list);
    } catch (e) {
        console.error("Ошибка загрузки списка отелей:", e);
    }
}

/* ---------------------------------------------------
    Рендер списка отелей 
---------------------------------------------------- */
function renderHotelsList(list) {
    const box = document.getElementById("ot-hotels-list");
    box.innerHTML = list.map(h => `
        <label>
            <input type="checkbox" value="${h.id}">
            ${h.name}
        </label>
    `).join("");
}

/* ---------------------------------------------------
      Живой поиск отелей по имени
---------------------------------------------------- */
async function searchHotelsByName(value) {
    value = value.trim();

    try {
        const res = await fetch("/api/hotels/search?name=" + encodeURIComponent(value));
        const list = await res.json();

        renderHotelsList(list);
    } catch (e) {
        console.error("Ошибка поиска отелей:", e);
    }
}

/* ---------------------------------------------------
                Загрузка типов питания
---------------------------------------------------- */
async function loadMealPlans() {
    try {
        const res = await fetch("/api/mealplans");
        const list = await res.json();

        document.getElementById("ot-meal-list").innerHTML = list.map(m => `
            <label>
                <input type="checkbox" value="${m.code}">
                ${m.code}
            </label>
        `).join("");
    } catch (e) {
        console.error("Ошибка загрузки питания:", e);
    }
}

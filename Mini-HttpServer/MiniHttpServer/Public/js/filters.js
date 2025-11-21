// Инициализация нижних фильтров (города/категории/отели/питание)

document.addEventListener("DOMContentLoaded", () => {
    // поле "Куда?"
    const toInput = document.querySelector("[data-field='to']");

    // если у "Куда?" уже есть выбранная страна (data-value),
    // берём города только этой страны, иначе — все города
    const initialCountryId =
        toInput && toInput.dataset && toInput.dataset.value
            ? toInput.dataset.value
            : null;

    // нижние чекбоксы — города / категории / отели / питание
    loadCities(initialCountryId);
    loadCategories();
    loadHotelsList();
    loadMealPlans();
});

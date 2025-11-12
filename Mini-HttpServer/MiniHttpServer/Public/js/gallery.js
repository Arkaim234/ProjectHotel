document.addEventListener('DOMContentLoaded', function() {
  const initGallery = () => {
    const galleryMain = document.querySelector('.photo-block__main');
    if (!galleryMain) return;

    // Собираем все изображения
    const images = [];
    const imageLinks = document.querySelectorAll('.photoslider-block__img-link');

    imageLinks.forEach(link => {
      const img = link.querySelector('img');
      if (img && img.src) {
        images.push({
          src: img.src,
          alt: img.alt || 'Галерея изображений'
        });
      }
    });

    if (images.length === 0) return;

    // Максимум 3 миниатюры в основном слайдере
    const visibleCount = Math.min(images.length, 3);
    
    // Создаем контейнер для новой галереи
    const galleryCard = document.createElement('div');
    galleryCard.className = 'gallery-card';

    // Основное изображение
    const mainContainer = document.createElement('div');
    mainContainer.className = 'gallery-main';

    const mainImg = document.createElement('img');
    mainImg.className = 'gallery-main-img';
    mainImg.src = images[0].src;
    mainImg.alt = images[0].alt;
    mainImg.style.cursor = 'pointer';

    // Кнопки навигации
    const prevBtn = document.createElement('button');
    prevBtn.className = 'gallery-nav-btn gallery-nav-prev';
    prevBtn.innerHTML = '&larr;';
    prevBtn.setAttribute('aria-label', 'Предыдущее изображение');

    const nextBtn = document.createElement('button');
    nextBtn.className = 'gallery-nav-btn gallery-nav-next';
    nextBtn.innerHTML = '&rarr;';
    nextBtn.setAttribute('aria-label', 'Следующее изображение');

    // Индикаторы (только 3)
    const indicators = document.createElement('div');
    indicators.className = 'gallery-indicators';

    for (let i = 0; i < visibleCount; i++) {
      const indicator = document.createElement('div');
      indicator.className = 'gallery-indicator';
      indicator.dataset.index = i;
      if (i === 0) indicator.classList.add('active');
      indicators.appendChild(indicator);
    }

    // Миниатюры (только 3)
    const thumbnails = document.createElement('div');
    thumbnails.className = 'gallery-thumbnails';

    for (let i = 0; i < visibleCount; i++) {
      const thumb = document.createElement('img');
      thumb.src = images[i].src;
      thumb.className = 'gallery-thumbnail';
      thumb.dataset.index = i;
      if (i === 0) thumb.classList.add('active');
      thumbnails.appendChild(thumb);
    }

    // Собираем компоненты
    mainContainer.appendChild(mainImg);
    mainContainer.appendChild(prevBtn);
    mainContainer.appendChild(nextBtn);
    mainContainer.appendChild(indicators);
    galleryCard.appendChild(mainContainer);
    galleryCard.appendChild(thumbnails);
    galleryMain.parentNode.replaceChild(galleryCard, galleryMain);

    // Управление основной галереей
    let currentIndex = 0;

    const updateMainGallery = (index) => {
      currentIndex = index;
      mainImg.src = images[index].src;
      mainImg.alt = images[index].alt;

      // Обновляем индикаторы
      document.querySelectorAll('.gallery-indicator').forEach(indicator => {
        indicator.classList.toggle('active', parseInt(indicator.dataset.index) === index);
      });

      // Обновляем миниатюры
      document.querySelectorAll('.gallery-thumbnail').forEach(thumb => {
        thumb.classList.toggle('active', parseInt(thumb.dataset.index) === index);
      });
    };

    // Обработчики событий для основной галереи (с остановкой всплытия)
    prevBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      const newIndex = (currentIndex - 1 + visibleCount) % visibleCount;
      updateMainGallery(newIndex);
    });

    nextBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      const newIndex = (currentIndex + 1) % visibleCount;
      updateMainGallery(newIndex);
    });

    indicators.addEventListener('click', (e) => {
      if (e.target.classList.contains('gallery-indicator')) {
        e.stopPropagation();
        const index = parseInt(e.target.dataset.index);
        updateMainGallery(index);
      }
    });

    thumbnails.addEventListener('click', (e) => {
      if (e.target.classList.contains('gallery-thumbnail')) {
        e.stopPropagation();
        const index = parseInt(e.target.dataset.index);
        updateMainGallery(index);
      }
    });

    // --- МОДАЛЬНОЕ ОКНО ---
    let modal = null;
    let modalInitialized = false;
    let modalCurrentIndex = 0;

    const createModal = () => {
      modal = document.createElement('div');
      modal.className = 'gallery-modal';

      const modalImg = document.createElement('img');
      modalImg.className = 'gallery-modal-img';
      modalImg.src = images[0].src;

      const modalNav = document.createElement('div');
      modalNav.className = 'gallery-modal-nav';

      const modalPrev = document.createElement('button');
      modalPrev.className = 'gallery-modal-prev';
      modalPrev.innerHTML = '&larr;';
      modalPrev.setAttribute('aria-label', 'Предыдущее изображение');

      const modalNext = document.createElement('button');
      modalNext.className = 'gallery-modal-next';
      modalNext.innerHTML = '&rarr;';
      modalNext.setAttribute('aria-label', 'Следующее изображение');

      const modalClose = document.createElement('span');
      modalClose.className = 'gallery-modal-close';
      modalClose.innerHTML = '&times;';
      modalClose.setAttribute('aria-label', 'Закрыть галерею');

      const modalThumbnails = document.createElement('div');
      modalThumbnails.className = 'gallery-thumbnails-modal';

      // Создаем миниатюры для модального окна (все фото)
      images.forEach((img, index) => {
        const thumb = document.createElement('img');
        thumb.src = img.src;
        thumb.alt = img.alt || 'Миниатюра';
        thumb.className = 'gallery-thumbnail-modal';
        thumb.dataset.index = index;
        if (index === 0) thumb.classList.add('active');
        modalThumbnails.appendChild(thumb);
      });

      modalNav.appendChild(modalPrev);
      modalNav.appendChild(modalNext);
      modal.appendChild(modalClose);
      modal.appendChild(modalImg);
      modal.appendChild(modalNav);
      modal.appendChild(modalThumbnails);
      document.body.appendChild(modal);

      // Настройка событий для модального окна
      modalClose.addEventListener('click', closeModal);
      
      modalPrev.addEventListener('click', () => {
        modalCurrentIndex = (modalCurrentIndex - 1 + images.length) % images.length;
        updateModalGallery(modalCurrentIndex);
      });

      modalNext.addEventListener('click', () => {
        modalCurrentIndex = (modalCurrentIndex + 1) % images.length;
        updateModalGallery(modalCurrentIndex);
      });

      modalThumbnails.addEventListener('click', (e) => {
        if (e.target.classList.contains('gallery-thumbnail-modal')) {
          const index = parseInt(e.target.dataset.index);
          updateModalGallery(index);
        }
      });

      modal.addEventListener('click', (e) => {
        if (e.target === modal) closeModal();
      });

      document.addEventListener('keydown', handleModalKeydown);
    };

    const updateModalGallery = (index) => {
      modalCurrentIndex = index;
      const modalImg = modal.querySelector('.gallery-modal-img');
      modalImg.src = images[index].src;
      modalImg.alt = images[index].alt;

      // Обновляем миниатюры в модальном окне
      modal.querySelectorAll('.gallery-thumbnail-modal').forEach((thumb, i) => {
        thumb.classList.toggle('active', i === index);
      });

      // Прокручиваем к активной миниатюре
      const activeThumb = modal.querySelector(`.gallery-thumbnail-modal[data-index="${index}"]`);
      if (activeThumb) {
        activeThumb.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
      }
    };

    const openModal = (index) => {
      if (!modalInitialized) {
        createModal();
        modalInitialized = true;
      }
      
      updateModalGallery(index);
      modal.style.display = 'flex';
      document.body.style.overflow = 'hidden'; // Запрещаем прокрутку страницы
    };

    const closeModal = () => {
      if (modal) {
        modal.style.display = 'none';
        document.body.style.overflow = ''; // Возвращаем прокрутку
      }
    };

    const handleModalKeydown = (e) => {
      if (modal.style.display !== 'flex') return;
      
      if (e.key === 'ArrowLeft') {
        e.preventDefault();
        modalCurrentIndex = (modalCurrentIndex - 1 + images.length) % images.length;
        updateModalGallery(modalCurrentIndex);
      } else if (e.key === 'ArrowRight') {
        e.preventDefault();
        modalCurrentIndex = (modalCurrentIndex + 1) % images.length;
        updateModalGallery(modalCurrentIndex);
      } else if (e.key === 'Escape') {
        e.preventDefault();
        closeModal();
      }
    };

    // Открываем модальное окно только при клике на само изображение
    mainImg.addEventListener('click', () => {
      openModal(currentIndex);
    });
  };

  initGallery();
});
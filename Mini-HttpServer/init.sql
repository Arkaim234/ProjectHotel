BEGIN;

-- ============================================
-- Схема БД (чистый запуск)
-- ============================================

DROP TABLE IF EXISTS public.hotel_category_map CASCADE;
DROP TABLE IF EXISTS public.hotel_mealplans CASCADE;
DROP TABLE IF EXISTS public.hoteldescriptions CASCADE;
DROP TABLE IF EXISTS public.hotelplaceinfos CASCADE;
DROP TABLE IF EXISTS public.hotelservices CASCADE;
DROP TABLE IF EXISTS public.roomtypes CASCADE;
DROP TABLE IF EXISTS public.hotels CASCADE;
DROP TABLE IF EXISTS public.cities CASCADE;
DROP TABLE IF EXISTS public.countries CASCADE;
DROP TABLE IF EXISTS public.hotel_categories CASCADE;
DROP TABLE IF EXISTS public.mealplans CASCADE;
DROP TABLE IF EXISTS public.users CASCADE;

CREATE TABLE public.countries
(
    id   serial PRIMARY KEY,
    name varchar(100) NOT NULL
);

CREATE TABLE public.cities
(
    id        serial PRIMARY KEY,
    name      varchar(100) NOT NULL,
    countryid integer      NOT NULL
);

CREATE TABLE public.hotel_categories
(
    id   serial PRIMARY KEY,
    name varchar(150) NOT NULL
);

CREATE TABLE public.mealplans
(
    id                         serial PRIMARY KEY,
    code                       varchar(20)  NOT NULL,
    description                varchar(255) NOT NULL,
    smokingallowedinrestaurant boolean      NOT NULL DEFAULT false
);

CREATE TABLE public.hotels
(
    id           serial PRIMARY KEY,
    name         varchar(150) NOT NULL,
    slug         varchar(150) NOT NULL,
    cityid       integer      NOT NULL,
    hoteltype    varchar(50)  NOT NULL,
    description  text         NOT NULL,
    photourl     varchar(255) NOT NULL,
    mealplancode varchar(20),
    price        integer      NOT NULL,
    contacts     text[],
    CONSTRAINT hotels_slug_key UNIQUE (slug)
);

CREATE TABLE public.hotel_category_map
(
    hotelid    integer NOT NULL,
    categoryid integer NOT NULL,
    CONSTRAINT hotel_category_map_pkey PRIMARY KEY (hotelid, categoryid)
);

CREATE TABLE public.hotel_mealplans
(
    hotelid    integer NOT NULL,
    mealplanid integer NOT NULL,
    CONSTRAINT hotel_mealplans_pkey PRIMARY KEY (hotelid, mealplanid)
);

CREATE TABLE public.hoteldescriptions
(
    id                     serial PRIMARY KEY,
    hotelid                integer NOT NULL,
    yearopened             integer,
    yearrenovated          integer,
    totalareasquaremeters  numeric(10, 2),
    buildinginfo           text,
    CONSTRAINT hoteldescriptions_hotelid_key UNIQUE (hotelid)
);

CREATE TABLE public.hotelplaceinfos
(
    id                serial PRIMARY KEY,
    hotelid           integer NOT NULL,
    address           varchar(255),
    city              varchar(100),
    country           varchar(100),
    distancetoairport varchar(100),
    distancetocenter  varchar(100),
    distancetobeach   varchar(100),
    CONSTRAINT hotelplaceinfos_hotelid_key UNIQUE (hotelid)
);

CREATE TABLE public.hotelservices
(
    id       serial PRIMARY KEY,
    hotelid  integer      NOT NULL,
    category varchar(50)  NOT NULL,
    name     varchar(150) NOT NULL
);

CREATE TABLE public.roomtypes
(
    id               serial PRIMARY KEY,
    hotelid          integer      NOT NULL,
    name             varchar(100) NOT NULL,
    view             varchar(100) NOT NULL,
    bedconfiguration varchar(150) NOT NULL,
    maxoccupancy     integer      NOT NULL,
    areasquaremeters integer      NOT NULL
);

CREATE TABLE public.users
(
    id           serial PRIMARY KEY,
    name         varchar(100) NOT NULL,
    email        varchar(120) NOT NULL,
    passwordhash varchar(255) NOT NULL,
    role         varchar(50)  NOT NULL DEFAULT 'User',
    createdat    timestamptz  NOT NULL DEFAULT now(),
    login        varchar(50)  NOT NULL,
    phone        varchar(30)  NOT NULL DEFAULT '',
    CONSTRAINT users_email_key UNIQUE (email),
    CONSTRAINT users_login_key UNIQUE (login)
);

-- Связи

ALTER TABLE public.cities
    ADD CONSTRAINT cities_countryid_fkey
        FOREIGN KEY (countryid) REFERENCES public.countries (id);

ALTER TABLE public.hotels
    ADD CONSTRAINT hotels_cityid_fkey
        FOREIGN KEY (cityid) REFERENCES public.cities (id);

ALTER TABLE public.hotel_category_map
    ADD CONSTRAINT hotel_category_map_categoryid_fkey
        FOREIGN KEY (categoryid) REFERENCES public.hotel_categories (id);

ALTER TABLE public.hotel_category_map
    ADD CONSTRAINT hotel_category_map_hotelid_fkey
        FOREIGN KEY (hotelid) REFERENCES public.hotels (id);

ALTER TABLE public.hotel_mealplans
    ADD CONSTRAINT hotel_mealplans_hotelid_fkey
        FOREIGN KEY (hotelid) REFERENCES public.hotels (id);

ALTER TABLE public.hotel_mealplans
    ADD CONSTRAINT hotel_mealplans_mealplanid_fkey
        FOREIGN KEY (mealplanid) REFERENCES public.mealplans (id);

ALTER TABLE public.hoteldescriptions
    ADD CONSTRAINT hoteldescriptions_hotelid_fkey
        FOREIGN KEY (hotelid) REFERENCES public.hotels (id);

ALTER TABLE public.hotelplaceinfos
    ADD CONSTRAINT hotelplaceinfos_hotelid_fkey
        FOREIGN KEY (hotelid) REFERENCES public.hotels (id);

ALTER TABLE public.hotelservices
    ADD CONSTRAINT hotelservices_hotelid_fkey
        FOREIGN KEY (hotelid) REFERENCES public.hotels (id);

ALTER TABLE public.roomtypes
    ADD CONSTRAINT roomtypes_hotelid_fkey
        FOREIGN KEY (hotelid) REFERENCES public.hotels (id);

-- Индексы

CREATE INDEX ix_cities_countryid      ON public.cities(countryid);
CREATE INDEX ix_hotels_cityid         ON public.hotels(cityid);
CREATE INDEX ix_hotelservices_hotelid ON public.hotelservices(hotelid);
CREATE INDEX ix_roomtypes_hotelid     ON public.roomtypes(hotelid);

-- ============================================
-- ДАННЫЕ ДЛЯ СПРАВОЧНИКОВ
-- ============================================

INSERT INTO public.countries (id, name) VALUES
    (1, 'Russia'),
    (2, 'Turkey');

INSERT INTO public.cities (id, name, countryid) VALUES
    (1,  'Волгоград',        1),
    (2,  'Грозный',          1),
    (3,  'Махачкала',        1),
    (4,  'Владикавказ',      1),
    (5,  'Ставрополь',       1),
    (6,  'Краснодар',        1),
    (7,  'Ростов-на-Дону',   1),
    (8,  'Казань',           1),
    (9,  'Сочи',             1),
    (10, 'Ялта',             1),
    (11, 'Анапа',            1),
    (12, 'Геленджик',        1),
    (13, 'Екатеринбург',     1),
    (14, 'Нижний Новгород',  1),
    (15, 'Самара',           1),
    (16, 'Уфа',              1),
    (17, 'Воронеж',          1),
    (18, 'Анталья',          2),
    (19, 'Алания',           2),
    (20, 'Кемер',            2),
    (21, 'Белек',            2),
    (22, 'Сиде',             2),
    (23, 'Бодрум',           2),
    (24, 'Мармарис',         2),
    (25, 'Кушадасы',         2),
    (26, 'Измир',            2),
    (27, 'Чешме',            2),
    (28, 'Даламан',          2),
    (29, 'Стамбул',          2),
    (30, 'Фетхие',           2);

INSERT INTO public.hotel_categories (id, name) VALUES
    (1,  '2*'),
    (2,  '3*'),
    (3,  '4*'),
    (4,  '5*'),
    (5,  'Hostel'),
    (6,  'Апарт-отель'),
    (7,  'Бутик-отель'),
    (8,  'Гостевой дом'),
    (9,  'SPA-отель'),
    (10, 'Пляжный'),
    (11, 'Городской'),
    (12, 'Деловой'),
    (13, 'Семейный'),
    (14, 'Только для взрослых'),
    (15, 'Курортный'),
    (16, 'Эко-отель'),
    (17, 'Люкс'),
    (18, 'Горнолыжный'),
    (19, 'Отель у моря'),
    (20, 'Уединённый'),
    (21, 'Сеть отелей'),
    (22, 'Хостел премиум'),
    (23, 'Виллы'),
    (24, 'Семейный клубный');

INSERT INTO public.mealplans (id, code, description, smokingallowedinrestaurant) VALUES
    (1, 'BB',  'BB – только завтрак. Шведский стол, безалкогольные напитки.', false),
    (2, 'AI',  'All Inclusive — завтрак, обед, ужин, закуски, местные напитки.', false),
    (3, 'UAI', 'Ultra All Inclusive — расширенное питание и импортные напитки.', false),
    (4, 'FB',  'Full Board — завтрак, обед и ужин.', false),
    (5, 'HB',  'Half Board — завтрак и ужин.', false),
    (6, 'RO',  'Room Only — без питания.', false);

-- Обновляем последовательности для справочников
SELECT setval('public.countries_id_seq',        COALESCE((SELECT MAX(id) FROM public.countries), 1), true);
SELECT setval('public.cities_id_seq',          COALESCE((SELECT MAX(id) FROM public.cities), 1), true);
SELECT setval('public.hotel_categories_id_seq',COALESCE((SELECT MAX(id) FROM public.hotel_categories), 1), true);
SELECT setval('public.mealplans_id_seq',       COALESCE((SELECT MAX(id) FROM public.mealplans), 1), true);

-- ============================================
-- ОТЕЛИ (примерные данные по Турции)
-- ============================================

INSERT INTO public.hotels
(id, name, slug, cityid, hoteltype, description, photourl, mealplancode, price, contacts)
VALUES
    (1, 'Antalya Sun Resort', 'antalya-sun-resort', 18, 'Resort',
     'Семейный курортный отель в Анталье: несколько бассейнов, горки, анимация и собственный пляж.',
     'https://example.com/hotels/antalya-sun-resort.jpg', 'AI', 18000,
     ARRAY['+90 242 000 11 11', 'info@antalya-sun-resort.com']),
    (2, 'Alanya Beach Hotel', 'alanya-beach-hotel', 19, 'Beach Hotel',
     'Отель на первой береговой линии в Алании, песчаный пляж, бар на пляже, программа для детей.',
     'https://example.com/hotels/alanya-beach-hotel.jpg', 'UAI', 16000,
     ARRAY['+90 242 000 22 22', 'contact@alanya-beach.com']),
    (3, 'Kemer Mountain View', 'kemer-mountain-view', 20, 'Resort',
     'Отель в Кемере с видом на горы, сосновый лес, чистый воздух, собственный пляж и пирс.',
     'https://example.com/hotels/kemer-mountain-view.jpg', 'HB', 15000,
     ARRAY['+90 242 000 33 33', 'info@kemer-mountain-view.com']),
    (4, 'Belek Golf & SPA', 'belek-golf-spa', 21, 'Resort',
     'Премиальный отель в Белеке: гольф-поля, SPA-центр, a-la carte рестораны, просторные номера.',
     'https://example.com/hotels/belek-golf-spa.jpg', 'UAI', 23000,
     ARRAY['+90 242 000 44 44', 'spa@belek-golf.com']),
    (5, 'Istanbul Old Town Hotel', 'istanbul-old-town-hotel', 29, 'City Hotel',
     'Городской отель в историческом центре Стамбула, рядом с Голубой мечетью и Айя-Софией.',
     'https://example.com/hotels/istanbul-old-town-hotel.jpg', 'BB', 17000,
     ARRAY['+90 212 000 55 55', 'booking@istanbul-old-town.com']);

SELECT setval('public.hotels_id_seq', COALESCE((SELECT MAX(id) FROM public.hotels), 1), true);

-- ОПИСАНИЯ ОТЕЛЕЙ

INSERT INTO public.hoteldescriptions (hotelid, yearopened, yearrenovated, totalareasquaremeters, buildinginfo) VALUES
    (1, 2010, 2020, 45000, 'Несколько корпусов, аквапарк, большая зелёная территория.'),
    (2, 2008, 2018, 28000, 'Главный корпус и несколько аннексов, пляж через дорогу.'),
    (3, 2012, 2019, 32000, 'Территория в сосновом бору, корпуса каскадом на склоне.'),
    (4, 2015, 2023, 60000, 'Основной корпус, виллы, гольф-поля, SPA-комплекс.'),
    (5, 2012, 2019, 12000, 'Многоэтажный городской отель, ресторан на крыше.');

INSERT INTO public.hotelplaceinfos
(hotelid, address, city, country, distancetoairport, distancetocenter, distancetobeach) VALUES
    (1, 'Lara bölgesi, 1',         'Анталья', 'Turkey', '10 км', '15 км', '0 м (первая линия)'),
    (2, 'Atatürk Cad. 25',         'Алания',  'Turkey', '45 км', '2 км',  '100 м'),
    (3, 'Kemer merkez, 10',        'Кемер',   'Turkey', '55 км', '1 км',  '400 м'),
    (4, 'Belek golf resort 7',     'Белек',   'Turkey', '35 км', '5 км',  '800 м (шаттл)'),
    (5, 'Old Town, Sultanahmet 3', 'Стамбул', 'Turkey', '45 км', '0 км',  'нет пляжа (городской отель)');

-- КАТЕГОРИИ ДЛЯ ОТЕЛЕЙ

INSERT INTO public.hotel_category_map (hotelid, categoryid) VALUES
    (1, 4),
    (2, 3),
    (3, 3),
    (4, 4),
    (5, 2);

-- ПЛАНЫ ПИТАНИЯ ДЛЯ ОТЕЛЕЙ

INSERT INTO public.hotel_mealplans (hotelid, mealplanid) VALUES
    (1, 2),
    (1, 3),
    (2, 2),
    (3, 5),
    (3, 4),
    (4, 3),
    (5, 1),
    (5, 6);

-- УСЛУГИ

INSERT INTO public.hotelservices (hotelid, category, name) VALUES
    (1, 'Пляж',       'Собственный песчано-галечный пляж, шезлонги и зонты бесплатно'),
    (1, 'Питание',    'Ресторан шведский стол, 3 a-la carte ресторана'),
    (1, 'Развлечения','Дневная и вечерняя анимация, мини-клуб'),
    (2, 'Пляж',       'Песчаный пляж через дорогу'),
    (2, 'Бассейны',   'Открытый бассейн и малая горка'),
    (3, 'Природа',    'Сосновый бор, горный воздух'),
    (3, 'Бассейны',   'Бассейн с подогревом вне сезона'),
    (4, 'SPA',        'Турецкий хамам, сауна, массажные кабинеты'),
    (4, 'Спорт',      'Гольф-поля, тренажёрный зал'),
    (5, 'Сервис',     'Трансфер в аэропорт, экскурсионное бюро'),
    (5, 'Питание',    'Завтрак «шведский стол», кафе на крыше');

-- ТИПЫ НОМЕРОВ

INSERT INTO public.roomtypes (hotelid, name, view, bedconfiguration, maxoccupancy, areasquaremeters) VALUES
    (1, 'Standard Room',      'Garden view', '1 двуспальная или 2 односпальные кровати', 3, 24),
    (1, 'Family Room',        'Pool view',   '1 двуспальная + 2 односпальные',           4, 32),
    (2, 'Standard Side Sea',  'Side sea',    '1 двуспальная',                            2, 22),
    (3, 'Mountain View Room', 'Mountain',    '2 односпальные',                           3, 26),
    (4, 'Deluxe Suite',       'Golf/Sea',    'King size bed + гостевая зона',            4, 45),
    (5, 'Standard City View', 'City view',   '1 двуспальная или 2 односпальные',         2, 18);

-- Обновляем последовательности для "дочерних" таблиц
SELECT setval('public.hoteldescriptions_id_seq', COALESCE((SELECT MAX(id) FROM public.hoteldescriptions), 1), true);
SELECT setval('public.hotelplaceinfos_id_seq',   COALESCE((SELECT MAX(id) FROM public.hotelplaceinfos), 1), true);
SELECT setval('public.hotelservices_id_seq',     COALESCE((SELECT MAX(id) FROM public.hotelservices), 1), true);
SELECT setval('public.roomtypes_id_seq',         COALESCE((SELECT MAX(id) FROM public.roomtypes), 1), true);

-- ============================================
-- Пользователь-администратор
-- ============================================

INSERT INTO public.users (name, email, passwordhash, role, login, phone)
VALUES (
    'Admin',
    'admin@example.com',
    '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',
    'Admin',
    'Admin',
    ''
);

-- Обновляем последовательность пользователей
SELECT setval('public.users_id_seq', COALESCE((SELECT MAX(id) FROM public.users), 1), true);

COMMIT;

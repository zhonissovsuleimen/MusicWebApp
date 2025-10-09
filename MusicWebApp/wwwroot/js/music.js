const table_body = document.getElementById('tableBody');
const likes_view = document.getElementById('likesValue');
const pagination = document.getElementById('pagination');
const paginationWrapper = document.getElementById('paginationWrapper');
const gallery_body = document.getElementById('galleryBody');
const table_container = document.getElementById('tableContainer');

const seed_input = document.getElementById('seedInput');
const likes_input = document.getElementById('likesInput');
const page_size_input = document.getElementById('pageSizeInput');
const page_size_container = page_size_input ? page_size_input.parentElement : null;
const locale_input = document.getElementById('localeInput');
  
const ulong_max = 18446744073709551615n;
const uint_max = 4294967295;

let songs_map = new Map();
let likes_map = new Map();
let page_id = 0;
let view_mode = 'table'; // 'table' | 'gallery'
let is_fetching_more = false;

function is_valid_seed(str) {
  if (!/^\d+$/.test(str)) return false;
  try {
    const n = BigInt(str);
    return n >= 0n && n <= ulong_max;
  } catch {
    return false;
  }
}

function is_valid_likes(str) {
  if (!/^\d+(\.\d+)?$/.test(str)) return false;
  try {
    const n = parseFloat(str);
    return n >= 0.0 && n <= 10.0;
  } catch {
    return false;
  }
}

function is_valid_page_size(str) {
  if (!/^\d+$/.test(str)) return false;
  try {
    const n = parseInt(str, 10);
    return n > 0 && n <= 100;
  } catch {
    return false;
  }
}

function is_valid_page_number(str) {
  if (!/^\d+$/.test(str)) return false;
  try {
    const n = parseInt(str, 10);
    return n >= 0;
  } catch {
    return false;
  }
}

function reset_page_id() {
  page_id = 0;
}

function redraw_table() {
  if (!table_body) {
    return;
  }

  let tableHtml = '';
  for (const [index, song] of songs_map) {
    const like = likes_map.get(index);

    const collapse_id = `cover-${song.index}`;
    const coverUrl = `/api/cover?title=${encodeURIComponent(song.title)}&artist=${encodeURIComponent(song.artist)}&seed=${seed_input.value}`;
    const soundUrl = `/api/sound?seed=${seed_input.value}&id=${song.index}`;

    tableHtml += `<div class="music-row clickable" role="button" data-bs-toggle="collapse" data-bs-target="#${collapse_id}">
      <div class="cell col-num text-center">${song.index + 1}</div>
      <div class="cell col-title">${song.title}</div>
      <div class="cell col-artist">${song.artist}</div>
      <div class="cell col-album">${song.album}</div>
      <div class="cell col-genre">${song.genre}</div>
      <div class="cell col-likes text-center">${like?.value ?? ''}</div>
    </div>
    <div id="${collapse_id}" class="collapse">
      <div class="music-row">
        <div class="cell col-collapse">
          <div class="expand-panel">
            <img src="${coverUrl}" class="expand-cover rounded border" alt="Cover: ${song.title} — ${song.artist}" />
            <div class="expand-meta">
              <div class="expand-title" title="${song.title}">${song.title}</div>
              <div class="expand-artist" title="${song.artist}">${song.artist}</div>
              <div class="expand-tags">
                <span class="tag"><strong>Album:</strong> ${song.album}</span>
                <span class="sep">•</span>
                <span class="tag"><strong>Genre:</strong> ${song.genre}</span>
                <span class="sep">•</span>
                <span class="tag"><strong>Likes:</strong> ${like?.value ?? ''}</span>
              </div>
              <audio class="expand-audio" controls preload="none" src="${soundUrl}"></audio>
            </div>
          </div>
        </div>
      </div>
    </div>`;
  }
  table_body.innerHTML = tableHtml;
}

function redraw_gallery(append = false) {
  if (!gallery_body) return;

  let html = append ? gallery_body.innerHTML : '';
  for (const [index, song] of songs_map) {
    if (append && document.getElementById(`g-${song.index}`)) continue;
    const like = likes_map.get(index);
    const coverUrl = `/api/cover?title=${encodeURIComponent(song.title)}&artist=${encodeURIComponent(song.artist)}&seed=${seed_input.value}`;
    const soundUrl = `/api/sound?seed=${seed_input.value}&id=${song.index}`;

    html += `<div id="g-${song.index}" class="gallery-card">
      <img class="gallery-cover" src="${coverUrl}" alt="${song.title} - ${song.artist}" loading="lazy" />
      <div class="gallery-meta">
        <p class="gallery-title">${song.title}</p>
        <p class="gallery-artist">${song.artist}</p>
        <p class="gallery-like">Likes: ${like?.value ?? ''}</p>
        <audio controls preload="none" src="${soundUrl}"></audio>
      </div>
    </div>`;
  }

  gallery_body.innerHTML = html;
}

function redraw_likes_input() {
  if (!likes_view) {
    return;
  }

  likes_view.innerHTML = likes_input.value;
}

function redraw_pagination() {
  if (!pagination) {
    return;
  }

  if (view_mode === 'gallery') {
    paginationWrapper?.classList.add('d-none');
    return;
  } else {
    paginationWrapper?.classList.remove('d-none');
  }

  let html = '';
  const disabled_status = page_id == 0 ? 'disabled' : '';
  html += `<li class="page-item ${disabled_status}">
        <button class="page-link" type="button" onclick="update_page_number(${page_id - 1})">
        <span>&laquo;</span>
        </button>
      </li>`;

  const renderPageButton = (i) => {
    const activeClass = i === page_id ? 'active' : '';
    return `<li class="page-item ${activeClass}"><button class="page-link" type="button" onclick="update_page_number(${i})">${i+1}</button></li>`;
  };

  if (page_id < 2) {
    for (let i = 0; i < 5; i++) {
      html += renderPageButton(i);
    }
  } else {
    let start = page_id - 2;
    for (let i = start; i < start + 5; i++) {
      html += renderPageButton(i);
    }
  }

  html += `<li class="page-item">
        <button class="page-link" type="button" onclick="update_page_number(${page_id + 1})">
        <span>&raquo;</span>
      </button>
      </li>`;

  pagination.innerHTML = html;
}

async function fetch_songs(locale, seed, start, end) {
  try {
    const response = await fetch(`/api/song?locale=${encodeURIComponent(locale)}&seed=${seed}&start=${start}&end=${end}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const songs = await response.json();

    for (const song of songs) {
      songs_map.set(song.index, song);
    }

  } catch (e) {
    console.error("Failed to fetch songs:", e);
  }
}

async function fetch_likes(input, start, end) {
  try {
    const response = await fetch(`/api/like?input=${input}&start=${start}&end=${end}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const likes = await response.json();

    for (const like of likes) {
      likes_map.set(like.index, like);
    }

  } catch (e) {
    console.error("Failed to fetch songs:", e);
  }
}

async function fetch_data(rangeStart, rangeEnd, clean = true) {
  if (!seed_input || !likes_input || !page_size_input) {
    return;
  }
  const seed_str = seed_input.value.trim();
  const like_str = likes_input.value.trim();
  const page_size_str = page_size_input.value.trim();
  const locale = (locale_input?.value ?? 'en').trim();

  if (!is_valid_seed(seed_str) || !is_valid_likes(like_str) || !is_valid_page_size(page_size_str)) {
    return;
  }

  if (clean) {
    songs_map.clear();
    likes_map.clear();
  }

  const start = rangeStart;
  const end = rangeEnd;

  await fetch_songs(locale, seed_str, start, end);
  await fetch_likes(like_str, start, end);
}

async function update() {
  if (view_mode === 'table') {
    const page_size = parseInt(page_size_input.value, 10);
    const start = page_id * page_size;
    const end = start + page_size;
    await fetch_data(start, end, true);
    redraw_table();
  } else {
    const start = 0;
    const end = parseInt(page_size_input.value, 10);
    await fetch_data(start, end, true);
    redraw_gallery(false);
  }
  redraw_likes_input();
  redraw_pagination();
}

async function update_page_number(page_id_str) {
  if (!is_valid_page_number(page_id_str)) {
    return;
  }
  page_id = parseInt(page_id_str, 10);
  update();
}

async function update_page_size() {
  reset_page_id();
  update();
}

function randomize_seed() {
  if (!seed_input) return;
  seed_input.value = Math.floor(Math.random() * uint_max).toString();
  reset_page_id();

  update();
}

function switch_view(mode) {
  if (mode !== 'table' && mode !== 'gallery') return;
  view_mode = mode;

  const tableBtn = document.getElementById('viewTableBtn');
  const galleryBtn = document.getElementById('viewGalleryBtn');

  if (mode === 'table') {
    tableBtn?.classList.add('active');
    galleryBtn?.classList.remove('active');
    table_container?.classList.remove('d-none');
    gallery_body?.classList.add('d-none');

    if (page_size_input) page_size_input.value = '10';
    if (page_size_container) page_size_container.classList.remove('d-none');
  } else {
    tableBtn?.classList.remove('active');
    galleryBtn?.classList.add('active');
    table_container?.classList.add('d-none');
    gallery_body?.classList.remove('d-none');

    if (page_size_input) page_size_input.value = '50';
    if (page_size_container) page_size_container.classList.add('d-none');
  }
  reset_page_id();


  update();
}

function is_near_bottom() {
  const threshold = 300; // px from bottom
  return window.innerHeight + window.scrollY >= document.body.offsetHeight - threshold;
}

async function load_more() {
  if (view_mode !== 'gallery') return;
  if (is_fetching_more) return;
  if (!is_near_bottom()) return;

  is_fetching_more = true;
  try {
    const page_size = parseInt(page_size_input.value, 10);
    const currentMaxIndex = songs_map.size === 0 ? -1 : Math.max(...songs_map.keys());
    const start = currentMaxIndex + 1;
    const end = start + page_size;
    await fetch_data(start, end, false);
    redraw_gallery(true);
  } finally {
    is_fetching_more = false;
  }
}

window.addEventListener('scroll', load_more, { passive: true });

randomize_seed();
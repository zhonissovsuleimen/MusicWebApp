const table_body = document.getElementById('tableBody');
const likes_view = document.getElementById('likesValue');
const pagination = document.getElementById('pagination');

const seed_input = document.getElementById('seedInput');
const likes_input = document.getElementById('likesInput');
const page_size_input = document.getElementById('pageSizeInput');
  
const ulong_max = 18446744073709551615n;
const uint_max = 4294967295;
const locale = "en";

let cached_songs = new Map();
let cached_likes = new Map();
let page_id = 0;

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


function redraw_table() {
  if (!table_body) {
    return;
  }

  let tableHtml = '';
  for (const [index, song] of cached_songs) {
    const like = cached_likes.get(index);
    const likeValue = like && like.value != null ? like.value : '-';

    tableHtml += `<div class="music-row">
      <div class="cell col-num text-center">${song.index + 1}</div>
      <div class="cell col-title">${song.title}</div>
      <div class="cell col-artist">${song.artist}</div>
      <div class="cell col-album">${song.album}</div>
      <div class="cell col-genre">${song.genre}</div>
      <div class="cell col-likes text-center">${likeValue}</div>
    </div>`;
  }
  table_body.innerHTML = tableHtml;
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
    const response = await fetch(`/api/song?locale=${locale}&seed=${seed}&start=${start}&end=${end}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const songs = await response.json();

    for (const song of songs) {
      cached_songs.set(song.index, song);
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
      cached_likes.set(like.index, like);
    }

  } catch (e) {
    console.error("Failed to fetch songs:", e);
  }
}

async function fetch_data(clean) {
  if (!seed_input || !likes_input || !page_size_input) {
    return;
  }
  const seed_str = seed_input.value.trim();
  const like_str = likes_input.value.trim();
  const page_size_str = page_size_input.value.trim();

  if (!is_valid_seed(seed_str) || !is_valid_likes(like_str) || !is_valid_page_size(page_size_str)) {
    return;
  }

  if (clean){
    cached_likes.clear();
    cached_songs.clear();
  }

  const page_size = parseInt(page_size_str, 10);

  const start = page_id * page_size;
  const end = start + page_size;

  await fetch_songs(locale, seed_str, start, end);
  await fetch_likes(like_str, start, end);
}

async function update() {
  await fetch_data(true);
  redraw_table();
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
  page_id = 0;
  update();
}

function randomize_seed() {
  if (!seed_input) return;
  seed_input.value = Math.floor(Math.random() * uint_max).toString();

  update();
}

randomize_seed();
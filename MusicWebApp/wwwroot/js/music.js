const table_body = document.getElementById('tableBody');
const seed_input = document.getElementById('seedInput');
const likes_input = document.getElementById('likesInput');
const likes_view = document.getElementById('likesValue');
const ulong_max = 18446744073709551615n;
const uint_max = 4294967295;
const locale = "en";

let cached_songs = new Map();
let cached_likes = new Map();

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

async function update_table_view() {
  if (!table_body) {
    return;
  }

  let tableHtml = '';
  for (const [index, song] of cached_songs) {
    const like = cached_likes.get(index);
    const likeValue = like && like.value != null ? like.value : '-';

    tableHtml += `<div class="music-row">
      <div class="cell col-num text-center">${song.index}</div>
      <div class="cell col-title">${song.title}</div>
      <div class="cell col-artist">${song.artist}</div>
      <div class="cell col-album">${song.album}</div>
      <div class="cell col-genre">${song.genre}</div>
      <div class="cell col-likes text-center">${likeValue}</div>
    </div>`;
  }
  table_body.innerHTML = tableHtml;
}

async function update_likes_view() {
  if (!likes_view) {
    return;
  }

  likes_view.innerHTML = likes_input.value;
}


async function update_songs() {
  if (!seed_input) {
    return;
  }

  const seed_str = seed_input.value.trim();
  if (!is_valid_seed(seed_str)) {
    return;
  }

  try {
    const response = await fetch(`/api/song?locale=${locale}&seed=${seed_str}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const songs = await response.json();

    for (const song of songs) {
      cached_songs.set(song.index, song);
    }

    update_table_view();
  } catch (e) {
    console.error("Failed to fetch songs:", e);
  }
}

async function update_likes() {
  if (!seed_input || !likes_input) {
    return;
  }

  const seed_str = seed_input.value.trim();
  const like_str = likes_input.value.trim();

  if (!is_valid_seed(seed_str) || !is_valid_likes(like_str)) {
    return;
  }
  cached_likes.clear();

  try {
    const response = await fetch(`/api/like?seed=${seed_str}&input=${like_str}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const likes = await response.json();

    for (const like of likes) {
      cached_likes.set(like.index, like);
    }

    update_likes_view();
    update_table_view();
  } catch (e) {
    console.error("Failed to fetch songs:", e);
  }
}

function randomize_seed() {
  if (!seed_input) return;
  seed_input.value = Math.floor(Math.random() * uint_max).toString();
  cached_songs.clear();

  update_songs();
  update_table_view();
}

randomize_seed();
update_likes();
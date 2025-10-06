const table_body = document.getElementById('tableBody');
const seed_input = document.getElementById('seedInput');
const likes_input = document.getElementById('likesInput');
const likes_view = document.getElementById('likesValue');
const ulong_max = 18446744073709551615n;
const uint_max = 4294967295;
const locale = "en";

let cached_songs = new Map();

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
    tableHtml += `<tr>
        <th scope="row" class="text-nowrap text-start">${song.index}</th>
        <td>
          <div class="w-100 text-truncate">${song.title}</div>
        </td>
        <td>
          <div class="w-100 text-truncate">${song.artist}</div>
        </td>
        <td>
          <div class="w-100 text-truncate">${song.album}</div>
        </td>
        <td>
          <div class="w-100 text-truncate">${song.genre}</div>
        </td>
        <td>
          <div class="text-nowrap text-end">${song.like}</div>
        </td>
      </tr>`
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

  try {
    const response = await fetch(`/api/like?seed=${seed_str}&input=${like_str}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const likes = await response.json();

    for (const like of likes) {
      if (cached_songs.has(like.index)) {
        const song = cached_songs.get(like.index);
        song.like = Number(like.value);
        cached_songs.set(like.index, song);
      }
    }

    update_table_view();
    update_likes_view();
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
//todo: decouple likes from the songs
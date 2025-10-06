const table_body = document.getElementById('tableBody');
const seed_input = document.getElementById('seedInput');
const ulong_max = 18446744073709551615n;
const uint_max = 4294967295;

function is_valid_ulong(str) {
  if (!/^\d+$/.test(str)) return false;
  try {
    const n = BigInt(str);
    return n >= 0n && n <= ulong_max;
  } catch {
    return false;
  }
}


async function updateTable() {
  if (!table_body || !seed_input) {
    return;
  }

  const seed_str = seed_input.value.trim();
  if (!is_valid_ulong(seed_str)) {
    return;
  }

  try {
    const response = await fetch(`/api/song?seed=${seed_str}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const songs = await response.json();
    let tableHtml = '';
    for (const song of songs) {
      tableHtml += `<tr>
        <th scope="row" class="text-nowrap">${song.index}</th>
        <td>
          <div class="w-100 text-truncate">${song.title}</div>
        </td>
        <td>
          <div class="w-100 text-truncate">${song.artist}</div>
        </td>
      </tr>`
    }
    table_body.innerHTML = tableHtml;
  } catch (e) {
    console.error("Failed to fetch songs:", e);
  }
}

function randomizeSeed() {
  if (!seed_input) return;
  seed_input.value = Math.floor(Math.random() * uint_max).toString();
  updateTable();
}

randomizeSeed();
(async () => {
  const tableBody = document.getElementById('tableBody');
  if (!tableBody) {
    return;
  }

  try {
    const response = await fetch('/api/song');
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const songs = await response.json();

    let tableHtml = '';
    for (const song of songs) {
      tableHtml += `<tr>
                <th scope="row">${song.index}</th>
                <td>${song.title}</td>
                <td>${song.artist}</td>
            </tr>`;
    }
    tableBody.innerHTML = tableHtml;
  } catch (e) {
    console.error("Failed to fetch songs:", e);
  }
})();
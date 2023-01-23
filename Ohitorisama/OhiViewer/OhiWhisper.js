let lastTime = 0;

const updateLog = () => {
  fetch("./OhiWhisper.json",{cache: "no-store"})
    .then((response) => {
      if (!response.ok) {
        setTimeout(updateLog, 100);
        return;
      }
      return response.json()
    })
    .then((data) => {
      if (lastTime == data.time) {
        setTimeout(updateLog, 100);
        return;
      }

      document.querySelector('#log').textContent = data.text;
      lastTime = data.time;
      setTimeout(updateLog, 100);
    })
    .catch((error) => {
      console.error('Error:', error);
      setTimeout(updateLog, 100);
    });
}
updateLog();

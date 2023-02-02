let voiceTextlastTime = 0;
let chatGptlastTime = 0;

const updateVoiceText = () => {
  fetch("./OhiVoiceText.json",{cache: "no-store"})
    .then((response) => {
      if (!response.ok) {
        setTimeout(updateVoiceText, 100);
        return;
      }
      return response.json()
    })
    .then((data) => {
      if (voiceTextlastTime == data.time) {
        setTimeout(updateVoiceText, 100);
        return;
      }

      var balloon6 = $(`
        <div class="balloon6 frame">
          <div class="chatting">
            <div class="says">
              <p></p>
            </div>
          </div>
        </div>`);
      balloon6.find('p').text(data.text);
      $('.line-bc').append(balloon6);

      $('html, body').animate(
          { scrollTop: $('body').get(0).scrollHeight },
          'normal'
      );

      if ($('.frame').length > 10) {
        $('.frame:eq(0)').remove();
      }

      voiceTextlastTime = data.time;
      setTimeout(updateVoiceText, 100);
    })
    .catch((error) => {
      console.error('Error:', error);
      setTimeout(updateVoiceText, 100);
    });
}
updateVoiceText();

const updateChatGpt = () => {
  fetch("./OhiChatGpt.json",{cache: "no-store"})
    .then((response) => {
      if (!response.ok) {
        setTimeout(updateChatGpt, 100);
        return;
      }
      return response.json()
    })
    .then((data) => {
      if (chatGptlastTime == data.time) {
        setTimeout(updateChatGpt, 100);
        return;
      }

      var mycomment = $(`
        <div class="mycomment frame">
          <p></p>
        </div>`);
      mycomment.find('p').text(data.text);
      $('.line-bc').append(mycomment);

      $('html, body').animate(
          { scrollTop: $('body').get(0).scrollHeight },
          'normal'
      );

      if ($('.frame').length > 10) {
        $('.frame:eq(0)').remove();
      }

      chatGptlastTime = data.time;
      setTimeout(updateChatGpt, 100);
    })
    .catch((error) => {
      console.error('Error:', error);
      setTimeout(updateChatGpt, 100);
    });
}
updateChatGpt();

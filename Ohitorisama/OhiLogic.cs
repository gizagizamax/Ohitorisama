using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.WebSockets;
using System.Drawing;
using System.Drawing.Imaging;
using Ohitorisama.VTubeStudio;
using System.Diagnostics;

namespace Ohitorisama
{
    public class OhiLogic
    {
        public OhiConfig config;
        public KeysConverter keysConverter;

        static HttpClient httpClient;

        string CONFIG_PATH = @".\OhiConfig.json";
        WaveFileWriter? waveWriter;
        MainWindow mainWindow;
        Dictionary<int, bool> lastKeyCode;
        WaveInEvent? waveIn;
        List<OpenAICompletionHistory> chatGptHistoryList;
        ClientWebSocket vtsSocket;
        HotkeysInCurrentModelResponse hotkeysInCurrentModelResponse;
        string vtsHotkeyName;

        static OhiLogic()
        {
            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            };
            httpClient = new HttpClient(socketsHandler);
        }
        public OhiLogic(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            lastKeyCode = new Dictionary<int, bool>();
            keysConverter = new KeysConverter();
            chatGptHistoryList = new List<OpenAICompletionHistory>();
            vtsHotkeyName = "";
        }
        public void readConfig()
        {
            if (File.Exists(CONFIG_PATH))
            {
                var strJson = File.ReadAllText(CONFIG_PATH, Encoding.UTF8);
                config = JsonSerializer.Deserialize<OhiConfig>(strJson);
            }
            else
            {
                config = new OhiConfig();
            }
        }
        public void writeConfig()
        {
            var strJson = JsonSerializer.Serialize(config);
            File.WriteAllText(CONFIG_PATH, strJson, Encoding.UTF8);
        }
        // キーボード
        public string keyboardCheck()
        {
            if (config.KeyboardTrigger == "")
            {
                return "キーボードを選択してください。";
            }
            return "OK";
        }
        public void keyboardTriggerStart(int keyCode)
        {
            if (lastKeyCode.ContainsKey(keyCode))
            {
                // キーイベントは連続で発火するため、同じイベントはスルーする
                return;
            }
            lastKeyCode.Add(keyCode, true);

            if (mainWindow.rdoKeyboardTrigger.IsChecked == true && keyCode.ToString() == config.KeyboardTrigger)
            {
                recordStart();
            }
        }
        public async Task keyboardTriggerEnd(int keyCode)
        {
            lastKeyCode.Remove(keyCode);
            if (mainWindow.rdoKeyboardTrigger.IsChecked == true && keyCode.ToString() == config.KeyboardTrigger)
            {
                recordEnd();

                var prompt = await whisperTranscribe();
                saveWhisperText(prompt);

                var text = await chatGptCompletion(prompt, true);
                if (text == null)
                {
                    text = "ううーん";
                }

                await voicevoxSpeek(text);
                writeChatGptLog(text);

                if (config.VTubeStudioOn)
                {
                    await getMotion();
                }
            }
        }
        // マイク
        public string mickCheck()
        {
            if (config.MicProductName == "")
            {
                return "マイクを選択してください。";
            }
            if (config.MicRecordPath == "")
            {
                return "録音ファイルパスを指定してください。";
            }
            return "OK";
        }
        public void recordStart()
        {
            if (mainWindow.cmbRecordDevice.SelectedItem == null)
            {
                mainWindow.WriteLog("マイクを選んでください。");
                return;
            }
            // キーイベントは連続で発火するため、未実行の時だけする
            if (waveWriter != null)
            {
                return;
            }

            var waveInCapabilities = ((OhiComboItem)mainWindow.cmbRecordDevice.SelectedItem).val;

            // ツール→NuGetパッケージマネージャー→ソリューションのNuGetパッケージの管理→NAudioをインスコしておく
            var deviceNumber = -1;
            for (var i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var capability = WaveInEvent.GetCapabilities(i);
                if (capability.ProductName == waveInCapabilities.ProductName)
                {
                    deviceNumber = i;
                    break;
                }
            }
            if (deviceNumber == -1)
            {
                mainWindow.WriteLog("選択したマイクが見つかりません。");
                return;
            }

            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = deviceNumber;
            waveIn.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(deviceNumber).Channels);

            waveWriter = new WaveFileWriter(config.MicRecordPath, waveIn.WaveFormat);

            waveIn.DataAvailable += (_, ee) =>
            {
                waveWriter.Write(ee.Buffer, 0, ee.BytesRecorded);
                waveWriter.Flush();
            };
            waveIn.RecordingStopped += (_, __) =>
            {
                waveWriter.Flush();
                waveIn?.Dispose();
                waveIn = null;

                waveWriter?.Close();
                waveWriter = null;
            };

            waveIn.StartRecording();
            mainWindow.WriteLog("録音Start");
        }
        public void recordEnd()
        {
            if (waveWriter == null)
            {
                return;
            }

            waveIn?.StopRecording();
            mainWindow.WriteLog("録音End");
        }
        // Whisper
        public string whisperCheck()
        {
            if (!int.TryParse(config.WhisperPort, out var whisperPort))
            {
                return "localhostのポートに数字を入力してください";
            }
            return "OK";
        }
        public void whisperStart()
        {
            var p = new Process();
            p.StartInfo.FileName = @".\OhiWhisper\OhiWhisper.bat";
            p.StartInfo.ArgumentList.Add(config.WhisperPort);
            p.StartInfo.ArgumentList.Add(config.WhisperModel);
            p.Start();
        }
        public async void whisperReload()
        {
            mainWindow.WriteLog("whisper reload start.");

            var data = new Dictionary<string, string>();
            data.Add("model", config.WhisperModel);

            var response = await httpClient.GetAsync($"http://localhost:{config.WhisperPort}/reload?{await new FormUrlEncodedContent(data).ReadAsStringAsync()}");
            mainWindow.WriteLog(string.Format("{0}", response));
            response.EnsureSuccessStatusCode();
            var strJson = await response.Content.ReadAsStringAsync();

            mainWindow.WriteLog("whisper exit." + strJson);

            whisperStart();

            mainWindow.WriteLog("whisper reload success." + strJson);
        }
        public async Task<string> whisperTranscribe()
        {
            mainWindow.WriteLog("whisper transcribe start.");

            var whisperParam = new Dictionary<string, string>();
            whisperParam.Add("path", config.MicRecordPath);

            var response = await httpClient.GetAsync($"http://localhost:{config.WhisperPort}/get_msg?{await new FormUrlEncodedContent(whisperParam).ReadAsStringAsync()}");
            mainWindow.WriteLog(string.Format("{0}", response));
            response.EnsureSuccessStatusCode();
            var strJson = await response.Content.ReadAsStringAsync();

            var whisperGetMsgRes = JsonSerializer.Deserialize<WhisperGetMsgRes>(strJson);
            mainWindow.WriteLog($"whisper transcribe success. {whisperGetMsgRes.text}");
            return whisperGetMsgRes.text;
        }
        void saveWhisperText(string text)
        {
            var jsonData = new Dictionary<string, object>();
            jsonData.Add("time", DateTime.Now.Ticks);
            jsonData.Add("text", text);
            var writer = new StreamWriter(@".\OhiViewer\OhiWhisper.json");
            writer.Write(JsonSerializer.Serialize(jsonData));
            writer.Close();
        }
        // ChatGPT
        public string chatGptCheck()
        {
            if (config.ChatGptModel == "")
            {
                return "モデルを入力してください。";
            }
            if (!int.TryParse(config.ChatGptTotalToken, out var chatGptTotalToken))
            {
                return "totaltokenに数字を入力してください。";
            }
            if (!int.TryParse(config.ChatGptMaxTokens, out var chatGptMaxTokens))
            {
                return "max_tokensに数字(MAX2048)を入力してください。";
            }
            if (!float.TryParse(config.ChatGptTemperature, out var chatGptTemperature))
            {
                return "temperatureに数字(0.0～1.0)を入力してください。";
            }
            if (config.ChatGptApiKey == "")
            {
                return "APIキーを入力してください。";
            }
            return "OK";
        }
        public async Task<string> chatGptCompletion(string prompt, bool logFlg)
        {
            mainWindow.WriteLog(string.Format("ChatGPT completion start. prompt:{0}", prompt));

            var historyText = new StringBuilder();
            foreach (var history in chatGptHistoryList)
            {
                historyText.Append("Human: " + history.new_prompt + "\nAI: " + history.openAICompletionRes.choices[0].text + "\n");
            }
            var chatGptContent = new OpenAICompletionReq()
            {
                model = config.ChatGptModel,
                prompt = historyText.ToString() + "\nHuman: " + prompt + "\nAI: ",
                max_tokens = int.Parse(config.ChatGptMaxTokens),
                temperature = float.Parse(config.ChatGptTemperature)
            };
            for (var retry = 0; retry < 3; retry++)
            {
                var content = chatGptContent == null ? null : new StringContent(JsonSerializer.Serialize(chatGptContent), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://api.openai.com/v1/completions"),
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ChatGptApiKey);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var strJson = await response.Content.ReadAsStringAsync();
                var openAICompletionRes = JsonSerializer.Deserialize<OpenAICompletionRes>(strJson);
                mainWindow.WriteLog(string.Format("OpenAICompletionResponse {0}", openAICompletionRes));

                // 改行があると無視されるので改行は消す
                openAICompletionRes.choices[0].text = openAICompletionRes.choices[0].text.Replace("\n", "");
                if (openAICompletionRes.choices[0].text.Replace("！", "").Replace("!", "").Replace("？", "").Replace("?", "") == "")
                {
                    // 発音する言葉が無かったらリトライ
                    continue;
                }

                if (logFlg)
                {
                    // 成功
                    var history = new OpenAICompletionHistory();
                    history.new_prompt = prompt;
                    history.new_token =
                        chatGptHistoryList.Count() == 0 ?
                        openAICompletionRes.usage.prompt_tokens :
                        openAICompletionRes.usage.prompt_tokens - chatGptHistoryList[chatGptHistoryList.Count() - 1].openAICompletionRes.usage.total_tokens;
                    history.openAICompletionRes = openAICompletionRes;
                    chatGptHistoryList.Add(history);

                    // tokenが限界を超えそうなら忘れさせる
                    int total_tokens = 0;
                    for (var i = chatGptHistoryList.Count - 1; 0 <= i; i--)
                    {
                        total_tokens += chatGptHistoryList[i].openAICompletionRes.usage.total_tokens;
                        if (total_tokens > int.Parse(config.ChatGptTotalToken) - int.Parse(config.ChatGptMaxTokens))
                        {
                            chatGptHistoryList.RemoveRange(0, i);
                            break;
                        }
                    }
                }

                mainWindow.WriteLog(string.Format("ChatGPT completion success. text:{0}", openAICompletionRes.choices[0].text));

                return openAICompletionRes.choices[0].text;
            }

            mainWindow.WriteLog("ChatGPT completion failed. text:''");
            return "";
        }

        void writeChatGptLog(string text)
        {
            mainWindow.WriteLog("write whisper log start.");

            var data = new Dictionary<string, object>();
            data.Add("time", DateTime.Now.Ticks);
            data.Add("text", text);
            var writer = new StreamWriter(@".\OhiViewer\OhiChatGpt.json");
            writer.Write(JsonSerializer.Serialize(data));
            writer.Close();

            mainWindow.WriteLog("write whisper log success.");
        }
        // Voicevox
        public string voicevoxCheck()
        {
            if (!int.TryParse(config.VoiceVoxPort, out var voicevoxPort))
            {
                return "ポートに数字を入力してください。";
            }
            if (!int.TryParse(config.VoiceVoxSpeaker, out var voicevoxSpeaker))
            {
                return "話者に数字を入力してください。";
            }
            return "OK";
        }
        public async Task voicevoxSpeek(string text)
        {
            // VOICEVOXに長文を送ると遅いので、キリのいい所で切る。話してる間に後半を処理することで待ち時間を減らす
            var endIndex = text.IndexOf("。");
            string line1st;
            string line2nd;
            if (endIndex >= 0)
            {
                line1st = text.Substring(0, endIndex + 1);
                line2nd = text.Substring(endIndex + 1, text.Length - endIndex - 1);
            }
            else
            {
                line1st = text;
                line2nd = null;
            }

            var byteSynthesis1st = await voicevoxSynthesis(line1st);

            var waveOut1st = new WaveOut();
            IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(byteSynthesis1st), new WaveFormat(24000, 1));
            waveOut1st.Init(provider);
            waveOut1st.Play();

            if (line2nd != null)
            {
                var byteSynthesis2nd = await voicevoxSynthesis(line2nd);

                waveOut1st.PlaybackStopped += (object? sender, StoppedEventArgs e) =>
                {
                    var waveOut2nd = new WaveOut();
                    IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(byteSynthesis2nd), new WaveFormat(24000, 1));
                    waveOut2nd.Init(provider);
                    waveOut2nd.Play();
                };
            }

            mainWindow.WriteLog("VoiceVox success.");
        }
        async Task<byte[]> voicevoxSynthesis(string text)
        {
            mainWindow.WriteLog("VoiceVox start.");

            var urlParam = new Dictionary<string, string>();
            urlParam.Add("text", text);
            urlParam.Add("speaker", config.VoiceVoxSpeaker);
            var strUrlParam = await new FormUrlEncodedContent(urlParam).ReadAsStringAsync();

            var requestAudioQuery = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://localhost:{config.VoiceVoxPort}/audio_query?{strUrlParam}")
            };

            var responseAudioQuery = await httpClient.SendAsync(requestAudioQuery);
            responseAudioQuery.EnsureSuccessStatusCode();
            var strJson = await responseAudioQuery.Content.ReadAsStringAsync();
            mainWindow.WriteLog(string.Format("voicevox audio_query success. response:{0}", strJson));

            var requestSynthesis = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://localhost:{config.VoiceVoxPort}/synthesis?{strUrlParam}"),
                Content = new StringContent(strJson, Encoding.UTF8, "application/json")
            };
            var responseSynthesis = await httpClient.SendAsync(requestSynthesis);
            responseSynthesis.EnsureSuccessStatusCode();
            var byteSynthesis = await responseSynthesis.Content.ReadAsByteArrayAsync();
            mainWindow.WriteLog(string.Format("voicevox synthesis success."));
            return byteSynthesis;
        }
        private async Task getMotion()
        {
            mainWindow.WriteLog("ChatGPT completion getMotion start.");

            List<string> hotkeyNameList = new List<string>();
            foreach (var availableHotkey in hotkeysInCurrentModelResponse.data.availableHotkeys)
            {
                hotkeyNameList.Add(availableHotkey.name);
            }

            var text = await chatGptCompletion("その時の気持ちとして、最も適している物は次のうちどれですか。" + string.Join("、", hotkeyNameList.ToArray()), false);

            // 動作を抽出
            Match matche = Regex.Match(text, string.Join("|", hotkeyNameList.ToArray()));
            var motion = matche.Value;

            mainWindow.WriteLog(string.Format("ChatGPT completion getMotion success. motion:{0}", motion));
            vtsHotkeyName = motion;
        }
        // VTubeStudio
        public string vTubeStudioCheck()
        {
            if (!int.TryParse(config.VTubeStudioPort, out var vTubeStudioPort))
            {
                return "ポートに数字を入力してください。";
            }
            return "OK";
        }
        public async Task vTubeStudioKeep()
        {
            if (config.VTubeStudioOn && mainWindow.rdoKeyboardTrigger.IsChecked == true)
            {
                await vTubeStudioConnect();
            }

            while (config.VTubeStudioOn && mainWindow.rdoKeyboardTrigger.IsChecked == true)
            {
                if (vtsHotkeyName == "")
                {
                    await vTubeStudioHotkey();
                    await Task.Delay(1000);
                }
                else
                {
                    await vTubeStudioTrigger("平常心");
                    await vTubeStudioTrigger(vtsHotkeyName);
                    vtsHotkeyName = "";
                    await Task.Delay(1000);
                }
            }
        }
        public async Task vTubeStudioMotion()
        {
            await vTubeStudioConnect();

            await vTubeStudioHotkey();
            foreach (var availableHotkey in hotkeysInCurrentModelResponse.data.availableHotkeys)
            {
                await vTubeStudioTrigger(availableHotkey.name);
                await Task.Delay(1000);
                await vTubeStudioTrigger(availableHotkey.name);
                await Task.Delay(1000);
            }
        }
        async Task vTubeStudioConnect()
        {
            vtsSocket = new ClientWebSocket();
            await vtsSocket.ConnectAsync(new Uri("ws://localhost:" + config.VTubeStudioPort), CancellationToken.None);

            var bmp = new Bitmap(this.GetType(), "VTubeStudio.VtsPlugin.png");
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);

            var authenticationTokenResponse = await vTubeStudioCall<AuthenticationTokenResponse>(new AuthenticationTokenRequest()
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                requestID = $"ohitorisama {typeof(AuthenticationTokenRequest).Name}",
                messageType = typeof(AuthenticationTokenRequest).Name,
                data = new AuthenticationTokenRequest.Data()
                {
                    pluginName = "Ohitorisama Plugin",
                    pluginDeveloper = "おひとり様連携用プラグイン",
                    pluginIcon = ms.GetBuffer()
                }
            }, true);
            if (authenticationTokenResponse.messageType != typeof(AuthenticationTokenResponse).Name)
            {
                return;
            }

            var authenticationResponse = await vTubeStudioCall<AuthenticationResponse>(new AuthenticationRequest()
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                requestID = $"ohitorisama {typeof(AuthenticationRequest).Name}",
                messageType = typeof(AuthenticationRequest).Name,
                data = new AuthenticationRequest.Data()
                {
                    pluginName = "Ohitorisama Plugin",
                    pluginDeveloper = "おひとり様連携用プラグイン",
                    authenticationToken = authenticationTokenResponse.data.authenticationToken
                }
            }, true);
            if (authenticationResponse.messageType != typeof(AuthenticationResponse).Name)
            {
                return;
            }
        }
        async Task vTubeStudioHotkey()
        {
            var currentModelResponse = await vTubeStudioCall<CurrentModelResponse>(new CurrentModelRequest()
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                requestID = $"ohitorisama {typeof(CurrentModelRequest).Name}",
                messageType = typeof(CurrentModelRequest).Name
            }, false);
            if (currentModelResponse.messageType != typeof(CurrentModelResponse).Name)
            {
                return;
            }

            var hotkeysInCurrentModelResponse = await vTubeStudioCall<HotkeysInCurrentModelResponse>(new HotkeysInCurrentModelRequest()
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                requestID = $"ohitorisama {typeof(HotkeysInCurrentModelRequest).Name}",
                messageType = typeof(HotkeysInCurrentModelRequest).Name,
                data = new HotkeysInCurrentModelRequest.Data()
                {
                    modelID = currentModelResponse.data.modelID
                }
            }, false);
            if (hotkeysInCurrentModelResponse.messageType != typeof(HotkeysInCurrentModelResponse).Name)
            {
                return;
            }
            this.hotkeysInCurrentModelResponse = hotkeysInCurrentModelResponse;
        }
        async Task vTubeStudioTrigger(string hotkeyName)
        {
            string hotkeyId = null;
            foreach (var availableHotkey in this.hotkeysInCurrentModelResponse.data.availableHotkeys)
            {
                if (availableHotkey.name == hotkeyName)
                {
                    hotkeyId = availableHotkey.hotkeyID;
                    break;
                }
            }

            if (hotkeyId == null)
            {
                mainWindow.WriteLog(string.Format("「{0}」モーションが見つかりませんでした。", hotkeyName));
            }
            else
            {
                await vTubeStudioCall<HotkeyTriggerResponse>(new HotkeyTriggerRequest()
                {
                    apiName = "VTubeStudioPublicAPI",
                    apiVersion = "1.0",
                    requestID = $"ohitorisama {typeof(HotkeyTriggerRequest).Name}",
                    messageType = typeof(HotkeyTriggerRequest).Name,
                    data = new HotkeyTriggerRequest.Data()
                    {
                        hotkeyID = hotkeyId
                    }
                }, true);
            }
        }
        async Task<T> vTubeStudioCall<T>(object arg, bool isWriteLog)
        {
            await vtsSocket.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(arg)), WebSocketMessageType.Text, true, CancellationToken.None);

            List<byte> byteList = new List<byte>();
            WebSocketReceiveResult result;
            do
            {
                var buffer = new byte[0xff];
                result = await vtsSocket.ReceiveAsync(buffer, CancellationToken.None);
                for (var i = 0; i < result.Count; i++)
                {
                    byteList.Add(buffer[i]);
                }
            } while (!result.EndOfMessage && result.Count > 0);
            var message = Encoding.UTF8.GetString(byteList.ToArray());
            if (isWriteLog)
            {
                mainWindow.WriteLog(message);
            }

            return JsonSerializer.Deserialize<T>(message);
        }
    }
}

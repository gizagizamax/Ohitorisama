using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Ohitorisama
{
    public partial class MainWindow : Window
    {
        OhiLogic ohiLogic;
        KeyboardHook keyboardHook;
        List<string> listLog;

        public MainWindow()
        {
            InitializeComponent();

            ohiLogic = new OhiLogic(this);

            readConfig();

            keyboardHook = new KeyboardHook();
            listLog = new List<string>();

            keyboardHook.KeyDownEvent += KeyboardHook_KeyDownEvent;
            keyboardHook.KeyUpEvent += KeyboardHook_KeyUpEvent;
            keyboardHook.Hook();
        }
        void readConfig()
        {
            ohiLogic.readConfig();

            initKeyboard();
            initRecordDevice();

            if (string.IsNullOrEmpty(ohiLogic.config!.MicRecordPath))
            {
                txtMicRecordPath.Text = new FileInfo(System.Windows.Forms.Application.ExecutablePath).DirectoryName + @"\Ohitorisama.wav";
            }
            else
            {
                txtMicRecordPath.Text = ohiLogic.config.MicRecordPath;
            }

            foreach (ComboBoxItem item in cmbWhisperModel.Items)
            {
                if (ohiLogic.config.VoiceTextWhisperModel == (string)item.Content)
                {
                    item.IsSelected = true;
                    break;

                }
            }
            if (cmbWhisperModel.SelectedItem == null)
            {
                cmbWhisperModel.SelectedIndex = 0;
            }

            if (ohiLogic.config.VoiceTextType == "Whisper")
            {
                rdoVoiceTextWhisper.IsChecked = true;
            }
            else if (ohiLogic.config.VoiceTextType == "ReazonSpeech")
            {
                rdoVoiceTextReazonSpeech.IsChecked = true;
            }
            else
            {
                rdoVoiceTextWhisper.IsChecked = true;
            }

            if (!int.TryParse(ohiLogic.config.VoiceTextPort, out var voiceTextPort))
            {
                txtVoiceTextPort.Text = "50023";
            }
            else
            {
                txtVoiceTextPort.Text = ohiLogic.config.VoiceTextPort;
            }

            txtChatGptApiKey.Password = ohiLogic.config.ChatGptApiKey;

            if (string.IsNullOrEmpty(ohiLogic.config.ChatGptModel))
            {
                txtChatGptModel.Text = "text-davinci-003";
            }
            else
            {
                txtChatGptModel.Text = ohiLogic.config.ChatGptModel;
            }

            if (!int.TryParse(ohiLogic.config.ChatGptTotalToken, out var chatGptTotalToken))
            {
                txtChatGptTotalToken.Text = "4000";
            }
            else
            {
                txtChatGptTotalToken.Text = ohiLogic.config.ChatGptTotalToken;
            }

            if (!float.TryParse(ohiLogic.config.ChatGptTemperature, out var chatGptTemperature))
            {
                txtChatGptTemperature.Text = "0.5";
            }
            else
            {
                txtChatGptTemperature.Text = ohiLogic.config.ChatGptTemperature;
            }

            if (!int.TryParse(ohiLogic.config.ChatGptMaxTokens, out var chatGptMaxTokens))
            {
                txtChatGptMaxTokens.Text = "60";
            }
            else
            {
                txtChatGptMaxTokens.Text = ohiLogic.config.ChatGptMaxTokens;
            }

            if (!int.TryParse(ohiLogic.config.ChatGptTopP, out var chatGptTopP))
            {
                txtChatGptTopP.Text = "1.0";
            }
            else
            {
                txtChatGptTopP.Text = ohiLogic.config.ChatGptTopP;
            }

            if (!int.TryParse(ohiLogic.config.ChatGptFrequencyPenalty, out var chatGptFrequencyPenalty))
            {
                txtChatGptFrequencyPenalty.Text = "0.5";
            }
            else
            {
                txtChatGptFrequencyPenalty.Text = ohiLogic.config.ChatGptFrequencyPenalty;
            }

            if (!int.TryParse(ohiLogic.config.ChatGptPresencePenalty, out var chatGptPresencePenalty))
            {
                txtChatGptPresencePenalty.Text = "0.0";
            }
            else
            {
                txtChatGptPresencePenalty.Text = ohiLogic.config.ChatGptPresencePenalty;
            }

            if (string.IsNullOrEmpty(ohiLogic.config.ChatGptMe))
            {
                txtChatGptPresencePenalty.Text = "You";
            }
            else
            {
                txtChatGptMe.Text = ohiLogic.config.ChatGptMe;
            }

            if (string.IsNullOrEmpty(ohiLogic.config.ChatGptAi))
            {
                txtChatGptAi.Text = "Friend";
            }
            else
            {
                txtChatGptAi.Text = ohiLogic.config.ChatGptAi;
            }

            if (string.IsNullOrEmpty(ohiLogic.config.ChatGptStop))
            {
                txtChatGptStop.Text = "You";
            }
            else
            {
                txtChatGptStop.Text = ohiLogic.config.ChatGptStop;
            }

            if (!int.TryParse(ohiLogic.config.VoiceVoxPort, out var voiceVoxPort))
            {
                txtVoiceVoxPort.Text = "50021";
            }
            else
            {
                txtVoiceVoxPort.Text = ohiLogic.config.VoiceVoxPort;
            }

            if (!int.TryParse(ohiLogic.config.VoiceVoxSpeaker, out var voiceVoxSpeaker))
            {
                txtVoiceVoxSpeaker.Text = "1";
            }
            else
            {
                txtVoiceVoxSpeaker.Text = ohiLogic.config.VoiceVoxSpeaker;
            }

            rdoVTubeStudioOn.IsChecked = ohiLogic.config.VTubeStudioOn == true;

            if (!int.TryParse(ohiLogic.config.VTubeStudioPort, out var vTubeStudioPort))
            {
                txtVTubeStudioPort.Text = "8001";
            }
            else
            {
                txtVTubeStudioPort.Text = ohiLogic.config.VTubeStudioPort;
            }
        }
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                keyboardHook.UnHook();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        // キーボード
        void initKeyboard()
        {
            for (var keyCode = 0; keyCode < 0xff; keyCode++)
            {
                var keyName = ohiLogic.keysConverter.ConvertToString(keyCode);
                if (keyName != "")
                {
                    cmbKeyboardTrigger.Items.Add(new OhiKeyboardTriggerItem(keyCode, keyName!));
                    if (ohiLogic.config!.KeyboardTrigger == keyCode.ToString())
                    {
                        cmbKeyboardTrigger.SelectedIndex = cmbKeyboardTrigger.Items.Count - 1;
                    }

                    cmbKeyboardSkip.Items.Add(new OhiKeyboardTriggerItem(keyCode, keyName!));
                    if (ohiLogic.config!.KeyboardSkip == keyCode.ToString())
                    {
                        cmbKeyboardSkip.SelectedIndex = cmbKeyboardSkip.Items.Count - 1;
                    }
                }
            }
        }
        async void rdoKeyboardTrigger_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                await ohiLogic.vTubeStudioKeep();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void cmbKeyboardTrigger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.KeyboardTrigger = ((OhiKeyboardTriggerItem)cmbKeyboardTrigger.SelectedItem).keyCode.ToString();
                ohiLogic.writeConfig();
                lblKeyboardStatus.Content = ohiLogic.keyboardCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void cmbKeyboardSkip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.KeyboardSkip = ((OhiKeyboardTriggerItem)cmbKeyboardSkip.SelectedItem).keyCode.ToString();
                ohiLogic.writeConfig();
                lblKeyboardStatus.Content = ohiLogic.keyboardCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void KeyboardHook_KeyDownEvent(object sender, KeyEventArg e)
        {
            try
            {
                ohiLogic.keyboardTriggerStart(e.KeyCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        async void KeyboardHook_KeyUpEvent(object sender, KeyEventArg e)
        {
            try
            {
                await ohiLogic.keyboardTriggerEnd(e.KeyCode);
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        // マイク
        void btnRecordDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                initRecordDevice();
                lblMicStatus.Content = ohiLogic.mickCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void cmbRecordDevice_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbRecordDevice.SelectedItem == null)
                {
                    ohiLogic.config!.MicProductName = null;
                }
                else
                {
                    ohiLogic.config!.MicProductName = ((OhiComboItem)cmbRecordDevice.SelectedItem).val.ProductName;
                }
                ohiLogic.writeConfig();
                lblMicStatus.Content = ohiLogic.mickCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void txtMicRecordPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.MicRecordPath = ((System.Windows.Controls.TextBox)sender).Text;
                ohiLogic.writeConfig();
                lblMicStatus.Content = ohiLogic.mickCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void btnMicRecordPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog();
                FileInfo micRecordFile;
                try
                {
                    micRecordFile = new FileInfo(ohiLogic!.config!.MicRecordPath!);
                }
                catch (ArgumentException)
                {
                    micRecordFile = new FileInfo(new FileInfo(System.Windows.Forms.Application.ExecutablePath).DirectoryName + @"\Ohitorisama.wav");
                }
                dialog.FileName = micRecordFile.Name;
                dialog.InitialDirectory = micRecordFile.DirectoryName;
                dialog.Filter = "WAVファイル(*.wav)|*.wav";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ohiLogic.config!.MicRecordPath = dialog.FileName;
                }
                ohiLogic.writeConfig();
                lblMicStatus.Content = ohiLogic.mickCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void initRecordDevice()
        {
            var selectedName = ohiLogic.config!.MicProductName;
            cmbRecordDevice.Items.Clear(); // Configもクリアされる

            for (var i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var capability = WaveInEvent.GetCapabilities(i);
                cmbRecordDevice.Items.Add(new OhiComboItem(string.Format("{0} - {1} channel", capability.ProductName, capability.Channels), capability));
            }

            if (selectedName != null)
            {
                foreach (OhiComboItem item in cmbRecordDevice.Items)
                {
                    if (selectedName == item.val.ProductName)
                    {
                        cmbRecordDevice.SelectedItem = item; // Configも更新される
                        break;
                    }
                }
            }
            if (cmbRecordDevice.SelectedItem == null && cmbRecordDevice.Items.Count > 0)
            {
                cmbRecordDevice.SelectedIndex = 0; // Configも更新される
            }
        }
        void btnMicRecordStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.recordStart();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void btnMicRecordEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.recordEnd();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        // VoiceText
        private void rdoVoiceTextWhisper_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VoiceTextType = "Whisper";
                ohiLogic.writeConfig();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        private void rdoVoiceTextReazonSpeech_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VoiceTextType = "ReazonSpeech";
                ohiLogic.writeConfig();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        void txtVoiceTextPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VoiceTextPort = txtVoiceTextPort.Text;
                ohiLogic.writeConfig();
                lblWhisperStatus.Content = ohiLogic.whisperCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void btnVoiceTextStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.voiceTextStart();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void btnVoiceTextReload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.voiceTextReload();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        // VoiceText - Whisper
        void cmbWhisperModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VoiceTextWhisperModel = (string)((ComboBoxItem)cmbWhisperModel.SelectedItem).Content;
                ohiLogic.writeConfig();
                lblWhisperStatus.Content = ohiLogic.whisperCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        async void btnWhisperTranscribe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ohiLogic.whisperTranscribe();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        // VoiceText - ReazonSpeech
        async void btnReazonSpeechTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ohiLogic.reazonSpeechLoad();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        // ChatGPT
        private void txtChatGptApiKey_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptApiKey = txtChatGptApiKey.Password;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void cmbChatGptPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptPreset = (string)((ContentControl)cmbChatGptPreset.SelectedItem).Content;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
                ohiLogic.chatGptPreset();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        private void txtChatGptModel_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptModel = txtChatGptModel.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptTotalToken_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptTotalToken = txtChatGptTotalToken.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptMaxTokens_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptMaxTokens= txtChatGptMaxTokens.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptTopP_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptTopP = txtChatGptTopP.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptFrequencyPenalty_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptFrequencyPenalty = txtChatGptFrequencyPenalty.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptPresencePenalty_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptPresencePenalty = txtChatGptPresencePenalty.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptMe_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptMe = txtChatGptMe.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptAi_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptAi = txtChatGptAi.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptStop_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptStop = txtChatGptStop.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptTemperature_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.ChatGptTemperature = txtChatGptTemperature.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        async void btnChatGptCompletion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ohiLogic.chatGptCompletion(txtChatGptText.Text, false);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        // VoiceVox
        private void txtVoiceVoxPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VoiceVoxPort = txtVoiceVoxPort.Text;
                ohiLogic.writeConfig();
                lblVoiceVoxStatus.Content = ohiLogic.voicevoxCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtVoiceVoxSpeaker_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VoiceVoxSpeaker = txtVoiceVoxSpeaker.Text;
                ohiLogic.writeConfig();
                lblVoiceVoxStatus.Content = ohiLogic.voicevoxCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        async void btnVoiceVoxSpeek_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ohiLogic.voicevoxSpeek(txtVoiceVoxText.Text);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        // VTubeStudio
        async void rdoVTubeStudioOn_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VTubeStudioOn = rdoVTubeStudioOn.IsChecked == true;
                ohiLogic.writeConfig();
                await ohiLogic.vTubeStudioKeep();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void rdoVTubeStudioOn_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VTubeStudioOn = rdoVTubeStudioOn.IsChecked == true;
                ohiLogic.writeConfig();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtVTubeStudioPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config!.VTubeStudioPort = txtVTubeStudioPort.Text;
                ohiLogic.writeConfig();
                lblVTubeStudioStatus.Content = ohiLogic.vTubeStudioCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        async void btnVTubeStudioMotion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ohiLogic.vTubeStudioMotion();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        async void btnVTubeStudioConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ohiLogic.vTubeStudioMotion();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        // ログ
        public void WriteLog(string log)
        {
            listLog.Add(log);
            if (listLog.Count > 30)
            {
                listLog.RemoveAt(0);
            }

            try
            {
                txtLog.Text = string.Join("\n", listLog.ToArray());
                txtLog.ScrollToEnd();
            }
            catch (Exception)
            {
            }
        }
    }
}

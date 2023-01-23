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

            if (string.IsNullOrEmpty(ohiLogic.config.MicRecordPath))
            {
                txtMicRecordPath.Text = new FileInfo(System.Windows.Forms.Application.ExecutablePath).DirectoryName + @"\Ohitorisama.wav";
            }
            else
            {
                txtMicRecordPath.Text = ohiLogic.config.MicRecordPath;
            }

            foreach (ComboBoxItem item in cmbWhisperModel.Items)
            {
                if (ohiLogic.config.WhisperModel == (string)item.Content)
                {
                    item.IsSelected = true;
                    break;

                }
            }
            if (cmbWhisperModel.SelectedItem == null)
            {
                cmbWhisperModel.SelectedIndex = 0;
            }

            if (!int.TryParse(ohiLogic.config.WhisperPort, out var whisperPort))
            {
                txtWhisperPort.Text = "50023";
            }
            else
            {
                txtWhisperPort.Text = ohiLogic.config.WhisperPort;
            }

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

            if (!int.TryParse(ohiLogic.config.ChatGptMaxTokens, out var chatGptMaxTokens))
            {
                txtChatGptMaxTokens.Text = "256";
            }
            else
            {
                txtChatGptMaxTokens.Text = ohiLogic.config.ChatGptMaxTokens;
            }

            if (!float.TryParse(ohiLogic.config.ChatGptTemperature, out var chatGptTemperature))
            {
                txtChatGptTemperature.Text = "0.5";
            }
            else
            {
                txtChatGptTemperature.Text = ohiLogic.config.ChatGptTemperature;
            }

            txtChatGptApiKey.Password = ohiLogic.config.ChatGptApiKey;

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
                    cmbKeyboardTrigger.Items.Add(new OhiKeyboardTriggerItem(keyCode, keyName));
                    if (ohiLogic.config.KeyboardTrigger == keyCode.ToString())
                    {
                        cmbKeyboardTrigger.SelectedIndex = cmbKeyboardTrigger.Items.Count - 1;
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
                ohiLogic.config.KeyboardTrigger = ((OhiKeyboardTriggerItem)cmbKeyboardTrigger.SelectedItem).keyCode.ToString();
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
                    ohiLogic.config.MicProductName = null;
                }
                else
                {
                    ohiLogic.config.MicProductName = ((OhiComboItem)cmbRecordDevice.SelectedItem).val.ProductName;
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
                ohiLogic.config.MicRecordPath = ((System.Windows.Controls.TextBox)sender).Text;
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
                    micRecordFile = new FileInfo(ohiLogic.config.MicRecordPath);
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
                    ohiLogic.config.MicRecordPath = dialog.FileName;
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
            var selectedName = ohiLogic.config.MicProductName;
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
        // Whisper
        void txtWhisperPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config.WhisperPort = txtWhisperPort.Text;
                ohiLogic.writeConfig();
                lblWhisperStatus.Content = ohiLogic.whisperCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void btnWhisperStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.whisperStart();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void btnWhisperReload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.whisperReload();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        void cmbWhisperModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ohiLogic.config.WhisperModel = (string)((ComboBoxItem)cmbWhisperModel.SelectedItem).Content;
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
        // ChatGPT
        private void txtChatGptModel_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ohiLogic.config.ChatGptModel = txtChatGptModel.Text;
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
                ohiLogic.config.ChatGptTotalToken = txtChatGptTotalToken.Text;
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
                ohiLogic.config.ChatGptMaxTokens= txtChatGptMaxTokens.Text;
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
                ohiLogic.config.ChatGptTemperature = txtChatGptTemperature.Text;
                ohiLogic.writeConfig();
                lblChatGptStatus.Content = ohiLogic.chatGptCheck();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void txtChatGptApiKey_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                ohiLogic.config.ChatGptApiKey = txtChatGptApiKey.Password;
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
                ohiLogic.config.VoiceVoxPort = txtVoiceVoxPort.Text;
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
                ohiLogic.config.VoiceVoxSpeaker = txtVoiceVoxSpeaker.Text;
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
                ohiLogic.config.VTubeStudioOn = rdoVTubeStudioOn.IsChecked == true;
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
                ohiLogic.config.VTubeStudioOn = rdoVTubeStudioOn.IsChecked == true;
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
                ohiLogic.config.VTubeStudioPort = txtVTubeStudioPort.Text;
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

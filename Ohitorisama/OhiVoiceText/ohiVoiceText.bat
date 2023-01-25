chcp 65001
cd %~dp0
echo arg1_port :%1
echo arg2_type :%2
echo arg3_model:%3

call activate.bat
if "%2"=="Whisper" (
	call activate ohiwhisper
)
if "%2"=="ReazonSpeech" (
	call activate ohireazonspeech
)

python ohiVoiceText.py %1 %2 %3

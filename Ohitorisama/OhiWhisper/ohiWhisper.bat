chcp 65001
call activate.bat
call activate ohitorisama
cd %~dp0
echo arg1_port :%1
echo arg2_model:%2
python ohiWhisper.py %1 %2

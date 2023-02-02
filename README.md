# おひとり様
AIと会話するアプリです。  
次のアプリやモジュールを使っています。  

- 音声認識:OpenAI Whisper、ReazonSpeech  
- 会話内容:OpenAI ChatGPT  
- TTS:VOICEVOX  

# インスコ方法
pythonで作った部分は少し環境構築が必要です。  
Anacondaを使う場合ことを推奨しますが、別に無くてもよいです。  
(間違っていたらお気軽にお問い合わせください。ただし詳しくないです。)  

anaconda
[https://www.anaconda.com/products/distribution](https://www.anaconda.com/products/distribution)

## 音声認識にWhisperを使う場合
ffmpeg win版をDLする  
[https://ffmpeg.org/download.html](https://ffmpeg.org/download.html)  

解凍したら ffmpeg-master-latest-win64-gpl フォルダが出てくるので、Ohitorisama.exeのあるフォルダに置く。

anacondaの環境を新しく作る
~~~
conda create -n ohiwhisper
~~~

anacondaの環境一覧を確認する
~~~
conda info -e
~~~
anacondaの環境を有効にする
~~~
conda activate ohiwhisper
~~~
anacondaの環境を削除(タイプミスとかで間違った時用)
~~~
conda remove -n ohiwhisper --all
~~~

gitインスコ
~~~
conda install git
~~~

whisperインスコ
~~~
pip install git+https://github.com/openai/whisper.git
~~~


GPU(グラボ)で動かす場合、CUDAが必要です。  
CUDAが無い場合はCPUで動作しますが遅いです。  
CUDAのバージョン確認
~~~
nvcc -V
~~~

CUDA公式  
[https://pytorch.org/get-started/previous-versions/](https://pytorch.org/get-started/previous-versions/)

Anacondaじゃない場合  
WhisperにくっついてきたCUDA系のモジュールはバージョンの関係で動かないためアンインスコする
[https://download.pytorch.org/whl/cu116](https://download.pytorch.org/whl/cu116)
~~~
pip uninstall pytorch
pip uninstall torchvision
pip uninstall torchaudio
pip uninstall pytorch-cuda
~~~

CUDA 11.6 のインスコ
Anacondaの場合
~~~
conda install pytorch==1.12.1 torchvision==0.13.1 torchaudio==0.12.1 cudatoolkit=11.6 -c pytorch -c conda-forge
~~~
Anacondaじゃない場合
~~~
pip install torch==1.12.1+cu116 torchvision==0.13.1+cu116 torchaudio==0.12.1 --extra-index-url 
~~~

GPUが使えるか確認。１行づつ入力
~~~python
python
import torch
print("GPU(cuda)対応グラボの数:{0}".format(torch.cuda.device_count()))
print("torchバージョン:{0}".format(torch.__version__))
print("GPUが使える？:{0}".format(torch.cuda.is_available()))
~~~
Ctrl + Z → Enter で抜ける

## 音声認識にReazonSpeechを使う場合
anacondaの環境を新しく作る  
最新のpythonだと動かないので、バージョン指定すること。
~~~
conda create -n ohireazonspeech python=3.8.10 anaconda
~~~
anacondaの環境一覧を確認する
~~~
conda info -e
~~~
anacondaの環境を有効にする
~~~
conda activate ohireazonspeech
~~~
ReazonSpeechに必要なライブラリのインスコ  
Anacondaの場合
~~~
conda install -c conda-forge hydra-core
conda install -c conda-forge opt-einsum
conda install -c pytorch torchaudio
~~~
ReazonSpeechのインスコ
~~~
conda install -q espnet==0.10.3
conda install -q espnet_model_zoo
~~~

Anacondaじゃない場合
~~~
pip install hydra-core
pip install opt-einsum
pip install torchaudio
~~~
ReazonSpeechのインスコ
~~~
pip install -q espnet==0.10.3
pip install -q espnet_model_zoo
pip install espnet
pip install espnet_model_zoo
~~~

git cloneする
~~~
git clone https://github.com/espnet/espnet
~~~
Ohitorisama.exeのあるフォルダへ配置する

CUDAのバージョン確認
~~~
nvcc -V
~~~

CUDA 11.6 のインスコ
Anacondaの場合
~~~
conda install pytorch==1.12.1 torchvision==0.13.1 torchaudio==0.12.1 cudatoolkit=11.6 -c pytorch -c conda-forge
~~~
Anacondaじゃない場合
~~~
pip install torch==1.12.1+cu116 torchvision==0.13.1+cu116 torchaudio==0.12.1 --extra-index-url 
~~~

GPUが使えるか確認。１行づつ入力
~~~python
python
import torch
print("GPU(cuda)対応グラボの数:{0}".format(torch.cuda.device_count()))
print("torchバージョン:{0}".format(torch.__version__))
print("GPUが使える？:{0}".format(torch.cuda.is_available()))
~~~
Ctrl + Z → Enter で抜ける


## VOICEVOXのインスコ
下記サイトからDLしてください。  
[https://voicevox.hiroshiba.jp/](https://voicevox.hiroshiba.jp/)

## ChatGPTのトークン

hoge

## VTubeStudio

hoge

# 使い方

ChatGPTのプリセット設定値
https://beta.openai.com/examples/default-friend-chat

hoge

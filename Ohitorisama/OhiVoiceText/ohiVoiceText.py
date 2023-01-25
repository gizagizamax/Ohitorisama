import json
import http.server
import urllib.parse
import torch
import time
import sys
import os

apiPort = sys.argv[1]
libType = sys.argv[2]
modelName = sys.argv[3]

# Whisperで使うため「環境変数：path」に"ffmpeg-master-latest-win64-gpl"を登録しておく
os.environ['PATH'] = os.environ['PATH'] + ";" + os.getcwd() + '\\ffmpeg-master-latest-win64-gpl\\bin'
# GPUが使えるかチェック
print("GPU(cuda)対応グラボの数:{0}".format(torch.cuda.device_count()))
print("torchバージョン:{0}".format(torch.__version__))
print("GPUが使える？:{0}".format(torch.cuda.is_available()))

class OhiHTTPRequestHandler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):

        parsed_path = urllib.parse.urlparse(self.path)
        query = urllib.parse.parse_qs(parsed_path.query)
        print('parsed: path = {}, query = {}'.format(parsed_path.path, query))

        resultBody = {}
        if parsed_path.path == "/reload":
            self.send_response(200)
            self.send_header('Content-Type', 'application/json; charset=utf-8')
            self.end_headers()
            self.wfile.write(json.dumps(resultBody).encode(encoding='utf_8'))
            sys.exit()

        if parsed_path.path == "/get_msg":
            if self.server.reazonspeech != None:
                print("reazonspeech transcribing")
                speech, sample_rate = librosa.load(query["path"][0], sr=16000)
                asr_results = self.server.reazonspeech(speech)
                print(asr_results[0][0])
                resultBody["status"] = "ok"
                resultBody["text"] = asr_results[0][0]

            elif self.server.model != None:
                print("whisper transcribing")
                result = self.server.model.transcribe(query["path"][0], language='japanese')
                print(result['text'])
                resultBody["status"] = "ok"
                resultBody["text"] = result['text']

        self.send_response(200)
        self.send_header('Content-Type', 'application/json; charset=utf-8')
        self.end_headers()
        self.wfile.write(json.dumps(resultBody).encode(encoding='utf_8'))

    def do_POST(self):

        parsed_path = urllib.parse.urlparse(self.path)
        query = urllib.parse.parse_qs(parsed_path.query)
        print('parsed: path = {}, query = {}'.format(parsed_path.path, query))

        print('headers\r\n-----\r\n{}-----'.format(self.headers))

        content_length = int(self.headers['content-length'])
        
        strBody = self.rfile.read(content_length).decode('utf-8')
        print('body = {}'.format(strBody))
        # body = json.loads(strBody)
        
        resultBody = {}
        self.send_response(200)
        self.send_header('Content-Type', 'application/json; charset=utf-8')
        self.end_headers()
        self.wfile.write(json.dumps(resultBody).encode(encoding='utf_8'))

with http.server.HTTPServer(('localhost', int(apiPort)), OhiHTTPRequestHandler) as server:
    if libType == "ReazonSpeech":
        print("ReazonSpeech initializing")
        from espnet2.bin.asr_inference import Speech2Text
        import librosa
        server.model = None
        server.reazonspeech = Speech2Text.from_pretrained(
            "reazon-research/reazonspeech-espnet-v1",
            beam_size=5,
            batch_size=0,
            device="cuda" if torch.cuda.is_available() else "cpu"
        )
        print("ReazonSpeech success")
    elif libType == "Whisper":
        import whisper
        print("whisper initializing")
        server.model = whisper.load_model(modelName)
        server.reazonspeech = None
        print("whisper initialize success")
    server.serve_forever()

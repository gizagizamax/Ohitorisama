import json
import http.server
import urllib.parse
import torch
import whisper
import time
import sys
import os

apiPort = sys.argv[1]
modelName = sys.argv[2]

# Whisperで使うため「環境変数：path」に"ffmpeg-master-latest-win64-gpl"を登録しておく
os.environ['PATH'] = os.environ['PATH'] + ";" + os.getcwd() + '\\ffmpeg-master-latest-win64-gpl\\bin'
# GPUが使えるかチェック
print("GPU(cuda)対応グラボの数:{0}".format(torch.cuda.device_count()))
print("torchバージョン:{0}".format(torch.__version__))
print("GPUが使える？:{0}".format(torch.cuda.is_available()))

# print("whisper initializing")
# model = whisper.load_model(modelName) # tiny, base, small, medium, large
# print("whisper initialize success")
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
            #print("whisper reload")
            #self.server.model = whisper.load_model(query["model"][0]) # tiny, base, small, medium, large
            #print("whisper reloaded")
            #resultBody["status"] = "ok"

        if parsed_path.path == "/get_msg":
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
    print("whisper initializing")
    server.model = whisper.load_model(modelName)
    print("whisper initialize success")
    server.serve_forever()

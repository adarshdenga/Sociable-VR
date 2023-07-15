import socket
import socketserver
import time
from functools import partial

import numpy as np
import whisper


class WhisperHandler(socketserver.BaseRequestHandler):
    def __init__(self, request: socket.socket, client_address, server, model: whisper.Whisper):
        self.model = model
        self.magic = 0xdeadbeef
        super().__init__(request, client_address, server)

    def handle(self):
        audio = self._read()
        then = time.time()
        result = self.model.transcribe(audio)
        text = result['text']
        # text = 'xkcd'
        print(f'[{time.time() - then:.2f}s] `{text}`')
        self._write(text)

    def _read(self):
        data = self.request.recv(4)
        if int.from_bytes(data, byteorder='little') != self.magic:
            return None
        data = self.request.recv(4)
        length = int.from_bytes(data, byteorder='little')
        if length == 0:
            return None
        data = self.request.recv(length)
        text = data.decode('utf-8')
        return text

    def _write(self, text: str):
        self.request.sendall(self.magic.to_bytes(length=4, byteorder='little'))
        self.request.sendall(len(text).to_bytes(length=4, byteorder='little'))
        self.request.sendall(text.encode('utf-8'))


def dry_run():
    then = time.time()
    model = whisper.load_model('small.en')
    audio = np.zeros(16000, dtype=np.float32)
    model.transcribe(audio)
    print(f'setup time: {time.time() - then:.2f}s')
    return model


def main():
    model = dry_run()
    port = 55442
    handler = partial(WhisperHandler, model=model)
    with socketserver.TCPServer(('127.0.0.1', port), handler) as server:
        print('server started')
        try:
            server.serve_forever()
        except KeyboardInterrupt:
            print('server stopped')


if __name__ == '__main__':
    main()

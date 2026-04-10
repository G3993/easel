"""
Intercom - simple HTTP message relay for two Claude Code instances.
Run on both machines. Each instance POSTs to the other's /send and GETs from its own /messages.
"""
import http.server
import json
import threading
import urllib.request
import sys
import time

PORT = 8411
messages_received = []
messages_sent = []

class Handler(http.server.BaseHTTPRequestHandler):
    def do_POST(self):
        if self.path == "/send":
            length = int(self.headers.get("Content-Length", 0))
            body = json.loads(self.rfile.read(length))
            messages_received.append({"from": body.get("from", "unknown"), "text": body["text"], "ts": time.time()})
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.end_headers()
            self.wfile.write(json.dumps({"ok": True, "queue_depth": len(messages_received)}).encode())
        else:
            self.send_response(404)
            self.end_headers()

    def do_GET(self):
        if self.path == "/messages":
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.end_headers()
            self.wfile.write(json.dumps({"messages": messages_received}).encode())
        elif self.path == "/ping":
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.end_headers()
            self.wfile.write(json.dumps({"status": "alive", "name": NAME, "received": len(messages_received), "sent": len(messages_sent)}).encode())
        else:
            self.send_response(404)
            self.end_headers()

    def log_message(self, format, *args):
        pass  # quiet

NAME = sys.argv[1] if len(sys.argv) > 1 else "unnamed"
server = http.server.HTTPServer(("0.0.0.0", PORT), Handler)
print(f"[intercom] {NAME} listening on port {PORT}")
server.serve_forever()

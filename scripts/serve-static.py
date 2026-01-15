#!/usr/bin/env python3
"""
Simple HTTP server to serve static files for MCP Resources widgets.
Similar to the static file server in OpenAI examples.

Usage:
    python scripts/serve-static.py [port]

Default port: 4444
"""

import http.server
import socketserver
import os
import sys

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 4444

# Change to wwwroot directory
wwwroot = os.path.join(os.path.dirname(__file__), '..', 'src', 'Backend', 'Server', 'wwwroot')
os.chdir(wwwroot)

# Enable CORS headers
class CORSRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', '*')
        super().end_headers()

    def do_OPTIONS(self):
        self.send_response(200)
        self.end_headers()

if __name__ == "__main__":
    with socketserver.TCPServer(("", PORT), CORSRequestHandler) as httpd:
        print(f"Serving static files from {wwwroot}")
        print(f"Server running at http://localhost:{PORT}/")
        print("Press Ctrl+C to stop")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nServer stopped")

.PHONY: help build-frontend build-backend clean install dev dev-http dev-mcp test run-static-server test-tools test-meta

# Variables
FRONTEND_DIR = src/Frontend/react-app
BACKEND_DIR = src/Backend
SERVER_PROJECT = $(BACKEND_DIR)/Server/Server.csproj
STATIC_PORT = 4444

# Default target
help:
	@echo "MCP Ecommerce - Makefile Commands"
	@echo ""
	@echo "Build Commands:"
	@echo "  make build-frontend    - Compilar frontend React a wwwroot/"
	@echo "  make build-backend      - Compilar backend .NET"
	@echo "  make build              - Compilar frontend y backend"
	@echo ""
	@echo "Development Commands:"
	@echo "  make dev-http           - Ejecutar servidor HTTP (static files) en puerto $(STATIC_PORT)"
	@echo "  make dev-mcp            - Ejecutar servidor MCP (stdio transport)"
	@echo "  make run-static-server  - Ejecutar servidor Python para static files"
	@echo ""
	@echo "Setup Commands:"
	@echo "  make install            - Instalar dependencias (npm y dotnet restore)"
	@echo "  make clean              - Limpiar archivos compilados"
	@echo ""
	@echo "Testing:"
	@echo "  make test               - Ejecutar tests (cuando estén implementados)"
	@echo "  make test-tools          - Listar todos los MCP Tools disponibles"
	@echo "  make test-meta           - Verificar que _meta está presente en tools/list"
	@echo "  make test-resources      - Probar MCP Resources"
	@echo "  make open-widget         - Abrir Catalog Widget HTML en el navegador"
	@echo ""

# Build frontend React
build-frontend:
	@echo "📦 Compilando frontend React..."
	cd $(FRONTEND_DIR) && npm run build
	@echo "✅ Frontend compilado en wwwroot/"

# Build backend .NET
build-backend:
	@echo "🔨 Compilando backend .NET..."
	dotnet build $(SERVER_PROJECT)
	@echo "✅ Backend compilado"

# Build everything
build: build-frontend build-backend
	@echo "✅ Todo compilado"

# Install dependencies
install:
	@echo "📥 Instalando dependencias..."
	cd $(FRONTEND_DIR) && npm install
	dotnet restore
	@echo "✅ Dependencias instaladas"

# Run server (HTTP + MCP)
run:
	@echo "🚀 Iniciando servidor (HTTP + MCP)..."
	@echo "   - HTTP: http://localhost:$(STATIC_PORT)"
	@echo "   - MCP: http://localhost:$(STATIC_PORT)/mcp"
	dotnet run --project $(SERVER_PROJECT)

# Alias for dev (HTTP + MCP server)
dev: run

# Alias for dev (HTTP)
dev-http: dev

# Alias for dev (MCP también funciona)
dev-mcp: dev

# Run Python static file server (alternative)
run-static-server:
	@echo "🐍 Iniciando servidor Python en puerto $(STATIC_PORT)..."
	python scripts/serve-static.py $(STATIC_PORT)

# Clean compiled files
clean:
	@echo "🧹 Limpiando archivos compilados..."
	cd $(FRONTEND_DIR) && rm -rf node_modules dist
	cd $(BACKEND_DIR) && find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	cd $(BACKEND_DIR) && find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
	rm -rf $(BACKEND_DIR)/Server/wwwroot/widgets $(BACKEND_DIR)/Server/wwwroot/assets 2>/dev/null || true
	@echo "✅ Limpieza completada"

# Test (placeholder for future tests)
test:
	@echo "🧪 Ejecutando tests..."
	@echo "⚠️  Tests aún no implementados"
	# dotnet test

# Test MCP Resources
test-resources:
	@echo "🧪 Probando MCP Resources..."
	@echo "   Asegúrate de que el servidor esté corriendo (make run)"
	@bash scripts/test-mcp-resources.sh

# Open widget HTML in browser
open-widget:
	@echo "🌐 Abriendo Catalog Widget en el navegador..."
	@echo "   Asegúrate de que el servidor esté corriendo (make run)"
	@bash scripts/open-catalog-widget.sh

# Test MCP Tools (list tools)
test-tools:
	@echo "🧪 Probando MCP Tools..."
	@echo "   Asegúrate de que el servidor esté corriendo (make run)"
	@bash scripts/list-tools.sh

# Test that _meta is present in tools/list response
test-meta:
	@echo "🔍 Verificando que _meta está presente en tools/list..."
	@echo "   Asegúrate de que el servidor esté corriendo (make run)"
	@SERVER_URL="http://localhost:$(STATIC_PORT)" && \
	MCP_ENDPOINT="$$SERVER_URL/mcp" && \
	echo "1️⃣  Inicializando conexión MCP..." && \
	curl -s -X POST "$$MCP_ENDPOINT" \
		-H "Content-Type: application/json" \
		-d '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}}' > /dev/null && \
	echo "2️⃣  Obteniendo tools/list..." && \
	RESPONSE=$$(curl -s -X POST "$$MCP_ENDPOINT" \
		-H "Content-Type: application/json" \
		-d '{"jsonrpc": "2.0", "id": 2, "method": "tools/list", "params": {}}') && \
	TOOLS_JSON=$$(echo "$$RESPONSE" | grep "^data:" | sed 's/^data: //') && \
	echo "3️⃣  Verificando _meta en cada tool..." && \
	echo "   (Buscando _meta en nivel raíz del tool)" && \
	echo "" && \
	echo "📄 Respuesta completa:" && \
	echo "$$TOOLS_JSON" | jq '.' 2>/dev/null || echo "$$TOOLS_JSON" && \
	echo "" && \
	HAS_META_ROOT=$$(echo "$$TOOLS_JSON" | jq -r '.result.tools[]? | select(._meta != null) | .name' 2>/dev/null | wc -l | tr -d ' ') && \
	HAS_META_IN_SCHEMA=$$(echo "$$TOOLS_JSON" | jq -r '.result.tools[]? | select(.inputSchema._meta != null) | .name' 2>/dev/null | wc -l | tr -d ' ') && \
	TOTAL_TOOLS=$$(echo "$$TOOLS_JSON" | jq -r '.result.tools[]?.name' 2>/dev/null | wc -l | tr -d ' ') && \
	echo "   - Tools con _meta en nivel raíz: $$HAS_META_ROOT" && \
	echo "   - Tools con _meta en inputSchema: $$HAS_META_IN_SCHEMA" && \
	echo "   - Total de tools: $$TOTAL_TOOLS" && \
	if [ "$$HAS_META_ROOT" -eq "$$TOTAL_TOOLS" ] && [ "$$TOTAL_TOOLS" -gt "0" ]; then \
		echo ""; \
		echo "✅ Todos los tools ($$TOTAL_TOOLS) tienen _meta en nivel raíz"; \
		echo ""; \
		echo "📋 Tools con _meta:"; \
		echo "$$TOOLS_JSON" | jq -r '.result.tools[]? | "   - \(.name): \(._meta | keys | length) campos en _meta"' 2>/dev/null; \
		echo ""; \
		echo "📋 Ejemplo completo de un tool:"; \
		echo "$$TOOLS_JSON" | jq '.result.tools[0]' 2>/dev/null; \
	elif [ "$$HAS_META_IN_SCHEMA" -eq "$$TOTAL_TOOLS" ] && [ "$$TOTAL_TOOLS" -gt "0" ]; then \
		echo ""; \
		echo "⚠️  Todos los tools tienen _meta pero está en inputSchema (debería estar en nivel raíz)"; \
		echo ""; \
		echo "📋 Tools con _meta en inputSchema:"; \
		echo "$$TOOLS_JSON" | jq -r '.result.tools[]? | "   - \(.name): \(.inputSchema._meta | keys | length) campos en _meta"' 2>/dev/null; \
		echo ""; \
		echo "Ejemplo de un tool:"; \
		echo "$$TOOLS_JSON" | jq '.result.tools[0]' 2>/dev/null; \
		exit 1; \
	else \
		echo ""; \
		echo "❌ Error: Solo $$HAS_META_ROOT tools tienen _meta en nivel raíz (esperado: $$TOTAL_TOOLS)"; \
		echo ""; \
		echo "Tools sin _meta en nivel raíz:"; \
		echo "$$TOOLS_JSON" | jq -r '.result.tools[]? | select(._meta == null) | "   - \(.name)"' 2>/dev/null; \
		echo ""; \
		echo "Ejemplo de un tool:"; \
		echo "$$TOOLS_JSON" | jq '.result.tools[0]' 2>/dev/null; \
		exit 1; \
	fi

# Watch frontend (development)
watch-frontend:
	@echo "👀 Modo watch para frontend..."
	cd $(FRONTEND_DIR) && npm run dev

# Full development setup (HTTP server + watch frontend in background)
dev-full: build-frontend
	@echo "🚀 Iniciando entorno de desarrollo completo..."
	@echo "   - Servidor HTTP en http://localhost:$(STATIC_PORT)"
	@echo "   - Frontend en modo watch"
	@trap 'kill 0' EXIT; \
	cd $(FRONTEND_DIR) && npm run dev -- --port 5173 --host & \
	dotnet run --project $(SERVER_PROJECT) & \
	wait

# Check if wwwroot exists and has files
check-wwwroot:
	@echo "🔍 Verificando wwwroot..."
	@if [ ! -d "$(BACKEND_DIR)/Server/wwwroot" ]; then \
		echo "❌ wwwroot no existe. Ejecuta 'make build-frontend' primero"; \
		exit 1; \
	fi
	@if [ -z "$$(ls -A $(BACKEND_DIR)/Server/wwwroot/widgets 2>/dev/null)" ]; then \
		echo "⚠️  wwwroot/widgets está vacío. Ejecuta 'make build-frontend' primero"; \
	else \
		echo "✅ wwwroot tiene archivos"; \
	fi

# Quick start: build and run HTTP server
start: build-frontend dev-http

# Quick start MCP: build and run MCP server
start-mcp: build-backend dev-mcp

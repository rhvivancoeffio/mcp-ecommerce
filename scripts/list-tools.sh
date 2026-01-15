#!/bin/bash

# Script para listar todos los MCP Tools disponibles
# Requiere que el servidor esté corriendo en http://localhost:4444

SERVER_URL="http://localhost:4444"
MCP_ENDPOINT="${SERVER_URL}/mcp"

echo "🔧 Listando MCP Tools disponibles..."
echo "=========================="
echo ""

# Primero inicializar la conexión MCP (si es necesario)
echo "1️⃣  Inicializando conexión MCP..."
INIT_RESPONSE=$(curl -s -X POST "${MCP_ENDPOINT}" \
    -H "Content-Type: application/json" \
    -d '{
        "jsonrpc": "2.0",
        "id": 1,
        "method": "initialize",
        "params": {
            "protocolVersion": "2024-11-05",
            "capabilities": {},
            "clientInfo": {
                "name": "curl-client",
                "version": "1.0.0"
            }
        }
    }')

echo "$INIT_RESPONSE" | jq '.' 2>/dev/null || echo "$INIT_RESPONSE"
echo ""

# Listar todos los tools
echo "2️⃣  Listando Tools..."
TOOLS_RESPONSE=$(curl -s -X POST "${MCP_ENDPOINT}" \
    -H "Content-Type: application/json" \
    -d '{
        "jsonrpc": "2.0",
        "id": 2,
        "method": "tools/list",
        "params": {}
    }')

# Extraer JSON del formato SSE (Server-Sent Events)
TOOLS_JSON=$(echo "$TOOLS_RESPONSE" | grep "^data:" | sed 's/^data: //')

echo "$TOOLS_JSON" | jq '.' 2>/dev/null || echo "$TOOLS_RESPONSE"
echo ""

# Mostrar solo los nombres de los tools
echo "📋 Tools disponibles:"
echo "$TOOLS_JSON" | jq -r '.result.tools[]?.name' 2>/dev/null | while read tool_name; do
    if [ ! -z "$tool_name" ]; then
        echo "   - $tool_name"
    fi
done
echo ""

# Verificar si _meta está presente
echo "🔍 Verificando _meta en tools..."
HAS_META=$(echo "$TOOLS_JSON" | jq -r '.result.tools[]? | select(._meta != null) | .name' 2>/dev/null | wc -l | tr -d ' ')
TOTAL_TOOLS=$(echo "$TOOLS_JSON" | jq -r '.result.tools[]?.name' 2>/dev/null | wc -l | tr -d ' ')

if [ "$HAS_META" -eq "$TOTAL_TOOLS" ] && [ "$TOTAL_TOOLS" -gt "0" ]; then
    echo "✅ Todos los tools ($TOTAL_TOOLS) tienen _meta"
    echo ""
    echo "📊 Detalles de _meta:"
    echo "$TOOLS_JSON" | jq -r '.result.tools[]? | "   \(.name):\n      - openai/visibility: \(._meta."openai/visibility" // "N/A")\n      - openai/outputTemplate: \(._meta."openai/outputTemplate" // "N/A")\n      - openai/widgetAccessible: \(._meta."openai/widgetAccessible" // "N/A")"' 2>/dev/null
else
    echo "⚠️  Solo $HAS_META de $TOTAL_TOOLS tools tienen _meta"
    echo ""
    echo "Tools sin _meta:"
    echo "$TOOLS_JSON" | jq -r '.result.tools[]? | select(._meta == null) | "   - \(.name)"' 2>/dev/null
fi
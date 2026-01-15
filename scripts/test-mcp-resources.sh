#!/bin/bash

# Script para probar MCP Resources
# Requiere que el servidor esté corriendo en http://localhost:4444

SERVER_URL="http://localhost:4444"
MCP_ENDPOINT="${SERVER_URL}/mcp"

echo "🧪 Testing MCP Resources"
echo "=========================="
echo ""

# Función para hacer una petición MCP
mcp_request() {
    local method=$1
    local params=$2
    local id=$3
    
    curl -s -X POST "${MCP_ENDPOINT}" \
        -H "Content-Type: application/json" \
        -d "{
            \"jsonrpc\": \"2.0\",
            \"id\": ${id},
            \"method\": \"${method}\",
            \"params\": ${params}
        }"
}

echo "1️⃣  Inicializando conexión MCP..."
INIT_RESPONSE=$(mcp_request "initialize" '{
    "protocolVersion": "2024-11-05",
    "capabilities": {},
    "clientInfo": {
        "name": "test-client",
        "version": "1.0.0"
    }
}' 1)

echo "$INIT_RESPONSE" | jq '.' 2>/dev/null || echo "$INIT_RESPONSE"
echo ""

echo "2️⃣  Listando Resources disponibles..."
LIST_RESOURCES=$(mcp_request "resources/list" '{}' 2)
echo "$LIST_RESOURCES" | jq '.' 2>/dev/null || echo "$LIST_RESOURCES"
echo ""

echo "3️⃣  Leyendo Catalog HTML Resource (ui://widget/catalog.html)..."
CATALOG_RESPONSE=$(mcp_request "resources/read" '{
    "uri": "ui://widget/catalog.html"
}' 3)

echo "$CATALOG_RESPONSE" | jq -r '.result.contents[0].text' 2>/dev/null > /tmp/catalog-resource.html
if [ $? -eq 0 ]; then
    echo "✅ Catalog Resource obtenido. Guardado en /tmp/catalog-resource.html"
    echo "   Abre el archivo en tu navegador para verlo"
else
    echo "❌ Error obteniendo Catalog Resource"
    echo "$CATALOG_RESPONSE"
fi
echo ""

echo "4️⃣  Leyendo Product Comparison HTML Resource (ui://widget/product-comparison.html)..."
COMPARISON_RESPONSE=$(mcp_request "resources/read" '{
    "uri": "ui://widget/product-comparison.html"
}' 4)

echo "$COMPARISON_RESPONSE" | jq -r '.result.contents[0].text' 2>/dev/null > /tmp/comparison-resource.html
if [ $? -eq 0 ]; then
    echo "✅ Comparison Resource obtenido. Guardado en /tmp/comparison-resource.html"
    echo "   Abre el archivo en tu navegador para verlo"
else
    echo "❌ Error obteniendo Comparison Resource"
    echo "$COMPARISON_RESPONSE"
fi
echo ""

echo "📝 Archivos HTML generados:"
echo "   - /tmp/catalog-resource.html"
echo "   - /tmp/comparison-resource.html"
echo ""
echo "💡 Para abrir en el navegador:"
echo "   open /tmp/catalog-resource.html"
echo "   open /tmp/comparison-resource.html"

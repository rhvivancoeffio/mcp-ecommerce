#!/bin/bash

# Script para obtener el HTML del Catalog Widget y abrirlo en el navegador
# Requiere que el servidor esté corriendo en http://localhost:4444

SERVER_URL="http://localhost:4444"
MCP_ENDPOINT="${SERVER_URL}/mcp"
OUTPUT_FILE="/tmp/catalog-widget.html"

echo "🔍 Obteniendo Catalog Widget HTML..."

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

# Inicializar conexión MCP
echo "1️⃣  Inicializando conexión MCP..."
INIT_RESPONSE=$(mcp_request "initialize" '{
    "protocolVersion": "2024-11-05",
    "capabilities": {},
    "clientInfo": {
        "name": "test-client",
        "version": "1.0.0"
    }
}' 1)

# Obtener el HTML del Resource
echo "2️⃣  Obteniendo Catalog HTML Resource..."
CATALOG_RESPONSE=$(mcp_request "resources/read" '{
    "uri": "ui://widget/catalog.html"
}' 2)

# Extraer solo el JSON del formato SSE (Server-Sent Events)
# El formato SSE es: event: message\ndata: {json}
JSON_DATA=$(echo "$CATALOG_RESPONSE" | grep "^data:" | sed 's/^data: //' | head -1)

# Extraer el HTML
HTML_CONTENT=$(echo "$JSON_DATA" | jq -r '.result.contents[0].text' 2>/dev/null)

if [ -z "$HTML_CONTENT" ] || [ "$HTML_CONTENT" = "null" ]; then
    echo "❌ Error obteniendo el HTML"
    echo "$CATALOG_RESPONSE"
    exit 1
fi

# Crear datos de prueba usando jq para asegurar JSON válido
# Usamos Unsplash Source API para generar imágenes placeholder basadas en el nombre del producto
TEST_DATA=$(jq -n '{
  "products": [
    {
      "id": "1",
      "name": "Wireless Bluetooth Headphones",
      "description": "Premium noise-cancelling headphones with 30-hour battery life and superior sound quality.",
      "price": 129.99,
      "sku": "WBH-001",
      "category": "Electronics",
      "brand": "AudioTech",
      "imageUrl": "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&h=800&fit=crop",
      "imageUrls": [
        "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&h=800&fit=crop",
        "https://images.unsplash.com/photo-1484704849700-f032a568e944?w=800&h=800&fit=crop",
        "https://images.unsplash.com/photo-1545127398-14699f92334b?w=800&h=800&fit=crop"
      ],
      "stock": 25,
      "attributes": {
        "Color": "Black",
        "Brand": "AudioTech",
        "Warranty": "2 years"
      },
      "features": [
        "Noise-cancelling technology",
        "30-hour battery life",
        "Superior sound quality",
        "Comfortable design",
        "Bluetooth 5.0"
      ]
    },
    {
      "id": "2",
      "name": "Smart Watch Pro",
      "description": "Feature-rich smartwatch with heart rate monitor, GPS, and water resistance up to 50m.",
      "price": 299.99,
      "sku": "SWP-002",
      "category": "Electronics",
      "brand": "TechWear",
      "imageUrl": "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=800&h=800&fit=crop",
      "imageUrls": [
        "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=800&h=800&fit=crop",
        "https://images.unsplash.com/photo-1434493789847-2f02dc6ca35d?w=800&h=800&fit=crop",
        "https://images.unsplash.com/photo-1523371686816-b199a9a2d1e3?w=800&h=800&fit=crop"
      ],
      "stock": 15,
      "attributes": {
        "Color": "Silver",
        "Brand": "TechWear",
        "Warranty": "1 year"
      },
      "features": [
        "Heart rate monitor",
        "GPS tracking",
        "Water resistance 50m",
        "Long battery life",
        "Fitness tracking"
      ]
    },
    {
      "id": "3",
      "name": "Laptop Stand Adjustable",
      "description": "Ergonomic aluminum laptop stand with adjustable height and ventilation design.",
      "price": 49.99,
      "sku": "LSA-003",
      "category": "Accessories",
      "brand": "ErgoDesk",
      "imageUrl": "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=800&h=800&fit=crop",
      "imageUrls": [
        "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=800&h=800&fit=crop",
        "https://images.unsplash.com/photo-1586953208448-b95a79798f07?w=800&h=800&fit=crop",
        "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=800&h=800&fit=crop&q=80"
      ],
      "stock": 0,
      "attributes": {
        "Material": "Aluminum",
        "Weight": "1.2 kg",
        "Max Weight": "10 kg"
      },
      "features": [
        "Adjustable height",
        "Ventilation design",
        "Aluminum construction",
        "Ergonomic design",
        "Lightweight"
      ]
    },
    {
      "id": "4",
      "name": "Mechanical Keyboard RGB",
      "description": "Gaming mechanical keyboard with RGB backlighting and Cherry MX switches.",
      "price": 149.99,
      "sku": "MKR-004",
      "category": "Electronics",
      "brand": "GameTech",
      "imageUrl": "https://images.unsplash.com/photo-1541140532154-b024d705b90a?w=800&h=800&fit=crop",
      "imageUrls": [
        "https://images.unsplash.com/photo-1541140532154-b024d705b90a?w=800&h=800&fit=crop",
        "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800&h=800&fit=crop",
        "https://images.unsplash.com/photo-1618384887929-16ec33cab9ef?w=800&h=800&fit=crop"
      ],
      "stock": 8,
      "attributes": {
        "Switch Type": "Cherry MX Red",
        "Layout": "Full Size",
        "Backlight": "RGB"
      },
      "features": [
        "Cherry MX switches",
        "RGB backlighting",
        "Gaming optimized",
        "Full size layout",
        "Durable construction"
      ]
    }
  ],
  "totalCount": 4,
  "page": 1,
  "pageSize": 20
}')

# Convertir JSON a string JavaScript seguro (minificar)
JSON_STRING=$(echo "$TEST_DATA" | jq -c .)

# Crear un script Python temporal para hacer el reemplazo de forma segura
TEMP_PYTHON=$(mktemp)
cat > "$TEMP_PYTHON" << 'PYTHON_EOF'
import sys
import json
import re

# Leer el HTML desde stdin
html_content = sys.stdin.read()

# Leer el JSON desde el argumento (ya viene como string JSON válido)
json_string = sys.argv[1]

# Crear el script a inyectar (usar formato de string para evitar problemas con comillas)
injected_script = '<script>\nwindow.openai = window.openai || {};\nwindow.openai.toolOutput = {\n  structuredContent: ' + json_string + '\n};\n</script>'

# Reemplazar el script original con el nuevo que incluye datos
pattern = r'<script>window\.openai = window\.openai \|\| \{\};</script>'
modified = re.sub(pattern, injected_script, html_content)
print(modified, end='')
PYTHON_EOF

# Inyectar los datos de prueba en el HTML usando Python
MODIFIED_HTML=$(echo "$HTML_CONTENT" | python3 "$TEMP_PYTHON" "$JSON_STRING")

# Limpiar archivo temporal
rm "$TEMP_PYTHON" 2>/dev/null

# Guardar el HTML modificado
echo "$MODIFIED_HTML" > "${OUTPUT_FILE}"

# Verificar que el HTML se guardó correctamente
if [ -z "$MODIFIED_HTML" ]; then
    echo "⚠️  Advertencia: El HTML modificado está vacío, guardando HTML original sin datos..."
    echo "$HTML_CONTENT" > "${OUTPUT_FILE}"
fi

if [ -s "${OUTPUT_FILE}" ]; then
    echo "✅ HTML guardado con datos de prueba en: ${OUTPUT_FILE}"
    echo ""
    echo "📦 Datos de prueba inyectados:"
    echo "   - 4 productos de ejemplo"
    echo "   - Incluye productos con stock y sin stock"
    echo "   - Con atributos y categorías variadas"
    echo ""
    
    # Copiar el archivo a /tmp para que el servidor pueda servirlo
    cp "${OUTPUT_FILE}" /tmp/catalog-widget.html 2>/dev/null || true
    
    # Abrir a través del servidor HTTP para que los recursos CSS/JS se carguen correctamente
    WIDGET_URL="${SERVER_URL}/test-widget.html"
    
    echo "🌐 Abriendo widget a través del servidor HTTP..."
    echo "   URL: ${WIDGET_URL}"
    echo "   (Esto asegura que los estilos CSS y JavaScript se carguen correctamente)"
    echo ""
    
    # Abrir en el navegador (macOS)
    if [[ "$OSTYPE" == "darwin"* ]]; then
        open "${WIDGET_URL}"
    else
        echo "🌐 Abre esta URL en tu navegador:"
        echo "   ${WIDGET_URL}"
    fi
else
    echo "❌ Error obteniendo el HTML"
    echo "$CATALOG_RESPONSE"
    exit 1
fi

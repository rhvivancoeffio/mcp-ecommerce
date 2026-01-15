using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.AspNetCore;
using Application;
using Infrastructure;
using Server;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System.Reflection;
using System.Collections.Concurrent;
using Server.Catalog.McpTools;
using Server.Catalog.McpResources;
using Server.Checkout.McpTools;
using Server.Checkout.McpResources;
using Server.Common.Mcp;
using Server.Common.Mcp.Attributes;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Configure HTTP server
builder.WebHost.UseUrls("http://localhost:4444");

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Server Presentation layer (MCP Tools and Resources)
builder.Services.AddServerPresentation();

// Add MCP Tool Metadata Service
builder.Services.AddSingleton<Server.Common.Mcp.McpToolMetadataService>();

// Add MCP Resource Metadata Service
builder.Services.AddSingleton<Server.Common.Mcp.McpResourceMetadataService>();

// Track initialized sessions to avoid duplicate tool/resource registration
var initializedSessions = new ConcurrentDictionary<string, bool>();

// Configure MCP Server with custom ListToolsHandler to add metadata
builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        // Configure custom ListToolsHandler and ListResourcesHandler to add _meta
        options.ConfigureSessionOptions = async (httpContext, mcpOptions, cancellationToken) =>
        {
            // Get IServiceScopeFactory to create scopes in handlers (HttpContext may be disposed when handlers execute)
            var serviceScopeFactory = httpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
            
            // Generate a unique session ID from connection info
            var sessionId = $"{httpContext.Connection.Id}_{httpContext.Connection.RemoteIpAddress}";
            
            // Store tool registry for manual execution
            // We don't use ToolCollection anymore - we'll handle everything manually
            // Store Type instead of Instance to avoid ObjectDisposedException
            var toolRegistry = new Dictionary<string, (Type Type, MethodInfo Method, string Name, string Description)>(StringComparer.OrdinalIgnoreCase);
            
            // Only initialize tools once per session using ConcurrentDictionary
            if (initializedSessions.TryAdd($"{sessionId}_tools", true))
            {
                // Register tools manually - get ToolName from OpenAiToolMetadataAttribute
                // Store Type instead of Instance to create new instances in handlers
                var catalogToolMethod = typeof(CatalogListTool).GetMethod(nameof(CatalogListTool.CatalogList));
                if (catalogToolMethod != null)
                {
                    var metadataAttr = catalogToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var descriptionAttr = catalogToolMethod.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "catalog_list";
                    toolRegistry[toolName] = (typeof(CatalogListTool), catalogToolMethod, toolName, 
                        descriptionAttr?.Description ?? "Retrieves a paginated list of products from the catalog. Supports filtering by category and search. ALWAYS call 'get_available_sellers' first to get shopKey. Use 'get_available_categories' to see available categories for filtering.");
                }
                
                var comparisonToolMethod = typeof(ProductComparisonTool).GetMethod(nameof(ProductComparisonTool.ProductComparison));
                if (comparisonToolMethod != null)
                {
                    var metadataAttr = comparisonToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var descriptionAttr = comparisonToolMethod.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "product_comparison";
                    toolRegistry[toolName] = (typeof(ProductComparisonTool), comparisonToolMethod, toolName,
                        descriptionAttr?.Description ?? "Compares multiple products by their IDs. Returns detailed product information for side-by-side comparison. ALWAYS call 'get_available_sellers' first to get shopKey.");
                }
                
                var categoryToolMethod = typeof(CategoryListTool).GetMethod(nameof(CategoryListTool.GetAvailableCategories));
                if (categoryToolMethod != null)
                {
                    var metadataAttr = categoryToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "get_available_categories";
                    toolRegistry[toolName] = (typeof(CategoryListTool), categoryToolMethod, toolName,
                        "Retrieves all available product categories for a shop. Use the shopKey from 'get_available_sellers'. Returns category names for use in catalog_list filter.");
                }
                
                var brandToolMethod = typeof(BrandListTool).GetMethod(nameof(BrandListTool.GetAvailableBrands));
                if (brandToolMethod != null)
                {
                    var metadataAttr = brandToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "get_available_brands";
                    toolRegistry[toolName] = (typeof(BrandListTool), brandToolMethod, toolName,
                        "Retrieves all available product brands for a shop. Use the shopKey from 'get_available_sellers'. Returns brand names for reference.");
                }
                
                // Register SellerListTool - MUST be called first to get available shop keys
                var sellerToolMethod = typeof(SellerListTool).GetMethod(nameof(SellerListTool.GetAvailableSellers));
                if (sellerToolMethod != null)
                {
                    var metadataAttr = sellerToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "get_available_sellers";
                    toolRegistry[toolName] = (typeof(SellerListTool), sellerToolMethod, toolName,
                        "Retrieves all available sellers (shops). MUST be called FIRST before any catalog operations. Use the shopKey from the response in other catalog tools.");
                }
                
                // Register AddToCartTool - Cart management
                var addToCartToolMethod = typeof(AddToCartTool).GetMethod(nameof(AddToCartTool.AddToCart));
                if (addToCartToolMethod != null)
                {
                    var metadataAttr = addToCartToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "add_to_cart";
                    toolRegistry[toolName] = (typeof(AddToCartTool), addToCartToolMethod, toolName,
                        "Adds items to the shopping cart. Creates a new cart if cartId is not provided. Returns cartId for session persistence. Use the returned cartId in subsequent cart operations.");
                }
                
                // Register GetCartTool - Get cart state
                var getCartToolMethod = typeof(GetCartTool).GetMethod(nameof(GetCartTool.GetCart));
                if (getCartToolMethod != null)
                {
                    var metadataAttr = getCartToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "get_cart";
                    toolRegistry[toolName] = (typeof(GetCartTool), getCartToolMethod, toolName,
                        "Retrieves the current state of a shopping cart by cartId. Returns all items, totals, and cart metadata. Use cartId from previous cart operations.");
                }
                
                // Register UpdateCartItemTool - Update item quantity
                var updateCartItemToolMethod = typeof(UpdateCartItemTool).GetMethod(nameof(UpdateCartItemTool.UpdateCartItem));
                if (updateCartItemToolMethod != null)
                {
                    var metadataAttr = updateCartItemToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "update_cart_item";
                    toolRegistry[toolName] = (typeof(UpdateCartItemTool), updateCartItemToolMethod, toolName,
                        "Updates the quantity of a specific item in the cart. Set quantity to 0 to remove the item. Returns updated cart state. Use cartId from previous cart operations.");
                }
                
                // Register RemoveFromCartTool - Remove item from cart
                var removeFromCartToolMethod = typeof(RemoveFromCartTool).GetMethod(nameof(RemoveFromCartTool.RemoveFromCart));
                if (removeFromCartToolMethod != null)
                {
                    var metadataAttr = removeFromCartToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "remove_from_cart";
                    toolRegistry[toolName] = (typeof(RemoveFromCartTool), removeFromCartToolMethod, toolName,
                        "Removes a specific item from the cart by productId. Returns updated cart state after removal. Use cartId from previous cart operations.");
                }
                
                // Register OpenCartWidgetTool - Open cart widget to view cart
                var openCartWidgetToolMethod = typeof(OpenCartWidgetTool).GetMethod(nameof(OpenCartWidgetTool.OpenCartWidget));
                if (openCartWidgetToolMethod != null)
                {
                    var metadataAttr = openCartWidgetToolMethod.GetCustomAttribute<OpenAiToolMetadataAttribute>();
                    var descriptionAttr = openCartWidgetToolMethod.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                    var toolName = metadataAttr?.ToolName ?? "open_cart_widget";
                    toolRegistry[toolName] = (typeof(OpenCartWidgetTool), openCartWidgetToolMethod, toolName,
                        descriptionAttr?.Description ?? "Opens the shopping cart widget to display the current cart contents. Use this when the user wants to view their cart. Requires cartId from previous cart operations.");
                }
                
                // Store registry in HttpContext.Items for use in handlers
                httpContext.Items["ToolRegistry"] = toolRegistry;
            }
            else
            {
                // Session already initialized, retrieve existing registry from HttpContext
                if (httpContext.Items.TryGetValue("ToolRegistry", out var existingRegistry))
                {
                    toolRegistry = existingRegistry as Dictionary<string, (Type Type, MethodInfo Method, string Name, string Description)> ?? new();
                }
            }
            
            // Store resource registry for manual execution
            // We don't use ResourceCollection anymore - we'll handle everything manually
            // Store Type instead of Instance to avoid ObjectDisposedException
            var resourceRegistry = new Dictionary<string, (Type Type, MethodInfo Method, string Uri, string? Title, string MimeType)>(StringComparer.OrdinalIgnoreCase);
            
            // Only initialize resources once per session using ConcurrentDictionary
            if (initializedSessions.TryAdd($"{sessionId}_resources", true))
            {
                // Register resources manually - get Uri from OpenAiResourceMetadataAttribute
                // Store Type instead of Instance to create new instances in handlers
                var catalogResourceMethod = typeof(CatalogHtmlResource).GetMethod(nameof(CatalogHtmlResource.Catalog));
                if (catalogResourceMethod != null)
                {
                    var metadataAttr = catalogResourceMethod.GetCustomAttribute<OpenAiResourceMetadataAttribute>();
                    var uri = metadataAttr?.Uri ?? "ui://widget/catalog.html";
                    resourceRegistry[uri] = (typeof(CatalogHtmlResource), catalogResourceMethod, uri, metadataAttr?.Title, metadataAttr?.MimeType ?? "text/html+skybridge");
                }
                
                var comparisonResourceMethod = typeof(ProductComparisonHtmlResource).GetMethod(nameof(ProductComparisonHtmlResource.ProductComparison));
                if (comparisonResourceMethod != null)
                {
                    var metadataAttr = comparisonResourceMethod.GetCustomAttribute<OpenAiResourceMetadataAttribute>();
                    var uri = metadataAttr?.Uri ?? "ui://widget/product-comparison.html";
                    resourceRegistry[uri] = (typeof(ProductComparisonHtmlResource), comparisonResourceMethod, uri, metadataAttr?.Title, metadataAttr?.MimeType ?? "text/html+skybridge");
                }
                
                var cartResourceMethod = typeof(CartHtmlResource).GetMethod(nameof(CartHtmlResource.Cart));
                if (cartResourceMethod != null)
                {
                    var metadataAttr = cartResourceMethod.GetCustomAttribute<OpenAiResourceMetadataAttribute>();
                    var uri = metadataAttr?.Uri ?? "ui://widget/cart.html";
                    resourceRegistry[uri] = (typeof(CartHtmlResource), cartResourceMethod, uri, metadataAttr?.Title, metadataAttr?.MimeType ?? "text/html+skybridge");
                }
                
                // Store registry in HttpContext.Items for use in handlers
                httpContext.Items["ResourceRegistry"] = resourceRegistry;
            }
            else
            {
                // Session already initialized, retrieve existing registry from HttpContext
                if (httpContext.Items.TryGetValue("ResourceRegistry", out var existingRegistry))
                {
                    resourceRegistry = existingRegistry as Dictionary<string, (Type Type, MethodInfo Method, string Uri, string? Title, string MimeType)> ?? new();
                }
            }
            
            mcpOptions.Handlers ??= new();
            
            // Helper function to build InputSchema from method parameters
            JsonObject BuildInputSchema(MethodInfo method)
            {
                var schema = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject(),
                    ["required"] = new JsonArray()
                };
                
                var properties = schema["properties"] as JsonObject;
                var required = schema["required"] as JsonArray;
                
                foreach (var param in method.GetParameters())
                {
                    // Skip McpServer and CancellationToken parameters
                    if (param.ParameterType == typeof(McpServer) || param.ParameterType == typeof(CancellationToken))
                        continue;
                    
                    var paramSchema = new JsonObject();
                    var descriptionAttr = param.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                    
                    var paramType = param.ParameterType;
                    var underlyingType = Nullable.GetUnderlyingType(paramType);
                    var actualType = underlyingType ?? paramType;
                    
                    // Handle arrays
                    if (actualType.IsArray)
                    {
                        paramSchema["type"] = "array";
                        var elementType = actualType.GetElementType();
                        
                        // Handle Guid[] arrays
                        if (elementType == typeof(Guid))
                        {
                            paramSchema["items"] = new JsonObject { ["type"] = "string" };
                        }
                        // Handle arrays of complex types (like CartItemInput[])
                        else if (elementType != null && elementType != typeof(string) && elementType != typeof(int) && elementType != typeof(decimal))
                        {
                            paramSchema["items"] = BuildObjectSchema(elementType);
                        }
                        else
                        {
                            // Fallback for other array types
                            paramSchema["items"] = new JsonObject { ["type"] = "string" };
                        }
                    }
                    // Handle basic types
                    else if (actualType == typeof(string))
                    {
                        paramSchema["type"] = "string";
                    }
                    else if (actualType == typeof(int))
                    {
                        paramSchema["type"] = "integer";
                    }
                    else if (actualType == typeof(decimal))
                    {
                        paramSchema["type"] = "number";
                    }
                    else if (actualType == typeof(bool))
                    {
                        paramSchema["type"] = "boolean";
                    }
                    else if (actualType == typeof(Guid))
                    {
                        paramSchema["type"] = "string";
                    }
                    // Handle complex types (records, classes)
                    else if (actualType.IsClass || (actualType.IsValueType && !actualType.IsPrimitive))
                    {
                        paramSchema = BuildObjectSchema(actualType);
                    }
                    else
                    {
                        paramSchema["type"] = "string"; // Default fallback
                    }
                    
                    if (descriptionAttr != null && !string.IsNullOrEmpty(descriptionAttr.Description))
                    {
                        paramSchema["description"] = descriptionAttr.Description;
                    }
                    
                    properties![param.Name] = paramSchema;
                    
                    // Add to required if not nullable and no default value
                    if (underlyingType != null || param.HasDefaultValue)
                    {
                        // Nullable or has default value - not required
                    }
                    else
                    {
                        required!.Add(param.Name);
                    }
                }
                
                return schema;
            }
            
            // Helper function to build schema for complex objects (records, classes)
            JsonObject BuildObjectSchema(Type objectType)
            {
                var objectSchema = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject(),
                    ["required"] = new JsonArray()
                };
                
                var props = objectSchema["properties"] as JsonObject;
                var req = objectSchema["required"] as JsonArray;
                
                // Get properties from the type (works for records and classes)
                var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var prop in properties)
                {
                    var propSchema = new JsonObject();
                    var propType = prop.PropertyType;
                    var underlyingPropType = Nullable.GetUnderlyingType(propType);
                    var actualPropType = underlyingPropType ?? propType;
                    
                    // Map property types to JSON schema types
                    if (actualPropType == typeof(string))
                    {
                        propSchema["type"] = "string";
                    }
                    else if (actualPropType == typeof(int))
                    {
                        propSchema["type"] = "integer";
                    }
                    else if (actualPropType == typeof(decimal))
                    {
                        propSchema["type"] = "number";
                    }
                    else if (actualPropType == typeof(bool))
                    {
                        propSchema["type"] = "boolean";
                    }
                    else if (actualPropType == typeof(Guid))
                    {
                        propSchema["type"] = "string";
                    }
                    else if (actualPropType == typeof(Dictionary<string, string>))
                    {
                        propSchema["type"] = "object";
                        propSchema["additionalProperties"] = new JsonObject { ["type"] = "string" };
                    }
                    else
                    {
                        propSchema["type"] = "string"; // Fallback
                    }
                    
                    props![prop.Name] = propSchema;
                    
                    // Add to required if not nullable
                    if (underlyingPropType == null)
                    {
                        // For records, constructor parameters without defaults are required
                        // For classes, non-nullable properties are required
                        if (actualPropType.IsValueType && actualPropType != typeof(bool))
                        {
                            req!.Add(prop.Name);
                        }
                        else if (!actualPropType.IsValueType)
                        {
                            // For reference types, check if they have default values
                            try
                            {
                                var instance = Activator.CreateInstance(objectType);
                                var defaultValue = prop.GetValue(instance);
                                if (defaultValue == null)
                                {
                                    req!.Add(prop.Name);
                                }
                            }
                            catch
                            {
                                // If we can't create instance, assume required for non-nullable reference types
                                req!.Add(prop.Name);
                            }
                        }
                    }
                }
                
                return objectSchema;
            }
            
            // Helper function to build Tool object manually
            Tool BuildTool(string name, string description, MethodInfo method, JsonObject? metadata = null)
            {
                var inputSchema = BuildInputSchema(method);
                
                // Convert JsonObject to JsonElement for InputSchema
                var inputSchemaJson = inputSchema.ToJsonString();
                var inputSchemaElement = JsonSerializer.Deserialize<JsonElement>(inputSchemaJson);
                
                var tool = new Tool
                {
                    Name = name,
                    Description = description,
                    InputSchema = inputSchemaElement
                };
                
                // Add _meta if provided
                if (metadata != null)
                {
                    // Serialize tool to JSON, add _meta, and deserialize back
                    var toolJson = System.Text.Json.JsonSerializer.Serialize(tool);
                    var toolNode = JsonNode.Parse(toolJson) as JsonObject;
                    if (toolNode != null)
                    {
                        toolNode["_meta"] = metadata;
                        var modifiedTool = System.Text.Json.JsonSerializer.Deserialize<Tool>(
                            toolNode.ToJsonString(), 
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (modifiedTool != null)
                        {
                            return modifiedTool;
                        }
                    }
                }
                
                return tool;
            }
            
            // Configure ListToolsHandler to build tools manually from registry
            mcpOptions.Handlers.ListToolsHandler = async (request, ct) =>
            {
                // Create a new scope using IServiceScopeFactory (HttpContext may be disposed)
                using var scope = serviceScopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var metadataService = scope.ServiceProvider.GetRequiredService<Server.Common.Mcp.McpToolMetadataService>();
                var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                
                // Get tool registry from HttpContext.Items
                var registry = toolRegistry;
                if (httpContextAccessor.HttpContext?.Items.TryGetValue("ToolRegistry", out var registryFromContext) == true)
                {
                    registry = registryFromContext as Dictionary<string, (Type Type, MethodInfo Method, string Name, string Description)> ?? registry;
                }
                var toolList = new List<Tool>();
                
                logger.LogInformation("[ListToolsHandler] Processing {Count} tools from registry", registry.Count);
                
                foreach (var (toolName, (toolType, method, name, desc)) in registry)
                {
                    // Get metadata for this tool
                    var metadata = metadataService.GetClonedMetadataForTool(toolName);
                    
                    // Build tool manually
                    var tool = BuildTool(name, desc, method, metadata);
                    toolList.Add(tool);
                    
                    logger.LogInformation("[ListToolsHandler] Added tool '{ToolName}' {WithMeta}", toolName, metadata != null ? "with _meta" : "without _meta");
                }
                
                logger.LogInformation("[ListToolsHandler] Returning {Count} unique tools", toolList.Count);
                return new ListToolsResult { Tools = toolList };
            };
            
            // Configure CallToolHandler to execute tools manually
            mcpOptions.Handlers.CallToolHandler = async (request, ct) =>
            {
                // Create a new scope using IServiceScopeFactory (HttpContext may be disposed)
                using var scope = serviceScopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                
                if (request.Params?.Name == null)
                {
                    throw new InvalidOperationException("Missing tool name");
                }
                
                var toolName = request.Params.Name;
                
                // Get tool registry from HttpContext.Items
                var registry = toolRegistry;
                if (httpContextAccessor.HttpContext?.Items.TryGetValue("ToolRegistry", out var registryFromContext) == true)
                {
                    registry = registryFromContext as Dictionary<string, (Type Type, MethodInfo Method, string Name, string Description)> ?? registry;
                }
                
                if (!registry.TryGetValue(toolName, out var toolInfo))
                {
                    throw new InvalidOperationException($"Unknown tool: '{toolName}'");
                }
                
                var (toolType, method, _, _) = toolInfo;
                
                // Create a new instance of the tool from DI (within the handler's scope)
                var instance = scope.ServiceProvider.GetRequiredService(toolType);
                
                // Get arguments from request
                var args = request.Params.Arguments ?? new Dictionary<string, JsonElement>();
                
                // Build parameter array for method invocation
                var parameters = method.GetParameters();
                var paramValues = new object?[parameters.Length];
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    
                    // Handle special parameters
                    if (param.ParameterType == typeof(McpServer))
                    {
                        // We don't have access to McpServer here, pass null or create a dummy
                        paramValues[i] = null;
                        continue;
                    }
                    
                    if (param.ParameterType == typeof(CancellationToken))
                    {
                        paramValues[i] = ct;
                        continue;
                    }
                    
                    // Get value from arguments
                    if (args.TryGetValue(param.Name, out var argValue))
                    {
                        var paramType = param.ParameterType;
                        var underlyingType = Nullable.GetUnderlyingType(paramType);
                        var actualType = underlyingType ?? paramType;
                        
                        // Convert JsonElement to parameter type
                        if (actualType == typeof(string))
                        {
                            paramValues[i] = argValue.GetString();
                        }
                        else if (actualType == typeof(int))
                        {
                            paramValues[i] = argValue.GetInt32();
                        }
                        else if (actualType == typeof(Guid[]))
                        {
                            var guidStrings = argValue.EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                            paramValues[i] = guidStrings.Select(Guid.Parse).ToArray();
                        }
                        else
                        {
                            paramValues[i] = JsonSerializer.Deserialize(argValue.GetRawText(), param.ParameterType);
                        }
                    }
                    else if (param.HasDefaultValue)
                    {
                        paramValues[i] = param.DefaultValue;
                    }
                    else if (param.ParameterType.IsValueType && Nullable.GetUnderlyingType(param.ParameterType) == null)
                    {
                        paramValues[i] = Activator.CreateInstance(param.ParameterType);
                    }
                    else
                    {
                        paramValues[i] = null;
                    }
                }
                
                // Invoke method
                logger.LogInformation("[CallToolHandler] Executing tool '{ToolName}'", toolName);
                var result = method.Invoke(instance, paramValues);
                
                // Handle async methods
                if (result is Task<CallToolResult> taskResult)
                {
                    return await taskResult;
                }
                else if (result is CallToolResult directResult)
                {
                    return directResult;
                }
                else
                {
                    throw new InvalidOperationException($"Tool '{toolName}' returned unexpected result type");
                }
            };
            
            // Helper function to build Resource object manually
            Resource BuildResource(string uri, string? title, string mimeType, JsonObject? metadata = null)
            {
                var resource = new Resource
                {
                    Uri = uri,
                    Name = title ?? uri.Split('/').Last(),
                    MimeType = mimeType,
                    Description = title ?? string.Empty
                };
                
                // Add _meta if provided
                if (metadata != null)
                {
                    // Serialize resource to JSON, add _meta, and deserialize back
                    var resourceJson = System.Text.Json.JsonSerializer.Serialize(resource);
                    var resourceNode = JsonNode.Parse(resourceJson) as JsonObject;
                    if (resourceNode != null)
                    {
                        resourceNode["_meta"] = metadata;
                        var modifiedResource = System.Text.Json.JsonSerializer.Deserialize<Resource>(
                            resourceNode.ToJsonString(), 
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (modifiedResource != null)
                        {
                            return modifiedResource;
                        }
                    }
                }
                
                return resource;
            }
            
            // Configure ListResourcesHandler to build resources manually from registry
            mcpOptions.Handlers.ListResourcesHandler = async (request, ct) =>
            {
                // Create a new scope using IServiceScopeFactory (HttpContext may be disposed)
                using var scope = serviceScopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var resourceMetadataService = scope.ServiceProvider.GetRequiredService<Server.Common.Mcp.McpResourceMetadataService>();
                var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                
                // Get resource registry from HttpContext.Items
                var registry = resourceRegistry;
                if (httpContextAccessor.HttpContext?.Items.TryGetValue("ResourceRegistry", out var registryFromContext) == true)
                {
                    registry = registryFromContext as Dictionary<string, (Type Type, MethodInfo Method, string Uri, string? Title, string MimeType)> ?? registry;
                }
                
                var resourceList = new List<Resource>();
                
                logger.LogInformation("[ListResourcesHandler] Processing {Count} resources from registry", registry.Count);
                
                foreach (var (uri, (resourceType, method, resourceUri, title, mimeType)) in registry)
                {
                    // Get metadata for this resource
                    var metadata = resourceMetadataService.GetClonedMetadataForResource(resourceUri);
                    
                    // Build resource manually
                    var resource = BuildResource(resourceUri, title, mimeType, metadata);
                    resourceList.Add(resource);
                    
                    logger.LogInformation("[ListResourcesHandler] Added resource '{ResourceUri}' {WithMeta}", resourceUri, metadata != null ? "with _meta" : "without _meta");
                }
                
                logger.LogInformation("[ListResourcesHandler] Returning {Count} unique resources", resourceList.Count);
                return new ListResourcesResult { Resources = resourceList };
            };
            
            // Configure ReadResourceHandler to execute resources manually
            mcpOptions.Handlers.ReadResourceHandler = async (request, ct) =>
            {
                // Create a new scope using IServiceScopeFactory (HttpContext may be disposed)
                using var scope = serviceScopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                
                if (request.Params?.Uri == null)
                {
                    throw new InvalidOperationException("Missing resource URI");
                }
                
                var resourceUri = request.Params.Uri;
                
                // Get resource registry from HttpContext.Items
                var registry = resourceRegistry;
                if (httpContextAccessor.HttpContext?.Items.TryGetValue("ResourceRegistry", out var registryFromContext) == true)
                {
                    registry = registryFromContext as Dictionary<string, (Type Type, MethodInfo Method, string Uri, string? Title, string MimeType)> ?? registry;
                }
                
                if (!registry.TryGetValue(resourceUri, out var resourceInfo))
                {
                    throw new InvalidOperationException($"Unknown resource: '{resourceUri}'");
                }
                
                var (resourceType, method, _, _, _) = resourceInfo;
                
                // Create a new instance of the resource from DI (within the handler's scope)
                var instance = scope.ServiceProvider.GetRequiredService(resourceType);
                
                // Build parameter array for method invocation
                var parameters = method.GetParameters();
                var paramValues = new object?[parameters.Length];
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    
                    // Handle special parameters
                    if (param.ParameterType == typeof(McpServer))
                    {
                        paramValues[i] = null;
                        continue;
                    }
                    
                    if (param.ParameterType == typeof(CancellationToken))
                    {
                        paramValues[i] = ct;
                        continue;
                    }
                    
                    // Resources typically don't have other parameters, but handle defaults
                    if (param.HasDefaultValue)
                    {
                        paramValues[i] = param.DefaultValue;
                    }
                    else if (param.ParameterType.IsValueType && Nullable.GetUnderlyingType(param.ParameterType) == null)
                    {
                        paramValues[i] = Activator.CreateInstance(param.ParameterType);
                    }
                    else
                    {
                        paramValues[i] = null;
                    }
                }
                
                // Invoke method
                logger.LogInformation("[ReadResourceHandler] Executing resource '{ResourceUri}'", resourceUri);
                var result = method.Invoke(instance, paramValues);
                
                // Handle async methods
                if (result is Task<ReadResourceResult> taskResult)
                {
                    return await taskResult;
                }
                else if (result is ReadResourceResult directResult)
                {
                    return directResult;
                }
                else
                {
                    throw new InvalidOperationException($"Resource '{resourceUri}' returned unexpected result type");
                }
            };
        };
    });

// Add CORS for ChatGPT UI and ngrok
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowChatGPT", policy =>
    {
        policy.WithOrigins("https://chat.openai.com", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    
    // Allow all origins for ngrok (development only)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use forwarded headers middleware (must be first to update Request.Scheme from ngrok)
app.UseForwardedHeaders();

// Enable CORS (use AllowAll for ngrok, AllowChatGPT for production)
app.UseCors("AllowAll");

// Serve static files from wwwroot
app.UseStaticFiles();

    // Default route to serve index.html
    // Use WebRootPath which is automatically set to wwwroot for Web SDK projects
    app.MapGet("/", () => 
    {
        var wwwrootPath = app.Environment.WebRootPath;
        if (!string.IsNullOrEmpty(wwwrootPath))
        {
            var indexPath = Path.Combine(wwwrootPath, "index.html");
            if (File.Exists(indexPath))
            {
                return Results.File(indexPath, "text/html");
            }
        }
        // Fallback: try relative path (for development)
        return Results.File("wwwroot/index.html", "text/html");
    });

    // Route to serve test widget HTML (for local testing)
    app.MapGet("/test-widget.html", () =>
    {
        var widgetPath = "/tmp/catalog-widget.html";
        if (File.Exists(widgetPath))
        {
            return Results.File(widgetPath, "text/html");
        }
        return Results.NotFound("Widget file not found. Run 'make open-widget' first.");
    });

// Mount MCP Server on HTTP endpoint
app.MapMcp("/mcp");

await app.RunAsync();

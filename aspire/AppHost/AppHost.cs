var builder = DistributedApplication.CreateBuilder(args);

// Add Redis cache
var redis = builder.AddRedis("redis");

// Add MCP Server
var mcpServer = builder.AddProject<Projects.Server>("mcpserver")
    .WithReference(redis);

builder.Build().Run();

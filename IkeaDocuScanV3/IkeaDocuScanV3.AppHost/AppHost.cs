var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.IkeaDocuScan_Web>("ikeadocuscan-web")
    .WithHttpEndpoint(port: 44100, name:"custom-http")
    .WithHttpsEndpoint(port: 44101, name: "custom-https")
    .WithExternalHttpEndpoints();

builder.Build().Run();

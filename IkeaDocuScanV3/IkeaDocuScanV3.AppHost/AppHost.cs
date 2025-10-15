var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.IkeaDocuScan_Web>("ikeadocuscan-web")
    .WithHttpEndpoint(port: 5100, name:"custom-http")
    .WithHttpsEndpoint(port: 5101, name: "custom-https")
    .WithExternalHttpEndpoints();

builder.Build().Run();

using CapstoneProject.API.Configurations;
using CapstoneProject.API.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .ConfigureServices();

var app = builder.Build()
    .ConfigurePipeline();

// Configure application with proper order: migrations → hangfire → jobs
await app.ConfigureApplicationAsync();

app.Run();
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CapstoneProject.API.Configurations;

/// <summary>
/// Configuration for Swagger UI
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configure Swagger generation options
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Configure basic information
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CapstoneProject API",
                Version = "v1",
                Description = "API for CapstoneProject"
            });
            
            // Cấu hình phân nhóm API theo controller
            options.TagActionsBy(api =>
            {
                // Ưu tiên sử dụng Tags từ attribute được đặt trên controller hoặc action
                var controllerTags = api.ActionDescriptor.EndpointMetadata
                    .OfType<TagsAttribute>()
                    .SelectMany(attr => attr.Tags)
                    .Distinct();
                    
                if (controllerTags.Any())
                {
                    // Nếu tìm thấy tag từ attribute
                    return controllerTags.ToList();
                }
                
                // Lấy tên controller
                var controllerName = api.ActionDescriptor.RouteValues["controller"];
                
                // Xác định nhóm chính (Mobile/CMS) dựa trên đường dẫn
                var relativePath = api.RelativePath?.ToLower();
                string mainTag;
                
                if (relativePath?.Contains("/cms/") == true)
                {
                    mainTag = "CMS";
                }
                else if (relativePath?.Contains("/learner/") == true)
                {
                    mainTag = "Learner";
                }
                else if (relativePath?.Contains("/chat/") == true)
                {
                    mainTag = "Chat";
                }
                else if (relativePath?.Contains("/challenge/") == true)
                {
                    mainTag = "Challenge";
                }
                else if (relativePath?.Contains("/gameplay/") == true)
                {
                    mainTag = "Gameplay";
                }
                else if (relativePath?.Contains("/competitive/") == true)
                {
                    mainTag = "Competitive";
                }
                else if (relativePath?.Contains("/marketplace/") == true)
                {
                    mainTag = "Marketplace";
                }
                else if (relativePath?.Contains("/community/") == true)
                {
                    mainTag = "Community";
                }
                else if (relativePath?.Contains("/common") == true)
                {
                    mainTag = "Common";
                }
                else
                {
                    // Fallback nếu không phát hiện được nhóm
                    return new[] { controllerName };
                }
                
                // Tạo tag kết hợp giữa nhóm chính và tên controller
                // Ví dụ: "Admin_Auth" hoặc "Learner_Auth"
                var combinedTag = $"{mainTag}_{controllerName}";
                return new[] { mainTag, combinedTag };
            });
            
            // Sắp xếp theo tag
            options.OrderActionsBy(apiDesc => $"{apiDesc.GroupName}");
            
            // Configure JWT authentication in Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            
            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
            
            // Customize operation IDs to include controller name
            options.CustomOperationIds(apiDesc =>
            {
                if (apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                {
                    var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"];
                    return $"{controllerName}_{methodInfo.Name}";
                }
                return null;
            });
        });
        
        return services;
    }
    
    /// <summary>
    /// Configure Swagger middleware
    /// </summary>
    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger(c =>
        {
            // Using the default route template to match the SwaggerUI expectation
            // c.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });
        
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CapstoneProject API v1");
            options.RoutePrefix = "swagger";
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.EnableDeepLinking();
            // options.DisplayOperationId(); // Commented out to hide operation IDs
            options.EnableFilter();
            
            // Enable tag grouping
            options.EnableTryItOutByDefault();
            
            // Custom CSS to style main tags differently
            options.InjectStylesheet("/swagger-custom/custom-swagger-ui.css");
        });
        
        return app;
    }
}

/// <summary>
/// Attribute to specify application group and controller group for API controllers
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class TagsAttribute : Attribute
{
    /// <summary>
    /// Gets the tags used by the action or controller
    /// </summary>
    public string[] Tags { get; }

    /// <summary>
    /// Creates a new TagsAttribute with the specified tags
    /// </summary>
    /// <param name="tags">The tags to apply to the action or controller</param>
    public TagsAttribute(params string[] tags)
    {
        Tags = tags;
    }
} 
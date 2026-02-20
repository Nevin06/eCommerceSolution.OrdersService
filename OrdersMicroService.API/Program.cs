using eCommerce.OrdersMicroService.BusinessLogicLayer;
using eCommerce.OrdersMicroService.DataAccessLayer;
using Microsoft.AspNetCore.Builder;
using OrdersMicroService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DAL and BLL services 83
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer();

// Add Controllers
builder.Services.AddControllers();

// Add Authentication and Authorization services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

// Add Swagger services for API documentation
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Allow requests from the specified origin (e.g., Angular development server)
        //.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.UseSwagger();
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("v1/swagger.json", "OrdersMicroService.API v1")
    );

app.UseHttpsRedirection();

app.MapControllers();

app.UseCors();

app.Run();

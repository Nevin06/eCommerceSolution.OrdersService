using eCommerce.OrdersMicroService.BusinessLogicLayer;
using eCommerce.OrdersMicroService.BusinessLogicLayer.HttpClients;
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
        builder.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")// Allow requests from the specified origin (e.g., Angular development server)
        //.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient<UsersMicroserviceClient>
    (client =>
    {
        //client.BaseAddress = new Uri("http://localhost:9090");
        client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:" +
            $"{builder.Configuration["UsersMicroservicePort"]}");
    }
    );

builder.Services.AddHttpClient<ProductsMicroserviceClient>
    (client =>
    {
        //client.BaseAddress = new Uri("http://localhost:8080");
        client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:" +
            $"{builder.Configuration["ProductsMicroservicePort"]}");
    }
    ); // 110

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

app.UseRouting();
app.UseCors();

app.UseAuthorization();
app.UseAuthentication();

app.UseSwagger();
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("v1/swagger.json", "OrdersMicroService.API v1")
    );

//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}

app.MapControllers();


app.Run();

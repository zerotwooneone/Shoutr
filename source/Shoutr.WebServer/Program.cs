using Shoutr.WebServer.Frontend;
using Shoutr.WebServer.Hubs;
using Shoutr.WebServer.Reactive;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddFrontend();
builder.Services.AddReactive();

builder.Services.AddHostedService<StartupService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapHub<FrontEndHub>("/frontend");
app.UseStaticFiles();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    //this must be AFTER UseRouting, but BEFORE UseAuthorization
    app.UseCors();
}


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("webapp/index.html");

app.Run();
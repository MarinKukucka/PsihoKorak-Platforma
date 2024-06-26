using Microsoft.EntityFrameworkCore;
using PsihoKorak_Platforma.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

IConfiguration configuration = builder.Configuration.AddJsonFile("appsettings.json").Build();

builder.Services.AddDbContext<PsihoKorakPlatformaContext>(
    options => options.UseSqlServer(configuration.GetConnectionString("PsihoKorakPlatforma"))
);

builder.Services.AddSession();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Psychologist}/{action=Index}/{id?}");

app.Run();

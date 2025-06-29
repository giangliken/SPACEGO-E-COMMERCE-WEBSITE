using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBrandRepository, EFBrandRepository>();
builder.Services.AddScoped<ICartItemRepository, EFCartItemRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IDetailCartItemRepository, EFDetailCartItemRepository>();
builder.Services.AddScoped<IOrderProductRepository, EFOrderProductRepository>();
//builder.Services.AddScoped<IProvinceRepository, EFProvinceRepository>();
//builder.Services.AddScoped<IDistrictRepository, EFDistrictRepository>();
//builder.Services.AddScoped<IWardRepository, EFWardRepository>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddScoped<IProductVariantRepository,EFProductVariantRepository>();
builder.Services.AddScoped<ICapacityRepository, EFCapacityRepository>();
builder.Services.AddScoped<IColorRepository, EFColorRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();
builder.Services.AddScoped<IProductImageRepository, EFProductImageRepository>();
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<IReviewRepository, EFReviewRepository>();
builder.Services.AddScoped<IActivityLogService, EFActivityLogService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IProvinceRepository, EFProvinceRepository>();
builder.Services.AddScoped<IDistrictRepository, EFDistrictRepository>();
builder.Services.AddScoped<IWardRepository, EFWardRepository>();
builder.Services.AddScoped<ICommentRepository, EFCommentRepository>();
builder.Services.AddScoped<IPostRepository, EFPostRepository>();

// Add authentication services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddDefaultTokenProviders()
.AddDefaultUI()
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddTransient<ApplicationUserManager>();

builder.Services.AddRazorPages();

// Add Google authentication
builder.Services.AddAuthentication(options =>
{
    //options.DefaultScheme = "Cookies";
    //options.DefaultChallengeScheme = "Google";
    options.DefaultScheme = IdentityConstants.ApplicationScheme; // hoặc "Cookies" nếu bạn dùng cookie
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie("Cookies")
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    options.SaveTokens = true;

    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.ClaimActions.MapJsonKey("picture", "picture");
    options.ClaimActions.MapJsonKey("locale", "locale");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(SD.Role_Admin, policy => policy.RequireRole(SD.Role_Admin));
});

// Add remaining services
builder.Services
    .AddControllersWithViews()
    .AddNewtonsoftJson();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        response.Redirect("/Home/NotFound");
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
  .WithStaticAssets();

app.Run();

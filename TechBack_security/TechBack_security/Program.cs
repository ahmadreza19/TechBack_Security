using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

string Password =builder.Configuration["Password"];
var connectionBuilder = new SqlConnectionStringBuilder(builder.Configuration["cs"]);// Configure the HTTP request pipeline.
connectionBuilder.Password = Password;
connectionBuilder.UserID = "";
connectionBuilder.InitialCatalog = "";
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

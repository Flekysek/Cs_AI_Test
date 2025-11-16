using FoodAI.Data;
using FoodAI.Services;
using DotNetEnv;
// todo Added file with urls this could be great as well.


Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpClient<IWebScraperService, WebScraperService>();
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddScoped<IGeminiAIService, GeminiAIService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

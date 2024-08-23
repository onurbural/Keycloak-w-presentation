using ClientApp;
using ClientApp.RefitServices;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.ComponentModel.Design;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddRefitClient<IRefitService>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7191/"));
builder.Services.AddRefitClient<IKeycloakService>().ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:8080/"));

await builder.Build().RunAsync();

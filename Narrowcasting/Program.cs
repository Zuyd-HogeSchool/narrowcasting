using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Narrowcasting.Data;
using Narrowcasting.Interfaces;
using Narrowcasting.Models;
using Narrowcasting.Services;
using System.Text.Json.Serialization;

namespace Narrowcasting
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure Serilog early so startup errors are captured
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .WriteTo.Console()
            //    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
            //    .CreateLogger();
            // Capture unhandled exceptions from AppDomain and TaskScheduler
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                //try { Log.Fatal((Exception?)e.ExceptionObject, "Unhandled domain exception"); }
                //catch { }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try
                {
                    //Log.Fatal(e.Exception, "Unobserved task exception");
                    //e.SetObserved();
                }
                catch { }
            };

            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password rules
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Sign-in
                options.SignIn.RequireConfirmedAccount = false;

                // Lockout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            // Cookie path
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(new
                        {
                            error = "Unauthorized",
                            message = "Je moet ingelogd zijn om deze API te gebruiken."
                        });
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(new
                        {
                            error = "Forbidden",
                            message = "Je hebt geen toegang tot deze API of afdeling."
                        });
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            // Services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserContextService, UserContextService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IScreenService, ScreenService>();
            builder.Services.AddScoped<IPlaylistService, PlaylistService>();
            builder.Services.AddScoped<IPlaylistItemService, PlaylistItemService>();
            builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IMediaFileService, MediaFileService>();

            // Razor Pages
            builder.Services.AddRazorPages(options =>
            {
                // All /Admin pages require login by default
                options.Conventions.AuthorizeFolder("/Admin");
                // Users management: Admin only
                options.Conventions.AuthorizeFolder("/Admin/Users", "Admin");
                options.Conventions.AuthorizeFolder("/Admin/AuditLog", "Admin");
                // Login page is public
                options.Conventions.AllowAnonymousToPage("/Admin/Index");
            });

            builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 300 * 1024 * 1024; // 300 MB ceiling
            });

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 300 * 1024 * 1024; // 300 MB
            });

            // API controllers
            builder.Services.AddControllers()
                 .AddJsonOptions(options =>
                 {
                     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                 });

            // Role-based policy
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", p => p.RequireRole("Admin"));
                options.AddPolicy("Employee", p => p.RequireRole("Admin", "Employee"));
            });

            var app = builder.Build();

            // Database migration + role seeding - only run when this assembly is the entry assembly
            // Only run migrations/seed when running the web app in Development environment.
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    await db.Database.MigrateAsync();

                    // Ensure roles exist
                    foreach (var role in new[] { "Admin", "Employee" })
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                            await roleManager.CreateAsync(new IdentityRole(role));
                    }

                    const string adminEmail = "admin@ziekenhuis.nl";
                    const string adminPassword = "Admin@123!";

                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser is null)
                    {
                        adminUser = new ApplicationUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true,
                            FullName = "Systeem Administrator",
                            DepartmentId = null,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                        if (!createResult.Succeeded)
                        {
                            //Log.Warning("Could not create default admin account: {Errors}",
                            //    string.Join("; ", createResult.Errors.Select(e => e.Description)));
                        }
                    }

                    // Ensure the account is in the Admin role (idempotent)
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                        if (!addRoleResult.Succeeded)
                        {
                            //Log.Warning("Could not add admin role to account: {Errors}",
                            //    string.Join("; ", addRoleResult.Errors.Select(e => e.Description)));
                        }
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Keep runtime uploads outside the web project so dotnet-watch does not restart on uploads.
            var uploadsPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "UploadedFiles"));
            Directory.CreateDirectory(uploadsPath);

            // Serve uploaded files under /uploads
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads"
            });

            app.UseRouting();
            //app.UseSerilogRequestLogging();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();
            app.MapControllers();

            try
            {
                await app.RunAsync();
            }
            finally
            {
                //await Log.CloseAndFlushAsync();
            }
        }
    }
}

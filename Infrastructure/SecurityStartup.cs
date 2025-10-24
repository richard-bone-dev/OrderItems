using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Api.Infrastructure.Persistence;

namespace Api.Infrastructure
{
    public static class SecurityStartup
    {
        public static void AddSecurityServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            _ = config;
            _ = env;
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/Login";
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (IsApiRequest(context.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (IsApiRequest(context.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // --- HTTPS + HSTS ---
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 443;
            });

            // --- Anti-forgery ---
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "__RequestVerificationToken";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // --- Secure cookies globally ---
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.Always;
            });

            // --- Rate Limiting ---
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("default", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromMinutes(1);
                });
            });
        }

        public static void UseSecurityPipeline(this WebApplication app)
        {
            // --- Error Handling ---
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // --- HTTPS Redirection ---
            app.UseHttpsRedirection();

            // --- Security Headers ---
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

                // --- Content Security Policy (CSP) ---
                context.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data:; " +
                    "font-src 'self' data:; " +
                    "object-src 'none'; " +
                    "base-uri 'none'; " +
                    "frame-ancestors 'none';";

                await next();
            });

            // --- Enforce cookie policy ---
            app.UseCookiePolicy();

            // --- Rate Limiting ---
            app.UseRateLimiter();
        }

        private static bool IsApiRequest(HttpRequest request)
        {
            if (request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (request.Headers.TryGetValue("Accept", out var accepts))
            {
                return accepts.Any(a => a.Contains("application/json", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }
    }
}

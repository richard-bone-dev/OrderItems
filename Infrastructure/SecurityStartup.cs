using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Infrastructure
{
    public static class SecurityStartup
    {
        public static void AddSecurityServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
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
    }
}

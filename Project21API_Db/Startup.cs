using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Project21API_Db.AuthContactApp;
using Project21API_Db.ContextFolder;
using Project21API_Db.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project21API_Db
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidIssuer = AuthOptions.ISSUER,
               ValidateAudience = true,
               ValidAudience = AuthOptions.AUDIENCE,
               ValidateLifetime = true,
               IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
               ValidateIssuerSigningKey = true
           };
       });
            services.AddAuthorization();

            services.AddScoped<ContactData>();
            services.AddDbContext<DataContext>();

            services.AddControllers();

            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<DataContext>()
                    .AddDefaultTokenProviders()
                    .AddRoles<IdentityRole>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/login", (UserLoginProp loginData) =>
                {

                    using (var context = new DataContext())
                    {

                        User newUser = new User()
                        {
                            UserName = loginData.UserName
                        };

                        User? user = context.Users.FirstOrDefault(p => p.UserName == newUser.UserName);

                        if (user is null) return Results.Unauthorized();
                        else
                        {
                            var passwordHasher = new PasswordHasher<string>();
                            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginData.Password);
                            switch (passwordVerificationResult)
                            {
                                case PasswordVerificationResult.Failed:
                                    return Results.Unauthorized();
                            }
                        }
                        var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.UserName) };

                        var jwt = new JwtSecurityToken(
                            issuer: AuthOptions.ISSUER,
                            audience: AuthOptions.AUDIENCE,
                            claims: claims,
                            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                        var response = new
                        {
                            access_token = encodedJwt,
                            username = user.UserName
                        };
                        return Results.Json(response);
                    }
                });

                endpoints.MapGet("/data", [Authorize] (HttpContext context) => $"Hello World!");
                endpoints.MapControllers();
            });
    }
        record class UserLoginProp(string UserName, string Password);
        public class AuthOptions
        {
            public const string ISSUER = "MyAuthServer"; // издатель токена
            public const string AUDIENCE = "MyAuthClient"; // потребитель токена
            const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
            public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using University_Management_System.Data;
using University_Management_System.Data.Seed;
using University_Management_System.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

//For LoginSignup

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//For EmailSmtp
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EnrollmentSyncService>();

//JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}
).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer=true,
        ValidateAudience=true,
        ValidateLifetime=true,
        ValidateIssuerSigningKey=true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience= builder.Configuration["JWT:Audience"],
        IssuerSigningKey= new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])
            )
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = context.Principal?.Identity?.Name;

            var user = !string.IsNullOrEmpty(userId)
                ? await userManager.FindByIdAsync(userId)
                : (!string.IsNullOrEmpty(userName) ? await userManager.FindByNameAsync(userName) : null);

            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Contains(University_Management_System.Constants.SystemRoles.Admin))
                {
                    var adminProfile = await dbContext.AdminProfiles
                        .FirstOrDefaultAsync(a => a.UserId == user.Id || (a.Email != null && a.Email.ToLower() == user.Email.ToLower()));

                    if (adminProfile != null && !adminProfile.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail("Admin account has been deactivated.");
                    }
                }
            }
        }
    };
});



//Policies for Authorization

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin Access", policy =>
    {
        policy.RequireRole(
            University_Management_System.Constants.SystemRoles.superAdmin,
            University_Management_System.Constants.SystemRoles.Admin
        );
    });

    options.AddPolicy("TeacherAccess", policy =>
    {
        policy.RequireRole(University_Management_System.Constants.SystemRoles.Teacher);
    });

    options.AddPolicy("StudentAccess", policy =>
    {
        policy.RequireRole(University_Management_System.Constants.SystemRoles.Student);
    });
});

//Axios Permission

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope= app.Services.CreateScope())
{

    //scope for roles
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    //scope for user

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<IdentityUser>>();

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await RoleSeeder.SeedRolesAsync(roleManager);
    await SuperAdminSeeder.SuperAdminSeederAsync(userManager,app.Configuration);
    await AcademicDataSeeder.SeedAcademicDataAsync(dbContext);
    await AcademicCurriculumSeeder.SeedCurriculumAsync(dbContext);

    var enrollmentSyncService = scope.ServiceProvider.GetRequiredService<EnrollmentSyncService>();
    await enrollmentSyncService.SynchronizeAllActiveSemestersAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

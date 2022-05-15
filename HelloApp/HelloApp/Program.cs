using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// �������� �� � ��������������
var people = new List<Person>
{
    new Person("tom@gmail.com", "12345"),
    new Person("bob@gmail.com", "55555")
};

var builder = WebApplication.CreateBuilder();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", (Person loginData) =>
{
����// ������� ������������ 
����Person? person = people.FirstOrDefault(p => p.Email == loginData.Email && p.Password == loginData.Password);
����// ���� ������������ �� ������, ���������� ��������� ��� 401
����if (person is null) return Results.Unauthorized();

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.Email) };
����// ������� JWT-�����
����var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

    // ��������� �����
    var response = new
    {
        access_token = encodedJwt,
        username = person.Email
    };

    return Results.Json(response);
});
app.Map("/data", [Authorize] () => new { message = "Hello World!" });

app.Run();

public class AuthOptions
{
    public const string ISSUER = "MyAuthServer"; // �������� ������
����public const string AUDIENCE = "MyAuthClient"; // ����������� ������
����const string KEY = "mysupersecret_secretkey!123";�� // ���� ��� ��������
����public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}

record class Person(string Email, string Password);


/* ��������� ������ ����� � metanit.com/sharp/aspnet6/11.1.php
����� ��������� �������� �� ��������� ������ � ����� �� �����.
���������������� � �������� ���������

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder();

// ���������� �������� ��������������
builder.Services.AddAuthentication("Bearer")  // ����� �������������� - � ������� jwt-�������
    .AddJwtBearer();      // ����������� �������������� � ������� jwt-�������
builder.Services.AddAuthorization();����������� // ���������� �������� �����������

// ���������� � ����� ������ applicationdb
string connection = "Server=(localdb)\\mssqllocaldb;Database=applicationdb;Trusted_Connection=True;";

// ��������� ����� ApplicationContext � ������� ����������
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));


var app = builder.Build();

app.UseAuthentication();   // ���������� middleware �������������� 
app.UseAuthorization();   // ���������� middleware ����������� 
// ���������� ��������� ��� ����������� �����������
app.Map("/hello", [Authorize] () => "Hello World!");
app.Map("/", () => "Home Page");

// ���������� ���������������� ����������� ������
app.UseDefaultFiles();
app.UseStaticFiles();

// ����������� ������ �������� Users �� ������� ����� ������ � ��
app.MapGet("/api/users", async (ApplicationContext db) => await db.Users.ToListAsync());

// GET ������ // ����������� ������ ������� Users �� ����, ���� ������ ����������, ����� 404
app.MapGet("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
����// �������� ������������ �� id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

����// ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (user == null) return Results.NotFound(new { message = "������������ �� ������" });

    // ���� ������������ ������, ���������� ���
    return Results.Json(user);
});

// DELETE ������ // �������� ������ ������� Person �� ����, ���� ������ ����������, ����� 404
app.MapDelete("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
����// �������� ������������ �� id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (user == null) return Results.NotFound(new { message = "������������ �� ������" });

    // ���� ������������ ������, ������� ���
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.Json(user);
});

// POST ������ // �������� ������������ (��������� ���� � ���������� � ������),
// � ����������� ��� ������� �� �������
app.MapPost("/api/users", async (User user, ApplicationContext db) =>
{
    // ��������� ������������ � ������
    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();
    return user;
});

// PUT ������ // ��������� ������������ �� �������, ���� ������������ ����������, ����� 404
app.MapPut("/api/users", async (User userData, ApplicationContext db) =>
{

    // �������� ������������ �� id
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userData.Id);

����// ���� �� ������, ���������� ��������� ��� � ��������� �� ������
����if (user == null) return Results.NotFound(new { message = "������������ �� ������" });

    // ���� ������������ ������, �������� ��� ������ � ���������� ������� �������
    user.Age = userData.Age;
    user.Name = userData.Name;
    await db.SaveChangesAsync();
    return Results.Json(user);
});

app.Run();

public static AuthenticationBuilder AddAuthentication(this IServiceCollection services);
public static AuthenticationBuilder AddAuthentication(this IServiceCollection services, string defaultScheme);
public static AuthenticationBuilder AddAuthentication(this IServiceCollection services, Action>AuthenticationOptions> configureOptions);

*/
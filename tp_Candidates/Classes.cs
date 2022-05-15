using System.Text;
using Microsoft.IdentityModel.Tokens;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = ""; // имя пользователя
    public int Age { get; set; } // возраст пользователя
}

public class AuthOptions
{
    public const string ISSUER = "MyAuthServer"; // издатель токена
    public const string AUDIENCE = "MyAuthClient"; // потребитель токена
    const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}

record class Person(string Email, string Password);

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RemindersApi.Contracts;
using RemindersApi.Model;

namespace RemindersApi.Services;

// JwtService
// זה המימוש בפועל של החוזה IJwtService. המחלקה נרשמת בקובץ Program, ונקראת
// אך ורק מתוך AuthApis, מיד אחרי שאותרו שם משתמש וסיסמה תואמים במסד הנתונים.
public class JwtService(IConfiguration config) : IJwtService
{
    // GenerateToken
    // בונה טוקן חתום עבור המשתמש שהתחבר. שלושת השדות שנלקחים מ-config, שם
    // המפתח, שם המנפיק, ושם הקהל, מוגדרים כולם בקובץ appsettings, ואותם
    // בדיוק ערכים נבדקים בחזרה מאוחר יותר, בקובץ Program, בכל בקשה שמגיעה
    // עם טוקן.
    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // שני פרטי המידע היחידים שנשמרים בתוך הטוקן: שם המשתמש, והתפקיד שלו.
        // התפקיד הוא זה שנבדק אחר כך מול המדיניות AdminOnly.
        var claims = new[] {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

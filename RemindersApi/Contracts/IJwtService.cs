using RemindersApi.Model;

namespace RemindersApi.Contracts;

// IJwtService
// חוזה קצר עם מתודה אחת בלבד. המימוש בפועל נמצא בקובץ JwtService, בתיקיית
// Services, והמתודה הזו נקראת ממקום אחד יחיד בכל הפרויקט - מתוך AuthApis,
// מיד אחרי שאותרו שם משתמש וסיסמה תואמים.
public interface IJwtService
{
    string GenerateToken(User user);
}

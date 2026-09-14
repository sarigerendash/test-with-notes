using Microsoft.EntityFrameworkCore;
using RemindersApi.Contracts;
using RemindersApi.Data;
using RemindersApi.Model;

namespace RemindersApi.Apis;

// AuthApis
// זה הקובץ הראשון שבו נוגעת כל בקשה שמנסה להתחבר. המתודה היחידה כאן נקראת
// פעם אחת בלבד, מתוך קובץ Program, ברגע שהשרת עולה - היא רק רושמת את המסלול,
// לא מריצה אותו בפועל. הריצה בפועל קורית בכל פעם שמגיעה בקשה אמיתית לנתיב.
public static class AuthApis
{
    public static void MapAuthApis(this WebApplication app)
    {
        // מסלול ההתחברות היחיד במערכת. שרשרת הקריאות כאן: קודם חיפוש ישיר
        // בתוך AppDbContext, בלי מעבר דרך שכבת Contracts כמו בשאר הפרויקט,
        // מכיוון שזו פעולה חד-פעמית ופשוטה שלא הצדיקה ממשק נפרד. אם נמצא
        // משתמש תואם, נקראת GenerateToken, שמוגדרת בקובץ JwtService, כדי
        // לבנות עבורו את הטוקן שיחזור ללקוח.
        app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db, IJwtService jwt) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.Password == req.Password);
            if (user is null) return Results.Unauthorized();
            return Results.Ok(new LoginResponse(jwt.GenerateToken(user), user.Role));
        });
    }
}

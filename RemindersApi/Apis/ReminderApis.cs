using RemindersApi.Contracts;
using RemindersApi.Model;

namespace RemindersApi.Apis;

// ReminderApis
// זה הקובץ שמרכז את כל הפעולות על תזכורות. בדיוק כמו ב-AuthApis, המתודה
// MapReminderApis רק רושמת את המסלולים פעם אחת, מתוך קובץ Program, והריצה
// בפועל של כל בלוק קורית בנפרד, בכל פעם שמגיעה בקשה מתאימה.
public static class ReminderApis
{
    public static void MapReminderApis(this WebApplication app)
    {
        // כל המסלולים כאן משותפים לקבוצה אחת, שדורשת לפחות התחברות בסיסית.
        // שני המסלולים הרגישים, יצירה ועריכה, דורשים בנוסף את המדיניות
        // AdminOnly, שמוגדרת בקובץ Program.
        var group = app.MapGroup("/reminders").RequireAuthorization();

        // מחזירה את כל התזכורות. קוראת ל-GetAllAsync, שמוגדרת בקובץ
        // ReminderService.
        group.MapGet("/", async (IReminderService svc) =>
            Results.Ok(await svc.GetAllAsync()));

        // מחזירה את כל ההיסטוריה. קוראת ל-GetHistoryAsync, שגם היא מוגדרת
        // בקובץ ReminderService.
        group.MapGet("/history", async (IReminderService svc) =>
            Results.Ok(await svc.GetHistoryAsync()));

        // יוצרת תזכורת חדשה. לפני הקריאה בפועל ל-CreateAsync, הבקשה עוברת
        // דרך המתודה Validate שנמצאת למטה בקובץ הזה. אם נמצאה בעיה, מוחזרת
        // תשובת שגיאה מיד, ו-CreateAsync לא נקראת בכלל.
        group.MapPost("/", async (ReminderRequest req, IReminderService svc) =>
        {
            var errors = Validate(req);
            if (errors is not null) return Results.ValidationProblem(errors);
            var created = await svc.CreateAsync(req);
            return Results.Created($"/reminders/{created.Id}", created);
        }).RequireAuthorization("AdminOnly");

        // מעדכנת תזכורת קיימת, לפי אותו עיקרון: קודם בדיקת תקינות, ורק אז
        // קריאה בפועל ל-UpdateAsync.
        group.MapPut("/{id:int}", async (int id, ReminderRequest req, IReminderService svc) =>
        {
            var errors = Validate(req);
            if (errors is not null) return Results.ValidationProblem(errors);
            var updated = await svc.UpdateAsync(id, req);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).RequireAuthorization("AdminOnly");
    }

    // Validate
    // בדיקת תקינות פשוטה וידנית, בלי שום ספרייה חיצונית. נקראת פעמיים למעלה,
    // פעם לפני יצירה ופעם לפני עדכון, כדי שאותם כללים בדיוק יחולו בשני
    // המקרים.
    private static Dictionary<string, string[]>? Validate(ReminderRequest req)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(req.Name))
            errors[nameof(req.Name)] = ["שם התזכורת הוא שדה חובה"];

        if (req.ScheduledAt == default)
            errors[nameof(req.ScheduledAt)] = ["יש לבחור תאריך ושעה לתזכורת"];

        if (req.FutureRunsCount < 0)
            errors[nameof(req.FutureRunsCount)] = ["מספר הרצות עתידיות לא יכול להיות שלילי"];

        return errors.Count > 0 ? errors : null;
    }
}

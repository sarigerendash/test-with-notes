using Microsoft.EntityFrameworkCore;
using RemindersApi.Contracts;
using RemindersApi.Data;
using RemindersApi.Model;

namespace RemindersApi.Services;

// ReminderService
// זה המימוש בפועל של החוזה IReminderService. כל המתודות כאן פונות ישירות אל
// AppDbContext, בלי שכבת Repository באמצע - הסיבה לכך מוסברת בקובץ README של
// הפרויקט. המחלקה הזו נרשמת בקובץ Program, ונקראת אך ורק מתוך הקבצים
// שנמצאים בתיקיית Apis.
public class ReminderService(AppDbContext db) : IReminderService
{
    // GetAllAsync
    // מחזירה את כל התזכורות שקיימות, בלי סינון. נקראת מתוך ReminderApis,
    // בתגובה לבקשת GET על הנתיב הראשי.
    public async Task<IEnumerable<ReminderResponse>> GetAllAsync() =>
        await db.Reminders.Select(r => ToResponse(r)).ToListAsync();

    // CreateAsync
    // בונה תזכורת חדשה מתוך מה שהתקבל מהלקוח, ושומרת אותה. נקראת מתוך
    // ReminderApis, בתגובה לבקשת POST, ורק אחרי שהבקשה עברה בהצלחה את בדיקת
    // התקינות שנמצאת שם.
    public async Task<ReminderResponse> CreateAsync(ReminderRequest req)
    {
        var reminder = new Reminder
        {
            Name = req.Name,
            Message = req.Message,
            ScheduledAt = req.ScheduledAt,
            Frequency = req.Frequency,
            IsActive = req.IsActive,
            FutureRunsCount = req.FutureRunsCount
        };
        db.Reminders.Add(reminder);
        await db.SaveChangesAsync();
        return ToResponse(reminder);
    }

    // UpdateAsync
    // מאתרת תזכורת קיימת לפי מזהה, ומעדכנת בה את כל השדות מחדש. אם לא נמצאה
    // תזכורת עם המזהה הזה, מוחזר null, וזה מה שגורם ל-ReminderApis להחזיר
    // ללקוח תשובת "לא נמצא".
    public async Task<ReminderResponse?> UpdateAsync(int id, ReminderRequest req)
    {
        var reminder = await db.Reminders.FindAsync(id);
        if (reminder is null) return null;
        reminder.Name = req.Name;
        reminder.Message = req.Message;
        reminder.ScheduledAt = req.ScheduledAt;
        reminder.Frequency = req.Frequency;
        reminder.IsActive = req.IsActive;
        reminder.FutureRunsCount = req.FutureRunsCount;
        await db.SaveChangesAsync();
        return ToResponse(reminder);
    }

    // GetHistoryAsync
    // מחזירה את כל ההרצות שכבר הסתיימו, מהחדשה לישנה. השורות עצמן לא נוצרות
    // כאן - הן נכתבות בקובץ נפרד לגמרי, ReminderBackgroundService, וכאן רק
    // קוראים אותן בחזרה.
    public async Task<IEnumerable<ReminderRunResponse>> GetHistoryAsync() =>
        await db.ReminderRuns
            .OrderByDescending(h => h.FinishedAt)
            .Select(h => new ReminderRunResponse(
                h.Id, h.ReminderId, h.Reminder.Name, h.Reminder.Message, h.StartedAt, h.FinishedAt, h.Status))
            .ToListAsync();

    // ToResponse
    // מתודה פנימית וקטנה שממירה Entity בודד לרשומת התשובה שהלקוח מקבל. נקראת
    // משלוש המתודות שמעל, כדי לא לחזור על אותה המרה שלוש פעמים בנפרד.
    private static ReminderResponse ToResponse(Reminder r) =>
        new(r.Id, r.Name, r.Message, r.ScheduledAt, r.Frequency, r.IsActive, r.FutureRunsCount, r.Status, r.CreatedAt);
}

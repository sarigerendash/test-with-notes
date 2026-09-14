using Microsoft.EntityFrameworkCore;
using RemindersApi.Data;
using RemindersApi.Model;

namespace RemindersApi.Services;

// ReminderBackgroundService
// זה השירות היחיד בכל הפרויקט שלא ממתין לבקשה מהלקוח, ולא נקרא על ידי אף
// קובץ אחר בקוד שלנו. הוא נרשם בקובץ Program, בשורה עם AddHostedService,
// ומאותו רגע התשתית המובנית של השרת מפעילה ועוצרת אותו לבד, לגמרי בלי קשר
// לתעבורת HTTP.
public class ReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ReminderBackgroundService> logger)
    : BackgroundService
{
    // ExecuteAsync
    // זו נקודת הכניסה שהתשתית קוראת לה אוטומטית ברגע שהשרת עולה. הלולאה כאן
    // רצה כל עוד השרת פעיל, וממתינה 30 שניות בין סבב לסבב, ובכל סבב קוראת
    // ל-ProcessPendingReminders.
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), ct);
            await ProcessPendingReminders(ct);
        }
    }

    // ProcessPendingReminders
    // מטפלת בסבב אחד: מוצאת תזכורות שהגיע זמנן, ומריצה כל אחת מהן לפי הסדר.
    //
    // המחלקה הזו עצמה נוצרת פעם אחת בלבד לאורך כל חיי האפליקציה, ולכן אי אפשר
    // להזריק אליה ישירות את AppDbContext, שאמור להתחדש בכל שימוש בנפרד. במקום
    // זה, בכל סבב נוצר כאן scope זמני משלו, שממנו מוציאים מופע טרי של מסד
    // הנתונים, ומשחררים אותו ברגע שהסבב מסתיים.
    private async Task ProcessPendingReminders(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // רק תזכורות במצב Pending, שגם פעילות וגם הגיע זמנן בפועל, נכנסות
        // לרשימה הזו. תזכורת שנקבעה לעתיד פשוט תישאר בהמתנה עד לסבב שבו התנאי
        // הזה יתקיים.
        var now = DateTime.UtcNow;
        var pending = await db.Reminders
            .Where(r => r.Status == ReminderStatus.Pending && r.IsActive && r.ScheduledAt <= now)
            .ToListAsync(ct);

        foreach (var reminder in pending)
        {
            var startedAt = DateTime.UtcNow;
            reminder.Status = ReminderStatus.Running;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Reminder {Id} is Running", reminder.Id);

            // ההמתנה הזו מדמה זמן שליחה אמיתי - אין כאן שום שליחת מייל או
            // הודעה בפועל, זו רק סימולציה.
            await Task.Delay(TimeSpan.FromSeconds(10), ct);

            // התוצאה הסופית נקבעת רנדומלית, ללא שום קשר עסקי אמיתי להצלחה
            // או לכישלון.
            reminder.Status = Random.Shared.Next(2) == 0 ? ReminderStatus.Success : ReminderStatus.Failed;

            // כל הרצה, גם מוצלחת וגם כושלת, נשמרת כאן כשורה חדשה בהיסטוריה.
            // זו הרשומה שמתודת GetHistoryAsync, בקובץ ReminderService, מציגה
            // בהמשך ללקוח.
            db.ReminderRuns.Add(new ReminderRun
            {
                ReminderId = reminder.Id,
                StartedAt = startedAt,
                FinishedAt = DateTime.UtcNow,
                Status = reminder.Status
            });
            logger.LogInformation("Reminder {Id} finished with {Status}", reminder.Id, reminder.Status);

            // אם לתזכורת יש תדירות חוזרת, וגם נשארו לה הרצות עתידיות, היא
            // חוזרת עכשיו למצב Pending עם תאריך מתקדם, כדי שהיא תיתפס שוב
            // בסבב עתידי. אחרת, היא נשארת עם התוצאה הסופית שקיבלה כרגע.
            if (reminder.Frequency != Frequency.Once && reminder.FutureRunsCount > 0)
            {
                reminder.FutureRunsCount--;
                reminder.ScheduledAt = NextOccurrence(reminder.ScheduledAt, reminder.Frequency);
                reminder.Status = ReminderStatus.Pending;
                logger.LogInformation("Reminder {Id} rescheduled to {Next}", reminder.Id, reminder.ScheduledAt);
            }

            await db.SaveChangesAsync(ct);
        }
    }

    // NextOccurrence
    // מחשבת מתי ההרצה הבאה אמורה לקרות, לפי סוג התדירות. נקראת רק ממקום אחד,
    // מתוך ProcessPendingReminders שמעל.
    private static DateTime NextOccurrence(DateTime from, Frequency frequency) => frequency switch
    {
        Frequency.Daily => from.AddDays(1),
        Frequency.Weekly => from.AddDays(7),
        Frequency.Monthly => from.AddMonths(1),
        _ => from
    };
}

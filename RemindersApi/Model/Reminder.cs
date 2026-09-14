namespace RemindersApi.Model;

// Reminder
// זו ה-Entity שמייצגת שורה אחת בטבלת התזכורות במסד הנתונים. את המחלקה הזו
// מנהל ישירות AppDbContext, דרך המאפיין Reminders שלו. Reminder לא עובר
// כמות שהוא ללקוח - כשמחזירים תשובה או מקבלים בקשה, ממירים אותו לרשומה
// מתאימה מתוך ReminderDtos.
public class Reminder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public Frequency Frequency { get; set; }
    public bool IsActive { get; set; } = true;
    public int FutureRunsCount { get; set; }

    // ברירת המחדל היא Pending, כך שתזכורת חדשה מתחילה תמיד באותו מצב, בלי
    // שהקוד שיוצר אותה יצטרך לקבוע זאת בעצמו.
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

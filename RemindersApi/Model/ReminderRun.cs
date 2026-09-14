namespace RemindersApi.Model;

// ReminderRun
// זו ה-Entity שמייצגת הרצה אחת שכבר הסתיימה, כלומר שורה אחת בהיסטוריה. רשומה
// כזו נוצרת רק במקום אחד בכל הפרויקט - בתוך ProcessPendingReminders, שנמצאת
// בקובץ ReminderBackgroundService, מיד אחרי שהתזכורת מסיימת לרוץ. נקראת חזרה
// על ידי GetHistoryAsync, שנמצאת בקובץ ReminderService.
public class ReminderRun
{
    public int Id { get; set; }
    public int ReminderId { get; set; }

    // הפניה חזרה לתזכורת המקורית, כדי שאפשר יהיה להציג בהיסטוריה גם את השם
    // וההודעה שלה, בלי לשמור אותם כפול.
    public Reminder Reminder { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public ReminderStatus Status { get; set; }
}

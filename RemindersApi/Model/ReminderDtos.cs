namespace RemindersApi.Model;

// שלוש הרשומות בקובץ הזה הן חוזה ה-API של תזכורות מול הלקוח. הן דומות מאוד
// לישות Reminder שנשמרת במסד הנתונים, אבל בכוונה נשארות נפרדות ממנה - כך
// שהלקוח לעולם לא שולח או מקבל ישירות את מבנה הטבלה הפנימי.

// ReminderRequest
// זה מה שהלקוח שולח בכל יצירה או עריכה של תזכורת. שימו לב שאין כאן Id ואין
// Status - אלה נקבעים אך ורק על ידי השרת, ואסור שהלקוח יוכל לקבוע אותם בעצמו.
public record ReminderRequest(
    string Name,
    string Message,
    DateTime ScheduledAt,
    Frequency Frequency,
    bool IsActive,
    int FutureRunsCount
);

// ReminderResponse
// זה מה שהשרת מחזיר בכל פעולה שמציגה תזכורת - כולל את השדות הנוספים שנקבעים
// רק בשרת, Status ו-CreatedAt, כדי שהלקוח יוכל להציג אותם במסך.
public record ReminderResponse(
    int Id,
    string Name,
    string Message,
    DateTime ScheduledAt,
    Frequency Frequency,
    bool IsActive,
    int FutureRunsCount,
    ReminderStatus Status,
    DateTime CreatedAt
);

// ReminderRunResponse
// זו שורה אחת במסך ההיסטוריה. שימו לב ששני השדות ReminderName ו-Message לא
// קיימים ב-Entity ששמו ReminderRun - הם מגיעים משם בזמן הבנייה, מתוך התזכורת
// המקורית, כדי שהלקוח לא יצטרך לבצע חיפוש נוסף כדי לדעת על איזו תזכורת מדובר.
public record ReminderRunResponse(
    int Id,
    int ReminderId,
    string ReminderName,
    string Message,
    DateTime StartedAt,
    DateTime FinishedAt,
    ReminderStatus Status
);

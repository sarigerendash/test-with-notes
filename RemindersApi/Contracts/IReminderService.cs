using RemindersApi.Model;

namespace RemindersApi.Contracts;

// IReminderService
// זה חוזה בלבד - אין כאן שום מימוש בפועל, רק רשימת פעולות שמישהו אחר חייב
// לספק. המימוש בפועל נמצא בקובץ ReminderService, בתיקיית Services. הקבצים
// שקוראים למתודות דרך החוזה הזה, ולא ישירות למחלקה שמממשת אותו, הם AuthApis
// ו-ReminderApis, בתיקיית Apis.
//
// היתרון בלכתוב את זה כחוזה נפרד: אפשר להחליף את המימוש בעתיד, למשל למסד
// נתונים אמיתי במקום In-Memory, מבלי לשנות שורה אחת בתוך Apis.
public interface IReminderService
{
    Task<IEnumerable<ReminderResponse>> GetAllAsync();
    Task<ReminderResponse> CreateAsync(ReminderRequest request);
    Task<ReminderResponse?> UpdateAsync(int id, ReminderRequest request);
    Task<IEnumerable<ReminderRunResponse>> GetHistoryAsync();
}

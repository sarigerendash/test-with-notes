namespace RemindersApi.Model;

// User
// זו ה-Entity שמייצגת משתמש שרשאי להתחבר למערכת. שתי השורות היחידות שקיימות
// בטבלה הזו נוצרות אוטומטית בתוך AppDbContext, בקטע שנקרא Seed, ולא נוספות
// דרך שום מסך במערכת. הקובץ היחיד שקורא מתוך הטבלה הזו הוא AuthApis, כדי
// לבדוק שם משתמש וסיסמה בזמן התחברות.
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;

    // הסיסמה נשמרת כאן כטקסט רגיל, בלי הצפנה או גיבוב. זו החלטה מודעת שהוסברה
    // בנפרד, ומתאימה לתרגיל בגודל הזה בלבד ולא לסביבת פרודקשן אמיתית.
    public string Password { get; set; } = string.Empty;

    // הערך כאן הוא בדיוק "Admin" או "Viewer", ומשמש בשני מקומות: בתוך הטוקן
    // שנוצר בהתחברות, ובתוך המדיניות AdminOnly שמוגדרת בקובץ Program.
    public string Role { get; set; } = string.Empty;
}

namespace RemindersApi.Model;

// שני הערכים הקבועים שמשמשים בכל הפרויקט, גם באובייקטים שנשמרים במסד הנתונים
// וגם בבקשות ובתשובות שעוברות מול הלקוח.

// Frequency
// באיזו תדירות תזכורת חוזרת על עצמה. נבחר בטופס היצירה, ונקרא על ידי שירות
// הרקע כדי לדעת אם ומתי לתזמן הרצה נוספת אחרי שההרצה הנוכחית הסתיימה.
public enum Frequency { Once, Daily, Weekly, Monthly }

// ReminderStatus
// מחזור החיים של תזכורת בודדת, בסדר הזה בדיוק: Pending הוא המצב מיד עם
// היצירה. Running הוא המצב בזמן שהתזכורת "רצה" בפועל אצל שירות הרקע.
// Success או Failed הוא המצב הסופי אחרי שההרצה הסתיימה.
public enum ReminderStatus { Pending, Running, Success, Failed }

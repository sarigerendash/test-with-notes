
# ארכיטקטורת השרת (RemindersApi)

בניתי זה כתרגיל בית, לא פרויקט ענק  אז החלטתי מראש לא להכביד עם שכבות כבדות כמו שרואים בפרויקטים גדולים.
אם זה היה פרויקט אמיתי וגדול, עם הרבה ישויות, הרבה לוגיקה עסקית משותפת, וכמה מפתחים שעובדים על זה במקביל - אז כן הייתי שוקלת שכבות נוספות כדי לשמור על סדר. אבל לתרגיל בגודל הזה, זה היה רק מוסיף בירוקרטיה מיותרת.

נשענתי על המבנה ששמעון דהאן לימד בקורס שלו (https://github.com/shimond/course-angular-net26_02_2025) ,

# המבנה בפועל

```
RemindersApi/
├── Program.cs   ← נקודת ההפעלה , תחילת הפרויקט
├── Model/       ← איך נראים הנתונים - גם מה שנשמר ב-DB וגם מה שעובר בבקשות
├── Data/        ← החיבור לבסיס הנתונים (AppDbContext)
├── Contracts/   ← הממשקים (interfaces) 
├── Services/    ← המימוש בפועל של הממשקים, ושירות הרקע ששולח תזכורות
└── Apis/        ← מה שקורה כשמגיעה בקשה מהדפדפן (ה-Endpoints עצמם).
```

# Repository Pattern

נבחר Service שפונה ישירות ל-DbContext ללא Repository Pattern, מכיוון שהפרויקט קטן - הוספת שכבת Repository הייתה מוסיפה קוד מיותר ללא תועלת אמיתית, אך בפרויקט גדול יותר הייתי מפרידה בין לוגיקה עסקית לגישה לנתונים.

# שירות הרקע

יש עוד דבר אחד שכדאי להזכיר - `ReminderBackgroundService`. זה לא endpoint שמישהו קורא לו, אלא תהליך שרץ כל הזמן ברקע מרגע שהשרת עולה, בודק כל 30 שניות אילו תזכורות הגיע הזמן שלהן, ו"שולח" אותן (מדמה שליחה ומחליט אקראית הצלחה/כישלון). הוא יושב ב-`Services/` כי מבחינת התפקיד שלו הוא עוד שירות במערכת - רק שהוא לא נחשף דרך `Apis/` כי אף אחד לא "קורא" לו מבחוץ.

# מה רץ ראשון

`Program.cs` הוא תמיד הקובץ הראשון שרץ - זו נקודת הכניסה של כל השרת. הוא רץ פעם אחת בלבד, ברגע שמפעילים `dotnet run`, ומבצע לפי הסדר: רישום כל השירותים, הגדרת האימות וההרשאות, בניית השרת בפועל, יצירת מסד הנתונים והזנת שני המשתמשים, ולבסוף רישום כל נקודות הקצה דרך `MapAuthApis` ו-`MapReminderApis`. אחרי זה השרת רק ממתין לבקשות - כל שאר הקבצים רצים רק בתגובה לבקשה שמגיעה, חוץ מ-`ReminderBackgroundService` שרץ לבדו ברקע.

הסבר מורחב ומחולק לשאלות ותשובות, כולל ההבדלים בין `AddScoped`, `AddHostedService` ו-`AddPolicy`, נמצא כהערות בתוך `Program.cs` עצמו, בסוף הקובץ.

# שלוש הזרימות המרכזיות

## זרימת התחברות

1. הלקוח שולח בקשה אל `AuthApis`, למסלול `/auth/login`.
2. `AuthApis` מחפש התאמה בתוך `AppDbContext.Users`.
3. אם נמצאה התאמה, `AuthApis` קורא ל-`JwtService.GenerateToken`.
4. הטוקן שחוזר נשלח ללקוח בתוך `LoginResponse`.

## זרימת בקשה מאומתת, לדוגמה יצירת תזכורת

1. הלקוח שולח בקשה אל `ReminderApis`, למסלול `/reminders`.
2. התשתית של השרת מאמתת את הטוקן ובודקת את ההרשאה, לפי מה שהוגדר ב-`Program.cs`.
3. `ReminderApis` מריץ קודם את `Validate`, הבדיקה הפנימית שנמצאת באותו קובץ.
4. אם הבדיקה עברה, `ReminderApis` קורא ל-`IReminderService.CreateAsync`.
5. המימוש בפועל, בקובץ `ReminderService`, שומר את התזכורת דרך `AppDbContext`, ומחזיר תשובה.

## זרימת שירות הרקע

1. `ReminderBackgroundService.ExecuteAsync` מתעורר כל 30 שניות, לבד, בלי שום קשר לבקשות.
2. הוא קורא ל-`ProcessPendingReminders`, שמאתר תזכורות שהגיע זמנן דרך `AppDbContext`.
3. לכל תזכורת: שינוי סטטוס, המתנה, קביעת תוצאה, ושמירת שורה בטבלת `ReminderRuns`.
4. אם צריך, `ProcessPendingReminders` קורא ל-`NextOccurrence` כדי לתזמן הרצה נוספת.

# טבלת מי קורא למי

| קובץ ומתודה | מי קורא לה | למי היא קוראת |
|---|---|---|
| `Program.cs` | התשתית של השרת, עם ההפעלה | `MapAuthApis`, `MapReminderApis`, וכל רישומי ה-DI |
| `AuthApis.MapAuthApis` | `Program.cs` | `AppDbContext.Users`, `JwtService.GenerateToken` |
| `ReminderApis.MapReminderApis` | `Program.cs` | `IReminderService`, `Validate` |
| `ReminderApis.Validate` | `MapReminderApis`, באותו קובץ | - |
| `JwtService.GenerateToken` | `AuthApis` | - |
| `ReminderService.GetAllAsync` | `ReminderApis` | `AppDbContext.Reminders` |
| `ReminderService.CreateAsync` | `ReminderApis` | `AppDbContext.Reminders`, `ToResponse` |
| `ReminderService.UpdateAsync` | `ReminderApis` | `AppDbContext.Reminders`, `ToResponse` |
| `ReminderService.GetHistoryAsync` | `ReminderApis` | `AppDbContext.ReminderRuns` |
| `ReminderService.ToResponse` | שלוש המתודות שמעל, באותו קובץ | - |
| `ReminderBackgroundService.ExecuteAsync` | התשתית של השרת, אוטומטית | `ProcessPendingReminders` |
| `ReminderBackgroundService.ProcessPendingReminders` | `ExecuteAsync`, באותו קובץ | `AppDbContext`, `NextOccurrence` |
| `ReminderBackgroundService.NextOccurrence` | `ProcessPendingReminders`, באותו קובץ | - |
| `AppDbContext.OnModelCreating` | Entity Framework, אוטומטית עם יצירת מסד הנתונים | - |

# מה כל קובץ עושה

- `Program.cs` - נקודת הכניסה. רושם שירותים, מגדיר אימות והרשאות, בונה את השרת, ורושם את נקודות הקצה.
- `Model/Enums.cs` - שני הערכים הקבועים, `Frequency` ו-`ReminderStatus`, שמשמשים בכל הפרויקט.
- `Model/Reminder.cs` - ה-Entity שמייצגת שורה בטבלת התזכורות.
- `Model/ReminderRun.cs` - ה-Entity שמייצגת שורה אחת בהיסטוריה.
- `Model/User.cs` - ה-Entity שמייצגת משתמש שרשאי להתחבר.
- `Model/AuthDtos.cs` - חוזה ההתחברות מול הלקוח, בקשה ותשובה.
- `Model/ReminderDtos.cs` - חוזה ה-API של תזכורות מול הלקוח, בקשה ותשובה ורשומת היסטוריה.
- `Data/AppDbContext.cs` - החיבור היחיד למסד הנתונים, כולל הזנת שני המשתמשים הקבועים.
- `Contracts/IReminderService.cs` - החוזה של פעולות התזכורות.
- `Contracts/IJwtService.cs` - החוזה של יצירת טוקן.
- `Services/ReminderService.cs` - המימוש בפועל של פעולות התזכורות.
- `Services/JwtService.cs` - המימוש בפועל של יצירת הטוקן.
- `Services/ReminderBackgroundService.cs` - שירות הרקע ששולח תזכורות ומתזמן הרצות חוזרות.
- `Apis/AuthApis.cs` - נקודת הקצה של התחברות.
- `Apis/ReminderApis.cs` - נקודות הקצה של תזכורות, כולל בדיקת התקינות.


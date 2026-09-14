<div dir="rtl">

# ארכיטקטורת הצד לקוח (reminders-app)

הסבר על מבנה הקוד, סדר ההרצה, וזרימת הקריאות בין הקבצים. המקביל לזה בצד השרת נמצא בקובץ `RemindersApi/README.md`.

# מה רץ ראשון

`main.ts` הוא תמיד הקובץ הראשון שרץ - זו נקודת הכניסה לכל האפליקציה, עוד לפני שנטען אפילו מסך אחד. הוא קורא ל-`bootstrapApplication`, ומעביר לו שני דברים: את הקומפוננטה הראשית `App`, ואת `appConfig`, שנמצא בקובץ `app.config.ts` ורושם שם את `authInterceptor`. מרגע זה, `App` מוצג, ומחליט לפי מצב ההתחברות אם להציג את `Login` או את אחד ממסכי התוכן.

# איך עובד authInterceptor

זה החלק התשתיתי הכי חשוב בפרויקט, ולכן מגיע לו הסבר נפרד. Interceptor הוא לא קובץ שקוראים לו במפורש משום מקום - הוא נרשם פעם אחת, בתוך `app.config.ts`, ומאותו רגע **כל** בקשת רשת שיוצאת מהאפליקציה, מכל שירות, עוברת קודם דרכו לבד, אוטומטית.

שני התפקידים שלו:

1. לפני שהבקשה יוצאת, הוא בודק אם יש טוקן שמור אצל `AuthService`, ואם כן, מצרף אותו לבקשה בתור כותרת `Authorization`. כך אף שירות אחר, לא `ReminderService` ולא שום קובץ אחר, לא צריך לדעת בכלל שיש דבר כזה טוקן.
2. אחרי שהתשובה חוזרת, אם קוד התשובה הוא `401`, כלומר הטוקן כבר לא תקף, הוא קורא ל-`logout`, שנמצאת ב-`AuthService`, ומנתק את המשתמש אוטומטית.

# מעקב חי אחרי סדר הקריאות

בתחילת כל פעולה משמעותית יש שורת `console.log`, עם מספר קבוע וברור, בתור: מספר, שם הפונקציה, ושם הקובץ או הקומפוננטה שבה היא נמצאת. כדי לצפות בזה: פותחים את כלי הפיתוח בדפדפן (מקש F12), עוברים ללשונית Console, ומשתמשים באפליקציה כרגיל - כניסה, יצירת תזכורת, מעבר בין מסכים. כל פעולה תופיע שם עם המספר הקבוע שלה, כולל הבקשות האוטומטיות שרצות כל 3 שניות ברענון. הרשימה המלאה, לפי סדר המספרים:

1. `bootstrap` בקובץ `main.ts`
2. `authInterceptor` בקובץ `auth.interceptor.ts`
3. `login` ב-`AuthService`
4. `logout` ב-`AuthService`
5. `loadAll` ב-`ReminderService`
6. `create` ב-`ReminderService`
7. `update` ב-`ReminderService`
8. `loadHistory` ב-`ReminderService`
9. `submit` ב-`Login`
10. `ngOnInit` ב-`Reminders`
11. `openCreate` ב-`Reminders`
12. `openEdit` ב-`Reminders`
13. `closeForm` ב-`Reminders`
14. `ngOnInit` ב-`ReminderHistory`
15. `ngOnInit` ב-`ReminderForm`
16. `submit` ב-`ReminderForm`

שימו לב שהמספר הזה הוא מזהה קבוע לאותה שורת קוד, ולא סופר כמה פעמים היא רצה - למשל `authInterceptor` תמיד תדפיס `2`, גם בפעם הראשונה וגם בפעם המאה שהיא רצה, כי כל בקשה יוצאת עוברת דרכה.

# שלוש הזרימות המרכזיות

## זרימת התחברות

1. `Login.submit` נקראת בלחיצה על כפתור הכניסה.
2. קוראת ל-`AuthService.login`.
3. `authInterceptor` נכנס לפעולה על הבקשה היוצאת, אך אין עדיין טוקן לצרף.
4. בתשובה מוצלחת, `AuthService` שומר את הטוקן והתפקיד, וכל התבנית מתעדכנת לבד דרך ה-signals.

## זרימת יצירת תזכורת

1. `Reminders.openCreate` נקראת בלחיצה על הכפתור, ופותחת את `ReminderForm`.
2. `ReminderForm.submit` נקראת בלחיצה על שמירה.
3. קוראת ל-`ReminderService.create`.
4. `authInterceptor` מצרף הפעם את הטוקן השמור אוטומטית.
5. בתשובה מוצלחת, `ReminderForm` משדרת את `closed`, ו-`Reminders.closeForm` סוגרת את המודאל.

## זרימת רענון אוטומטי

1. עם הכניסה למסך, `Reminders.ngOnInit` או `ReminderHistory.ngOnInit` מפעילה טיימר.
2. כל 3 שניות, הטיימר קורא שוב ל-`ReminderService.loadAll` או ל-`loadHistory`.
3. כל קריאה כזו עוברת גם היא דרך `authInterceptor`, בדיוק כמו כל בקשה אחרת.

# טבלת מי קורא למי

| קובץ ומתודה | מי קורא לה | למי היא קוראת |
|---|---|---|
| `main.ts` | הדפדפן, עם טעינת הדף | `bootstrapApplication` |
| `App` (הקומפוננטה כולה) | `main.ts` | `Login`, `Reminders`, `ReminderHistory` |
| `Login.submit` | `login.html`, בלחיצת כפתור | `AuthService.login` |
| `AuthService.login` | `Login.submit` | השרת, דרך `authInterceptor` |
| `AuthService.logout` | `app.html`, וגם `authInterceptor` אוטומטית | - |
| `authInterceptor` | Angular, אוטומטית על כל בקשה | `AuthService.token`, `AuthService.logout` |
| `Reminders.ngOnInit` | Angular, עם הכניסה למסך | `ReminderService.loadAll` |
| `Reminders.openCreate` / `openEdit` | `reminders.html`, בלחיצת כפתור | פותחת את `ReminderForm` |
| `ReminderForm.ngOnInit` | Angular, עם פתיחת המודאל | - |
| `ReminderForm.submit` | `reminder-form.html`, בלחיצת כפתור | `ReminderService.create` או `update` |
| `ReminderService.loadAll` / `create` / `update` / `loadHistory` | `Reminders`, `ReminderForm`, `ReminderHistory` | השרת, דרך `authInterceptor` |
| `ReminderHistory.ngOnInit` | Angular, עם הכניסה למסך | `ReminderService.loadHistory` |

# מה כל קובץ עושה

- `main.ts` - נקודת הכניסה, מפעילה את הקומפוננטה הראשית.
- `app.config.ts` - ההגדרות הגלובליות, כולל רישום `authInterceptor`.
- `app.ts` - הקומפוננטה הראשית, מחליטה מה להציג לפי מצב ההתחברות ומצב התצוגה.
- `auth.ts` - שירות ההתחברות, מחזיק את הטוקן והתפקיד.
- `auth.interceptor.ts` - מצרף טוקן לכל בקשה, ומטפל בניתוק אוטומטי.
- `models.ts` - הצורות המקבילות לחוזה ה-API של השרת.
- `reminder.ts` - שירות התזכורות, כל התקשורת מול השרת בנושא הזה.
- `login/login.ts` - מסך ההתחברות.
- `reminders/reminders.ts` - מסך רשימת התזכורות.
- `reminder-form/reminder-form.ts` - המודאל של יצירה ועריכה.
- `history/history.ts` - מסך היסטוריית השליחות.

</div>

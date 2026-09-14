# Reminders System - מדריך לבודק

תרגיל בית: מערכת תזכורות עם צד שרת ב-.NET Core וצד לקוח ב-Angular.

## דרישות מקדימות

- .NET SDK 10.0 ומעלה
- Node.js 20 ומעלה
- Angular 20

הפרויקט מורכב משני חלקים שצריכים לרוץ **במקביל**:
- **RemindersApi** - צד השרת, .NET 10 Minimal API (`http://localhost:5197`)
- **reminders-app** - צד הלקוח, Angular 20 (`http://localhost:4200`)

## איך מריצים

### 1. הרצת השרת (Backend)

בטרמינל נפרד:

```bash
cd RemindersApi
dotnet run
```
> שימו לב: מסד הנתונים הוא **In-Memory** - כל המידע (כולל תזכורות שתיצרו) נמחק בכל הפעלה מחדש של השרת.

### 2. הרצת האפליקציה (Frontend)

בטרמינל נוסף:

```bash
cd reminders-app
npm install
npm start
```

> חשוב: תריצו קודם את השרת ותוודאו שהוא רץ, כי צד הלקוח מוגדר לדבר איתו רק בכתובת `http://localhost:5197` (ה-CORS מוגבל לכתובת הזו).

## משתמשים לבדיקה

המערכת מגיעה עם שני משתמשים מוכנים מראש:

| שם משתמש | סיסמה | תפקיד | הרשאות |
|---|---|---|---|
| `admin` | `admin123` | Admin | צפייה + יצירה + עריכה של תזכורות |
| `viewer` | `viewer123` | Viewer | צפייה בלבד (לא יכול ליצור או לערוך) |

## החלטות טכנולוגיה וארכיטקטורה

### צד שרת (.NET)

בניתי את זה כתרגיל בית ולא כפרויקט ענק, אז בכוונה לא הוספתי שכבות מיותרות. המבנה מסודר לפי תפקיד: `Model` (איך הנתונים נראים), `Data` (החיבור למסד הנתונים), `Contracts` (הממשקים), `Services` (המימוש בפועל, כולל שירות הרקע), ו-`Apis` (ה-endpoints עצמם). בחרתי לא להשתמש ב-Repository Pattern ולתת ל-Services לפנות ישירות ל-DbContext - בפרויקט בגודל הזה שכבה נוספת רק הייתה מוסיפה בירוקרטיה בלי תועלת אמיתית.
**פירוט מלא ומורחב יותר על הארכיטקטורה נמצא בקובץ `RemindersApi/README.md`.**

### צד לקוח (Angular)

בחרתי ב-**Angular 20** כי רציתי לעבוד עם היכולות החדשות - standalone components, ו-Signals לניהול State, במקום גישות ישנות יותר. עשיתי מבנה קטן ומסודר.

**ניהול State** - לא השתמשתי ב-store נפרד, כי זה פרויקט קטן שמספיק לו להתנהל עם Signal פשוט: כשמשהו משתנה, ה-signal מתעדכן וכל מי שמשתמש בו ב-template מתעדכן אוטומטית. לפרויקט בגודל הזה זה נראה לי מספיק ופשוט יותר מלהכניס ספריית state בנפרד.

## הנחות עבודה

כמה מקומות שההוראות היו פתוחות, וההחלטה שלי:

- **תזמון ה-Background Service** - לא היה מוגדר בדרישות, אז בחרתי זמנים שיאפשרו לראות את כל מחזור החיים בזמן סביר בלי לחכות יותר מדי (פירוט מלא בסעיף "מחזור החיים של תזכורת" בהמשך).
- **שמירת ה-Token** ב-localStorage בצד הלקוח (ולא למשל ב-cookie) - כדי לפשט את הבדיקה, כולל מה שקורה ברענון דף.
- **מסד נתונים In-Memory** במקום מסד אמיתי - לנוחות ההרצה של הבודק, כפי שהומלץ בהוראות.
- **CORS** מוגבל אך ורק לכתובת `localhost:4200`/`localhost:5197` - בהנחה שהבדיקה תתבצע מקומית בלבד.

## שקיפות על שימוש בכלי AI

השתמשתי בכלי AI בכל שלבי העבודה. בהתחלה התייעצתי עם Gemini כדי להבין איך לגשת למשימה - איך לפרק אותה למשימות קטנות וסדר עבודה הגיוני. את מימוש הפרויקט עצמו - כתיבת הקוד, ההחלטות הארכיטקטוניות, הדיבוג - עשיתי יחד עם Claude, דרך התוסף שלו ב-VS Code.

## מחזור החיים של תזכורת
אחרי שתזכורת נוצרת, הסטטוס שלה משתנה אוטומטית:
1. **Pending** - מיד עם היצירה.
2. **Running** - שירות הרקע בודק כל 30 שניות תזכורות ב-Pending ומעביר אותן למצב הזה. השלב הזה נמשך כ-10 שניות.
3. **Success / Failed** - בסוף ה-10 שניות, השרת  (50/50 רנדומלי) וקובע את הסטטוס הסופי. אין כרגע לוגיקה עסקית אמיתית מאחורי כישלון - זו סימולציה.

צד הלקוח מרענן את הרשימה כל 3 שניות אוטומטית, כך שאפשר לראות את כל השלבים בזמן אמת בלי לרענן ידנית.

## זרימת הכפתורים במערכת

הסבר מפורט, כפתור אחר כפתור, על כל הדרך מהלחיצה בצד הלקוח ועד הקריאה בצד השרת וחזרה.

### כפתור "כניסה" (Login)

**[login.html:6](reminders-app/src/app/login/login.html#L6)**
```html
<button (click)="submit()">כניסה</button>
```

**[login.ts](reminders-app/src/app/login/login.ts)**
```ts
submit() {
  this.auth.login(this.username(), this.password()).subscribe({
    error: () => this.error.set('שם משתמש או סיסמה שגויים')
  });
}
```

**[auth.ts](reminders-app/src/app/auth.ts)**
```ts
login(username: string, password: string) {
  return this.http.post<LoginResponse>(`${this.API}/auth/login`, { username, password }).pipe(
    tap(res => {
      localStorage.setItem('token', res.token);
      localStorage.setItem('role', res.role);
      this._token.set(res.token);
      this._role.set(res.role);
    })
  );
}
```

הבקשה עוברת דרך `authInterceptor` (בשלב הזה עוד אין טוקן לצרף), ומגיעה לשרת:

**[AuthApis.cs](RemindersApi/Apis/AuthApis.cs)**
```csharp
app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db, IJwtService jwt) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.Password == req.Password);
    if (user is null) return Results.Unauthorized();
    return Results.Ok(new LoginResponse(jwt.GenerateToken(user), user.Role));
});
```

**[JwtService.cs](RemindersApi/Services/JwtService.cs)**
```csharp
var claims = new[] {
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Role, user.Role)
};
var token = new JwtSecurityToken(
    issuer: config["Jwt:Issuer"], audience: config["Jwt:Audience"],
    claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
return new JwtSecurityTokenHandler().WriteToken(token);
```

**ההבדל בין admin ל-viewer**: הקוד זהה לגמרי בשני המקרים. ההבדל היחיד הוא איזו שורה נמצאה בטבלת `Users`, ולכן איזה `Role` הוטבע בטוקן. זה קובע בהמשך את `AuthService.isAdmin()` בצד הלקוח (מה מוצג במסך), ואת המדיניות `AdminOnly` בצד השרת (מה מותר בפועל, גם אם מנסים לעקוף את המסך).

חזרה ללקוח: התשובה `{ token, role }` נשמרת גם ב-`localStorage` וגם בשני signals (`_token`, `_role`), וזה מעדכן אוטומטית כל מקום שמשתמש ב-`isLoggedIn`/`isAdmin`.

### כפתור "+ תזכורת חדשה" (יצירה)

**פתיחה, בלי קשר לשרת - [reminders.ts](reminders-app/src/app/reminders/reminders.ts)**
```ts
openCreate() {
  this.editing.set(null);
  this.showForm.set(true);
}
```

`editing` מתאפס ל-`null`, מה שגורם ל-`ReminderForm.ngOnInit` להשאיר את הטופס ריק (במקום למלא אותו מתזכורת קיימת).

**שמירה - [reminder-form.ts](reminders-app/src/app/reminder-form/reminder-form.ts)**
```ts
submit() {
  if (!this.form.name || !this.form.scheduledAt) { this.error.set('שם ותאריך הם שדות חובה'); return; }
  const req = { ...this.form, scheduledAt: new Date(this.form.scheduledAt).toISOString() };
  const action = this.reminder
    ? this.svc.update(this.reminder.id, req)
    : this.svc.create(req);
  action.subscribe({ next: () => this.closed.emit(), error: () => this.error.set('שגיאה בשמירה') });
}
```

מכיוון ש-`this.reminder` הוא `null`, נבחר `create`:

**[reminder.ts](reminders-app/src/app/reminder.ts)**
```ts
create(req: ReminderRequest) {
  return this.http.post<Reminder>(this.API, req).pipe(
    tap(r => this.reminders.update(list => [...list, r]))
  );
}
```

**[ReminderApis.cs](RemindersApi/Apis/ReminderApis.cs)**
```csharp
group.MapPost("/", async (ReminderRequest req, IReminderService svc) =>
{
    var errors = Validate(req);
    if (errors is not null) return Results.ValidationProblem(errors);
    var created = await svc.CreateAsync(req);
    return Results.Created($"/reminders/{created.Id}", created);
}).RequireAuthorization("AdminOnly");
```

הבקשה נחסמת כאן עם `403` אם המשתמש אינו Admin, עוד לפני שה-handler בכלל רץ. לאחר מכן `Validate` בודקת שם לא ריק, תאריך נבחר, ומספר הרצות עתידיות לא שלילי.

**[ReminderService.cs](RemindersApi/Services/ReminderService.cs)**
```csharp
public async Task<ReminderResponse> CreateAsync(ReminderRequest req)
{
    var reminder = new Reminder { Name = req.Name, Message = req.Message, ScheduledAt = req.ScheduledAt,
        Frequency = req.Frequency, IsActive = req.IsActive, FutureRunsCount = req.FutureRunsCount };
    db.Reminders.Add(reminder);
    await db.SaveChangesAsync();
    return ToResponse(reminder);
}
```

`Id` ו-`Status` לא מגיעים מהלקוח - `Id` נקבע על ידי מסד הנתונים, ו-`Status` מקבל את ברירת המחדל `Pending`. התשובה חוזרת עם `201 Created`, מתווספת מיד ל-signal `reminders` בלקוח, והמודאל נסגר.

### כפתור "עריכה"

כמעט זהה ליצירה, עם שלושה הבדלים:

**פתיחה עם תזכורת קיימת - [reminders.ts](reminders-app/src/app/reminders/reminders.ts)**
```ts
openEdit(reminder: Reminder) {
  this.editing.set(reminder);
  this.showForm.set(true);
}
```

`ReminderForm.ngOnInit` ממלא את הטופס מהתזכורת שהתקבלה. בשמירה, `submit()` בוחר הפעם ב-`update`:

**[reminder.ts](reminders-app/src/app/reminder.ts)**
```ts
update(id: number, req: ReminderRequest) {
  return this.http.put<Reminder>(`${this.API}/${id}`, req).pipe(
    tap(r => this.reminders.update(list => list.map(x => x.id === id ? r : x)))
  );
}
```

**[ReminderApis.cs](RemindersApi/Apis/ReminderApis.cs)**
```csharp
group.MapPut("/{id:int}", async (int id, ReminderRequest req, IReminderService svc) =>
{
    var errors = Validate(req);
    if (errors is not null) return Results.ValidationProblem(errors);
    var updated = await svc.UpdateAsync(id, req);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
}).RequireAuthorization("AdminOnly");
```

`id` מגיע הפעם מהנתיב עצמו (`{id:int}`), לא מגוף הבקשה.

**[ReminderService.cs](RemindersApi/Services/ReminderService.cs)**
```csharp
public async Task<ReminderResponse?> UpdateAsync(int id, ReminderRequest req)
{
    var reminder = await db.Reminders.FindAsync(id);
    if (reminder is null) return null;
    reminder.Name = req.Name; reminder.Message = req.Message; reminder.ScheduledAt = req.ScheduledAt;
    reminder.Frequency = req.Frequency; reminder.IsActive = req.IsActive; reminder.FutureRunsCount = req.FutureRunsCount;
    await db.SaveChangesAsync();
    return ToResponse(reminder);
}
```

`FindAsync` מאתר את השורה הקיימת (לא בונה חדשה), ו-`Status`/`CreatedAt` לא נוגעים בהם - עריכה לא "מאפסת" תזכורת שכבר רצה. אם המזהה לא נמצא, מוחזר `null`, וזה הופך ל-`404` ללקוח.

### כפתור "היסטוריית שליחות"

זה כפתור שלא פותח מודאל ולא שולח בקשה בעצמו - הוא רק מחליף קומפוננטה:

**[app.html](reminders-app/src/app/app.html)**
```html
<button [class.active]="view() === 'history'" (click)="view.set('history')">היסטוריית שליחות</button>
...
@if (view() === 'list') { <app-reminders /> } @else { <app-history /> }
```

זה `@if`/`@else` מבני - `<app-reminders>` נהרסת לגמרי (וה-`interval` שלה נעצר דרך `takeUntilDestroyed`), ו-`<app-history>` נוצרת מהתחלה, מה שמפעיל את ה-`ngOnInit` שלה:

**[history.ts](reminders-app/src/app/history/history.ts)**
```ts
ngOnInit() {
  interval(REFRESH_INTERVAL_MS).pipe(
    startWith(0),
    switchMap(() => this.svc.loadHistory()),
    takeUntilDestroyed(this.destroyRef)
  ).subscribe();
}
```

**[ReminderApis.cs](RemindersApi/Apis/ReminderApis.cs)**
```csharp
group.MapGet("/history", async (IReminderService svc) =>
    Results.Ok(await svc.GetHistoryAsync()));
```

זה לא דורש `AdminOnly` - גם viewer יכול לראות היסטוריה.

**[ReminderService.cs](RemindersApi/Services/ReminderService.cs)**
```csharp
public async Task<IEnumerable<ReminderRunResponse>> GetHistoryAsync() =>
    await db.ReminderRuns
        .OrderByDescending(h => h.FinishedAt)
        .Select(h => new ReminderRunResponse(h.Id, h.ReminderId, h.Reminder.Name, h.Reminder.Message, h.StartedAt, h.FinishedAt, h.Status))
        .ToListAsync();
```

`h.Reminder.Name`/`h.Reminder.Message` הוא JOIN אוטומטי דרך ה-navigation property שמוגדר ב-`ReminderRun`.

**נקודה חשובה: מי כותב את ההיסטוריה, ואיפה היא נשמרת בפועל.** `GetHistoryAsync` **רק קורא**. מי שכותב לשם זה קוד אחר לגמרי - `ReminderBackgroundService`, שרץ ברקע כל 30 שניות ולא קשור בשום צורה ישירה ל-`ReminderService`:

```csharp
db.ReminderRuns.Add(new ReminderRun { ReminderId = reminder.Id, StartedAt = startedAt,
    FinishedAt = DateTime.UtcNow, Status = reminder.Status });
await db.SaveChangesAsync(ct);
```

שני הקבצים האלה "נפגשים" רק דרך מקום אחד משותף: מסד הנתונים `"RemindersDb"`, המוגדר פעם אחת ב-`Program.cs` (`UseInMemoryDatabase("RemindersDb")`). כל `AppDbContext` שנוצר בכל מקום בקוד, כולל בתוך שירות הרקע וגם בתוך כל בקשת HTTP בנפרד, מתחבר לאותו מאגר משותף בזיכרון - זו הסיבה שקריאה במקום אחד רואה כתיבה שנעשתה במקום אחר לגמרי. מכיוון שזה מאגר **בזיכרון** ולא על הדיסק, כל הנתונים - גם תזכורות וגם היסטוריה - נמחקים לחלוטין בכל הפעלה מחדש של השרת.

### כפתור "התנתקות"

הכפתור הקצר ביותר במערכת - ודווקא בגלל זה יש בו נקודה חשובה.

**[app.html](reminders-app/src/app/app.html)**
```html
<button (click)="auth.logout()">התנתקות</button>
```

הטמפלט קורא ישירות ל-`auth.logout()`, בלי מתודה נפרדת בקומפוננטה `App` עצמה.

**[auth.ts](reminders-app/src/app/auth.ts)**
```ts
logout() {
  localStorage.clear();
  this._token.set(null);
  this._role.set(null);
}
```

**אין כאן שום קריאה לשרת.** כל הפעולה היא ניקוי מקומי בלבד - מחיקה מ-`localStorage` ואיפוס שני signals.

**למה אין קריאה לשרת**: JWT הוא Stateless - השרת לא שומר רשימת "מחוברים", ולכן אין לו מה "לסגור". טוקן תקף נשאר תקף עד שפג תוקפו הטבעי (8 שעות), גם אחרי "התנתקות" - "התנתקות" כאן משמעה רק "תפסיק להשתמש בטוקן שבדפדפן", לא ביטול תוקף אמיתי.

**אותה מתודה נקראת גם אוטומטית**, בלי לחיצה על שום כפתור, מתוך [auth.interceptor.ts](reminders-app/src/app/auth.interceptor.ts) כשמתקבל `401` (למשל טוקן שפג תוקפו):
```ts
catchError(err => {
  if (err.status === 401) auth.logout();
  return throwError(() => err);
})
```

ברגע שה-signal `_token` הופך ל-`null`, ה-signal הגזור `isLoggedIn` חוזר `false`, ו-`app.html` מציג שוב אוטומטית את `<app-login>` - בלי שום ניווט מפורש.


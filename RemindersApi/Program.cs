using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RemindersApi.Apis;
using RemindersApi.Contracts;
using RemindersApi.Data;
using RemindersApi.Services;

// זו נקודת הכניסה של השרת - הקובץ הזה רץ פעם אחת, ברגע שמריצים "dotnet run".
// המשתנה builder הוא מכונת הבנייה של השרת - כאן רושמים הכל לפני שהשרת בפועל קם.
var builder = WebApplication.CreateBuilder(args);

// --- שלב 1: רישום שירותים אצל מיכל ההזרקה (Dependency Injection) ---
// זה כמו קטלוג של כל הדברים שהקוד שלנו יכול לבקש שיוזרקו אליו (דרך Constructor או פרמטר).
// מגדירים כאן שיש גישה למסד נתונים, ובוחרים לעבוד מול מסד מסוג In-Memory (לא מסד אמיתי) לנוחות ההרצה.
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("RemindersDb"));

// כל פעם שמישהו מבקש IReminderService, מקבל מופע חדש של ReminderService לכל בקשת HTTP.
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// זהו שירות שרץ ברקע לבדו מרגע עליית השרת, ולא מחכה לבקשת HTTP.
// הוא אחראי על שליחת תזכורות בזמן שנקבע להן.
builder.Services.AddHostedService<ReminderBackgroundService>();

// --- שלב 2: Authentication, כלומר איך מזהים מי המשתמש ---
// אומרים לשרת: כשמגיע טוקן, לקרוא אותו בתור JWT ולוודא שהוא תקין:
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           // הטוקן הונפק על ידינו ולא על ידי מערכת אחרת
        ValidateAudience = true,         // הטוקן מיועד לאפליקציה שלנו
        ValidateLifetime = true,         // הטוקן עדיין בתוקף
        ValidateIssuerSigningKey = true, // החתימה תואמת למפתח שלנו, אף אחד לא זייף אותו
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // המפתח החשאי לחתימה ולאימות נלקח מתוך appsettings.json, מהשדה Jwt:Key.
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    });

// --- שלב 3: Authorization, כלומר מה מותר למשתמש המזוהה לעשות ---
// מגדירים מדיניות בשם AdminOnly - מי שהתפקיד שלו, מתוך הטוקן, הוא בדיוק Admin.
// נשתמש בה בהמשך, בתוך Apis, על פעולות רגישות כמו יצירה ועריכה.
builder.Services.AddAuthorization(o =>
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));

// --- שלב 4: CORS, כלומר ממי מותר לקבל בקשות מהדפדפן ---
// בלי זה, הדפדפן היה חוסם בקשות שיוצאות מצד הלקוח (פורט 4200) ומגיעות אל השרת (פורט 5197),
// כי אלו שני מקורות שונים מבחינת הדפדפן.
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

// גורם לכך שערכי enum, למשל Frequency או ReminderStatus, יוצגו כטקסט קריא בתוך JSON,
// במקום כמספר גולמי שהיה הרבה פחות ברור לצד הלקוח.
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// מאפשר תיעוד API אוטומטי, שימושי לבדיקות ידניות בלבד ולא חלק מהלוגיקה עצמה.
builder.Services.AddOpenApi();

// --- כאן השרת בפועל נבנה - מכאן והלאה אי אפשר יותר להוסיף רישומי שירותים ---
var app = builder.Build();

// ברגע שהשרת עולה, נוצר scope זמני כדי לגשת אל מסד הנתונים ולוודא שהוא קיים,
// ולהריץ את פעולת ה-Seed, כלומר יצירת המשתמשים admin ו-viewer, המוגדרת בתוך AppDbContext.
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

// תיעוד ה-API זמין רק בסביבת פיתוח, ולא בסביבת פרודקשן.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

// --- שלב 5: סדר הביניים, ה-Middleware Pipeline, שהסדר בו קריטי ---
// כל בקשת HTTP עוברת דרך השלבים האלה, בדיוק בסדר הזה:
app.UseCors();            // ראשית בודקים שמותר בכלל לקבל בקשה מהמקור הזה
app.UseAuthentication();  // לאחר מכן מזהים מי המשתמש, על ידי קריאה ואימות של הטוקן
app.UseAuthorization();   // ורק בסוף בודקים אם מותר לו לבצע את הפעולה, כי בדיקה זו
                          // מסתמכת על מידע שרק שלב האימות הקודם מילא

// רישום ה-Endpoints בפועל - מי מטפל בכל נתיב URL.
app.MapAuthApis();
app.MapReminderApis();

// מריץ את השרת בפועל - התהליך ממתין כאן לבקשות עד שהוא נסגר.
app.Run();

// ================================================================
// הסברים נוספים על הקובץ הזה
// ================================================================
//
// שאלה 1: מה ההבדל בין האפשרויות לרישום שירות, ומתי בוחרים בכל אחת?
// תשובה: יש שלוש רמות חיים אפשריות לכל שירות שנרשם.
//
// Transient
// בכל פעם שמבקשים את השירות נוצר מופע חדש לגמרי, גם בתוך אותה קריאה בודדת.
//
// Scoped
// נוצר מופע אחד יחיד לכל בקשה שלמה, וכל מי שמבקש את השירות במהלך אותה בקשה
// מקבל בדיוק את אותו מופע.
//
// Singleton
// יש מופע אחד בלבד לאורך כל חיי האפליקציה, משותף לכולם תמיד.
//
// ReminderService, JwtService
// שני אלה נרשמו ברמה השנייה שהוסברה למעלה, מכיוון שהם תלויים במחלקת הגישה
// למסד הנתונים, שגם היא נרשמת באותה רמה, כדי למנוע מצב שבו שתי בקשות שונות
// משתפות בטעות אותו חיבור למסד הנתונים באותו רגע.
//
// ----------------------------------------------------------------
//
// שאלה 2: למה ומתי משתמשים בשירות רקע?
// תשובה: שירות רקע שונה משאר השירותים - הוא לא מחכה שמישהו יבקש אותו במהלך
// בקשה מהלקוח. ברגע שהשרת עולה, התשתית המובנית שלו מפעילה אותו אוטומטית
// בעצמה, וממשיכה להריץ אותו לאורך כל חיי האפליקציה, גם אם אף אחד לא שולח שום
// בקשה. משתמשים בזה כשיש עבודה שצריכה לקרות באופן רציף או תקופתי, בלי שום
// קשר לבקשות מהלקוח - במקרה שלנו, הבדיקה כל 30 שניות אילו תזכורות הגיע זמנן.
//
// ----------------------------------------------------------------
//
// שאלה 3: מה זו מדיניות הרשאה?
// תשובה: מדיניות היא כלל בשם קבוע, שמחליט אם למשתמש המחובר מותר לבצע פעולה
// מסוימת. כאן הוגדרה מדיניות אחת, שבודקת דבר יחיד: שהתפקיד של המשתמש, כפי
// שמופיע בטוקן שלו, שווה בדיוק לתפקיד מסוים אחד. אחר כך, כל נקודת קצה שרוצה
// להגביל גישה רק למי שעונה על הכלל הזה, פשוט מפנה למדיניות הזו לפי השם שלה,
// בלי לכתוב את אותה בדיקה מחדש בכל מקום.
//
// AdminOnly
// זהו השם שניתן למדיניות שהוגדרה בקובץ הזה.
//
// ----------------------------------------------------------------
//
// שאלה 4: האם השורה שמפעילה תיעוד אוטומטי של השרת הכרחית?
// תשובה: לא. היא לא קשורה ללוגיקה העסקית של האפליקציה בכלל, וגם לא נצרכת על
// ידי צד הלקוח. התפקיד היחיד שלה הוא לייצר תיעוד אוטומטי של כל נקודות הקצה,
// שאפשר לצפות בו או לבדוק דרכו את פעולת השרת ידנית בזמן פיתוח. אפשר להסיר
// אותה לגמרי בלי שום השפעה על התפקוד בפועל.
//
// ----------------------------------------------------------------
//
// שאלה 5: האם שני השלבים שבודקים מי המשתמש ומה מותר לו הם קוד שכתבנו בעצמנו?
// תשובה: לא. שני השלבים האלה מבוססים על שתי מתודות מוכנות שמגיעות מתוך
// הפלטפורמה עצמה, ולא כתבנו שום לוגיקה פנימית שלהן בעצמנו. מה שכן כתבנו זה
// את ההגדרות שמוזנות להן מראש, למעלה בקובץ הזה: אילו כללי אימות להפעיל על
// הטוקן, ואילו מדיניות הרשאה קיימות במערכת. שתי המתודות עצמן רק מריצות את
// ההגדרות האלה על כל בקשה שמגיעה.
//
// UseAuthentication
// זהו שמו של השלב הראשון שהוסבר למעלה - זיהוי המשתמש.
//
// UseAuthorization
// זהו שמו של השלב השני שהוסבר למעלה - בדיקת ההרשאה.

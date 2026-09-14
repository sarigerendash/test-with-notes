namespace RemindersApi.Model;

// שני הרשומות בקובץ הזה הן חוזה ההתחברות מול הלקוח - לא נשמרות במסד הנתונים
// בכלל, רק עוברות בגוף הבקשה והתשובה של המסלול POST /auth/login, המוגדר
// בקובץ AuthApis.

// LoginRequest
// זה מה שהלקוח שולח: שם משתמש וסיסמה כפי שהוקלדו במסך ההתחברות.
public record LoginRequest(string Username, string Password);

// LoginResponse
// זה מה שהשרת מחזיר אחרי התחברות מוצלחת: הטוקן שהלקוח יצרף מעכשיו לכל בקשה,
// והתפקיד, שמשמש את הלקוח כדי להחליט אילו כפתורים להציג במסך.
public record LoginResponse(string Token, string Role);

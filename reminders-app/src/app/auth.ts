import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';

interface LoginResponse { token: string; role: string; }

// AuthService
// אחראי על כל מה שקשור להתחברות: שמירת הטוקן והתפקיד, וחשיפת שני מצבים
// גזורים, isLoggedIn ו-isAdmin, שכל שאר הקומפוננטות משתמשות בהם כדי להחליט
// מה להציג. נקרא מתוך Login, בעת התחברות, ומתוך authInterceptor, כדי לקרוא
// את הטוקן השמור ולבצע logout כשצריך.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = 'http://localhost:5197';

  // שני ה-signals הפרטיים האלה הם מקור האמת היחיד. הם מאותחלים כאן ישירות
  // מתוך localStorage, כך שאם המשתמש כבר התחבר בעבר, גם רענון של הדף לא
  // ינתק אותו.
  private _token = signal<string | null>(localStorage.getItem('token'));
  private _role = signal<string | null>(localStorage.getItem('role'));

  // שני אלה הם signals גזורים - כל פעם שהערך הפרטי שמעליהם משתנה, כל מקום
  // בתבנית שמשתמש בהם מתעדכן אוטומטית, בלי שצריך לרענן ידנית.
  isLoggedIn = computed(() => !!this._token());
  isAdmin = computed(() => this._role() === 'Admin');
  token = this._token.asReadonly();

  constructor(private http: HttpClient) {}

  // login
  // נקראת מתוך Login, בעת לחיצה על כפתור הכניסה. שולחת בקשה אל AuthApis
  // בצד השרת, ובתגובה מוצלחת שומרת את הטוקן והתפקיד גם ב-localStorage וגם
  // ב-signals, כדי שכל האפליקציה תדע מיד שהמשתמש מחובר.
  login(username: string, password: string) {
    console.log('3 - login + AuthService');
    return this.http.post<LoginResponse>(`${this.API}/auth/login`, { username, password }).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('role', res.role);
        this._token.set(res.token);
        this._role.set(res.role);
      })
    );
  }

  // logout
  // מנקה את כל מה ששמור, גם ב-localStorage וגם ב-signals. נקראת משני מקומות:
  // מתוך app.html, בלחיצה ידנית על כפתור התנתקות, ומתוך authInterceptor,
  // אוטומטית, כשמתגלה שהטוקן כבר לא תקף.
  logout() {
    console.log('4 - logout + AuthService');
    localStorage.clear();
    this._token.set(null);
    this._role.set(null);
  }
}

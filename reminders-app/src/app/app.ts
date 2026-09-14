import { Component, signal } from '@angular/core';
import { AuthService } from './auth';
import { Login } from './login/login';
import { Reminders } from './reminders/reminders';
import { ReminderHistory } from './history/history';

// App
// הקומפוננטה הראשית, שנטענת ישירות מתוך main.ts. לא מכילה לוגיקה עסקית
// בעצמה - רק מחליטה מה להציג: אם אין משתמש מחובר, לפי isLoggedIn שנמצא
// ב-AuthService, מוצג Login. אחרת, מוצג Reminders או ReminderHistory, לפי
// ה-signal view שמוגדר כאן, ומוחלף בלחיצה על אחד משני הכפתורים בתפריט
// העליון.
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [Login, Reminders, ReminderHistory],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  view = signal<'list' | 'history'>('list');

  constructor(public auth: AuthService) {}
}

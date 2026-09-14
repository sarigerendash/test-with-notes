import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { Reminder, ReminderRequest, ReminderRun } from './models';

// ReminderService
// אחראי על כל התקשורת מול השרת בנושא תזכורות, וגם מחזיק את שני אוספי הנתונים
// המשותפים, reminders ו-history, שכל קומפוננטה שצריכה להציג אותם קוראת מהם
// ישירות. שימו לב שאין כאן שום התייחסות לטוקן - הוא מצורף אוטומטית על ידי
// authInterceptor, לכל בקשה שיוצאת מכאן.
@Injectable({ providedIn: 'root' })
export class ReminderService {
  private readonly API = 'http://localhost:5197/reminders';
  reminders = signal<Reminder[]>([]);
  history = signal<ReminderRun[]>([]);

  constructor(private http: HttpClient) {}

  // loadAll
  // נקראת מתוך Reminders, גם עם הכניסה למסך וגם באופן חוזר כל 3 שניות.
  // מביאה מהשרת את כל התזכורות, ומעדכנת את ה-signal reminders.
  loadAll() {
    console.log('5 - loadAll + ReminderService');
    return this.http.get<Reminder[]>(this.API).pipe(
      tap(data => this.reminders.set(data))
    );
  }

  // create
  // נקראת מתוך ReminderForm, בעת שמירת תזכורת חדשה. מוסיפה את התזכורת
  // שהתקבלה בתשובה גם לתוך ה-signal reminders, כדי שהמסך יתעדכן מיד, בלי
  // לחכות לסבב הרענון הבא.
  create(req: ReminderRequest) {
    console.log('6 - create + ReminderService');
    return this.http.post<Reminder>(this.API, req).pipe(
      tap(r => this.reminders.update(list => [...list, r]))
    );
  }

  // update
  // נקראת מתוך ReminderForm, בעת שמירת עריכה לתזכורת קיימת. מחליפה בתוך
  // ה-signal reminders רק את השורה שהשתנתה, ומשאירה את כל השאר כמו שהיו.
  update(id: number, req: ReminderRequest) {
    console.log('7 - update + ReminderService');
    return this.http.put<Reminder>(`${this.API}/${id}`, req).pipe(
      tap(r => this.reminders.update(list => list.map(x => x.id === id ? r : x)))
    );
  }

  // loadHistory
  // נקראת מתוך ReminderHistory, גם עם הכניסה למסך וגם באופן חוזר כל 3 שניות.
  // מביאה מהשרת את כל ההרצות שכבר הסתיימו, ומעדכנת את ה-signal history.
  loadHistory() {
    console.log('8 - loadHistory + ReminderService');
    return this.http.get<ReminderRun[]>(`${this.API}/history`).pipe(
      tap(data => this.history.set(data))
    );
  }
}

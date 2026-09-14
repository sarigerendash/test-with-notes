import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Reminder, ReminderRequest, Frequency } from '../models';
import { ReminderService } from '../reminder';

// ReminderForm
// המודאל של יצירה ועריכה של תזכורת - קומפוננטה אחת שמשרתת את שני המצבים.
// נפתחת מתוך Reminders. אם המאפיין reminder התקבל עם ערך, זה מצב עריכה,
// ואם הוא ריק, זה מצב יצירה.
@Component({
  selector: 'app-reminder-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './reminder-form.html',
  styleUrl: './reminder-form.css'
})
export class ReminderForm implements OnInit {
  @Input() reminder: Reminder | null = null;

  // closed
  // אירוע שנשלח אחורה אל Reminders, כדי לסגור את המודאל. נשלח גם בביטול
  // וגם אחרי שמירה מוצלחת.
  @Output() closed = new EventEmitter<void>();

  frequencies: Frequency[] = ['Once', 'Daily', 'Weekly', 'Monthly'];
  error = signal('');

  form: ReminderRequest = {
    name: '', message: '', scheduledAt: '', frequency: 'Once', isActive: true, futureRunsCount: 0
  };

  constructor(private svc: ReminderService) {}

  // ngOnInit
  // רץ פעם אחת עם פתיחת המודאל. אם התקבלה תזכורת קיימת, ממלא את הטופס
  // מהערכים שלה, כדי שהעריכה תתחיל מהמצב הנוכחי ולא מטופס ריק.
  ngOnInit() {
    console.log('15 - ngOnInit + ReminderForm');
    if (this.reminder) {
      const { name, message, scheduledAt, frequency, isActive, futureRunsCount } = this.reminder;
      this.form = { name, message, scheduledAt: scheduledAt.slice(0, 16), frequency, isActive, futureRunsCount };
    }
  }

  // submit
  // נקראת מתוך reminder-form.html, בלחיצה על כפתור שמירה. בודקת תקינות
  // בסיסית בצד הלקוח, ואז קוראת ל-update או ל-create, שתי המתודות נמצאות
  // ב-ReminderService, לפי זה אם reminder קיים או לא. בסיום מוצלח, משדרת
  // את האירוע closed כדי שהמודאל ייסגר.
  submit() {
    console.log('16 - submit + ReminderForm');
    if (!this.form.name || !this.form.scheduledAt) { this.error.set('שם ותאריך הם שדות חובה'); return; }
    const req = { ...this.form, scheduledAt: new Date(this.form.scheduledAt).toISOString() };
    const action = this.reminder
      ? this.svc.update(this.reminder.id, req)
      : this.svc.create(req);
    action.subscribe({ next: () => this.closed.emit(), error: () => this.error.set('שגיאה בשמירה') });
  }
}

import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe, NgClass } from '@angular/common';
import { interval, startWith, switchMap } from 'rxjs';
import { ReminderService } from '../reminder';
import { AuthService } from '../auth';
import { ReminderForm } from '../reminder-form/reminder-form';
import { Reminder } from '../models';

const REFRESH_INTERVAL_MS = 3000;

// Reminders
// מסך רשימת התזכורות - המסך הראשי של האפליקציה. מציג את reminders מתוך
// ReminderService, ופותח מעליו את ReminderForm כמודאל, גם ליצירה וגם לעריכה.
@Component({
  selector: 'app-reminders',
  standalone: true,
  imports: [ReminderForm, DatePipe, NgClass],
  templateUrl: './reminders.html',
  styleUrl: './reminders.css'
})
export class Reminders implements OnInit {
  showForm = signal(false);
  editing = signal<Reminder | null>(null);
  private destroyRef = inject(DestroyRef);

  constructor(public svc: ReminderService, public auth: AuthService) {}

  // ngOnInit
  // רץ פעם אחת עם הכניסה למסך. מפעיל טיימר שקורא ל-loadAll, שנמצאת
  // ב-ReminderService, מיד עם הכניסה, ואחר כך שוב כל 3 שניות, כדי שהמסך
  // יתעדכן לבד בלי לרענן את הדף.
  ngOnInit() {
    console.log('10 - ngOnInit + Reminders');
    interval(REFRESH_INTERVAL_MS).pipe(
      startWith(0),
      switchMap(() => this.svc.loadAll()),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  // openCreate
  // נקראת מתוך reminders.html, בלחיצה על כפתור תזכורת חדשה. פותחת את
  // ReminderForm כשהוא ריק, כלומר במצב יצירה.
  openCreate() {
    console.log('11 - openCreate + Reminders');
    this.editing.set(null);
    this.showForm.set(true);
  }

  // openEdit
  // נקראת מתוך reminders.html, בלחיצה על כפתור עריכה בשורה מסוימת. פותחת את
  // ReminderForm כשהוא מלא בנתוני התזכורת שנבחרה, כלומר במצב עריכה.
  openEdit(reminder: Reminder) {
    console.log('12 - openEdit + Reminders');
    this.editing.set(reminder);
    this.showForm.set(true);
  }

  // closeForm
  // נקראת מתוך ReminderForm, דרך ה-Output ששמו closed, בין אם נשמר משהו
  // ובין אם המשתמש ביטל. פשוט סוגרת את המודאל.
  closeForm() {
    console.log('13 - closeForm + Reminders');
    this.showForm.set(false);
  }

  // statusClass
  // קוראים לה מהתבנית, בכל שורה בטבלה, כדי לבחור את הצבע המתאים לסטטוס.
  // לא נוספה כאן שורת הדפסה, כי היא רצה הרבה פעמים בכל רענון מסך.
  statusClass(status: string) {
    return { pending: status === 'Pending', running: status === 'Running', success: status === 'Success', failed: status === 'Failed' };
  }
}

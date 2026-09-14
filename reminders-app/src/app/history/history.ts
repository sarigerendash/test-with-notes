import { Component, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe, NgClass } from '@angular/common';
import { interval, startWith, switchMap } from 'rxjs';
import { ReminderService } from '../reminder';

const REFRESH_INTERVAL_MS = 3000;

// ReminderHistory
// מסך היסטוריית השליחות. מציג את history מתוך ReminderService, כלומר את כל
// ההרצות שכבר הסתיימו, מהחדשה לישנה.
@Component({
  selector: 'app-history',
  standalone: true,
  imports: [DatePipe, NgClass],
  templateUrl: './history.html',
  styleUrl: './history.css'
})
export class ReminderHistory implements OnInit {
  private destroyRef = inject(DestroyRef);

  constructor(public svc: ReminderService) {}

  // ngOnInit
  // רץ פעם אחת עם הכניסה למסך. מפעיל טיימר שקורא ל-loadHistory, שנמצאת
  // ב-ReminderService, מיד עם הכניסה, ואחר כך שוב כל 3 שניות.
  ngOnInit() {
    console.log('14 - ngOnInit + ReminderHistory');
    interval(REFRESH_INTERVAL_MS).pipe(
      startWith(0),
      switchMap(() => this.svc.loadHistory()),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  // statusClass
  // קוראים לה מהתבנית, בכל שורה בטבלה, כדי לבחור את הצבע המתאים לסטטוס.
  statusClass(status: string) {
    return { success: status === 'Success', failed: status === 'Failed' };
  }
}

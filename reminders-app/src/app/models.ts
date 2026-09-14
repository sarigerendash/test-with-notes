// שלוש הצורות האלה הן חוזה ה-API מול השרת, ומקבילות בדיוק לרשומות שנמצאות
// שם, בקובץ ReminderDtos. שינוי בצד אחד בלי לעדכן את הצד השני יגרום לשגיאות
// שקטות בזמן ריצה, כי TypeScript לא בודק את זה מול השרת בפועל.

export type Frequency = 'Once' | 'Daily' | 'Weekly' | 'Monthly';
export type ReminderStatus = 'Pending' | 'Running' | 'Success' | 'Failed';

// Reminder
// מקביל ל-ReminderResponse בצד השרת - זה מה שמתקבל בתשובה מהשרת, ומוצג
// במסך הרשימה.
export interface Reminder {
  id: number;
  name: string;
  message: string;
  scheduledAt: string;
  frequency: Frequency;
  isActive: boolean;
  futureRunsCount: number;
  status: ReminderStatus;
  createdAt: string;
}

// ReminderRequest
// מקביל ל-ReminderRequest בצד השרת - זה מה שנשלח ביצירה ובעריכה. שימו לב
// שאין כאן id ואין status, בדיוק כמו בצד השרת, כי אלה נקבעים רק שם.
export interface ReminderRequest {
  name: string;
  message: string;
  scheduledAt: string;
  frequency: Frequency;
  isActive: boolean;
  futureRunsCount: number;
}

// ReminderRun
// מקביל ל-ReminderRunResponse בצד השרת - זו שורה אחת במסך ההיסטוריה.
export interface ReminderRun {
  id: number;
  reminderId: number;
  reminderName: string;
  message: string;
  startedAt: string;
  finishedAt: string;
  status: ReminderStatus;
}

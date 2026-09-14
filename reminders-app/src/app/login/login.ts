import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../auth';

// Login
// מסך ההתחברות. מוצג מתוך app.html, רק כשעדיין אין משתמש מחובר. מחזיק שלושה
// signals לשדות הטופס ולהודעת שגיאה, ומאציל את הפעולה בפועל אל AuthService.
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  username = signal('');
  password = signal('');
  error = signal('');

  constructor(private auth: AuthService) {}

  // submit
  // נקראת מתוך login.html, בלחיצה על כפתור הכניסה. קוראת ל-login, שנמצאת
  // ב-AuthService, ואם הבקשה נכשלת, מציגה הודעת שגיאה במקום להתחבר.
  submit() {
    console.log('9 - submit + Login');
    this.auth.login(this.username(), this.password()).subscribe({
      error: () => this.error.set('שם משתמש או סיסמה שגויים')
    });
  }
}

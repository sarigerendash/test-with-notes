import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth';

// authInterceptor
// זהו Interceptor פונקציונלי - קובץ תשתיתי שנרשם פעם אחת בלבד, בתוך
// app.config.ts, ומאותו רגע כל בקשה יוצאת מהאפליקציה, מכל שירות ומכל
// קומפוננטה, עוברת דרכו קודם, בלי שאף אחד צריך לקרוא לו במפורש. זו הסיבה
// שאין לו קובץ נפרד שקורא לו - Angular עצמו מפעיל אותו אוטומטית על כל בקשה.
//
// שני התפקידים שלו:
// ראשית, אם יש טוקן שמור, הוא מצרף אותו לבקשה בעצמו, כדי ששום שירות לא יצטרך
// לדעת על קיומו של טוקן בכלל.
// שנית, אם התשובה חוזרת עם קוד 401, סימן שהטוקן לא תקף, הוא מבצע logout
// אוטומטי, שמוגדר בקובץ auth.ts.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  console.log('2 - authInterceptor + auth.interceptor.ts');
  const auth = inject(AuthService);
  const token = auth.token();
  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authReq).pipe(
    catchError(err => {
      if (err.status === 401) auth.logout();
      return throwError(() => err);
    })
  );
};

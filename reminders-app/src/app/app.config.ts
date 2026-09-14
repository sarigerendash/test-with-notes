import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './auth.interceptor';

// appConfig
// ההגדרות הגלובליות של האפליקציה, שנקראות פעם אחת בלבד מתוך main.ts. השורה
// היחידה כאן רושמת את שירות ה-HttpClient, ומצרפת אליו את authInterceptor -
// מרגע זה, כל בקשת רשת בכל האפליקציה תעבור קודם דרכו, בלי שום צורך לרשום
// אותו שוב בשום מקום אחר.
export const appConfig: ApplicationConfig = {
  providers: [provideHttpClient(withInterceptors([authInterceptor]))]
};

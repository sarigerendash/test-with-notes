import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// זו נקודת הכניסה של כל האפליקציה - הקובץ הראשון שרץ, עוד לפני שכל שאר
// הקוד מתחיל לפעול. bootstrapApplication מקבל שני דברים: את הקומפוננטה
// הראשית, App, ואת ההגדרות הגלובליות, appConfig, שנמצאות בקובץ app.config.
console.log('1 - bootstrap + main.ts');
bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));

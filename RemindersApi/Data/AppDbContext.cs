using Microsoft.EntityFrameworkCore;
using RemindersApi.Model;

namespace RemindersApi.Data;

// AppDbContext
// זה הקובץ היחיד בכל הפרויקט שמדבר בפועל מול מסד הנתונים. כל שאר הקבצים
// שצריכים לגשת לנתונים מקבלים מופע של המחלקה הזו דרך הזרקת תלויות, ולא
// פונים למסד הנתונים בשום דרך אחרת.
//
// המחלקה נרשמת בקובץ Program, בשורה שבה מוגדר גם שהיא תעבוד מול מסד נתונים
// מסוג In-Memory, כלומר כזה שקיים רק בזיכרון ונמחק בכל הפעלה מחדש של השרת.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // שלושת המאפיינים האלה הם שלוש הטבלאות שקיימות במסד הנתונים. כל אחד מהם
    // הוא בעצם אוסף שאפשר לשאול ממנו נתונים, ואליו אפשר להוסיף שורות חדשות.
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ReminderRun> ReminderRuns => Set<ReminderRun>();

    // OnModelCreating
    // המתודה הזו רצה אוטומטית פעם אחת, כשמסד הנתונים נוצר לראשונה. כאן, ובכל
    // הפרויקט הזה רק כאן, מוזנים שני המשתמשים הקבועים: admin ו-viewer, כדי
    // שיהיה עם מי להתחבר מבלי לבנות מסך הרשמה נפרד.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", Password = "admin123", Role = "Admin" },
            new User { Id = 2, Username = "viewer", Password = "viewer123", Role = "Viewer" }
        );
    }
}

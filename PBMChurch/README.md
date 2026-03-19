# PBM Church Management + Attendance + Automation System

## 📋 Overview
Complete Church Management System with Attendance Tracking, Financial Management, Birthday Automation, Daily Verse Posting, and WhatsApp Integration. Built with ASP.NET Core 8.0, Entity Framework Core, and SQL Server.

## ✨ Features

### Core Modules
1. **Dashboard** - Real-time statistics and overview
2. **Church Management** - Add/Edit churches with meeting schedules
3. **Member Management** - Complete member profiles with search
4. **Attendance Module** - Smart attendance tracking with auto-absent marking
5. **Reports Module** - Attendance and financial reports
6. **Income & Expense Management** - Complete financial tracking
7. **Birthday List** - Today, upcoming, monthly views
8. **Daily Verse Posting** - Upload and auto-post Bible verses
9. **WhatsApp Automation** - Birthday wishes & daily verses
10. **Admin-Only Access** - Secure authentication system

### Key Highlights
- ✅ **100% Responsive** - Mobile, Tablet, Desktop, Ultrawide
- ✅ **Collapsible Sidebar** - Smooth navigation on all devices
- ✅ **Fixed Navbar** - Always accessible
- ✅ **Auto-Attendance** - Absent marking for unmarked members
- ✅ **Meeting Day Validation** - Only scheduled days allowed
- ✅ **Background Services** - Automated birthday wishes & verse posting
- ✅ **Real-time Dashboard** - Live statistics and charts

## 🗄️ Database Setup

### Step 1: Execute SQL Scripts

Run these scripts in order in SQL Server Management Studio (SSMS):

```sql
-- 1. First, ensure database exists
CREATE DATABASE PBM;
GO

-- 2. Then run:
Database/01_CreateTables.sql
Database/02_InsertSeedData.sql
Database/03_CreateProceduresAndViews.sql
```

### Database Schema

**Tables Created:**
- `AdminUsers` - Admin authentication
- `Churches` - Church information with meeting days
- `Members` - Member profiles with unique UserId
- `Attendance` - Attendance records with week tracking
- `Income` - Income transactions
- `Expenses` - Expense transactions
- `Verses` - Daily verse images
- `YouTubeLinks` - Sermon links
- `AutomationSettings` - WhatsApp automation config
- `BirthdayWishHistory` - Birthday wish tracking

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone/Open Project**
   ```powershell
   cd c:\Users\aviditej\source\repos\PBMChurch\PBMChurch
   ```

2. **Restore NuGet Packages**
   ```powershell
   dotnet restore
   ```

3. **Update Database Connection String**
   
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=PBM;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

4. **Execute Database Scripts**
   - Open SQL Server Management Studio
   - Connect to your SQL Server
   - Run scripts from `Database/` folder in order (01, 02, 03)

5. **Build and Run**
   ```powershell
   dotnet build
   dotnet run
   ```

6. **Access Application**
   - Open browser: `https://localhost:5001`
   - Default Login:
     - **Username:** `admin`
     - **Password:** `Admin@123`

## 📱 Responsive Breakpoints

The system is fully responsive across all device sizes:

| Device Type | Width Range | Layout Adjustments |
|------------|-------------|-------------------|
| Small Mobile | 360-480px | Compact sidebar, stacked cards |
| Large Mobile | 480-768px | Sidebar overlay, mobile-optimized |
| Tablet | 768-1024px | Narrower sidebar (220px) |
| Laptop | 1024-1440px | Standard layout (260px sidebar) |
| Desktop | 1440-1920px | Full layout with max container |
| Ultrawide | 2560px+ | Wider sidebar (300px), spacious layout |

## 🎯 Key Features Explained

### Attendance System
- Select church, week, and specific meeting date
- Enter User ID to auto-fetch member details
- Mark present with one click
- Auto-mark absent for all unmarked members
- View today's attendance in real-time

### Church Management
- Define 2 meeting days per church (e.g., Tuesday + Sunday)
- Active/Inactive status management
- Location tracking

### Member Management
- Unique User ID for each member
- Optional fields: Phone, Gender, Family
- Date of Birth for birthday tracking
- Church assignment
- Active/Inactive status

### Financial Management
- Separate Income and Expense tracking
- Church-wise categorization
- Monthly/Yearly filtering
- Summary reports with charts

### Birthday Automation
- **Today's Birthdays** - Current day celebrations
- **Upcoming 7 Days** - Week ahead planning
- **Upcoming 30 Days** - Month ahead view
- **Month Filter** - View any month
- **WhatsApp Integration** - Auto-send wishes at 7 AM

### Daily Verse Posting
- Upload Bible verse images
- System picks random/least-used verse
- Auto-posts to WhatsApp group at 6 AM
- Track posting history

### WhatsApp Automation
Three automation triggers:
1. **Birthday Wishes** (7:00 AM daily)
2. **Daily Verse** (6:00 AM daily)
3. **YouTube Links** (On-demand posting)

## 📊 Dashboard Components

The dashboard displays:
- Total active members count
- Today's present/absent statistics
- Today's birthdays with quick view
- Monthly income, expense, and balance
- Church-wise attendance comparison
- Upcoming birthdays (next 7 days)
- This week's meeting schedule

## 🛠️ Technology Stack

- **Backend:** ASP.NET Core 8.0 MVC
- **Database:** SQL Server 2019+
- **ORM:** Entity Framework Core 8.0
- **Frontend:** Bootstrap 5.3, Font Awesome 6.4
- **Authentication:** Cookie-based with ASP.NET Identity
- **Background Jobs:** Hosted Background Services

## 📂 Project Structure

```
PBMChurch/
├── Controllers/         # MVC Controllers
│   ├── AccountController.cs
│   ├── DashboardController.cs
│   ├── ChurchController.cs
│   ├── MemberController.cs
│   ├── AttendanceController.cs
│   ├── ReportController.cs
│   ├── FinanceController.cs
│   ├── BirthdayController.cs
│   ├── VerseController.cs
│   └── AutomationController.cs
├── Models/              # Data Models
│   ├── Church.cs
│   ├── Member.cs
│   ├── Attendance.cs
│   ├── Income.cs
│   ├── Expense.cs
│   ├── Verse.cs
│   ├── AutomationSetting.cs
│   └── ViewModels/
├── Services/            # Business Logic
│   ├── AttendanceService.cs
│   ├── ReportService.cs
│   ├── WhatsAppService.cs
│   └── AutomationBackgroundService.cs
├── Data/
│   └── AppDbContext.cs  # EF Core Context
├── Views/               # Razor Views
│   ├── Account/
│   ├── Dashboard/
│   ├── Church/
│   ├── Member/
│   ├── Attendance/
│   ├── Report/
│   ├── Finance/
│   ├── Birthday/
│   ├── Verse/
│   ├── Automation/
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/
│   ├── css/
│   │   └── site.css     # Responsive Styles
│   ├── js/
│   │   └── site.js      # JavaScript Functions
│   └── uploads/
│       └── verses/      # Uploaded verse images
└── Database/            # SQL Scripts
    ├── 01_CreateTables.sql
    ├── 02_InsertSeedData.sql
    └── 03_CreateProceduresAndViews.sql
```

## 🔐 Security Features

- Cookie-based authentication
- Password hashing (ready for BCrypt)
- HTTPS enforcement
- Anti-forgery tokens on all forms
- Authorize attribute on all controllers
- SQL injection prevention via EF Core parameterization

## 🎨 UI/UX Features

- **Collapsible Sidebar** - Toggle between expanded and collapsed
- **Fixed Navbar** - Always visible on scroll
- **Responsive Tables** - Horizontal scroll on mobile
- **Card-based Layout** - Modern, clean design
- **Alert Messages** - Success/Error notifications
- **Font Awesome Icons** - Visual indicators
- **Smooth Animations** - Professional transitions

## 📋 Navigation Menu

1. **Dashboard** - Home overview
2. **Churches** - Church management
3. **Members** - Member directory
4. **Attendance** - Mark attendance
5. **Reports** (Dropdown)
   - Attendance Report
   - Financial Report
6. **Finance** (Dropdown)
   - Income
   - Expense
7. **Birthdays** - Birthday list
8. **Daily Verses** - Verse manager
9. **Automation Settings** - WhatsApp config

## 🔧 Configuration

### WhatsApp Automation Settings

Configure in `AutomationSettings` table or via UI:

| Setting Key | Description | Default Value |
|------------|-------------|---------------|
| WhatsApp_Enabled | Enable/Disable automation | true |
| WhatsApp_API_URL | API endpoint | - |
| WhatsApp_Group_ID | Group for verse posting | - |
| Birthday_Wish_Time | Send time (24-hour) | 07:00 |
| Daily_Verse_Time | Send time (24-hour) | 06:00 |
| Birthday_Wish_Message | Message template | "Happy Birthday {Name}..." |
| YouTube_Auto_Post | Auto-post links | true |

## 📝 Default Login Credentials

```
Username: admin
Password: Admin@123
```

**⚠️ Important:** Change the default password after first login!

## 🎯 Business Logic Rules

1. **Attendance:**
   - Only on scheduled meeting days
   - One record per member per date
   - Auto-absent marking available
   - Week number auto-calculated

2. **Churches:**
   - Must have exactly 2 meeting days
   - Active/Inactive status management

3. **Members:**
   - Unique User ID across system
   - Must be assigned to a church
   - Active/Inactive status

4. **Birthday Wishes:**
   - Sent only once per year per member
   - Requires phone number
   - Tracked in history table

5. **Daily Verses:**
   - Picks least-posted verses first
   - Tracks posting count and date
   - Only active verses eligible

## 🚀 Future Enhancements

- [ ] SMS Integration
- [ ] Email Notifications
- [ ] Mobile App
- [ ] Multi-language Support
- [ ] Advanced Reporting
- [ ] Bulk Import/Export
- [ ] Role-based Permissions
- [ ] Online Giving Integration

## 🐛 Troubleshooting

### Database Connection Issues
```powershell
# Test connection string
dotnet ef dbcontext info
```

### Missing Dependencies
```powershell
dotnet restore
dotnet build
```

### Port Already in Use
Edit `Properties/launchSettings.json` to change ports

## 📞 Support

For issues or questions:
1. Check SQL scripts executed correctly
2. Verify connection string
3. Ensure all NuGet packages restored
4. Check SQL Server is running

## 📄 License

This project is created for PBM Church Management.

---

## 🎉 Quick Start Summary

1. **Database:** Run 3 SQL scripts in order
2. **Config:** Update connection string in appsettings.json
3. **Build:** `dotnet restore` then `dotnet build`
4. **Run:** `dotnet run`
5. **Login:** admin / Admin@123
6. **Enjoy:** Fully responsive church management system!

---

**Built with ❤️ for PBM Church Community**

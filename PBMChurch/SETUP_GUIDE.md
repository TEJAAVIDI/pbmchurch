# PBM Church Management System - Complete Setup Guide

## 🎉 Implementation Complete!

### ✅ What's Been Implemented

1. **Landing Page with Carousel**
   - Beautiful homepage with church activity carousel
   - Generic church information and features
   - Login button in navigation
   - No sidebar/navbar until logged in

2. **JWT Token Authentication**
   - Custom JWT token generation and validation
   - Token stored in HTTP-only cookies
   - 8-hour expiration (configurable)
   - Refresh token support

3. **Plain Text Password Storage**
   - AdminUsers table stores passwords in plain text
   - No password hashing (as requested)
   - Multiple user roles supported

4. **Forgot Password with OTP**
   - OTP generation (6-digit code)
   - 10-minute expiry
   - Email/SMS support (logs to console for demo)
   - Password reset functionality

5. **Conditional Sidebar Display**
   - Sidebar only shows after successful login
   - Landing page has no sidebar/navbar
   - JWT token validates authentication

6. **All CRUD Views Created**
   - Church Management (Index, Create, Edit)
   - Member Management (Index, Create, Edit)
   - Attendance Tracking (Index/MarkAttendance)
   - Dashboard with statistics

---

## 🔐 Default Login Credentials

### Admin Users (All Active)

| Username    | Password      | Role       | Email                      |
|-------------|---------------|------------|----------------------------|
| admin       | Admin@123     | Admin      | admin@pbmchurch.com        |
| pastor      | Pastor@123    | Pastor     | pastor@pbmchurch.com       |
| secretary   | Secretary@123 | Secretary  | secretary@pbmchurch.com    |

---

## 📊 Database Setup Instructions

### Important: Choose the Right Option

#### Option 1: Fresh Database (New Installation)
If you're setting up for the first time:

1. Create the database:
   ```sql
   CREATE DATABASE PBM;
   GO
   ```

2. Execute scripts in order:
   - `Database/01_CreateTables.sql`
   - `Database/02_InsertSeedData.sql`
   - `Database/03_CreateProceduresAndViews.sql`

#### Option 2: Update Existing Database (Already have PBM database)
If you already have a PBM database with old table structure:

1. **FIRST**, run the migration script:
   - `Database/00_UpdateAdminUsersTable.sql`
   
   ⚠️ This will update the AdminUsers table and set all passwords to `Admin@123`

2. Verify the migration worked by checking for these columns:
   ```sql
   SELECT * FROM AdminUsers;
   -- Should show: UserId, Username, Password, Role, RefreshToken, ResetOtp, OtpExpiry, etc.
   ```

3. **DO NOT** re-run `01_CreateTables.sql` - it will drop all your data!

4. Optionally run:
   - `Database/02_InsertSeedData.sql` (if you want additional sample users)
   - `Database/03_CreateProceduresAndViews.sql` (if procedures don't exist)

For detailed instructions, see `Database/README_DATABASE_SETUP.md`

---

## 🚀 Running the Application

### Step 1: Restore NuGet Packages
```powershell
dotnet restore
```

### Step 2: Build the Project
```powershell
dotnet build
```

### Step 3: Run the Application
```powershell
dotnet run
```

### Step 4: Access the Application
- **Landing Page**: `https://localhost:5001/` or `http://localhost:5000/`
- **Login Page**: Click "Login" button on landing page or navigate to `/Account/Login`

---

## 🔑 JWT Configuration

Located in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "PBMChurch_SuperSecretKey_2024_ForJWTAuthentication_MustBe32CharactersOrMore!",
    "Issuer": "PBMChurch",
    "Audience": "PBMChurchUsers",
    "ExpiryHours": 8
  }
}
```

**Token Storage:**
- Stored in HTTP-only cookie named `jwt_token`
- Refresh token in `refresh_token` cookie
- Also stored in Session for easy access

---

## 🔄 Authentication Flow

### Login Process
1. User visits landing page (no authentication required)
2. Clicks "Login" button
3. Enters username and plain text password
4. System validates credentials against AdminUsers table
5. JWT token generated and stored in cookie
6. User redirected to Dashboard
7. Sidebar and navbar now visible

### Logout Process
1. User clicks "Logout" button
2. JWT and refresh tokens cleared from cookies
3. Session cleared
4. User redirected to landing page

### Forgot Password Process
1. User clicks "Forgot Password" link on login page
2. Enters username or email
3. System generates 6-digit OTP
4. OTP sent via email/SMS (logs to console for demo)
5. OTP valid for 10 minutes
6. User enters OTP and new password
7. Password updated in plain text
8. User can login with new password

---

## 📂 Key Files Modified/Created

### Controllers
- `AccountController.cs` - JWT login, logout, forgot password, reset password
- `HomeController.cs` - Landing page route

### Services
- `JwtTokenService.cs` - JWT generation and validation
- `OtpService.cs` - OTP generation and sending

### Views
- `Views/Home/Index.cshtml` - Landing page with carousel
- `Views/Account/Login.cshtml` - Login page with forgot password link
- `Views/Account/ForgotPassword.cshtml` - OTP request page
- `Views/Account/ResetPassword.cshtml` - Password reset page
- `Views/Shared/_Layout.cshtml` - Conditional sidebar display
- `Views/Church/Index.cshtml` - Church list
- `Views/Church/Create.cshtml` - Add church
- `Views/Church/Edit.cshtml` - Edit church
- `Views/Member/Index.cshtml` - Member list with search
- `Views/Member/Create.cshtml` - Add member
- `Views/Member/Edit.cshtml` - Edit member
- `Views/Attendance/Index.cshtml` - Mark attendance

### Configuration
- `Program.cs` - JWT authentication setup
- `appsettings.json` - JWT configuration
- `PBMChurch.csproj` - JWT NuGet packages

### Database
- `Database/01_CreateTables.sql` - Updated AdminUsers table
- `Database/02_InsertSeedData.sql` - 3 users with plain passwords

---

## 🎨 Landing Page Features

### Carousel Images
- 4 slides with church activities
- Worship services
- Bible study
- Youth ministry
- Community outreach

### Activities Section
- 6 activity cards with images
- Worship services (Sundays 9 AM & 11 AM)
- Bible study (Wednesdays 7 PM)
- Youth ministry (Fridays 6:30 PM)
- Community outreach (Monthly)
- Prayer meetings (Daily 6 AM)
- Special celebrations

### Features Section
- 8 feature cards highlighting system capabilities
- Member management
- Attendance tracking
- Finance management
- Birthday wishes
- Daily verse
- Reports & analytics
- Multi-church support
- WhatsApp automation

---

## 🛡️ Security Notes

⚠️ **Important**: Passwords are stored in plain text as requested. This is NOT recommended for production environments. For production:
- Use BCrypt, Argon2, or PBKDF2 for password hashing
- Implement rate limiting on login attempts
- Add CAPTCHA for failed login attempts
- Use HTTPS in production
- Rotate JWT secret keys regularly

---

## 📱 Responsive Design

The system is fully responsive:
- **Mobile** (360px - 767px): Stack layout, mobile menu
- **Tablet** (768px - 1023px): 2-column layout
- **Desktop** (1024px - 1439px): Full sidebar, 3-column layout
- **Large Desktop** (1440px+): Wide layout, 4-column grids
- **Ultrawide** (2560px+): Maximum width constraints

---

## 🔧 Troubleshooting

### "The view 'Index' was not found" Error
✅ **Fixed** - All missing views have been created

### Login Doesn't Work
- Verify database connection string in `appsettings.json`
- Ensure seed data script has been executed
- Check console for error messages
- Try credentials: `admin` / `Admin@123`

### Sidebar Not Showing After Login
- Check if JWT token is in cookies (F12 > Application > Cookies)
- Verify Session has UserName and FullName
- Check browser console for JavaScript errors

### OTP Not Received
- OTPs are logged to console (not actually sent for demo)
- Check application logs for OTP output
- OTP is valid for 10 minutes only

---

## 📞 Support & Documentation

For additional help:
1. Check `README.md` for general information
2. Review `PROJECT_SUMMARY.md` for technical details
3. See `Database/README_SQL_SCRIPTS.md` for database documentation

---

**Built with ❤️ for the Kingdom of God**

© 2024 PBM Church Management System

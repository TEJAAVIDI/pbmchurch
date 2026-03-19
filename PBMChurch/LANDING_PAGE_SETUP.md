# Landing Page Setup Instructions

## Overview
A professional, church-themed landing page has been created for your PBM Church Management System. The landing page includes:
- Beautiful hero section with church branding
- About section with mission and vision
- Services and programs showcase
- Stats section with live church count
- Individual church listings with meeting times
- Image gallery (when populated)
- Contact information
- Professional navigation and footer

## Database Setup

### Step 1: Run SQL Script
Execute the SQL script to create required tables:

```sql
-- File: SQL_Scripts/CreateLandingPageTables.sql
```

This creates two new tables:
1. **LandingPageContents** - Stores text content for landing page sections
2. **GalleryImages** - Stores church photos and event images

### Step 2: Verify Tables Created
Check that tables exist:
```sql
SELECT * FROM LandingPageContents;
SELECT * FROM GalleryImages;
```

## Image Upload Instructions

### Gallery Images Folder
1. Create folder structure:
   ```
   wwwroot/
     └── images/
         └── gallery/
   ```

2. Upload your church images to `wwwroot/images/gallery/`

### Recommended Images
- **Service Photos**: Sunday worship, prayer meetings
- **Worship Photos**: Praise team, congregation worship
- **Community Photos**: Fellowship events, gatherings
- **Youth Ministry**: Youth activities, children's programs
- **Events**: Special celebrations, baptisms
- **Outreach**: Community service activities

### Image Specifications
- **Format**: JPG or PNG
- **Size**: 1200x800 pixels (recommended)
- **Aspect Ratio**: 3:2 or 16:9
- **Max File Size**: 5MB
- **Orientation**: Landscape preferred

### Insert Gallery Images
After uploading images, add them to database:

```sql
INSERT INTO GalleryImages (ImagePath, Title, Description, ChurchId, Category, DisplayOrder, IsActive)
VALUES 
('/images/gallery/sunday-service.jpg', 'Sunday Worship Service', 'Our vibrant Sunday morning worship', 1, 'Service', 1, 1),
('/images/gallery/praise-worship.jpg', 'Praise & Worship', 'Worshipping together in spirit', 1, 'Worship', 2, 1),
('/images/gallery/community-gathering.jpg', 'Community Fellowship', 'Building relationships together', 2, 'Community', 3, 1),
('/images/gallery/youth-ministry.jpg', 'Youth Ministry', 'Empowering the next generation', 1, 'Youth', 4, 1),
('/images/gallery/prayer-meeting.jpg', 'Prayer Meeting', 'United in prayer and faith', 3, 'Service', 5, 1),
('/images/gallery/baptism-service.jpg', 'Baptism Service', 'Celebrating new believers', 2, 'Event', 6, 1),
('/images/gallery/outreach-program.jpg', 'Community Outreach', 'Serving our local community', 1, 'Outreach', 7, 1),
('/images/gallery/worship-team.jpg', 'Worship Team', 'Leading praise and worship', 1, 'Worship', 8, 1);
```

### Image Categories
Use these categories for organization:
- **Service** - Regular worship services
- **Worship** - Praise and worship moments
- **Community** - Fellowship and gatherings
- **Event** - Special celebrations
- **Youth** - Youth ministry activities
- **Outreach** - Community outreach programs

## Content Management

### Update Landing Page Content
Modify text content in database:

```sql
-- Update Hero section
UPDATE LandingPageContents 
SET Title = 'Your Custom Title', 
    Content = 'Your custom description'
WHERE SectionName = 'Hero';

-- Update About section
UPDATE LandingPageContents 
SET Content = 'Your church mission statement and vision'
WHERE SectionName = 'About';
```

### Add New Content Sections
```sql
INSERT INTO LandingPageContents (SectionName, Title, Content, DisplayOrder, IsActive)
VALUES ('NewSection', 'Section Title', 'Section content text', 4, 1);
```

## Features Implemented

### Landing Page Features
✅ Professional navigation with smooth scrolling
✅ Hero section with church branding
✅ About/Mission section
✅ Services showcase with icons
✅ Live statistics (church count, members)
✅ Church listings with locations and meeting days
✅ Image gallery (when populated)
✅ Contact section
✅ Responsive footer
✅ Mobile-friendly design
✅ Scroll-to-top button

### Sidebar Enhancement
✅ Added **Automation** dropdown menu with:
   - Settings (existing)
   - **WhatsApp Groups** (new) - Links to `https://localhost:44388/Automation/ShowGroups`

### Technical Updates
✅ New Models: `LandingPageContent`, `GalleryImage`
✅ Updated `AppDbContext` with new DbSets
✅ Updated `HomeController` to load landing page data
✅ Enhanced `_Layout.cshtml` with Automation submenu
✅ Beautiful landing page with modern design

## Accessing the Landing Page

### For Unauthenticated Users
- Navigate to: `https://localhost:44388/`
- See professional landing page with church information

### For Authenticated Users
- Navigate to: `https://localhost:44388/`
- Automatically redirected to Dashboard

## Accessing WhatsApp Groups

### Via Sidebar (When Logged In)
1. Click **Automation** in sidebar
2. Click **WhatsApp Groups**
3. View all WhatsApp groups for configured churches

### Direct URL
- `https://localhost:44388/Automation/ShowGroups`

## Customization Options

### Change Color Scheme
Edit CSS variables in `Views/Home/Index.cshtml`:
```css
:root {
    --primary-color: #2c5282;     /* Change to your primary color */
    --secondary-color: #4299e1;   /* Change to your secondary color */
    --accent-color: #ed8936;      /* Change to your accent color */
}
```

### Update Contact Information
Search for "Contact" section in `Views/Home/Index.cshtml` and update:
- Phone number
- Email address
- Physical address

### Modify Service Times
Update footer section with your actual service times:
```html
<p><i class="fas fa-clock me-2"></i><strong>Friday:</strong> 7:00 PM - 9:00 PM</p>
<p><i class="fas fa-clock me-2"></i><strong>Sunday:</strong> 9:00 AM - 11:00 AM</p>
```

## Testing

### Test Landing Page
1. Logout from system
2. Navigate to `https://localhost:44388/`
3. Verify all sections display correctly
4. Test navigation links (smooth scrolling)
5. Test Login button redirects to `/Account/Login`
6. Test mobile responsiveness

### Test WhatsApp Groups Access
1. Login to system
2. Click **Automation** → **WhatsApp Groups**
3. Verify groups display for configured churches

## Troubleshooting

### Landing Page Not Showing
- Check that you ran the SQL script
- Verify `LandingPageContents` table exists
- Check `HomeController.Index()` method loads data

### Images Not Displaying
- Verify folder exists: `wwwroot/images/gallery/`
- Check image paths in database start with `/images/gallery/`
- Ensure images are uploaded to correct folder
- Check file permissions

### Sidebar Menu Issues
- Clear browser cache
- Rebuild project: `dotnet build`
- Restart application

### WhatsApp Groups Not Loading
- Verify WhatsApp configuration in Churches table
- Check `AutomationController.ShowGroups()` method exists
- Verify church has `WhatsAppInstanceId`, `WhatsAppApiToken`, `WhatsAppApiUrl` configured

## Future Enhancements

Consider adding:
- [ ] Admin panel to edit landing page content
- [ ] Image upload interface (instead of manual SQL)
- [ ] Video gallery for sermon recordings
- [ ] Online donation integration
- [ ] Newsletter signup form
- [ ] Live streaming links
- [ ] Event calendar
- [ ] Blog/announcements section

## Support

For questions or issues:
1. Check this README
2. Review SQL script comments
3. Verify all steps completed
4. Test in different browsers

---

**Created for PBM Church Management System**
*Building communities of faith with modern technology*

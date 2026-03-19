# Gallery Management System - Quick Guide

## ✅ What's Been Created

### 1. **Gallery Management Page** (Admin Only)
- **Location:** Sidebar → Gallery
- **URL:** `https://localhost:44388/Gallery`
- **Features:**
  - Upload images with preview
  - Edit existing images
  - Delete images
  - Categorize images (Service, Worship, Community, Event, Youth, Outreach)
  - Assign images to specific churches
  - Set display order
  - Activate/Deactivate images

### 2. **Landing Page Gallery Section**
- **Location:** Public landing page
- **URL:** `https://localhost:44388/` (when not logged in)
- **Shows:** All active gallery images from database
- **Features:**
  - Beautiful grid layout
  - Hover effects with title/description
  - Category badges
  - Responsive design

### 3. **Database Tables**
- **GalleryImages** - Stores all gallery data
- **LandingPageContents** - Stores landing page text content

---

## 📝 How to Use

### Step 1: Run SQL Script
Execute the SQL script to create tables:
```
File: SQL_Scripts/CreateLandingPageTables.sql
```

Run this in your SQL Server Management Studio or Azure Data Studio.

### Step 2: Upload Images via Admin Panel

1. **Login** to your system
2. Click **Gallery** in the sidebar
3. Click **Upload Image** button
4. **Fill in the form:**
   - Select image file (JPG, PNG, GIF - max 5MB)
   - Enter title (e.g., "Sunday Worship Service")
   - Enter description (e.g., "Our vibrant Sunday morning worship")
   - Select church (optional)
   - Select category (Service, Worship, Community, etc.)
   - Set display order (lower numbers appear first)
   - Check "Active" to show on landing page
5. Click **Upload Image**

### Step 3: View on Landing Page

1. **Logout** or open incognito window
2. Go to `https://localhost:44388/`
3. Scroll to **Gallery** section
4. See your uploaded images!

---

## 🎨 Image Guidelines

### Best Practices:
- **Size:** 1200x800 pixels (landscape)
- **Format:** JPG (best for photos), PNG (with transparency)
- **Quality:** High quality but web-optimized
- **Orientation:** Landscape works best
- **Content:** Clear, well-lit photos with visible faces

### Categories:
- **Service** - Sunday services, prayer meetings
- **Worship** - Praise team, worship moments
- **Community** - Fellowship events, gatherings
- **Event** - Special celebrations, baptisms
- **Youth** - Youth ministry activities
- **Outreach** - Community service programs

---

## 📂 Where Images Are Stored

### Physical Location:
```
wwwroot/images/gallery/
```

### Database:
- Path stored in `GalleryImages` table
- Example: `/images/gallery/abc123-def456.jpg`

### How It Works:
1. You upload image through admin panel
2. Image saved to `wwwroot/images/gallery/` with unique name
3. Path saved in database
4. Landing page reads from database and displays images

---

## 🔧 Management Features

### Edit Image:
1. Go to Gallery page
2. Click **Edit** on any image
3. Update title, description, category, etc.
4. Optionally upload new image (replaces old one)
5. Click **Update Image**

### Delete Image:
1. Go to Gallery page
2. Click **Delete** on any image
3. Confirm deletion
4. Image file deleted from server
5. Database record removed

### Activate/Deactivate:
- Uncheck "Active" to hide image from landing page
- Image remains in database but won't show publicly
- Useful for seasonal or temporary images

---

## 🎯 Common Tasks

### Upload Church Event Photos:
1. Gallery → Upload Image
2. Select photo
3. Title: "Christmas Celebration 2024"
4. Description: "Our annual Christmas service"
5. Church: Select your church
6. Category: Event
7. Display Order: 1 (to show first)
8. Active: ✓ Checked
9. Upload

### Organize Gallery Order:
- Lower display order = shows first
- Example: Order 1, 2, 3, 4, 5...
- Edit any image to change its order

### Filter by Church:
- Each image can be linked to a specific church
- Useful if managing multiple locations
- Leave blank for general church images

---

## 🚀 Features

### Admin Panel:
✅ Upload images with live preview
✅ Image validation (type, size)
✅ Automatic unique filename generation
✅ Edit without re-uploading (optional)
✅ Bulk viewing with thumbnails
✅ Status badges (Active/Inactive)
✅ Church and category filters
✅ Display order management

### Landing Page:
✅ Beautiful grid layout
✅ Hover effects showing details
✅ Responsive (mobile-friendly)
✅ Only shows active images
✅ Sorted by display order
✅ Category badges
✅ Smooth animations

---

## 💡 Tips

1. **Start Small:** Upload 4-8 images initially
2. **Quality Over Quantity:** Better to have few great photos than many poor ones
3. **Update Regularly:** Keep gallery fresh with recent events
4. **Use Descriptions:** Help visitors understand the context
5. **Test on Mobile:** Check how it looks on phones
6. **Seasonal Updates:** Update for Christmas, Easter, etc.
7. **Backup Images:** Keep originals before uploading

---

## 🐛 Troubleshooting

### Images Not Showing on Landing Page?
- ✓ Check image is marked "Active"
- ✓ Verify image uploaded successfully
- ✓ Logout and view as public user
- ✓ Check browser cache (Ctrl+F5)
- ✓ Verify SQL tables created

### Upload Failed?
- Check file size (must be under 5MB)
- Check file type (JPG, PNG, GIF only)
- Verify folder permissions
- Check server logs for errors

### Image Looks Wrong?
- Re-upload with correct dimensions (1200x800)
- Check image isn't corrupted
- Try different image format

---

## 📊 Summary

**You can now:**
- ✅ Upload church photos through admin panel
- ✅ Manage gallery images (edit, delete, organize)
- ✅ Display images on public landing page
- ✅ Categorize by type (Service, Worship, etc.)
- ✅ Link images to specific churches
- ✅ Control visibility (active/inactive)

**Images are stored:**
- 🖼️ Physically: `wwwroot/images/gallery/`
- 💾 Database: `GalleryImages` table
- 🌐 Web path: `/images/gallery/filename.jpg`

**No more manual SQL inserts needed!**

---

*Built for PBM Church Management System*
*Easy image management for your church website*

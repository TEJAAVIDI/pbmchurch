# WhatsApp API Integration Guide for PBM Church Management System

## 📱 Overview
This guide will help you integrate WhatsApp API with your church management system to automatically send:
- Birthday wishes to members
- Daily Bible verses
- Attendance reminders
- YouTube video links

## 🆓 Recommended Service: Green API

### Free Developer Plan
✅ **UNLIMITED messages per month**  
✅ **Limited to 3 chats** (perfect for testing or small groups)  
✅ No credit card required  
✅ Easy setup with QR code scanning  
✅ Supports text, images, and media  

### Paid Plans (if you need more than 3 chats)
- **Starter:** $15/month - Unlimited chats, 5000 messages
- **Business:** $50/month - Unlimited chats, 20,000 messages

### Why Green API?
✅ Free tier with unlimited messages (3 chats)  
✅ Perfect for testing with key staff  
✅ Easy upgrade path when needed  
✅ Good documentation and support  
✅ Works with regular WhatsApp (no Business account required)

### Smart Usage Strategies

#### Strategy 1: Group-Based Approach (FREE Plan)
Use your 3 free chats for:
1. **Church Admin Group** - Main coordination
2. **All Members Group** - Daily verses, announcements
3. **Leadership Group** - Pastor, elders, ministry heads

💡 **Result:** Unlimited messages to entire church via groups!

#### Strategy 2: Individual Messages (Paid Plan)
- Send personal birthday wishes to each member
- Individual prayer request updates
- One-on-one communication
- **Cost:** $15-50/month depending on volume  

## 🚀 Step-by-Step Setup

### Step 1: Create Green API Account

1. Go to [https://green-api.com/](https://green-api.com/)
2. Click **"Sign Up"** or **"Get Started"**
3. Register with your email
4. Verify your email address
5. **Select "FREE Developer" plan** (Unlimited messages, 3 chats)

### Step 2: Create WhatsApp Instance

1. After login, click **"Create Instance"**
2. You'll receive:
   - **Instance ID** (e.g., `1234567890`)
   - **API Token** (e.g., `abc123def456...`)
3. **Save these credentials** - you'll need them!

### Step 3: Connect Your WhatsApp

1. In your Green API dashboard, find your instance
2. Click **"Scan QR Code"**
3. Open WhatsApp on your phone:
   - **Android/iPhone:** WhatsApp > Settings > Linked Devices > Link a Device
4. Scan the QR code shown on Green API
5. Wait for "Connected" status (green indicator)

### Step 4: Configure in PBM Church System

1. Login to your PBM Church admin panel
2. Go to **Automation > Settings**
3. In the WhatsApp API Configuration section, enter:
   - **Instance ID:** (from Step 2)
   - **API Token:** (from Step 2)
   - **API URL:** `https://api.green-api.com` (default)
   - **Phone Number:** Your WhatsApp number (e.g., +91 9876543210)
4. Enable **"Enable WhatsApp Integration"** toggle
5. Click **"Save WhatsApp Settings"**
6. Click **"Test Connection"** to verify

### Step 5: Configure Automation Features

#### Birthday Wishes
1. Scroll to **"Birthday Automation"** section
2. Enable **"Enable Automatic Birthday Wishes"**
3. Set **Birthday Wish Time** (e.g., 09:00 AM)
4. Customize message template:
   ```
   🎂 Happy Birthday {Name}! 🎉
   
   May God bless you abundantly on your special day!
   Wishing you a year filled with joy, health, and prosperity.
   
   - PBM Church Family
   ```
5. Click **"Save Birthday Settings"**

#### Daily Verse Sharing
1. Scroll to **"Verse of the Day Sharing"** section
2. Enable **"Enable Daily Verse Sharing"**
3. Set **Verse Sharing Time** (e.g., 07:00 AM)
4. Choose who receives: Members and/or Groups
5. Click **"Save Verse Settings"**

#### Attendance Reminders
1. Scroll to **"Attendance Reminders"** section
2. Enable **"Enable Attendance Reminders"**
3. Set **Reminder Time** (e.g., 6:00 PM day before meeting)
4. Customize reminder message
5. Select **"Send Reminder Before"** (Same Day, 1 Day, or 2 Days)
6. Click **"Save Reminder Settings"**

## 📊 Message Limits & Strategies

### FREE Developer Plan
- **UNLIMITED messages per month** 🎉
- **3 chats maximum** (contacts or groups)
- Perfect for testing and small churches using groups

**Smart Setup for Free Plan:**
1. Create 3 WhatsApp Groups:
   - **"PBM Church - All Members"** (daily verses, announcements)
   - **"PBM Church - Leadership"** (admin coordination)
   - **"PBM Church - Ministry Teams"** (volunteers)
2. Add all members to appropriate groups
3. Send unlimited messages to all groups
4. **Cost:** $0/month ✅

**Example Usage (FREE):**
- Unlimited daily verses to "All Members" group
- Unlimited reminders to all groups
- Unlimited YouTube links
- **Total: UNLIMITED messages to 300+ members via 3 groups!**

### Paid Plans (For Individual Messages)

If you need to send individual birthday wishes or personal messages:

**Starter Plan - $15/month**
- Unlimited chats
- 5000 messages/month
- Good for 50-100 members with individual messages

**Business Plan - $50/month**
- Unlimited chats
- 20,000 messages/month
- Good for 100-300 members with individual messages

### Recommendation by Church Size

#### Small Church (< 50 members)
- **Use:** FREE Developer Plan + Groups
- **Cost:** $0/month
- **Why:** Everyone can be in 3 groups, unlimited messages

#### Medium Church (50-150 members)
- **Option 1:** FREE Plan + Groups ($0/month)
- **Option 2:** Starter Plan for personal messages ($15/month)
- **Recommended:** Start free, upgrade if needed

#### Large Church (> 150 members)
- **Use:** FREE Plan for groups + announcements ($0/month)
- **Or:** Business Plan for personal messages ($50/month)
- **Best:** Combine both (groups + key personal messages)

## 🧪 Testing Your Setup

### Test 1: Connection Test
1. Go to Automation > Settings
2. Click **"Test Connection"**
3. You should see: ✅ "Connection successful!"

### Test 2: Send Test Message
1. Go to Birthday Management
2. Find a member with today's birthday
3. Click **"Send SMS"** button
4. Check if message arrives on WhatsApp

### Test 3: Manual Birthday Wish
1. In Automation Settings, scroll to bottom
2. Click **"Test Automation"**
3. This sends a test birthday wish

## 🔍 Troubleshooting

### Issue: "Connection Failed"
**Solutions:**
- Check Instance ID and API Token are correct
- Ensure WhatsApp is still connected (check Green API dashboard)
- Verify API URL is `https://api.green-api.com`
- Try scanning QR code again

### Issue: "3 Chat Limit Reached" (Free Plan)
**Solutions:**
- You've used all 3 chats in the free plan
- **Option 1:** Use WhatsApp Groups (recommended)
  * Create 3 groups instead of messaging individuals
  * Add all members to groups
  * Send unlimited messages to groups
- **Option 2:** Upgrade to paid plan ($15/month)
  * Get unlimited chats
  * Send individual messages
- **Option 3:** Wait until next month
  * Free plan resets monthly
- **Best Practice:** Free plan works great with groups!
**Check:**
1. **WhatsApp Integration Enabled:** Settings toggle is ON
2. **Instance Active:** Green dashboard shows "Connected"
3. **Phone Numbers Valid:** Format +91XXXXXXXXXX
4. **Message Limit:** Haven't exceeded 1000/month
5. **Background Service Running:** Check "Background Service Status" in Automation Settings

### Issue: "Messages Not Sending"
- Phone numbers must include country code
- India format: `+919876543210` or `919876543210`
- Remove spaces and special characters
- System auto-adds `91` prefix for 10-digit numbers

### Issue: "QR Code Expired"
- QR codes expire after 2 minutes
- Refresh the Green API page
- Generate a new QR code
- Scan immediately

## 📝 Best Practices

### For FREE Plan Users:
1. **Use Groups:** Create 3 strategic WhatsApp groups
2. **Group Strategy:**
   - Group 1: All Members (verses, announcements)
   - Group 2: Leadership Team (admin updates)
   - Group 3: Ministry Volunteers (coordination)
3. **Unlimited Messages:** Send as many messages as needed to these 3 groups
4. **Member Engagement:** All 300+ members can receive unlimited messages

### For Paid Plan Users:
1. **Personal Touch:** Send individual birthday wishes
2. **Direct Messages:** One-on-one prayer support
3. **Both Approaches:** Use groups for general + individual for special occasions

### General Best Practices:
1. **Test Before Deploying:** Always test with your own number first
2. **Message Templates:** Keep messages short, friendly, and branded
3. **Timing:** Send messages during appropriate hours (9 AM - 8 PM)
4. **Monitor Usage:** Check Green API dashboard for chat count (free) or message count (paid)
5. **Backup Phone:** Have a backup admin phone number
6. **Regular QR Scan:** WhatsApp may require re-scanning every 30-60 days

## 🔐 Security Tips

1. **Keep API Token Secret:** Never share your API Token publicly
2. **Restrict Access:** Only give admin panel access to trusted staff
3. **Use HTTPS:** Always access admin panel via HTTPS
4. **Regular Updates:** Keep the system updated
5. **Monitor Logs:** Check "Background Service Status" for unusual activity

## 📞 Support & Resources

### Green API Resources
- Documentation: [https://green-api.com/en/docs/](https://green-api.com/en/docs/)
- Support: support@green-api.com
- Status Page: [https://status.green-api.com/](https://status.green-api.com/)

### Alternative Services (if needed)

#### Option 2: WAHA (WhatsApp HTTP API)
- Free and open-source
- Self-hosted solution
- Requires Docker installation
- Website: [https://waha.devlike.pro/](https://waha.devlike.pro/)

#### Option 3: Twilio WhatsApp Business API
- Enterprise solution
- Pay-as-you-go pricing
- Very reliable
- Website: [https://www.twilio.com/whatsapp](https://www.twilio.com/whatsapp)

## 📈 Usage Recommendations by Church Size

### Small Church (< 50 members)
- **Plan:** FREE Developer (3 chats)
- **Strategy:** Use WhatsApp Groups
- **Cost:** $0/month
- **Groups Needed:** 1-2 groups sufficient
- ✅ **Perfect fit!**

### Medium Church (50-150 members)
- **Plan:** FREE Developer (3 chats) 
- **Strategy:** 3 strategic groups cover everyone
- **Cost:** $0/month
- **Alternative:** Starter Plan if you want individual birthday messages ($15/month)
- ✅ **Free plan works great!**

### Large Church (> 150 members)
- **Plan:** FREE Developer for groups
- **Strategy:** 
  * Group 1: All Members (daily content)
  * Group 2: Leaders (coordination)
  * Group 3: Rotate between ministry teams
- **Cost:** $0/month
- **Upgrade:** Only if you want personal messages to all
- ✅ **Free plan still works!**

### Key Insight
💡 **With the FREE plan + WhatsApp Groups approach, you can reach UNLIMITED members with UNLIMITED messages!**

## ✅ Quick Checklist

Before going live, ensure:
- [ ] Green API account created
- [ ] Instance created and WhatsApp connected
- [ ] Instance ID and API Token saved in system
- [ ] Connection test passed
- [ ] Birthday automation configured
- [ ] Message templates customized
- [ ] Test messages sent successfully
- [ ] Background service running
- [ ] Phone numbers validated
- [ ] Admin trained on system

## 🎯 Next Steps

After successful setup:
1. Add all member phone numbers in Member Management
2. Verify birthday dates are accurate
3. Upload verse images for daily sharing
4. Configure church meeting days for reminders
5. Monitor first week of automated messages
6. Adjust message templates based on feedback

---

**Need Help?** Contact your system administrator or refer to the PBM Church system documentation.

**Last Updated:** December 2025

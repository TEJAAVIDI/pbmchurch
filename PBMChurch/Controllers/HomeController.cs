using Microsoft.AspNetCore.Mvc;
using PBMChurch.Models;
using PBMChurch.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace PBMChurch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Redirect to dashboard if authenticated
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            
            // Show landing page for non-authenticated users
            var contents = await _context.LandingPageContents
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var galleryImages = await _context.GalleryImages
                .Where(g => g.IsActive)
                .Include(g => g.Church)
                .OrderBy(g => g.DisplayOrder)
                .Take(8)
                .ToListAsync();

            var churches = await _context.Churches
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.ChurchName)
                .ToListAsync();

            var churchActivities = await _context.ChurchActivities
                .Where(a => a.IsActive && a.ToDateTime >= DateTime.Now)
                .OrderBy(a => a.FromDateTime)
                .Take(6)
                .ToListAsync();

            var bibleReading = GetTodayBibleReading();

            ViewBag.GalleryImages = galleryImages;
            ViewBag.Churches = churches;
            ViewBag.ChurchActivities = churchActivities;
            ViewBag.BibleReading = bibleReading;

            return View(contents);
        }

        private object GetTodayBibleReading()
        {
            var today = DateTime.Today;
            var dayOfYear = today.DayOfYear;
            var readingText = GetReadingForDay(dayOfYear);

            return new
            {
                DayOfYear = dayOfYear,
                Year = today.Year,
                TodaysReading = readingText
            };
        }

        private string GetReadingForDay(int dayOfYear)
        {
            // Same reading plan as Dashboard and Wishes - 3 chapters per day to complete Bible in a year
            var readings = new string[]
            {
                "Genesis 1-3", "Genesis 4-6", "Genesis 7-9", "Genesis 10-12", "Genesis 13-15",
                "Genesis 16-18", "Genesis 19-21", "Genesis 22-24", "Genesis 25-27", "Genesis 28-30",
                "Genesis 31-33", "Genesis 34-36", "Genesis 37-39", "Genesis 40-42", "Genesis 43-45",
                "Genesis 46-48", "Genesis 49-50, Exodus 1", "Exodus 2-4", "Exodus 5-7", "Exodus 8-10",
                "Exodus 11-13", "Exodus 14-16", "Exodus 17-19", "Exodus 20-22", "Exodus 23-25",
                "Exodus 26-28", "Exodus 29-31", "Exodus 32-34", "Exodus 35-37", "Exodus 38-40",
                "Leviticus 1-3", "Leviticus 4-6", "Leviticus 7-9", "Leviticus 10-12", "Leviticus 13-15",
                "Leviticus 16-18", "Leviticus 19-21", "Leviticus 22-24", "Leviticus 25-27", "Numbers 1-3",
                "Numbers 4-6", "Numbers 7-9", "Numbers 10-12", "Numbers 13-15", "Numbers 16-18",
                "Numbers 19-21", "Numbers 22-24", "Numbers 25-27", "Numbers 28-30", "Numbers 31-33",
                "Numbers 34-36", "Deuteronomy 1-3", "Deuteronomy 4-6", "Deuteronomy 7-9", "Deuteronomy 10-12",
                "Deuteronomy 13-15", "Deuteronomy 16-18", "Deuteronomy 19-21", "Deuteronomy 22-24", "Deuteronomy 25-27",
                "Deuteronomy 28-30", "Deuteronomy 31-34", "Joshua 1-3", "Joshua 4-6", "Joshua 7-9",
                "Joshua 10-12", "Joshua 13-15", "Joshua 16-18", "Joshua 19-21", "Joshua 22-24",
                "Judges 1-3", "Judges 4-6", "Judges 7-9", "Judges 10-12", "Judges 13-15",
                "Judges 16-18", "Judges 19-21", "Ruth 1-4", "1 Samuel 1-3", "1 Samuel 4-6",
                "1 Samuel 7-9", "1 Samuel 10-12", "1 Samuel 13-15", "1 Samuel 16-18", "1 Samuel 19-21",
                "1 Samuel 22-24", "1 Samuel 25-27", "1 Samuel 28-31", "2 Samuel 1-3", "2 Samuel 4-6",
                "2 Samuel 7-9", "2 Samuel 10-12", "2 Samuel 13-15", "2 Samuel 16-18", "2 Samuel 19-21",
                "2 Samuel 22-24", "1 Kings 1-3", "1 Kings 4-6", "1 Kings 7-9", "1 Kings 10-12",
                "1 Kings 13-15", "1 Kings 16-18", "1 Kings 19-22", "2 Kings 1-3", "2 Kings 4-6",
                "2 Kings 7-9", "2 Kings 10-12", "2 Kings 13-15", "2 Kings 16-18", "2 Kings 19-21",
                "2 Kings 22-25", "1 Chronicles 1-3", "1 Chronicles 4-6", "1 Chronicles 7-9", "1 Chronicles 10-12",
                "1 Chronicles 13-15", "1 Chronicles 16-18", "1 Chronicles 19-21", "1 Chronicles 22-24", "1 Chronicles 25-27",
                "1 Chronicles 28-29, 2 Chronicles 1", "2 Chronicles 2-4", "2 Chronicles 5-7", "2 Chronicles 8-10", "2 Chronicles 11-13",
                "2 Chronicles 14-16", "2 Chronicles 17-19", "2 Chronicles 20-22", "2 Chronicles 23-25", "2 Chronicles 26-28",
                "2 Chronicles 29-31", "2 Chronicles 32-34", "2 Chronicles 35-36, Ezra 1", "Ezra 2-4", "Ezra 5-7",
                "Ezra 8-10", "Nehemiah 1-3", "Nehemiah 4-6", "Nehemiah 7-9", "Nehemiah 10-13",
                "Esther 1-3", "Esther 4-6", "Esther 7-10", "Job 1-3", "Job 4-6",
                "Job 7-9", "Job 10-12", "Job 13-15", "Job 16-18", "Job 19-21",
                "Job 22-24", "Job 25-27", "Job 28-30", "Job 31-33", "Job 34-36",
                "Job 37-39", "Job 40-42", "Psalms 1-3", "Psalms 4-6", "Psalms 7-9",
                "Psalms 10-12", "Psalms 13-15", "Psalms 16-18", "Psalms 19-21", "Psalms 22-24",
                "Psalms 25-27", "Psalms 28-30", "Psalms 31-33", "Psalms 34-36", "Psalms 37-39",
                "Psalms 40-42", "Psalms 43-45", "Psalms 46-48", "Psalms 49-51", "Psalms 52-54",
                "Psalms 55-57", "Psalms 58-60", "Psalms 61-63", "Psalms 64-66", "Psalms 67-69",
                "Psalms 70-72", "Psalms 73-75", "Psalms 76-78", "Psalms 79-81", "Psalms 82-84",
                "Psalms 85-87", "Psalms 88-90", "Psalms 91-93", "Psalms 94-96", "Psalms 97-99",
                "Psalms 100-102", "Psalms 103-105", "Psalms 106-108", "Psalms 109-111", "Psalms 112-114",
                "Psalms 115-117", "Psalms 118-119:88", "Psalms 119:89-176", "Psalms 120-122", "Psalms 123-125",
                "Psalms 126-128", "Psalms 129-131", "Psalms 132-134", "Psalms 135-137", "Psalms 138-140",
                "Psalms 141-143", "Psalms 144-146", "Psalms 147-150", "Proverbs 1-3", "Proverbs 4-6",
                "Proverbs 7-9", "Proverbs 10-12", "Proverbs 13-15", "Proverbs 16-18", "Proverbs 19-21",
                "Proverbs 22-24", "Proverbs 25-27", "Proverbs 28-31", "Ecclesiastes 1-3", "Ecclesiastes 4-6",
                "Ecclesiastes 7-9", "Ecclesiastes 10-12", "Song of Songs 1-3", "Song of Songs 4-6", "Song of Songs 7-8, Isaiah 1",
                "Isaiah 2-4", "Isaiah 5-7", "Isaiah 8-10", "Isaiah 11-13", "Isaiah 14-16",
                "Isaiah 17-19", "Isaiah 20-22", "Isaiah 23-25", "Isaiah 26-28", "Isaiah 29-31",
                "Isaiah 32-34", "Isaiah 35-37", "Isaiah 38-40", "Isaiah 41-43", "Isaiah 44-46",
                "Isaiah 47-49", "Isaiah 50-52", "Isaiah 53-55", "Isaiah 56-58", "Isaiah 59-61",
                "Isaiah 62-64", "Isaiah 65-66, Jeremiah 1", "Jeremiah 2-4", "Jeremiah 5-7", "Jeremiah 8-10",
                "Jeremiah 11-13", "Jeremiah 14-16", "Jeremiah 17-19", "Jeremiah 20-22", "Jeremiah 23-25",
                "Jeremiah 26-28", "Jeremiah 29-31", "Jeremiah 32-34", "Jeremiah 35-37", "Jeremiah 38-40",
                "Jeremiah 41-43", "Jeremiah 44-46", "Jeremiah 47-49", "Jeremiah 50-52", "Lamentations 1-3",
                "Lamentations 4-5, Ezekiel 1", "Ezekiel 2-4", "Ezekiel 5-7", "Ezekiel 8-10", "Ezekiel 11-13",
                "Ezekiel 14-16", "Ezekiel 17-19", "Ezekiel 20-22", "Ezekiel 23-25", "Ezekiel 26-28",
                "Ezekiel 29-31", "Ezekiel 32-34", "Ezekiel 35-37", "Ezekiel 38-40", "Ezekiel 41-43",
                "Ezekiel 44-46", "Ezekiel 47-48, Daniel 1", "Daniel 2-4", "Daniel 5-7", "Daniel 8-10",
                "Daniel 11-12, Hosea 1", "Hosea 2-4", "Hosea 5-7", "Hosea 8-10", "Hosea 11-14",
                "Joel 1-3", "Amos 1-3", "Amos 4-6", "Amos 7-9", "Obadiah 1, Jonah 1-2",
                "Jonah 3-4, Micah 1-2", "Micah 3-5", "Micah 6-7, Nahum 1", "Nahum 2-3, Habakkuk 1", "Habakkuk 2-3, Zephaniah 1",
                "Zephaniah 2-3, Haggai 1", "Haggai 2, Zechariah 1-2", "Zechariah 3-5", "Zechariah 6-8", "Zechariah 9-11",
                "Zechariah 12-14", "Malachi 1-3", "Malachi 4, Matthew 1-2", "Matthew 3-5", "Matthew 6-8",
                "Matthew 9-11", "Matthew 12-14", "Matthew 15-17", "Matthew 18-20", "Matthew 21-23",
                "Matthew 24-26", "Matthew 27-28, Mark 1", "Mark 2-4", "Mark 5-7", "Mark 8-10",
                "Mark 11-13", "Mark 14-16", "Luke 1-3", "Luke 4-6", "Luke 7-9",
                "Luke 10-12", "Luke 13-15", "Luke 16-18", "Luke 19-21", "Luke 22-24",
                "John 1-3", "John 4-6", "John 7-9", "John 10-12", "John 13-15",
                "John 16-18", "John 19-21", "Acts 1-3", "Acts 4-6", "Acts 7-9",
                "Acts 10-12", "Acts 13-15", "Acts 16-18", "Acts 19-21", "Acts 22-24",
                "Acts 25-28", "Romans 1-3", "Romans 4-6", "Romans 7-9", "Romans 10-12",
                "Romans 13-16", "1 Corinthians 1-3", "1 Corinthians 4-6", "1 Corinthians 7-9", "1 Corinthians 10-12",
                "1 Corinthians 13-16", "2 Corinthians 1-3", "2 Corinthians 4-6", "2 Corinthians 7-9", "2 Corinthians 10-13",
                "Galatians 1-3", "Galatians 4-6", "Ephesians 1-3", "Ephesians 4-6", "Philippians 1-3",
                "Philippians 4, Colossians 1-2", "Colossians 3-4, 1 Thessalonians 1", "1 Thessalonians 2-4", "1 Thessalonians 5, 2 Thessalonians 1-2", "2 Thessalonians 3, 1 Timothy 1-2",
                "1 Timothy 3-5", "1 Timothy 6, 2 Timothy 1-2", "2 Timothy 3-4, Titus 1", "Titus 2-3, Philemon 1", "Hebrews 1-3",
                "Hebrews 4-6", "Hebrews 7-9", "Hebrews 10-12", "Hebrews 13, James 1-2", "James 3-5",
                "1 Peter 1-3", "1 Peter 4-5, 2 Peter 1", "2 Peter 2-3, 1 John 1", "1 John 2-4", "1 John 5, 2 John 1, 3 John 1",
                "Jude 1, Revelation 1-2", "Revelation 3-5", "Revelation 6-8", "Revelation 9-11", "Revelation 12-14",
                "Revelation 15-17", "Revelation 18-20", "Revelation 21-22"
            };

            // Return the reading for the specific day (dayOfYear - 1 because array is 0-indexed)
            if (dayOfYear > 0 && dayOfYear <= readings.Length)
            {
                return readings[dayOfYear - 1];
            }

            // Fallback for leap year day 366
            return "Revelation 21-22";
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

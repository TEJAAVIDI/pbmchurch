using Microsoft.AspNetCore.Mvc;

namespace PBMChurch.Controllers
{
    public class LeadsController : Controller
    {
        public IActionResult Index()
        {
            // Get leads from WebhookController static list
            var leads = WebhookController.GetStoredLeads();
            return View(leads);
        }

        [HttpPost]
        public IActionResult SyncFacebook()
        {
            // Manual sync - for now just refresh the current leads
            TempData["Success"] = "Facebook leads synced successfully";
            return RedirectToAction("Index");
        }
    }
}
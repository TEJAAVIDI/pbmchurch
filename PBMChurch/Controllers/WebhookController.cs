using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace PBMChurch.Controllers
{
    [Route("api/meta")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private const string VERIFY_TOKEN = "MY_SECRET_TOKEN_123";
        private static readonly List<object> _receivedLeads = new List<object>();

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
        }

        // GET: api/meta/test - Simple test endpoint
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { status = "Webhook is working", timestamp = DateTime.Now });
        }

        // GET: api/meta/webhook - Facebook Webhook Verification
        [HttpGet("webhook")]
        public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string? mode,
                                          [FromQuery(Name = "hub.verify_token")] string? token,
                                          [FromQuery(Name = "hub.challenge")] string? challenge)
        {
            _logger.LogInformation($"Webhook verification - Mode: {mode}, Token: {token}, Challenge: {challenge}");

            if (mode == "subscribe")
            {
                _logger.LogInformation("Webhook verified successfully - accepting any token for now");
                return Content(challenge ?? "VERIFIED", "text/plain");
            }

            _logger.LogWarning($"Invalid verification request - Mode: {mode}");
            return Unauthorized(new { error = "Invalid verify token" });
        }

        // POST: api/meta/webhook - Receive Lead Data
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var rawBody = await reader.ReadToEndAsync();
                
                _logger.LogInformation($"Webhook received: {rawBody}");

                var webhookData = JsonSerializer.Deserialize<MetaWebhookPayload>(rawBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var leads = new List<object>();

                if (webhookData?.Entry != null)
                {
                    foreach (var entry in webhookData.Entry)
                    {
                        if (entry.Changes != null)
                        {
                            foreach (var change in entry.Changes)
                            {
                                if (change.Field == "leadgen" && change.Value?.FieldData != null)
                                {
                                    var lead = ExtractLeadData(change.Value.FieldData, change.Value);
                                    if (lead != null)
                                    {
                                        _receivedLeads.Add(lead);
                                        leads.Add(lead);
                                    }
                                }
                            }
                        }
                    }
                }

                return Ok(new { status = "success", leadsReceived = leads.Count, leads });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing webhook: {ex.Message}");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // GET: api/meta/leads - Get all Facebook leads
        [HttpGet("leads")]
        public IActionResult GetLeads()
        {
            return Ok(new { success = true, count = _receivedLeads.Count, leads = _receivedLeads });
        }

        // Static method to access leads from other controllers
        public static List<object> GetStoredLeads()
        {
            return _receivedLeads.ToList();
        }

        // GET: api/meta/fetch-leads - Fetch leads from Facebook
        [HttpGet("fetch-leads")]
        public async Task<IActionResult> FetchFacebookLeads()
        {
            return Ok(new { success = false, error = "HttpClient not available in this deployment" });
        }

        private object ExtractLeadData(List<FieldData> fieldData, WebhookValue webhookValue)
        {
            var firstName = GetFieldValue(fieldData, "first_name") ?? "";
            var lastName = GetFieldValue(fieldData, "last_name") ?? "";
            var phone = GetFieldValue(fieldData, "phone_number") ?? "";
            var email = GetFieldValue(fieldData, "email") ?? "";

            return new
            {
                Name = $"{firstName} {lastName}".Trim(),
                Phone = phone,
                Email = email,
                LeadId = webhookValue?.LeadgenId,
                CreatedTime = webhookValue?.CreatedTime,
                Source = "Facebook Webhook"
            };
        }

        private string? GetFieldValue(List<FieldData> fieldData, string fieldName)
        {
            return fieldData?.FirstOrDefault(f => f.Name?.Equals(fieldName, StringComparison.OrdinalIgnoreCase) == true)?.Values?.FirstOrDefault();
        }
    }
    }

    // Models
    public class MetaWebhookPayload
    {
        public List<WebhookEntry>? Entry { get; set; }
    }

    public class WebhookEntry
    {
        public List<WebhookChange>? Changes { get; set; }
    }

    public class WebhookChange
    {
        public string? Field { get; set; }
        public WebhookValue? Value { get; set; }
    }

    public class WebhookValue
    {
        [System.Text.Json.Serialization.JsonPropertyName("leadgen_id")]
        public string? LeadgenId { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("created_time")]
        public string? CreatedTime { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("field_data")]
        public List<FieldData>? FieldData { get; set; }
    }

    public class FieldData
    {
        public string? Name { get; set; }
        public List<string>? Values { get; set; }
    }

    public class FacebookAdResponse
    {
        public FacebookCreative? Creative { get; set; }
    }

    public class FacebookCreative
    {
        [System.Text.Json.Serialization.JsonPropertyName("object_story_spec")]
        public FacebookObjectStorySpec? ObjectStorySpec { get; set; }
    }

    public class FacebookObjectStorySpec
    {
        [System.Text.Json.Serialization.JsonPropertyName("link_data")]
        public FacebookLinkData? LinkData { get; set; }
    }

    public class FacebookLinkData
    {
        [System.Text.Json.Serialization.JsonPropertyName("call_to_action")]
        public FacebookCallToAction? CallToAction { get; set; }
    }

    public class FacebookCallToAction
    {
        public FacebookCallToActionValue? Value { get; set; }
    }

    public class FacebookCallToActionValue
    {
        [System.Text.Json.Serialization.JsonPropertyName("lead_gen_form_id")]
        public string? LeadGenFormId { get; set; }
    }

    public class FacebookLeadsResponse
    {
        public List<FacebookLeadSummary>? Data { get; set; }
    }

    public class FacebookLeadSummary
    {
        public string? Id { get; set; }
    }

    public class FacebookLeadData
    {
        public string? Id { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("created_time")]
        public string? CreatedTime { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("field_data")]
        public List<FacebookFieldData>? FieldData { get; set; }
    }

    public class FacebookFieldData
    {
        public string? Name { get; set; }
        public List<string>? Values { get; set; }
    }

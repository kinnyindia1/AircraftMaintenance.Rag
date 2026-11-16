using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartRAG.Interfaces.Document;
using SmartRAG.Models.RequestResponse;
using System.Security.Claims;

namespace AircraftMaintenance.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatController : ControllerBase
    {
        private readonly IDocumentSearchService _searchService;
        private readonly UserManager<IdentityUser> _userManager;

        public ChatController(IDocumentSearchService searchService, UserManager<IdentityUser> userManager)
        {
            _searchService = searchService;
            _userManager = userManager;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            // Step 1: Get the user's ID from the JWT token
            var userId = User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID claim is missing. Please log in again.");
            }

            // Step 2: Find the user in the database
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("User not found in database.");
            }

            // Step 3: Get the user's role from the JWT token
            var userRole = User.FindFirstValue("role");
            if (string.IsNullOrEmpty(userRole))
            {
                return StatusCode(403, "User has no role assigned.");
            }

            // Step 4: Log who is asking (shows in Visual Studio Output window)
            Console.WriteLine($"User {user.Email} (Role: {userRole}) is asking: {request.Question}");

            // Step 5: Build the search request with the user's question
            var queryRequest = new QueryIntelligenceRequest
            {
                Query = request.Question,
                MaxResults = 5
            };

            // Step 6: Ask SmartRAG to find the answer
            var response = await _searchService.QueryIntelligenceAsync(queryRequest);

            // Step 7: Send back the answer and the user's role
            return Ok(new { answer = response.Answer, role = userRole });
        }
    }

    // Simple class for the request body (what the user sends)
    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}

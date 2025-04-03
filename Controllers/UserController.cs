using FormPlay.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormPlay.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> SwitchUser(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();
                
            return RedirectToAction("Index", "Home", new { userId });
        }
    }
}

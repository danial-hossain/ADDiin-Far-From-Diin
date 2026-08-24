using AdDiin.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdDiin.Controllers
{
    public class EventsController : Controller
    {
        private readonly IIslamicEventService _eventService;

        public EventsController(IIslamicEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var evt = await _eventService.GetByIdAsync(id);
            if (evt == null) return NotFound();

            return View(evt);
        }
    }
}

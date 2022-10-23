using front_to_back.Areas.Admin.ViewModels;
using front_to_back.DAL;
using front_to_back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace front_to_back.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContactController(AppDbContext appDbContext, IWebHostEnvironment webHostEnvironment)
        {
            _appDbContext = appDbContext;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var model = new ContactIndexViewModel
            {
                ContactIntroComponents = await _appDbContext.ContactIntroComponent.ToListAsync()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(ContactIntroComponent contactIntroComponent)
        {
            if (!ModelState.IsValid) return View(contactIntroComponent);
            if (!contactIntroComponent.Photo.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Photo", "File is not in image format ");
                return View(contactIntroComponent);
            }
            if (contactIntroComponent.Photo.Length / 1024 > 60)
            {
                ModelState.AddModelError("Photo", "Image size should be less than 70 kb");
                return View(contactIntroComponent);
            }

            var fileName = $"{Guid.NewGuid()}_{contactIntroComponent.Photo.FileName}";
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "assets/img", fileName);

            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                await contactIntroComponent.Photo.CopyToAsync(fileStream);
            }

            contactIntroComponent.FilePath = fileName;
            await _appDbContext.ContactIntroComponent.AddAsync(contactIntroComponent);


            await _appDbContext.SaveChangesAsync();


            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var contactBanner = await _appDbContext.ContactIntroComponent.FindAsync(id);
            if (contactBanner == null) return NotFound();

            return View(contactBanner);

        }
    }
}
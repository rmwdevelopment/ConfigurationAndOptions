using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MicroBlog.Models;
using MicroBlog.Services;

namespace MicroBlog.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly IBlogRepository _repo;

        public DetailsModel(IBlogRepository repo) => _repo = repo;

        public Post? Post { get; private set; }

        public IActionResult OnGet(int id)
        {
            Post = _repo.GetById(id);
            if (Post is null) return NotFound();
            return Page();
        }
    }
}

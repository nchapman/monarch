using Monarch.ActionPack;
using Monarch.Blog.Models;

namespace Monarch.Blog.Controllers
{
    public class PostsController : Controller
    {
        public void List()
        {
            Add(Post.FindAll());
        }

        public void View(string id)
        {
            Add(Post.Find(id));
        }
    }
}

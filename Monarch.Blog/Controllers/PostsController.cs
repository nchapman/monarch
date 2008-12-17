using Monarch.ActionPack;
using Monarch.Blog.Models;

namespace Monarch.Blog.Controllers
{
    public class PostsController : Controller
    {
        public void List()
        {
            Data["posts"] = Post.FindAll();
        }

        public void View(string id)
        {
            Data["post"] = Post.Find(id);
        }

        public void Hello(string name)
        {
            Add("name", name);
        }
    }
}

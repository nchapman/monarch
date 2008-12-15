using Monarch.ActionPack.Helpers;

namespace Monarch.Blog.Helpers
{
    public class PostsHelper : Helper
    {
        public string Hello(string name)
        {
            return string.Format("Hello {0}!", name);
        }
    }
}

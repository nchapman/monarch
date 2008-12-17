using Monarch.ActiveRecord;

namespace Monarch.Blog.Models
{
    public class Post : Model<Post>
    {
        public string Title
        {
            get
            {
                return this["Title"] as string;
            }
            set
            {
                this["Title"] = value;
            }
        }
    }
}

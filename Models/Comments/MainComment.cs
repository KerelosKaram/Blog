namespace Blog.Models.Comments
{
    public class MainComment : Comment
    {
        public List<SubComment>? SubComments { get; set; }
    }
}

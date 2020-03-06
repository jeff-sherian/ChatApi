using System.Collections.Generic;

namespace ChatApi.Models
{
    public class Group
    {
        public string Name { get; set; }
        public List<string> Connections { get; set; }
    }
}

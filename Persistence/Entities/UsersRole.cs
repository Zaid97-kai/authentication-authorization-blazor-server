using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Persistence.Entities
{
    public class UsersRole
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public virtual User? User { get; set; }

        public int RoleId { get; set; }

        [JsonIgnore]
        public virtual Role? Role { get; set; }
    }
}

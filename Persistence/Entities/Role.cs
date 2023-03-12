using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Persistence.Entities
{
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public virtual List<UsersRole>? UsersRoles { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Communicator.Entities
{
    public class UserEntity
    {
        public int ID { get; set; }
        public string login { get; set; }
        public string email { get; set; }
        public string passwordEncrypted { get; set; }
        public string bankAccount { get; set; }
    }
}

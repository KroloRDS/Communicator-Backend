using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Communicator.Entities
{
    public class MessageEntity
    {
        public int ID { get; set; }
        public int senderId  { get; set; }
        public int receiverId { get; set; }
        public string senderEncryptedContent { get; set; }
        public string receiverEncryptedContent { get; set; }
        public string sendDateTime { get; set; }
        public bool seenByReceiver { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFD_Challenge_1.Models
{
    public class Descriptor
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CredentialList
    {
        public string aaGuid { get; set; }
        public string country { get; set; }
        public DateTime createdAt { get; set; }
        public string credType { get; set; }
        public Descriptor descriptor { get; set; }
        public string device { get; set; }
        public DateTime lastUsedAt { get; set; }
        public string origin { get; set; }
        public string publicKey { get; set; }
        public string rpid { get; set; }
        public int signatureCounter { get; set; }
        public string userHandle { get; set; }
        public string userId { get; set; }
        public object nickname { get; set; }
    }
}

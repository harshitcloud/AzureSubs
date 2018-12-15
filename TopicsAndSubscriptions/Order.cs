using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TopicsAndSubscriptions
{
    [DataContract]
    class Order
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime OrderDate { get; set; }

        [DataMember]
        public int Items { get; set; }

        [DataMember]
        public double Value { get; set; }

        [DataMember]
        public string Priority { get; set; }

        [DataMember]
        public string Region { get; set; }

        [DataMember]
        public bool HasLoyltyCard { get; set; }
    }

}

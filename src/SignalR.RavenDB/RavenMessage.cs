using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.RavenDB
{
    public class RavenMessage
    {
        public string Id { get; set; }

        public int StreamIndex { get; set; }

        public byte[] Data { get; set; }

        internal static RavenMessage FromMessages(int streamIndex, IList<Message> messages)
        {
            var scaleoutMessage = new ScaleoutMessage(messages);
            return new RavenMessage
            {
                StreamIndex = streamIndex,
                Data = scaleoutMessage.ToBytes()
            };
        }

        internal ScaleoutMessage ToScaleoutMessage()
        {
            return ScaleoutMessage.FromBytes(this.Data);
        }
    }    
}

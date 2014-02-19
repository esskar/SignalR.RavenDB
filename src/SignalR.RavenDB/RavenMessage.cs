using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.RavenDB
{
    public class RavenMessage
    {
        public string Id { get; set; }

        public int StreamIndex { get; set; }

        public byte[] Data { get; set; }

        public static RavenMessage FromMessages(int streamIndex, IList<Message> messages)
        {
            var scaleoutMessage = new ScaleoutMessage(messages);
            return new RavenMessage
            {
                StreamIndex = streamIndex,
                Data = scaleoutMessage.ToBytes()
            };
        }

        public ScaleoutMessage ToScaleoutMessage()
        {
            return ScaleoutMessage.FromBytes(this.Data);
        }

        public ulong ToLongId()
        {
            return Convert.ToUInt64(this.Id.Substring(this.Id.LastIndexOf('/') + 1));
        }
    }    
}
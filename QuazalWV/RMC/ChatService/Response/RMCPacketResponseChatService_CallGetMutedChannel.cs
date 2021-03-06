﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazalWV
{
    public class RMCPacketResponseChatService_GetMutedChannel : RMCPResponse
    {
        public List<GR5_ChatChannelMute> list = new List<GR5_ChatChannelMute>();

        public RMCPacketResponseChatService_GetMutedChannel()
        {
            list.Add(new GR5_ChatChannelMute());
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)list.Count);
            foreach (GR5_ChatChannelMute c in list)
                c.toBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RMCPacketResponseChatService_GetMutedChannel]";
        }

        public override string PayloadToString()
        {
            return "";
        }
    }
}

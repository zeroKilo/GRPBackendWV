﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRPBackendWV
{
    public class RMCPacket
    {
        public enum PROTOCOL
        {
            Authentication = 0xA,
            Secure = 0xB,
            Telemetry = 0x24,
            AMMGameClient = 0x65,
            PlayerProfileService = 0x67,
            InventoryService = 0x69,
            Unknown6A = 0x6A,
            Unknown6B = 0x6B,
            ChatService = 0x6E,
            Unknown6F = 0x6F,
            PartyService = 0x70,
            ProgressionService = 0x74,
            RewardService = 0x76,
            Unknown7A = 0x7A,
            Loadout = 0x7B,
            UnlockService = 0x7D,
            OpsProtocolService = 0x80,
            ServerInfo = 0x82,
            Unknown83 = 0x83,
            Unknown86 = 0x86,
            ProfanityFilterService = 0x87,
            AbilityService = 0x89
        }

        public PROTOCOL proto;
        public bool isRequest;
        public uint callID;
        public uint methodID;
        public RMCPacketHeader header;

        public RMCPacket()
        {
        }

        public RMCPacket(QPacket p)
        {
            MemoryStream m = new MemoryStream(p.payload);
            Helper.ReadU32(m);
            ushort b = Helper.ReadU8(m);
            isRequest = (b >> 7) == 1;
            try
            {
                if ((b & 0x7F) != 0x7F)
                    proto = (PROTOCOL)(b & 0x7F);
                else
                {
                    b = Helper.ReadU16(m);
                    proto = (PROTOCOL)(b);
                }
            }
            catch
            {
                callID = Helper.ReadU32(m);
                methodID = Helper.ReadU32(m); 
                WriteLog("Error: Unknown RMC packet protocol 0x" + b.ToString("X2"));
                return;
            }
            callID = Helper.ReadU32(m);
            methodID = Helper.ReadU32(m);          
            switch (proto)
            {
                case PROTOCOL.Authentication:
                    HandleAuthenticationMethods(m);
                    break;
                case PROTOCOL.Secure:
                    HandleSecureMethods(m);
                    break;
                case PROTOCOL.Telemetry:
                    HandleTelemetryMethods(m);
                    break;
                case PROTOCOL.AMMGameClient:
                case PROTOCOL.PlayerProfileService:
                case PROTOCOL.InventoryService:
                case PROTOCOL.Unknown6A:
                case PROTOCOL.Unknown6B:
                case PROTOCOL.ChatService:
                case PROTOCOL.Unknown6F:
                case PROTOCOL.PartyService:
                case PROTOCOL.ProgressionService:
                case PROTOCOL.RewardService:
                case PROTOCOL.Unknown7A:
                case PROTOCOL.Loadout:
                case PROTOCOL.UnlockService:
                case PROTOCOL.OpsProtocolService:
                case PROTOCOL.ServerInfo:
                case PROTOCOL.Unknown83:
                case PROTOCOL.Unknown86:
                case PROTOCOL.ProfanityFilterService:
                case PROTOCOL.AbilityService:
                    break;
                default:
                    WriteLog("Error: No reader implemented for packet protocol " + proto);
                    break;
            }
        }

        private void HandleAuthenticationMethods(Stream s)
        {
            switch (methodID)
            {
                case 2:
                    header = new RMCPacketRequestLoginCustomData(s);
                    break;
                case 3:
                    header = new RMCPacketRequestRequestTicket(s);
                    break;
                default:
                    WriteLog("Error: Unknown RMC Packet Authentication Method 0x" + methodID.ToString("X"));
                    break;
            }
        }

        private void HandleSecureMethods(Stream s)
        {
            switch (methodID)
            {
                case 4:
                    header = new RMCPacketRequestRegisterEx(s);
                    break;
                default:
                    WriteLog("Error: Unknown RMC Packet Secure Method 0x" + methodID.ToString("X"));
                    break;
            }
        }

        private void HandleTelemetryMethods(Stream s)
        {
            switch (methodID)
            {
                case 1:
                    header = new RMCPacketRequestTelemetry_Method1(s);
                    break;
                default:
                    WriteLog("Error: Unknown RMC Packet Method 0x" + methodID.ToString("X"));
                    break;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[RMC Packet : Proto = " + proto + " CallID=" + callID + " MethodID=" + methodID+"]");
            if(header != null)
                sb.Append(header);
            return sb.ToString();
        }

        public byte[] ToBuffer()
        {
            MemoryStream result = new MemoryStream();
            byte[] buff = header.ToBuffer();
            Helper.WriteU32(result, (uint)(buff.Length + 9));
            byte b = (byte)proto;
            if (isRequest)
                b |= 0x80;
            Helper.WriteU8(result, b);
            Helper.WriteU32(result, callID);
            Helper.WriteU32(result, methodID);
            result.Write(buff, 0, buff.Length);
            return result.ToArray();
        }

        private static void WriteLog(string s)
        {
            Log.WriteLine("[RMC Packet] " + s);
        }
    }
}

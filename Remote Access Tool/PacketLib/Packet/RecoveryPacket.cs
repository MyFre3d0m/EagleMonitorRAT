﻿using System;

namespace PacketLib.Packet
{
    [Serializable]
    public class RecoveryPacket : IPacket
    {
        public RecoveryPacket() : base()
        {
            this.packetType = PacketType.RECOVERY_ALL;
        }

        public string HWID { get; set; }
        public string baseIp { get; set; }
        public byte[] plugin { get; set; }
        public PacketType packetType { get; }
        public string status { get; set; }
        public string datePacketStatus { get; set; }
    }
}

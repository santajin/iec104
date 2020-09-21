﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Shouyuan.IEC104
{
    public enum ElementType : byte
    {
        /// <summary>
        /// 无内容。
        /// </summary>
        Empty = 0,

        /// <summary>
        /// 带品质描述词的单点信息。
        /// </summary>
        SIQ = 1,

        /// <summary>
        /// 带品质描述词的双点信息。
        /// </summary>
        DIQ = 2,

        /// <summary>
        /// 带变位检索的单点遥信变位信息。
        /// </summary>
        SCD = 3,

        /// <summary>
        /// 品质描述词。
        /// </summary>        
        QDS = 4,

        /// <summary>
        /// 规一化遥测值。
        /// </summary>
        NVA = 5,

        /// <summary>
        /// 标度化值。
        /// </summary>
        SVA = 6,

        /// <summary>
        /// 短浮点数。
        /// </summary>
        R = 7,

        /// <summary>
        /// 二进制计数器读数。
        /// </summary>
        BCR = 8,

        /// <summary>
        /// 初始化原因。
        /// </summary>        
        COI = 9,

        /// <summary>
        /// 单命令。
        /// </summary>
        SCO = 10,

        /// <summary>
        /// 双命令。
        /// </summary>
        DCO = 11,

        /// <summary>
        /// 召唤限定词。
        /// </summary>
        QOI = 12,

        /// <summary>
        /// 命令限定词。
        /// </summary>
        QOC = 13,

        /// <summary>
        /// 设定命令限定词。
        /// </summary>
        QOS = 14,

        /// <summary>
        /// 复位进程命令限定词。
        /// </summary>
        QRP = 15

    }

    /// <summary>
    /// 信息体对象
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 信息体地址。可选1、2、3个字节。3字节时最高字节一般置0。
        /// </summary>
        public readonly byte[] Addr;

        /// <summary>
        /// 信息体元素。
        /// </summary>
        public readonly byte[] Element;

        /// <summary>
        /// 附加的限定描述词。
        /// </summary>
        public readonly byte[] Extra;

        /// <summary>
        /// 信息体时标。可选2、3、7个字节。
        /// </summary>
        public readonly byte[] TimeStamp;

        /// <summary>
        /// 信息体总长度。
        /// </summary>
        public byte Length
        {
            get
            {
                byte c = 0;
                if (Addr != null) c += (byte)Addr.Length;
                if (Element != null) c += (byte)Element.Length;
                if (Extra != null) c += (byte)Extra.Length;
                if (TimeStamp != null) c += (byte)TimeStamp.Length;
                return c;
            }
        }

        private static readonly byte[] elementTypeLengths =
        {
            0,//Empty
            1,//SIQ
            1,//DIQ
            5,//SCD
            1,//QDS
            2,//NVA
            2,//SVA
            4,//R
            5,//BCR
            1,//COI
            1,//SCO
            1,//DCO
            1,//QOI
            1,//QOC
            1,//QOS
            1//QRP            
        };
        public static byte GetElementTypeLength(ElementType t)
        {
            return elementTypeLengths[(byte)t];
        }

        public ElementType Type { get; }

        /// <summary>
        /// 实例化信息体。
        /// </summary>
        /// <param name="t">信息元素类型。</param>
        /// <param name="addrl">地址长度，可选1、2、3个字节。</param>
        /// <param name="extrl">附加的限定描述词的数量。</param>
        /// <param name="tml">时标长度，可选2、3、7个字节。</param>
        public Message(ElementType t, byte addrl = 3, byte extrl = 0, byte tml = 0)
        {
            Type = t;
            if (t.Length() > 0)
                Element = new byte[t.Length()];
            if (addrl > 0)
                Addr = new byte[addrl];
            if (extrl > 0)
                Extra = new byte[extrl];
            if (tml > 0)
                TimeStamp = new byte[tml];
        }

        uint actualAddress = 0;
        public uint Address
        {
            get
            {
                if (Addr != null)
                {
                    uint v = Addr[0];
                    for (var i = 1; i < Addr.Length; i++)
                        v |= (uint)Addr[i] << (i * 8);
                    return v;
                }
                else
                    return actualAddress;
            }
            set
            {
                if (Addr != null)
                {
                    for (var i = 0; i < Addr.Length; i++)
                        Addr[i] = (byte)(value >> (i * 8));
                }
                else
                    actualAddress = value;
            }
        }

        public ushort Milisecond
        {
            get
            {
                if (TimeStamp == null || TimeStamp.Length < 2)
                    return 0;
                ushort ms = (ushort)(TimeStamp[0] | TimeStamp[1] << 8);
                switch (TimeStamp.Length)
                {
                    case 2:
                        return ms;
                    case 3:
                    case 7:
                        return (ushort)(ms % 1000);
                }
                return 0;
            }
            set
            {
                if (TimeStamp == null || TimeStamp.Length < 2)
                    return;
                if (TimeStamp.Length == 3 || TimeStamp.Length == 7)
                {
                    ushort ms = (ushort)(TimeStamp[0] | TimeStamp[1] << 8);
                    ms -= (ushort)(ms % 1000);
                    ms += (ushort)(value % 1000);
                    value = ms;
                }
                TimeStamp[0] = (byte)value;
                TimeStamp[1] = (byte)(value >> 8);
            }
        }

        public ushort Second
        {
            get
            {
                if (TimeStamp != null && (TimeStamp.Length == 3 || TimeStamp.Length == 7))
                {
                    ushort ms = (ushort)(TimeStamp[0] | TimeStamp[1] << 8);
                    return (ushort)(ms / 1000);
                }
                else
                    return 0;
            }
            set
            {
                if (TimeStamp != null && (TimeStamp.Length == 3 || TimeStamp.Length == 7))
                {
                    ushort ms = (ushort)(TimeStamp[0] | TimeStamp[1] << 8);
                    ms = (ushort)(ms % 1000);
                    value = (ushort)(value * 1000 + ms);
                    TimeStamp[0] = (byte)value;
                    TimeStamp[1] = (byte)(value >> 8);
                }
            }
        }

        public byte Minute
        {
            get
            {
                if (TimeStamp != null && (TimeStamp.Length == 3 || TimeStamp.Length == 7))
                    return (byte)(TimeStamp[2] & 0x3f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp != null && (TimeStamp.Length == 3 || TimeStamp.Length == 7))
                    TimeStamp[2] = (byte)(TimeStamp[2] & 0x80 | value & 0x3f);
            }
        }

        /// <summary>
        /// 对3、7字节的时标，指示时标是否失效，有效时为0，返回false,无效是为1，返回true。
        /// </summary>
        public bool Time_IV
        {
            get
            {
                if (TimeStamp != null && (TimeStamp.Length == 3 || TimeStamp.Length == 7))
                    return TimeStamp[2].Bit(7);
                else
                    return false;
            }
            set
            {
                if (TimeStamp != null && (TimeStamp.Length == 3 || TimeStamp.Length == 7))
                    TimeStamp[2] = value ? TimeStamp[2].SetBit(7) : TimeStamp[2].ClearBit(7);
            }
        }
        public byte Hour
        {
            get
            {

                if (TimeStamp != null && TimeStamp.Length == 7)
                    return (byte)(TimeStamp[3] & 0x1f);
                else
                    return 0;
            }
            set
            {

                if (TimeStamp != null && TimeStamp.Length == 7)
                {
                    TimeStamp[3] = (byte)(value & 0x1f);
                }
            }
        }


        public byte Day
        {
            get
            {

                if (TimeStamp != null && TimeStamp.Length == 7)
                    return (byte)(TimeStamp[4] & 0x1f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp != null && TimeStamp.Length == 7)
                {
                    TimeStamp[4] = (byte)(TimeStamp[4] & 0xe0 | value & 0x1f);
                }
            }
        }
        public byte DayInWeek
        {
            get
            {
                if (TimeStamp != null && TimeStamp.Length == 7)
                    return (byte)((TimeStamp[4] & 0xe0) >> 5);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp != null && TimeStamp.Length == 7)
                {
                    TimeStamp[4] = (byte)(TimeStamp[4] & 0x1f | (value << 5));
                }
            }
        }

        public byte Month
        {
            get
            {
                if (TimeStamp != null && TimeStamp.Length == 7)
                    return (byte)(TimeStamp[5] & 0x0f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp != null && TimeStamp.Length == 7)
                {
                    TimeStamp[5] = (byte)(value & 0x0f);
                }
            }
        }

        public byte Year
        {
            get
            {
                if (TimeStamp != null && TimeStamp.Length == 7)
                    return (byte)(TimeStamp[6] & 0x7f);
                else
                    return 0;
            }
            set
            {
                if (TimeStamp != null && TimeStamp.Length == 7)
                {
                    TimeStamp[6] = (byte)(value & 0x7f);
                }
            }
        }

        #region 品质描述词QDS
        public byte QDSi = 0;

        /// <summary>
        /// 品质描述词溢出标志，true为溢出。
        /// </summary>
        public bool QDS_OV
        {
            get => Extra[QDSi].Bit(0);
            set => Extra[QDSi] = value ? Extra[QDSi].SetBit(0) : Extra[QDSi].ClearBit(0);
        }

        /// <summary>
        /// 品质描述词封锁标志，true为被封锁。
        /// </summary>
        public bool QDS_BL
        {
            get => Extra[QDSi].Bit(4);
            set => Extra[QDSi] = value ? Extra[QDSi].SetBit(4) : Extra[QDSi].ClearBit(4);
        }

        /// <summary>
        /// 品质描述词取代标志，ture为被取代。
        /// </summary>
        public bool QDS_SB
        {
            get => Extra[QDSi].Bit(5);
            set => Extra[QDSi] = value ? Extra[QDSi].SetBit(5) : Extra[QDSi].ClearBit(5);

        }

        /// <summary>
        /// 品质描述词刷新失败标志，true为刷新失败。
        /// </summary>
        public bool QDS_NT
        {
            get => Extra[QDSi].Bit(6);
            set => Extra[QDSi] = value ? Extra[QDSi].SetBit(6) : Extra[QDSi].ClearBit(6);

        }

        /// <summary>
        /// 品质描述词失效标志，true为无效。
        /// </summary>
        public bool QDS_IV
        {
            get => Extra[QDSi].Bit(7);
            set => Extra[QDSi] = value ? Extra[QDSi].SetBit(7) : Extra[QDSi].ClearBit(7);

        }
        #endregion

        public byte QOIi = 0;
        public const byte QOI_WholeStation = 20;
        public byte QOI
        {
            get => Extra[QOIi];
            set => Extra[QOIi] = value;
        }

        # region 设定命令限定词QOS
        public byte QOSi = 0;
        public byte QOS
        {
            get => Extra[QOSi];
            set => Extra[QOSi] = value;
        }
        public byte QOS_QL
        {
            get => (byte)(QOS & 0x7f);
            set => QOS = (byte)(value | (QOS & 0x80));
        }
        public bool QOS_SE
        {
            get => QOS.Bit(7);
            set => QOS = value ? QOS.SetBit(7) : QOS.ClearBit(7);
        }
        #endregion

        # region 单命令SCO
        public bool SCO_SCS
        {
            get => SCO.Bit(0);
            set => SCO = value ? SCO.SetBit(0) : SCO.ClearBit(0);
        }
        public byte SCO_QU
        {
            get => (byte)((SCO & 0x7c) >> 2);
            set => SCO = (byte)(((value << 2) & 0x7c) | (SCO & 0x83));
        }
        public bool SCO_SE
        {
            get => SCO.Bit(7);
            set => SCO = value ? SCO.SetBit(7) : SCO.ClearBit(7);
        }
        #endregion


        private bool CheckType(ElementType t, bool err = true)
        {
            if (t == Type)
                return true;
            if (err)
                throw new Exception("当前操作与信息体类型不兼容，本操作作用于" + t.ToString());
            return false;
        }

        /// <summary>
        /// 单点遥信。
        /// </summary>
        public byte SIQ
        {
            get
            {
                CheckType(ElementType.SIQ);
                return (byte)Element[0];
            }
            set
            {
                CheckType(ElementType.SIQ);
                Element[0] = value;
            }
        }

        /// <summary>
        /// 单点命令。
        /// </summary>
        public byte SCO
        {
            get
            {
                CheckType(ElementType.SCO);
                return (byte)Element[0];
            }
            set
            {
                CheckType(ElementType.SCO);
                Element[0] = value;
            }
        }

        /// <summary>
        /// NVA满码值。
        /// </summary>
        public float NVA_M = 1;
        /// <summary>
        /// 规一化表示的数据的实际值。
        /// </summary>
        public float NVA
        {
            get
            {
                CheckType(ElementType.NVA);
                return (float)BitConverter.ToInt16(Element, 0) / (1 << 15) * NVA_M;
            }
            set
            {
                CheckType(ElementType.NVA);
                BitConverter.GetBytes((Int16)(value / NVA_M * (1 << 15))).CopyTo(Element, 0);
            }
        }

        /// <summary>
        /// 标度化值。
        /// </summary>
        public Int16 SVA
        {
            get
            {
                CheckType(ElementType.SVA);
                return BitConverter.ToInt16(Element, 0);
            }
            set
            {
                CheckType(ElementType.SVA);
                BitConverter.GetBytes(value).CopyTo(Element, 0);
            }
        }

        /// <summary>
        /// 短浮点数。
        /// </summary>
        public float R
        {
            get
            {
                CheckType(ElementType.R);
                return BitConverter.ToSingle(Element, 0);
            }
            set
            {
                CheckType(ElementType.R);
                BitConverter.GetBytes(value).CopyTo(Element, 0);
            }
        }

        /// <summary>
        /// 二进制计数器计数。
        /// </summary>
        public Int32 BCR
        {
            get
            {
                CheckType(ElementType.BCR);
                return BitConverter.ToInt32(Element, 0);
            }
            set
            {
                CheckType(ElementType.BCR);
                BitConverter.GetBytes(value).CopyTo(Element, 0);
            }
        }

        /// <summary>
        /// BCR顺序号。
        /// </summary>
        public byte BCR_SQ
        {
            get
            {
                CheckType(ElementType.BCR);
                return (byte)(Element[4] & 0x1f);
            }
            set
            {
                CheckType(ElementType.BCR);
                Element[4] = (byte)(Element[4] & 0xe0 | value & 0x1f);
            }
        }

        /// <summary>
        /// BCR进位标志，1有效。
        /// </summary>
        public bool BCR_CY
        {
            get
            {
                CheckType(ElementType.BCR);
                return Element[4].Bit(5);
            }
            set
            {
                CheckType(ElementType.BCR);
                Element[4] = value ? Element[4].SetBit(5) : Element[4].ClearBit(5);
            }
        }

        /// <summary>
        /// BCR调整标志，1有效。
        /// </summary>
        public bool BCR_CA
        {
            get
            {
                CheckType(ElementType.BCR);
                return Element[4].Bit(6);
            }
            set
            {
                CheckType(ElementType.BCR);
                Element[4] = value ? Element[4].SetBit(6) : Element[4].ClearBit(6);
            }
        }

        /// <summary>
        /// BCR有效标志，0有效。
        /// </summary>
        public bool BCR_IV
        {
            get
            {
                CheckType(ElementType.BCR);
                return !Element[4].Bit(7);
            }
            set
            {
                CheckType(ElementType.BCR);
                Element[4] = !value ? Element[4].SetBit(7) : Element[4].ClearBit(7);
            }
        }

        public void SaveTo(List<byte> buf)
        {
            if (Addr != null)
                buf.AddRange(Addr);
            if (Element != null)
                buf.AddRange(Element);
            if (Extra != null)
                buf.AddRange(Extra);
            if (TimeStamp != null)
                buf.AddRange(TimeStamp);
        }
    }
}

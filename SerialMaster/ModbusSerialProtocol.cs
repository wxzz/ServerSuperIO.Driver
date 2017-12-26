using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerSuperIO.Communicate;
using ServerSuperIO.DataCache;
using ServerSuperIO.Device;
using ServerSuperIO.Protocol;
using ServerSuperIO.Protocol.Filter;

namespace ServerSuperIO.Driver.Serial
{
    internal class ModbusSerialProtocol : ProtocolDriver
    {
        public override bool CheckData(byte[] data)
        {
            if(data.Length==7 || data.Length==15) //rtu ascii
            {
                return true;
            }
            else
            {
                return false;
            }
            //byte[] srcCrc = data.Take(data.Length - 2).ToArray();
            //byte[] crc = Modbus.Utility.ModbusUtility.CalculateCrc(srcCrc);
            //if(data[data.Length-2]==crc[0] && data[data.Length-1]==crc[1])
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        public override int GetAddress(byte[] data)
        {
            return data[0];
        }

        public override byte[] GetCheckData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override string GetCode(byte[] data)
        {
            return data[0].ToString();
        }

        public override byte[] GetCommand(byte[] data)
        {
            return new byte[] { data[1] };
        }

        public override byte[] GetEnd(byte[] data)
        {
            return new byte[] { };
        }

        public override byte[] GetHead(byte[] data)
        {
            return new byte[]{ };
        }

        /// <summary>
        /// 不传大块文件 ，不涉及到这个功能，与配置参数CheckPackageLength相关。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="channel"></param>
        /// <param name="readTimeout"></param>
        /// <returns></returns>
        public override int GetPackageLength(byte[] data, IChannel channel, ref int readTimeout)
        {
            return 0;
        }
    }
}

using ServerSuperIO.Data;
using ServerSuperIO.Modbus.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSuperIO.Driver
{
    public class SendObject
    {
        public IModbusMessage ModbusMessage { get; set; }

        public ITag Tag { get; set; }

        public SendObject(IModbusMessage modbusMessage,ITag tag)
        {
            ModbusMessage = modbusMessage;
            Tag = tag;
        }
    }
}

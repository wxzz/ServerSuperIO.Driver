using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerSuperIO.Communicate;
using ServerSuperIO.Device;
using ServerSuperIO.Device.Connector;
using ServerSuperIO.Protocol;
using ServerSuperIO.Service.Connector;
using ServerSuperIO.Data;
using ServerSuperIO.Modbus.Device;
using ServerSuperIO.Modbus;
using ServerSuperIO.Modbus.Message;
using ServerSuperIO.WebSocket;

namespace ServerSuperIO.Driver.Tcp
{
    public class ModbusTcpDriver : ServerSuperIO.Device.RunDevice
    {
        /// <summary>
        /// 这是Server框架必须使用的对象
        /// </summary>
        private ModbusTcpProtocol _rtuPro;
        private IModbusMaster _modbusTcpMaster;

        /// <summary>
        /// 临时记录当前正在发送的数据信息
        /// </summary>
        private SendObject _sendObject;

        public ModbusTcpDriver()
        {
            _rtuPro = new ModbusTcpProtocol();
            _modbusTcpMaster = ModbusMasterFactory.CreateSocket();
        }

        /// <summary>
        /// 设备驱动初始化函数
        /// </summary>
        /// <param name="obj">obj在Designer宿主程序里默认传入 ServerSuperIO.Config.DeviceConfig 对象。如果使用ServerSuperIO可以自定义传入的参数</param>
        public override void Initialize(object obj)
        {
            base.Initialize(obj);
        }

        /// <summary>
        /// 此设备的类型，一般为Common，另一类是Virtual虚拟设备。
        /// </summary>
        public override DeviceType DeviceType
        {
            get
            {
                return DeviceType.Common;
            }
        }

        /// <summary>
        /// 设备型号，代表一类设备类型
        /// </summary>
        public override string ModelNumber
        {
            get
            {
                return "ModbusTcp";
            }
        }

        /// <summary>
        /// 设备协议，用于打包发送数据和解析接收数据包。
        /// </summary>
        public override IProtocolDriver Protocol
        {
            get
            {
                return _rtuPro;
            }
        }

        /// <summary>
        /// IO通道状态已经发生改变。可以更新到相应的数据库，或状态持久化的媒介。
        /// </summary>
        /// <param name="channelState"></param>
        public override void ChannelStateChanged(ChannelState channelState)
        {
            
        }

        /// <summary>
        /// 通讯状态已经发生改变，包括：通讯正常、通讯中断、通讯干扰，以及通讯未知
        /// </summary>
        /// <param name="comState"></param>
        public override void CommunicateStateChanged(CommunicateState comState)
        {
            
        }

        /// <summary>
        /// 通讯正常，把返回的数据返回到此函数接口。在Protocol类中的CheckData函数对数据进行校验。
        /// </summary>
        /// <param name="info"></param>
        public override void Communicate(IResponseInfo info)
        {
            if (_sendObject == null)
            {
                OnDeviceRuningLog("没有获得对应的发送请求实例");
                return;
            }

            byte[] revData = info.Data;
            IModbusMessage requestMessage = _sendObject.ModbusMessage;
            ITag tag = _sendObject.Tag;
            bool deal = false;

            if (tag.Function == Modbus.Modbus.ReadCoils)
            {
                #region
                bool[] responseVals = _modbusTcpMaster.GetReadCoilsResponse(revData, tag.Quantity, requestMessage);
                if (responseVals.Length >= 1)
                {
                    this.DeviceDynamic.DynamicData.Write(tag.TagName, responseVals[0] == true ? 1 : 0);
                    deal = true;
                }
                #endregion
            }
            else if(tag.Function == Modbus.Modbus.ReadInputs)
            {
                #region
                bool[] responseVals = _modbusTcpMaster.GetReadInputsResponse(revData, tag.Quantity, requestMessage);
                if (responseVals.Length >= 1)
                {
                    this.DeviceDynamic.DynamicData.Write(tag.TagName, responseVals[0] == true ? 1 : 0);
                    deal = true;
                }
                #endregion
            }
            else if (tag.Function == Modbus.Modbus.ReadHoldingRegisters)
            {
                #region
                ushort[] responseVals = _modbusTcpMaster.GetReadHoldingRegistersResponse(revData, requestMessage);
                if (responseVals.Length >= 1)
                {
                    this.DeviceDynamic.DynamicData.Write(tag.TagName, responseVals[0]);
                    deal = true;
                }
                #endregion
            }
            else if (tag.Function == Modbus.Modbus.ReadInputRegisters)
            {
                #region
                ushort[] responseVals = _modbusTcpMaster.GetReadInputRegistersResponse(revData, requestMessage);
                if (responseVals.Length >= 1)
                {
                    this.DeviceDynamic.DynamicData.Write(tag.TagName, responseVals[0]);
                    deal = true;
                }
                #endregion
            }

            if(deal)
            {
                OnDeviceRuningLog("通讯正常，已经处理数据");
            }
        }

        /// <summary>
        /// 通讯干扰，代表接收到数据，但是数据格式不符合协议标准。可能没有接收全数据，也可能受到电磁干扰。
        /// </summary>
        /// <param name="info"></param>
        public override void CommunicateError(IResponseInfo info)
        {
            _sendObject = null;
            OnDeviceRuningLog("通讯干扰");
        }

        /// <summary>
        /// 通讯中断，代表发送数据给设备后，在超时时间内没有返回任何数据信息
        /// </summary>
        /// <param name="info"></param>
        public override void CommunicateInterrupt(IResponseInfo info)
        {
            _sendObject = null;
            OnDeviceRuningLog("通讯中断");
        }

        /// <summary>
        /// 通讯未知，一般IO实例为空的时间调用此函数接口，例如没有打开串口或网络没有连接。
        /// </summary>
        public override void CommunicateNone()
        {
            _sendObject = null;
        }

        /// <summary>
        /// 如果调用server.RemoveDevice则会调用这个函数接口。可以联动删除持久存储内的设备信息。Designer程序已经实现
        /// </summary>
        public override void Delete()
        {
           
        }

        /// <summary>
        /// 退出设备，在宿主程序的时候调用。
        /// </summary>
        public override void Exit()
        {
           
        }

        /// <summary>
        /// 每次返回请求数据命令。
        /// </summary>
        /// <returns></returns>
        public override IList<IRequestInfo> GetConstantCommand()
        {
            IList<ITag> tags = this.DeviceDynamic.DynamicData.GetTags();
            if (tags.Count <= 0) return null;
            IList<IRequestInfo> requestList = new List<IRequestInfo>();
            foreach(ITag tag in tags)
            {
                IModbusMessage requestMsg=null;
                byte[] reqeustBytes=null;
                if(tag.Function == Modbus.Modbus.ReadCoils)
                {
                    #region 
                    reqeustBytes = _modbusTcpMaster.BuildReadCoilsCommand((byte)tag.SlaveId, (ushort)tag.Address, (ushort)tag.Quantity, out requestMsg);
                    #endregion
                }
                else if(tag.Function ==Modbus.Modbus.ReadInputs)
                {
                    #region 
                    reqeustBytes = _modbusTcpMaster.BuildReadInputsCommand((byte)tag.SlaveId, (ushort)tag.Address, (ushort)tag.Quantity, out requestMsg);
                    #endregion
                }
                else if(tag.Function == Modbus.Modbus.ReadHoldingRegisters)
                {
                    #region 
                    reqeustBytes = _modbusTcpMaster.BuildReadHoldingRegistersCommand((byte)tag.SlaveId, (ushort)tag.Address, (ushort)tag.Quantity, out requestMsg);
                    #endregion
                }
                else if(tag.Function == Modbus.Modbus.ReadInputRegisters)
                {
                    #region 
                    reqeustBytes = _modbusTcpMaster.BuildReadInputRegistersCommand((byte)tag.SlaveId, (ushort)tag.Address, (ushort)tag.Quantity, out requestMsg);
                    #endregion
                }

                if (reqeustBytes!=null)
                {
                    requestList.Add(new RequestInfo(reqeustBytes, new SendObject(requestMsg,tag)));
                }
            }
            return requestList;
        }

        /// <summary>
        /// 重写发送，记录当前正在发送的数据实例
        /// </summary>
        /// <param name="io"></param>
        /// <param name="request"></param>
        /// <param name="frameType"></param>
        /// <returns></returns>
        public override int Send(IChannel io, IRequestInfo request, WebSocketFrameType frameType = WebSocketFrameType.Binary)
        {
            _sendObject = (SendObject)request.State;

            return base.Send(io, request, frameType);
        }

        /// <summary>
        /// 在其他地方获得设备对象信息，可以自定义
        /// </summary>
        /// <returns></returns>
        public override object GetObject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 在宿主程序中调用上下文菜单，现在在Designer中还没有实现。
        /// </summary>
        public override void ShowContextMenu()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 未知IO接口，
        /// </summary>
        public override void UnknownIO()
        {
            OnDeviceRuningLog("未知通道");
        }

        /// <summary>
        /// 与服务（Service）交互使用的接口，与service.OnServiceConnector配合使用。
        /// </summary>
        /// <param name="fromService"></param>
        /// <param name="toDevice"></param>
        /// <param name="asyncService"></param>
        /// <returns></returns>
        public override IServiceConnectorCallbackResult RunServiceConnector(IFromService fromService, IServiceToDevice toDevice, AsyncServiceConnectorCallback asyncService)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 设备之间传递信息，使用该接口，与OnDeviceConnector配合使用
        /// </summary>
        /// <param name="obj"></param>
        public override void DeviceConnectorCallback(object obj)
        {
            throw new NotImplementedException();
        }

        public override void DeviceConnectorCallbackError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public override IDeviceConnectorCallbackResult RunDeviceConnector(IFromDevice fromDevice, IDeviceToDevice toDevice, AsyncDeviceConnectorCallback asyncCallback)
        {
            throw new NotImplementedException();
        }
    }
}

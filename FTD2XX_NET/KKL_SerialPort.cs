using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using static FTD2XX_NET.FTDI;

namespace FTD2XX_NET
{
    public class KKL_SerialPort : SerialPortBase
    {
        public SerialPort serialPort;
        public uint lastChosenPort;

        public override bool IsOpen => serialPort?.IsOpen??false;

        private IEnumerable<FT_DEVICE_INFO_NODE> convertComPortsToFtdiDevices(string[] allComPorts)
        {
            return allComPorts.Select(cp => new FT_DEVICE_INFO_NODE
            {
                Description = cp,
                LocId = uint.Parse(cp.Substring(3))
            });
        }

        public new static bool IsFTDChipIDDLLLoaded() => true;

        public override IEnumerable<FT_DEVICE_INFO_NODE> EnumerateFTDIDevices()
        {
            return convertComPortsToFtdiDevices(SerialPort.GetPortNames());
        }

        public override FT_STATUS GetChipIDFromDeviceIndex(uint deviceIndex, out uint chipID)
        {
            chipID = 0;
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS OpenByLocation(uint locId)
        {
            lastChosenPort = locId;
            serialPort = new SerialPort($"COM{lastChosenPort}");
            serialPort.Open();
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS ResetDevice()
        {            
            serialPort = new SerialPort($"COM{lastChosenPort}");
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS Purge(uint v)
        {
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS GetChipIDFromCurrentDevice(out uint chipID)
        {
            chipID = lastChosenPort;
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS Close()
        {
            serialPort.Close();
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS SetDataCharacteristics(byte DataBits, byte StopBits, byte Parity)
        {
            serialPort.DataBits = DataBits;

            switch(StopBits)
            {
                //case 0:
                //    serialPort.StopBits = System.IO.Ports.StopBits.None;
                //    break;
                case FT_STOP_BITS.FT_STOP_BITS_1:
                    serialPort.StopBits = System.IO.Ports.StopBits.One;
                    break;
                case FT_STOP_BITS.FT_STOP_BITS_2:
                    serialPort.StopBits = System.IO.Ports.StopBits.Two;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Stop bit value: {StopBits} not allowed in FTDI spec!");
            }

            switch(Parity)
            {
                case FT_PARITY.FT_PARITY_NONE:
                    serialPort.Parity = System.IO.Ports.Parity.None;
                    break;
                case FT_PARITY.FT_PARITY_EVEN:
                    serialPort.Parity = System.IO.Ports.Parity.Even;
                    break;
                case FT_PARITY.FT_PARITY_ODD:
                    serialPort.Parity = System.IO.Ports.Parity.Odd;
                    break;
                case FT_PARITY.FT_PARITY_SPACE:
                    serialPort.Parity = System.IO.Ports.Parity.Space;
                    break;
                case FT_PARITY.FT_PARITY_MARK:
                    serialPort.Parity = System.IO.Ports.Parity.Mark;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Parity value: {Parity} not allowed in FTDI spec!");
            }

            return FT_STATUS.FT_OK;
        }

        // mapped FTDI latency setting to read timeout serial port in .NET
        public override FT_STATUS SetLatency(byte v)
        {
            serialPort.ReadTimeout = v;
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS SetTimeouts(uint fTDIDeviceReadTimeOutMs, uint fTDIDeviceWriteTimeOutMs)
        {
            serialPort.ReadTimeout = (int)fTDIDeviceReadTimeOutMs;
            serialPort.WriteTimeout = (int)fTDIDeviceWriteTimeOutMs;

            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS SetDTR(bool enableDtr)
        {
            serialPort.DtrEnable = enableDtr;

            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS SetRTS(bool enableRts)
        {
            serialPort.RtsEnable = enableRts;

            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS SetBreak(bool enableBreak)
        {
            serialPort.BreakState = enableBreak;

            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS SetBitMode(byte Mask, byte BitMode)
        {
            //??
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS SetBaudRate(uint baudRate)
        {
            serialPort.BaudRate = (int)baudRate;

            return FT_STATUS.FT_OK;
        }

        //NOTE: Not handling error state very well..
        public override FT_STATUS Write(byte[] data, int numBytesToWrite, ref uint numBytesWritten, uint maxNumAttempts)
        {
            serialPort.Write(data, 0, numBytesToWrite);
            numBytesWritten = (uint)numBytesToWrite;
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS Read(byte[] dataBuffer, uint numBytesToRead, ref uint numBytesRead, uint maxNumAttempts)
        {
            var bytesRead = serialPort.Read(dataBuffer, 0, (int)numBytesToRead);
            numBytesRead = (uint)bytesRead;
            return FT_STATUS.FT_OK;
        }

        //public class FT_FLOW_CONTROL
        //{
        //    public const ushort FT_FLOW_DTR_DSR = 0x200;
        //    public const ushort FT_FLOW_NONE = 0;
        //    public const ushort FT_FLOW_RTS_CTS = 0x100;
        //    public const ushort FT_FLOW_XON_XOFF = 0x400;
        //}
        public override FT_STATUS SetFlowControl(ushort FlowControl, byte Xon, byte Xoff)
        {
            switch(FlowControl)
            {
                case FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE:
                    serialPort.Handshake = Handshake.None;
                    break;
                case FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS:
                    serialPort.Handshake = Handshake.RequestToSend;
                    break;
                case FTDI.FT_FLOW_CONTROL.FT_FLOW_DTR_DSR:
                    throw new ArgumentOutOfRangeException($"Flow control value: FT_FLOW_DTR_DSR is out of range. .NET Serial port does not support");
                case FT_FLOW_CONTROL.FT_FLOW_XON_XOFF:
                    serialPort.Handshake = Handshake.XOnXOff;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Flow control value: {FlowControl} is out of range. Check FTDI documentation");
            }

            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS Write(string dataBuffer, int numBytesToWrite, ref uint numBytesWritten)
        {
            serialPort.Write(dataBuffer.ToCharArray(), 0, numBytesToWrite);
            numBytesWritten = (uint)numBytesToWrite;
            return FT_STATUS.FT_OK;
        }

        public override FT_STATUS GetTxBytesWaiting(ref uint TxQueue)
        {
            TxQueue = (uint)serialPort.BytesToWrite;
            return FT_STATUS.FT_OK;

        }

        public override FT_STATUS GetRxBytesAvailable(ref uint RxQueue, uint maxNumAttempts)
        {
            RxQueue = (uint)serialPort.BytesToRead;
            return FT_STATUS.FT_OK;
        }
    }
}

using System.Collections.Generic;
using static FTD2XX_NET.FTDI;

namespace FTD2XX_NET
{
    public enum SerialMode { FTDI, Serial };
    public abstract class SerialPortBase
    {
        public abstract bool IsOpen { get; }
        public abstract IEnumerable<FT_DEVICE_INFO_NODE> EnumerateFTDIDevices();
        public abstract FT_STATUS GetChipIDFromDeviceIndex(uint deviceIndex, out uint chipID);
        public static bool IsFTDChipIDDLLLoaded() { return true; } // default to true for all implementations
        public abstract FT_STATUS OpenByLocation(uint locId);
        public abstract FT_STATUS ResetDevice();
        public abstract FT_STATUS Purge(uint v);
        public abstract FT_STATUS GetChipIDFromCurrentDevice(out uint chipID);
        public abstract FT_STATUS Close();
        public abstract FT_STATUS SetDataCharacteristics(byte fT_BITS_8, byte fT_STOP_BITS_1, byte fT_PARITY_NONE);
        public abstract FT_STATUS SetFlowControl(ushort FlowControl, byte Xon, byte Xoff);
        public abstract FT_STATUS SetLatency(byte Latency);
        public abstract FT_STATUS SetTimeouts(uint fTDIDeviceReadTimeOutMs, uint fTDIDeviceWriteTimeOutMs);
        public abstract FT_STATUS SetDTR(bool enableDtr);
        public abstract FT_STATUS SetRTS(bool enableRts);
        public abstract FT_STATUS SetBreak(bool enableBreak);
        public abstract FT_STATUS SetBitMode(byte Mask, byte BitMode);
        public abstract FT_STATUS SetBaudRate(uint baudRate);
        public abstract FT_STATUS Write(byte[] data, int length, ref uint numBytesWritten, uint v);
        public abstract FT_STATUS Write(string dataBuffer, int numBytesToWrite, ref uint numBytesWritten);
        public abstract FT_STATUS Read(byte[] dataBuffer, uint numBytesToRead, ref uint numBytesRead, uint maxNumAttempts);
        public abstract FT_STATUS GetTxBytesWaiting(ref uint TxQueue);
        public abstract FT_STATUS GetRxBytesAvailable(ref uint RxQueue, uint maxNumAttempts);
    }
}

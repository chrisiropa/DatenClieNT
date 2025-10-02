using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public enum SpsReturnCode
   {
      Ok = 0,
      CriticalReadError = -1024,
      CriticalTimeout = -1025,
      
      TerminateReceived = 0xFFFF ,
      NotImplemented = 0xEEEE, 
      Unspecified = 0xDDDD,   
      
      DaveResOK = 0,
      DaveResNoPeripheralAtAddress = 1,
      DaveResMultipleBitsNotSupported = 6,
      DaveResItemNotAvailable200 = 3,
      DaveResItemNotAvailable = 10,
      DaveAddressOutOfRange = 5,
      DaveWriteDataSizeMismatch = 7,
      DaveResCannotEvaluatePDU = -123,
      DaveResCPUNoData = -124,
      DaveUnknownError = -125,
      DaveEmptyResultError = -126,
      DaveEmptyResultSetError = -127,
      DaveResUnexpectedFunc = -128,
      DaveResUnknownDataUnitSize = -129,
      DaveResShortPacket = -1024,
      DaveResTimeout = -1025
   }
}

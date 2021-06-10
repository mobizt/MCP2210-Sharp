
// Copyright(c) 2016 Andrea Cervesato <sawk.ita@gmail.com>

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using MCP2210;
using System;
using System.Text;

namespace Application {
    class Program {

        private const int NumberOfBytes = 16;
        private const string Password = "123456";

        static void Main() {
            // create device
            IUsbToSpiDevice device = new UsbToSpiDevice();
            device.Connect(Password);

            // test device modules
            TestNonVoltatileRam(device.NonVolatileRam);
            TestVoltatileRam(device.VolatileRam);
            TestExternalInterruptPin(device.ExternalInterruptPin);
            TestSpiDataTransfer(device.SpiDataTransfer);
            TestEeprom(device.EEPROM);

            // disconnect the device
            device.Disconnect();

            // close when the user wants to
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }

        static void PrintException(Exception ex) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n");
            sb.AppendLine("***************ERRROR**************");
            sb.AppendLine();
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);
            sb.AppendLine();
            sb.AppendLine("***********************************");
            sb.AppendLine();

            Console.WriteLine(sb.ToString());
        }

        static void TestNonVoltatileRam(INonVolatileRam nvram) {
            Console.Write("*** Test NVRAM: ");

            try {
                // read/write manufacters and product names
                string manufacterName = nvram.ManufacterName;
                string productName = nvram.ProductName;

                nvram.ManufacterName = "Myself";
                nvram.ProductName = "SomeProduct";

                string manufacterNameAfter = nvram.ManufacterName;
                string productNameAfter = nvram.ProductName;

                nvram.ManufacterName = manufacterName;
                nvram.ProductName = productName;

                // read USB settings
                UsbKeyPowerSettings usbPowerSettings0 = nvram.ReadUsbSettings();
                nvram.WriteUsbSettings(usbPowerSettings0);
                UsbKeyPowerSettings usbPowerSettings1 = nvram.ReadUsbSettings();

                // set the chip configuration
                ChipSettings chipSettings = new ChipSettings();
                chipSettings.InterruptBitMode = DedicatedFunction.NoInterruptCounting;
                chipSettings.RemoteWakeUpEnabled = true;
                chipSettings.SpiBusReleaseEnable = true;
                chipSettings.AccessControl = NramChipAccessControl.PasswordProtected;
                chipSettings.Password = "123456";
                chipSettings.PinDirections = new PinDirection[] {
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output
                };

                chipSettings.PinModes = new PinMode[] {
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO
                };

                chipSettings.DefaultOutput = new bool[] {
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    false,
                    true,
                    true
                };

                nvram.ConfigureChip(chipSettings);
                ChipSettings readSettings = nvram.ReadChipConfiguration();
                
                // configure SPI
                SpiSetup spiSetup = new SpiSetup();
                spiSetup.BitRate = 300000;
                spiSetup.BytesToTransfer = NumberOfBytes;
                spiSetup.ChipSelectToDataDelay = 3;
                spiSetup.BetweenDataDelay = 3;
                spiSetup.DataToChipSelectDelay = 3;
                spiSetup.Mode = SpiModes.Spi0;
                spiSetup.ActiveChipSelectValues = new bool[] {
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true
                };
                spiSetup.IdleChipSelectValues = new bool[] {
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false
                };

                nvram.ConfigureSpi(spiSetup);

                // read spi configuration
                SpiSetup readSpiSetup = nvram.ReadSpiConfiguration();

                Console.WriteLine("completed successfully");
            } catch (Exception ex) {
                PrintException(ex);
            }
        }

        static void TestVoltatileRam(IVolatileRam ram) {
            Console.Write("*** Test RAM: ");

            try {
                // set the chip configuration
                ChipSettings chipSettings = new ChipSettings();
                chipSettings.InterruptBitMode = DedicatedFunction.NoInterruptCounting;
                chipSettings.RemoteWakeUpEnabled = false;
                chipSettings.SpiBusReleaseEnable = false;
                // these are never used by the volatile RAM
                //chipSettings.AccessControl = NramChipAccessControl.PasswordProtected;
                //chipSettings.Password = "abaco";
                chipSettings.PinDirections = new PinDirection[] {
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output,
                    PinDirection.Output
                };

                chipSettings.PinModes = new PinMode[] {
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.GPIO,
                    PinMode.DedicatedFunction, // release status bit
                    PinMode.GPIO
                };

                chipSettings.DefaultOutput = new bool[] {
                    true,
                    false,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true,
                    true
                };

                ram.ConfigureChip(chipSettings);
                ChipSettings readSettings = ram.ReadChipConfiguration();

                // configure SPI
                SpiSetup spiSetup = new SpiSetup();
                spiSetup.BitRate = 200000;
                spiSetup.BytesToTransfer = NumberOfBytes;
                spiSetup.ChipSelectToDataDelay = 0;
                spiSetup.BetweenDataDelay = 0;
                spiSetup.DataToChipSelectDelay = 0;
                spiSetup.Mode = SpiModes.Spi2;
                spiSetup.ActiveChipSelectValues = new bool[] {
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true,
                    true
                };
                spiSetup.IdleChipSelectValues = new bool[] {
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false
                };

                ram.ConfigureSpi(spiSetup);

                // read spi configuration 
                SpiSetup readSpiSetup = ram.ReadSpiConfiguration();

                // set the gpio output
                bool[] output = new bool[] {
                    false,
                    true,
                    true,
                    false,
                    true,
                    true,
                    false,
                    true,
                    true
                };

                ram.GpioPinsValue = output;

                Console.WriteLine("completed successfully");
            } catch (Exception ex) {
                PrintException(ex);
            }
        }

        static void TestExternalInterruptPin(IExternalInterruptPin extIntPin) {
            Console.Write("*** Test External interrupt pin: ");

            try {
                int numofEvents = extIntPin.ReadNumberOfEvents(true);

                Console.WriteLine("completed successfully");
            } catch (Exception ex) {
                PrintException(ex);
            }
        }

        static void TestSpiDataTransfer(ISpiDataTransfer spi) {
            Console.Write("*** Test SPI data transfer: ");

            try {
                // cancel the current transfer
                spi.CancelCurrentTransfer();

                // release the SPI bus, and setthe release status bit to 1
                spi.RequestSpiBusRelease(true);

                // read the status
                SpiEngineStatus response1 = spi.ReadEngineStatus();

                // send a SPI word
                byte[] data = new byte[NumberOfBytes];
                TransferReply reply = spi.Transfer(data);

                // cancel current spi transfer
                SpiEngineStatus response0 = spi.CancelCurrentTransfer();

                // release the spi bus
                spi.RequestSpiBusRelease();

                Console.WriteLine("completed successfully");
            } catch (Exception ex) {
                PrintException(ex);
            }
        }

        static void TestEeprom(IEepromMemory eeprom) {
            Console.Write("*** Test EEPROM reading: ");
            try {
                byte addr0 = eeprom.ReadAddress(0x00);
                byte addr1 = eeprom.ReadAddress(0x10);
                byte addr2 = eeprom.ReadAddress(0x20);
                byte addr3 = eeprom.ReadAddress(0x30);
                byte addr4 = eeprom.ReadAddress(0x40);
                byte addr5 = eeprom.ReadAddress(0x50);
                byte addr6 = eeprom.ReadAddress(0x60);

                Console.WriteLine("completed successfully");
            } catch (Exception ex) {
                PrintException(ex);
            }
        }
    }
}

/*  PLC_SQL communicates with MODBUS TCP IP, reads integers from the memory of PLC 
    can convert or regulate the value and  store it to local SQL database.
    Using EasyModbus values are read from the PLC. System is set to communicate with
    setup created for CSD3.1.14 - 15 at UCN Sofiendalsvej.
    IP 192.168.18.10 port 502
    EasyModbus: https://sourceforge.net/p/easymodbustcp/wiki/Methods%20ModbusClient/
 */
using EasyModbus;
using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading;
//using System.Threading.Tasks;

namespace PCL_SQL
{
    class Program
    {
        static void Main(string[] args)
        {
            ModbusClient modbusClient = new ModbusClient("192.168.18.10", 502);

            while (true)
            {
                DateTime myDateTime = DateTime.Now; // Read time value
                string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss"); // Store timestamp as formatted string

                try
                {
                    if (modbusClient.Connected) //Controlling of the system is connected.
                    {


                        int[] csd3_1_14Array = modbusClient.ReadHoldingRegisters(0, 30); //Read data from MODBUS TCP IP, start in memory block 0 and read 30 memory blocks.
                        //int[] csd3_1_15Array = modbusClient.ReadHoldingRegisters(101, 100);
                        //int[] csd3_1_16Array = modbusClient.ReadHoldingRegisters(101, 100);

                        using (PLC_dbEntities db = new PLC_dbEntities()) // Entity framework linQ
                        {
                            CSD3_1_14 dataset_14 = new CSD3_1_14
                            {
                                Timestamp = sqlFormattedDate,   // Store timestamp, is used as primary key
                                CO2 = csd3_1_14Array[0],    // Store CO2 value 400 - 10000 ppm
                                Temperatur = (float)csd3_1_14Array[1] / 10, // Convert Temperature integer to float with 1 decimal and store.
                                Fugtighed = csd3_1_14Array[2],  // Store Fugtighed value 0 - 100 %
                                Strøm = (float) csd3_1_14Array[3]/100,  // Convert Strøm integer to float with 2 decimals and store. (Do we need to remove negative values)
                                Spænding = (float) csd3_1_14Array[4]/100,   // Convert Spænding integer to float with 2 decimals and store. (Do we need to remove negative values)
                                Effekt = csd3_1_14Array[5], // Store Effekt value (Do we need to remove negative values)
                                Energi = csd3_1_14Array[6] + csd3_1_14Array[7]  // Convert the two integers for Energi to one and store. (What operation is needed?)
                            };
                            db.CSD3_1_14.Add(dataset_14);      // Save dataset to variable
                            

                            db.SaveChanges();   //Save changes to SQL database
                        }

                        Console.WriteLine("[Data recieved] [" + sqlFormattedDate + "]");
                        Thread.Sleep(60000);    // Sleeps for 60 s to limit data storing
                    }
                    else
                    {
                        Console.WriteLine("[" + sqlFormattedDate + "]: Connection not established, trying to connect.");
                        modbusClient.Connect(); // Connect to ModBUS TCPIP
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] " + ex.ToString());
                    modbusClient.Disconnect();
                    Thread.Sleep(30000);
                }
            }
        }
    }
}

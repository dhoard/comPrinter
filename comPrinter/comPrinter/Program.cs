/*
 * Copyright 2016 Doug Hoard
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;

namespace comPrinter
{
    class Program
    {        
        private SerialPort serialPort = null;

        private string portName = null;
        private int baudRate = 9600;
        private int dataBits = 8;
        private Parity parity = Parity.None;
        private Handshake handshake = Handshake.None;       
        
        private StopBits stopBits = StopBits.One;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.execute(args);
        }

        public void execute(string[] args)
        {
            if ((null == args) || (2 != args.Length))
            {
                Console.WriteLine("");
                Console.WriteLine("Usage: comPrinter <COM port> <filename>");
                return;
            }

            StreamReader streamReader = null;
            
            try
            {
                this.portName = args[0];
                this.serialPort = new SerialPort(this.portName, this.baudRate, this.parity, this.dataBits, this.stopBits);
                this.serialPort.Open();

                streamReader = new StreamReader(args[1]);

                List<String> list = new List<String>();
                String line = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    list.Add(line);                    
                }

                long minX = Int64.MaxValue;
                long minY = Int64.MaxValue;
                List<String> outputList = new List<String>();

                foreach (String line2 in list)
                {
                    String upperCaseLine = line2.ToUpper();
                    if (upperCaseLine.StartsWith("PU") || upperCaseLine.StartsWith("PD"))
                    {
                        // PU-17593 21909;
                        // PD-17599 21784;

                        upperCaseLine = upperCaseLine.Substring(2);
                        String[] tokens = upperCaseLine.Split(' ', ';');

                        long xValue = Int64.Parse(tokens[0]);                        
                        long yValue = Int64.Parse(tokens[1]);

                        if (xValue < minX)
                        {
                            minX = xValue;
                        }

                        if (yValue < minY)
                        {
                            minY = yValue;
                        }

                        outputList.Add(line2.Substring(0, 2) + Convert.ToString(xValue) + " " + Convert.ToString(yValue) + ";");
                    }
                    else
                    {
                        outputList.Add(line2);
                    }
                }

                foreach (String line2 in outputList)
                {
                    String upperCaseLine = line2.ToUpper();
                    if (upperCaseLine.StartsWith("PU") || upperCaseLine.StartsWith("PD"))
                    {
                        // PU-17593 21909;
                        // PD-17599 21784;

                        upperCaseLine = upperCaseLine.Substring(2);
                        String[] tokens = upperCaseLine.Split(' ', ';');

                        long xValue = Int64.Parse(tokens[0]);
                        xValue = xValue - minX;
                        //xValue = xValue / 5;

                        long yValue = Int64.Parse(tokens[1]);
                        yValue = yValue - minY;
                        //yValue = yValue / 5;
 
                        line = line2.Substring(0, 2) + Convert.ToString(xValue) + " " + Convert.ToString(yValue) + ";";
                        this.serialPort.WriteLine(line);
                    }
                    else
                    {
                        this.serialPort.WriteLine(line2);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e);
            }
            finally
            {
                if (null != streamReader)
                {
                    try
                    {
                        streamReader.Close();
                    }
                    catch (Exception e)
                    {
                        // DO NOTHING
                    }
                }

                if (null != this.serialPort)
                {
                    if (true == this.serialPort.IsOpen)
                    {
                        try
                        {
                            this.serialPort.Close();
                        }
                        catch (Exception e)
                        {
                            // DO NOTHING
                        }
                    }
                }
            }
        }
    }
}

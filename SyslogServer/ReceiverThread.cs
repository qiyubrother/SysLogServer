using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace WatsonSyslog
{
    public partial class SyslogServer
    {
        static void ReceiverThread()
        {
            if (_ListenerUdp == null) _ListenerUdp = new UdpClient(_Settings.UdpPort);

            try
            {
                #region Start-Listener

                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, _Settings.UdpPort);
                string receivedData;
                byte[] receivedBytes; 

                while (true)
                {
                    #region Receive-Data

                    receivedBytes = _ListenerUdp.Receive(ref endpoint);
                    receivedData = Encoding.ASCII.GetString(receivedBytes, 0, receivedBytes.Length);
                    //var data2 = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);
                    //var data3 = Encoding.Unicode.GetString(receivedBytes, 0, receivedBytes.Length);
                    //var data4 = Encoding.UTF7.GetString(receivedBytes, 0, receivedBytes.Length);
                    //var data5 = Encoding.UTF32.GetString(receivedBytes, 0, receivedBytes.Length);
                    string msg = null;
                    if (_Settings.DisplayTimestamps) msg = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + " ";
                    msg += receivedData;
                    //Console.WriteLine($"[ASCII]:{receivedData}, [UTF8]:{data2}, [Unicode]:{data3}, [UTF7]:{data4}, [UTF32]:{data5}, [Length]:{receivedBytes.Length}");

                    Console.WriteLine(msg);
                    
                    #endregion

                    #region Add-to-Queue

                    lock (_WriterLock)
                    {
                        _MessageQueue.Add(msg);
                    }

                    #endregion
                } 

                #endregion
            }
            catch (Exception e)
            {
                _ListenerUdp.Close();
                _ListenerUdp = null;
                Console.WriteLine("***");
                Console.WriteLine("ReceiverThread exiting due to exception: " + e.Message);
                return;
            }
        }
    }
}

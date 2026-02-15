using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Collections;

namespace mcpeonline;

class Program{

	public static async Task Main(){
		Console.Write("Пиши айпи сервера в формате ip:port\n>>>");
		var rediks = Console.ReadLine();
		if(rediks == null) return;
		string[] rede = rediks.Split(":");
		
		string ip = rede[0];
		int port = Convert.ToInt32(rede[1]);
		using Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		var serverIP = new IPEndPoint(Dns.GetHostAddresses(ip)[0], port);

		byte[] data = new byte[1492];

		await sendPing(udpSocket, serverIP);

		while(true){
			EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
			var result = await udpSocket.ReceiveFromAsync(data, SocketFlags.None, remoteEp);
			var remoteIp = (IPEndPoint)result.RemoteEndPoint;

			if(TryParsePong(data, out string message)){
				//Console.WriteLine(message);
				string[] online = message.Split(";");
				Console.WriteLine($"Информация о сервере {online[1]}:\n - Версия: {online[3]}\n - Количество игроков: {online[4]}/{online[5]}\n - Ядро: {online[7]}");
			} else {
				Console.WriteLine("Получен кривой пакет.");
			}

			//Console.WriteLine($"Получено {result.ReceivedBytes} байт от {remoteIp.Address}:{remoteIp.Port}");
		}
	}

	public static async Task sendPing(Socket udpSocket, IPEndPoint ip){
	    byte[] buff = Convert.FromBase64String("AX//////////AP//AP7+/v79/f39EjRWeH//////////");
		await udpSocket.SendToAsync(buff, SocketFlags.None, ip);
	}

	public static bool TryParsePong(byte[] data, out string message){
		message = "NULL";
		if(data == null) return false;

		byte[] msgCount = data.Skip(33).Take(2).ToArray();
		Array.Reverse(msgCount);
		var finalcount = BitConverter.ToUInt16(msgCount);
		
		//Console.WriteLine($"COUNT BYTES OF MESSAGE: {finalcount}");
		byte[] msgOrig = data.Skip(35).Take(finalcount).ToArray();
		string msg = Encoding.UTF8.GetString(msgOrig);
		message = msg;
		return true;
	}
}
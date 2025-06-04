// OVAJ KOD JE UZET IZ: Vezba 8 - Zadatak 2 i Vezba 9 - Zadatak 1
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Model; // Zadatak 2 – korišćenje klase Igrac

namespace TCPServer
{
    class Server
    {
        static List<Igrac> igraci = new List<Igrac>();
        static int MaksimalnoIgraca = 2;

        static void Main(string[] args)
        {
            try
            {
                // Zadatak 2 Tačka 1 – pokretanje TCP osluškivača
                TcpListener listener = new TcpListener(IPAddress.Any, 9001);
                listener.Start();
                Console.WriteLine("Server je pokrenut i čeka 2 igrača...");

                while (igraci.Count < MaksimalnoIgraca)
                {
                    TcpClient klijent = listener.AcceptTcpClient();
                    Console.WriteLine("Klijent povezan.");

                    NetworkStream tok = klijent.GetStream();
                    BinaryFormatter formater = new BinaryFormatter();

                    try
                    {
                        // Zadatak 2 Tačka 2 – prijem podataka
                        Igrac igrac = (Igrac)formater.Deserialize(tok);
                        igraci.Add(igrac); // Zadatak 2 Tačka 3

                        Console.WriteLine($"Prijavljen igrač: {igrac}");

                        // Zadatak 2 Tačka 4 – potvrda
                        formater.Serialize(tok, "Uspešna prijava. Čekajte ostale igrače...");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Greška tokom prijema: " + ex.Message);
                    }

                    klijent.Close();
                }

                Console.WriteLine("Dva igrača su prijavljena. Server zatvara TCP slušanje.");
                listener.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška pri pokretanju servera: " + ex.Message);
            }
        }
    }
}

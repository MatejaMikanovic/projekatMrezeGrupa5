// Vezba 8 - Zadatak 2 + Vezba 9 - Zadatak 1
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Model; // Zadatak 2 - pristup klasi Igrac

namespace TCPServer
{
    class Program
    {
        // Zadatak 2 Tačka 3 – čuvanje podataka o prijavljenim igračima
        static List<Igrac> igraci = new List<Igrac>();

        // Zadatak 2 - Entry point programa
        static void Main(string[] args)
        {
            // Zadatak 2 Tačka 1 – pokretanje TCP osluškivača
            TcpListener listener = new TcpListener(IPAddress.Any, 9000);
            listener.Start();
            Console.WriteLine("Server je pokrenut i čeka igrače...");

            while (true)
            {
                TcpClient klijent = listener.AcceptTcpClient(); // prihvatanje konekcije
                Console.WriteLine("Klijent povezan.");

                // Zadatak 2 Tačka 2 B – prijem podataka o igraču (ime, prezime)
                NetworkStream tok = klijent.GetStream();
                BinaryFormatter formater = new BinaryFormatter();

                try
                {
                    Igrac igrac = (Igrac)formater.Deserialize(tok); // deserijalizacija objekta
                    igraci.Add(igrac);                              // Zadatak 2 Tačka 3 B – dodavanje u listu
                    Console.WriteLine($"Prijavljen igrač: {igrac}");

                    // Zadatak 2 Tačka 4 – slanje potvrde o uspešnoj prijavi
                    formater.Serialize(tok, "Uspešna prijava");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška: " + ex.Message);
                }

                klijent.Close();
            }
        }
    }
}

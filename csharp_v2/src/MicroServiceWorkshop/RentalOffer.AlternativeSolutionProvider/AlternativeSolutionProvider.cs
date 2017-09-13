using System;
using MicroServiceWorkshop.RapidsRivers;
using MicroServiceWorkshop.RapidsRivers.RabbitMQ;
using Newtonsoft.Json.Linq;

namespace RentalOffer.AlternativeSolutionProvider
{
    class AlternativeSolutionProvider : River.IPacketListener
    {
        static void Main(string[] args)
        {
            string host = args[0];
            string port = args[1];

            var rapidsConnection = new RabbitMqRapids("monitor_in_csharp", host, port);
            var river = new River(rapidsConnection);
            river.RequireValue("need", "car_rental_offer"); 
            river.Register(new AlternativeSolutionProvider());
        }

        public void ProcessPacket(RapidsConnection connection, JObject jsonPacket, PacketProblems warnings)
        {
            Console.WriteLine(" [*] {0}", warnings);

            jsonPacket.Remove("need");
            jsonPacket["solution"] = "alternative_solution_provider";
            jsonPacket["price"] = 3000;
            jsonPacket["frequency"] = 0.5;
            connection.Publish(jsonPacket.ToString());
        }

        public void ProcessError(RapidsConnection connection, PacketProblems errors)
        {
            Console.WriteLine(" [x] {0}", errors);
        }
    }
}

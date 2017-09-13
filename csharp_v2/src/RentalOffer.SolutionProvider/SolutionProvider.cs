using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicroServiceWorkshop.RapidsRivers;
using MicroServiceWorkshop.RapidsRivers.RabbitMQ;
using Newtonsoft.Json.Linq;

namespace RentalOffer.SolutionProvider
{
    class SolutionProvider : River.IPacketListener
    {
        private readonly Random _randomGen = new Random(1000);

        static void Main(string[] args)
        {
            string host = args[0];
            string port = args[1];

            var rapidsConnection = new RabbitMqRapids("monitor_in_csharp", host, port);
            var river = new River(rapidsConnection);
            river.RequireValue("need", "car_rental_offer");
            river.Forbid("solution");
            river.Register(new SolutionProvider());
        }

        public void ProcessPacket(RapidsConnection connection, JObject jsonPacket, PacketProblems warnings)
        {
            Console.WriteLine(" [*] {0}", warnings);
            
            jsonPacket["solution"] = "solution_provider";
            if (jsonPacket["level"] != null)
            {
                jsonPacket["offer"] = DetermineDiscount(int.Parse(jsonPacket["level"].ToString()));
            }

            jsonPacket["price"] = _randomGen.Next(1000, 5000);
            jsonPacket["frequency"] = Math.Round(_randomGen.NextDouble(), 1);

            connection.Publish(jsonPacket.ToString());
        }

        private decimal DetermineDiscount(int membershipLevel)
        {
            switch (membershipLevel)
            {
                case 1:
                    return 1.2m;
                case 2:
                    return 1.4m;
                default:
                    return 1m;
            }
        }

        public void ProcessError(RapidsConnection connection, PacketProblems errors)
        {
            //Console.WriteLine(" [x] {0}", errors);
        }
    }
}

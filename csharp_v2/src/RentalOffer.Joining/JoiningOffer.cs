using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroServiceWorkshop.RapidsRivers;
using MicroServiceWorkshop.RapidsRivers.RabbitMQ;
using Newtonsoft.Json.Linq;

namespace RentalOffer.Joining
{
    class JoiningOffer : River.IPacketListener
    {
        private static readonly List<int> NewUsers = new List<int>
        {
            6, 7, 8, 9, 10
        };

        static void Main(string[] args)
        {
            string host = args[0];
            string port = args[1];

            var rapidsConnection = new RabbitMqRapids("monitor_in_csharp", host, port);
            var river = new River(rapidsConnection);
            river.RequireValue("need", "car_rental_offer");
            river.Require("user_id");
            river.Forbid("solution", "level");
            river.Register(new JoiningOffer());
        }

        public void ProcessPacket(RapidsConnection connection, JObject jsonPacket, PacketProblems warnings)
        {
            Console.WriteLine(" [*] {0}", warnings);

            var userId = int.Parse(jsonPacket["user_id"].ToString());
            if (!NewUsers.Contains(userId))
            {
                return;
            }

            Console.WriteLine($" [+] user with id {userId} is new user");

            jsonPacket["solution"] = "joining_offer";

            connection.Publish(jsonPacket.ToString());
        }

        public void ProcessError(RapidsConnection connection, PacketProblems errors)
        {
            //Console.WriteLine(" [x] {0}", errors);
        }
    }
}

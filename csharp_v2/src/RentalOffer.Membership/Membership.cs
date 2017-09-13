using System;
using System.Collections.Generic;
using MicroServiceWorkshop.RapidsRivers;
using MicroServiceWorkshop.RapidsRivers.RabbitMQ;
using Newtonsoft.Json.Linq;

namespace RentalOffer.Membership
{
    class Membership : River.IPacketListener
    {
        private static readonly Dictionary<int, int> _users = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 2 },
            { 4, 1 },
            { 5, 2 }
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
            river.Register(new Membership());
        }

        public void ProcessPacket(RapidsConnection connection, JObject jsonPacket, PacketProblems warnings)
        {
            Console.WriteLine(" [*] {0}", warnings);

            var userId = int.Parse(jsonPacket["user_id"].ToString());
            if (!_users.ContainsKey(userId))
            {
                Console.WriteLine($" [-] user with id {userId} not found");

                return;
            }

            Console.WriteLine($" [+] user with id {userId} has membership level {_users[userId]}");
            jsonPacket["level"] = _users[userId];
            connection.Publish(jsonPacket.ToString());
        }

        public void ProcessError(RapidsConnection connection, PacketProblems errors)
        {
            //Console.WriteLine(" [x] {0}", errors);
        }
    }
}

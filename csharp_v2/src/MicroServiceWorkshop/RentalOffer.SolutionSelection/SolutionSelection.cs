using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroServiceWorkshop.RapidsRivers;
using MicroServiceWorkshop.RapidsRivers.RabbitMQ;
using Newtonsoft.Json.Linq;

namespace RentalOffer.SolutionSelection
{
    class SolutionSelection : River.IPacketListener
    {
        private readonly Dictionary<string, JObject> _currentBestSolution;

        public SolutionSelection()
        {
            _currentBestSolution = new Dictionary<string, JObject>();
        }

        static void Main(string[] args)
        {
            string host = args[0];
            string port = args[1];

            var rapidsConnection = new RabbitMqRapids("monitor_in_csharp", host, port);
            var river = new River(rapidsConnection);
            river.Require("solution", "id");
            river.Register(new SolutionSelection());
        }

        public void ProcessPacket(RapidsConnection connection, JObject jsonPacket, PacketProblems warnings)
        {
            Console.WriteLine(" [*] {0}", warnings);

            var needId = jsonPacket["id"].ToString();
            _currentBestSolution[needId] = DetermineBestOffer(needId, jsonPacket);

            Console.WriteLine(
                $" [======] current best offer for {needId} is from {_currentBestSolution[needId]["solution"]}");
        }

        private JObject DetermineBestOffer(string needId, JObject incoming)
        {
            if (!_currentBestSolution.ContainsKey(needId) || 
                IsJoiningOffer(incoming))
            {
                return incoming;
            }

            var current = _currentBestSolution[needId];
            if (IsJoiningOffer(current))
            {
                Console.WriteLine(" [+] incoming offer is joining offer");
                return current;
            }
            
            var currentOfferValue = CalculateOfferValue(current);
            var newOfferValue = CalculateOfferValue(incoming);

            Console.WriteLine(
                $" [+] need id {needId}, new offer is {newOfferValue}, current offer {currentOfferValue}");

            return (currentOfferValue < newOfferValue) ? incoming : current;
        }

        private bool IsJoiningOffer(JObject jsonPacket)
        {
            return jsonPacket["solution"].ToString() == "joining_offer";
        }

        private decimal CalculateOfferValue(JObject jsonPacket)
        {
            var baseValue = int.Parse(jsonPacket["price"].ToString()) *
                   decimal.Parse(jsonPacket["frequency"].ToString());
            return jsonPacket["offer"] != null ? baseValue * decimal.Parse(jsonPacket["offer"].ToString()) : baseValue;
        }

        public void ProcessError(RapidsConnection connection, PacketProblems errors)
        {
            Console.WriteLine(" [x] {0}", errors);
        }
    }
}

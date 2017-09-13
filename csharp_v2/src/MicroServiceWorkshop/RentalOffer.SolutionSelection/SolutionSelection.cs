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
            river.Require("solution");
            river.Register(new SolutionSelection());
        }

        public void ProcessPacket(RapidsConnection connection, JObject jsonPacket, PacketProblems warnings)
        {
            Console.WriteLine(" [*] {0}", warnings);

            var needId = jsonPacket["id"].ToString();
            if (_currentBestSolution.ContainsKey(needId))
            {
                var current = _currentBestSolution[needId];
                var currentOfferValue = CalculateOfferValue(current);
                var newOfferValue = CalculateOfferValue(jsonPacket);

                Console.WriteLine($" [+] need id {needId}, new offer is {newOfferValue}, current offer {currentOfferValue}");

                if (currentOfferValue < newOfferValue)
                {
                    _currentBestSolution[needId] = jsonPacket;
                }
            }
            else
            {
                _currentBestSolution[needId] = jsonPacket;
            }

            Console.WriteLine($" [======] current best offer for {needId} is from {_currentBestSolution[needId]["solution"]}, " +
                              $"value: {CalculateOfferValue(_currentBestSolution[needId])}");
        }

        private decimal CalculateOfferValue(JObject jsonPacket)
        {
            return int.Parse(jsonPacket["price"].ToString()) *
                   decimal.Parse(jsonPacket["frequency"].ToString());
        }

        public void ProcessError(RapidsConnection connection, PacketProblems errors)
        {
            Console.WriteLine(" [x] {0}", errors);
        }
    }
}

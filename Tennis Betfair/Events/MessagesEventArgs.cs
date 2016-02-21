using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennis_Betfair.TO;

namespace Tennis_Betfair.Events
{
    public class MessagesEventArgs : EventArgs
    {
        private string message;
        private TypeDBO typeDbo;
        private string messagePlayerOne;
        private string messagePlayerTwo;

        public MessagesEventArgs(string message, TypeDBO typeDbo)
        {
            this.message = message;
            this.typeDbo = typeDbo;
        }

        public MessagesEventArgs(TypeDBO typeDbo, string messagePlayerOne, string messagePlayerTwo)
        {
            this.typeDbo = typeDbo;
            this.messagePlayerOne = messagePlayerOne;
            this.messagePlayerTwo = messagePlayerTwo;
        }

        public string Message
        {
            get { return message; }
        }

        public TypeDBO TypeDbo
        {
            get { return typeDbo; }
        }

        public string MessagePlayerOne
        {
            get { return messagePlayerOne; }
        }

        public string MessagePlayerTwo
        {
            get { return messagePlayerTwo; }
        }
    }
}

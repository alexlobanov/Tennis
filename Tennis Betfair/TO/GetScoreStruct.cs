using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennis_Betfair.TO
{
    public class GetScoreStruct
    {
        private TypeDBO typeDbo;
        private object eventId;

        public GetScoreStruct(TypeDBO typeDbo, object eventId)
        {
            this.typeDbo = typeDbo;
            this.eventId = eventId;
        }

        public TypeDBO TypeDbo
        {
            get { return typeDbo; }
        }

        public object EventId
        {
            get { return eventId; }
        }
    }
}

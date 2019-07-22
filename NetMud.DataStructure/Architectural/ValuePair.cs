using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural
{
    public class ValuePair<T>
    {
        [DataType(DataType.Text)]
        public T Actor { get; set; }

        [DataType(DataType.Text)]
        public T Victim { get; set; }

        public ValuePair()
        {
            Actor = default;
            Victim = default;
        }

        public ValuePair(T actor, T victim)
        {
            Actor = actor;
            Victim = victim;
        }

        public override string ToString()
        {
            return string.Format("{0} to {1}", Actor, Victim);
        }
    }
}

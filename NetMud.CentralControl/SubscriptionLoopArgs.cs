namespace NetMud.CentralControl
{
    public class SubscriptionLoopArgs
    {
        public int CurrentPulse { get; set; }

        public string Designation { get; set; }

        public SubscriptionLoopArgs(string designation, int pulse)
        {
            CurrentPulse = pulse;
            Designation = designation;
        }
    }
}

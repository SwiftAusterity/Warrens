namespace NetMud.DataStructure.Inanimate
{
    public interface IInanimateComponent
    {
        IInanimateTemplate Item { get; set; }

        int Amount { get; set; }
    }
}

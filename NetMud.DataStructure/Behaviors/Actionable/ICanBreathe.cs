namespace NetMud.DataStructure.Behaviors.Actionable
{
    /// <summary>
    /// Entity can breathe (for mobiles) in specified medium
    /// </summary>
    /// <typeparam name="IMaterial">The specified medium the entity can breathe in</typeparam>
    public interface ICanBreathe
    {
    }

    public enum RespiratoryType : short
    {
        Air = 0,
        Water = 1,
        All = 2
    }
}

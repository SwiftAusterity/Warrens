namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Midpoint entity interface for players/npcs
    /// </summary>
    public interface IMobile : IActor, IGetTired, ICanBeHarmed, ICanFight
    {
        MobilityState StancePosition { get; set; }
    }
}

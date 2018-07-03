namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Trigger types for output sent to this entity
    /// </summary>
    public enum AITriggerType
    {
        SpokenTo,
        Heard,
        Seen,
        Sensed,
        Smelled,
        PassiveActAt,
        AggressiveActAt
    }
}

namespace NetMud.DataStructure.NPC.IntelligenceControl
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

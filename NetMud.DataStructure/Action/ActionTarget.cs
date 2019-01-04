namespace NetMud.DataStructure.Action
{
    /// <summary>
    /// The target of an action's criteria check or result
    /// </summary>
    public enum ActionTarget : short
    {
        Self = 0,
        Tile = 1,
        Tool = 2,
        Item = 3,
        NPC = 4,
        Player = 5
    }
}

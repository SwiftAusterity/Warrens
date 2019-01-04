namespace NetMud.Commands.Attributes
{
    /// <summary>
    /// How this parameter can be found (is it data, is it a live entity)
    /// </summary>
    public enum CacheReferenceType
    {
        Entity, //things in the world
        LookupData, // reference data
        Code, //commands, help files
        Inventory, //Actor's inventory only
        MerchantStock, //target merchant's sellable inventory
        TrainerKnowledge, //target trainers's abilities/qualities list
        Data, //entity backing data
        String, //Just a string we're looking for that has no other reference
        Interaction, //When we're looking for an interaction on an entity
        Use, //When we're looking for a Use type action on an entity in our inventory
        Direction, //When we're looking specifically for directionals
        Pathway, //When we're looking specifically for pathways
        Greedy, //hacky add for communications
        Help //hacky add for help specifically
    }
}

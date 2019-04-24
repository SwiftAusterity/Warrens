namespace NetMud.DataStructure.Combat
{
    public enum FightingArtComboUsage
    {
        None, //Used when no conditions apply or as a fallback.
        Opener, // Used to start a round of combat. Wont be used if you are attacked first.
        Surprise, //Used from stealth or when you catch victims offguard.
        Punisher, //Used if the opponent whiffs an attack.
        Breaker, //Used if the opponent blocks an attack and is staggered.
        Riposte, //Used if you parry an attack and get a stagger.
        Recovery, //Used if you whiff an attack but still have initiative (are not staggered)
        Finisher //Used if you hit the opponent and get a stagger from the hit.
    }
}

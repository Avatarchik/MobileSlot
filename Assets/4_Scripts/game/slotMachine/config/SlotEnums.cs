//------------------------------------------------------------------
//custom enum
//------------------------------------------------------------------
public enum FreeSpinTriggerType
{
    Auto,
    Select
}

public enum FreeSpinRetriggerType
{
    None,
    Add,
    Refill
}

public enum ExpectReelType
{
    Null,
    BonusSpin,
    FreeSpin,
    Progressive
}

public enum PayoutWinType
{
    LOSE,
    NORMAL,
    BIGWIN,
    MEGAWIN,
    JACPOT
}

public enum SymbolType
{
    Low,
    Middle,
    High,
    Wild,
    FSScatter,//FreeSpinScatter
    PGSVScatter,//ProgressiveScatter
    Blank
}
namespace StrEnum.Npgsql.IntegrationTests;

public class Sport : StringEnum<Sport>
{
    public static readonly Sport RoadCycling = Define("ROAD_CYCLING");
    public static readonly Sport MountainBiking = Define("MTB");
    public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
}

public class Race
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Sport Sport { get; set; } = null!;
}
